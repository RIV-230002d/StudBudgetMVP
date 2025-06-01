using SQLite;
using StudBudgetMVP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace StudBudgetMVP.Services
{
    public class SqliteDataService : IDataService
    {
        private readonly SQLiteAsyncConnection db;

        public SqliteDataService(string dbPath)
        {
            db = new SQLiteAsyncConnection(dbPath);
            db.CreateTableAsync<User>().Wait();
            db.CreateTableAsync<Category>().Wait();
            db.CreateTableAsync<Transaction>().Wait();
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            var exists = await db.Table<User>().Where(u => u.Username == username).FirstOrDefaultAsync();
            if (exists != null)
                return false;
            await db.InsertAsync(new User { Username = username, Password = password });
            return true;
        }

        public Task<User?> LoginAsync(string username, string password)
            => db.Table<User>().Where(u => u.Username == username && u.Password == password).FirstOrDefaultAsync();

        public Task<List<Category>> GetCategoriesAsync(int userId)
            => db.Table<Category>().Where(c => c.UserId == userId).ToListAsync();

        public Task AddCategoryAsync(Category cat)
            => db.InsertAsync(cat);

        public Task DeleteCategoryAsync(int catId)
            => db.DeleteAsync<Category>(catId);

        // Исправлено: фильтрация транзакций по месяцу/году через диапазон дат (работает в SQLite)
        public async Task<List<Transaction>> GetTransactionsAsync(int userId, int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var list = await db.Table<Transaction>()
                .Where(t => t.UserId == userId && t.Date >= monthStart && t.Date < nextMonth)
                .ToListAsync();

            return list;
        }

        public Task AddTransactionAsync(Transaction tx)
            => db.InsertAsync(tx);

        public Task DeleteTransactionAsync(int id)
            => db.DeleteAsync<Transaction>(id);

        public async Task<decimal> GetTotalIncomeAsync(int userId, int month, int year)
        {
            var monthStart = new DateTime(year, month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var cats = await db.Table<Category>().Where(c => c.UserId == userId && c.IsIncome).ToListAsync();
            var catIds = cats.Select(c => c.Id).ToList();

            var txs = await db.Table<Transaction>()
                .Where(t => t.UserId == userId && t.Date >= monthStart && t.Date < nextMonth)
                .ToListAsync();

            return txs.Where(t => catIds.Contains(t.CategoryId)).Sum(t => t.Amount);
        }

        public async Task<decimal> GetTotalExpenseAsync(int userId, int month, int year)
        {
            var monthStart = new DateTime(year, month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var cats = await db.Table<Category>().Where(c => c.UserId == userId && !c.IsIncome).ToListAsync();
            var catIds = cats.Select(c => c.Id).ToList();

            var txs = await db.Table<Transaction>()
                .Where(t => t.UserId == userId && t.Date >= monthStart && t.Date < nextMonth)
                .ToListAsync();

            return txs.Where(t => catIds.Contains(t.CategoryId)).Sum(t => t.Amount);
        }

        public async Task<List<(string Category, decimal Limit, decimal Expense)>> GetOverspentCategoriesAsync(int userId, int month, int year)
        {
            var monthStart = new DateTime(year, month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var cats = await db.Table<Category>().Where(c => c.UserId == userId && !c.IsIncome && c.Limit != null).ToListAsync();
            var catIds = cats.Select(c => c.Id).ToList();
            var txs = await db.Table<Transaction>()
                .Where(t => t.UserId == userId && t.Date >= monthStart && t.Date < nextMonth)
                .ToListAsync();

            var result = new List<(string, decimal, decimal)>();
            foreach (var cat in cats)
            {
                var exp = txs.Where(t => t.CategoryId == cat.Id).Sum(t => t.Amount);
                if (exp > (cat.Limit ?? 0))
                    result.Add((cat.Name, cat.Limit ?? 0, exp));
            }
            return result;
        }
    }
}