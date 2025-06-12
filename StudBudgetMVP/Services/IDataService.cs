using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudBudgetMVP.Models;

namespace StudBudgetMVP.Services
{
    public interface IDataService
    {
        // --- авторизация ---
        Task<bool> RegisterAsync(string username, string password);
        Task<User?> LoginAsync(string username, string password);

        // --- категории ---
        Task<List<Category>> GetCategoriesAsync(int userId);
        Task AddCategoryAsync(Category cat);
        Task DeleteCategoryAsync(int catId);

        // --- транзакции ---
        Task<List<Transaction>> GetTransactionsAsync(int userId, int year, int month);
        Task AddTransactionAsync(Transaction tx);
        Task DeleteTransactionAsync(int id);

        // --- агрегаты для бюджета ---
        Task<decimal> GetTotalIncomeAsync(int userId, int month, int year);
        Task<decimal> GetTotalExpenseAsync(int userId, int month, int year);
        Task<List<(string Category, decimal Limit, decimal Expense)>> GetOverspentCategoriesAsync(int userId, int month, int year);
    }
}