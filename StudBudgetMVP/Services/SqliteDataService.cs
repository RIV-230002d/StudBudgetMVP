using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StudBudgetMVP.Models;
using Microsoft.Maui.Storage;

namespace StudBudgetMVP.Services;
public class SqliteDataService : IDataService
{
    SQLiteAsyncConnection db;

    public SqliteDataService()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "studbudget.db3");
        db = new SQLiteAsyncConnection(path);
        db.CreateTableAsync<User>().Wait();
        db.CreateTableAsync<Transaction>().Wait();
    }

    public Task<bool> RegisterAsync(string username, string password)
    {
        return db.Table<User>().Where(u => u.Username == username)
            .FirstOrDefaultAsync()
            .ContinueWith(t =>
            {
                if (t.Result != null) return false;
                db.InsertAsync(new User { Username = username, Password = password }).Wait();
                return true;
            });
    }

    public Task<User?> LoginAsync(string username, string password)
        => db.Table<User>()
             .Where(u => u.Username == username && u.Password == password)
             .FirstOrDefaultAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task<List<Transaction>> GetTransactionsAsync(int userId)
        => db.Table<Transaction>()
             .Where(t => t.UserId == userId)
             .OrderByDescending(t => t.Date)
             .ToListAsync();

    public Task AddTransactionAsync(Transaction tx) => db.InsertAsync(tx);
    public Task UpdateTransactionAsync(Transaction tx) => db.UpdateAsync(tx);
    public Task DeleteTransactionAsync(int id) => db.DeleteAsync<Transaction>(id);

    public async Task<BudgetSummary> GetMonthlySummaryAsync(int userId, int year, int month)
    {
        var list = await GetTransactionsAsync(userId);
        var filtered = list.Where(t => t.Date.Year == year && t.Date.Month == month);
        return new BudgetSummary
        {
            Income = filtered.Where(t => t.Amount > 0).Sum(t => t.Amount),
            Expense = filtered.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount))
        };
    }
}