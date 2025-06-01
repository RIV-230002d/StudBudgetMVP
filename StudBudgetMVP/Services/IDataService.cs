using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudBudgetMVP.Models;

namespace StudBudgetMVP.Services
{
    public interface IDataService
    {
        Task<bool> RegisterAsync(string username, string password);
        Task<User?> LoginAsync(string username, string password);

        Task<List<Category>> GetCategoriesAsync(int userId);
        Task AddCategoryAsync(Category cat);
        Task DeleteCategoryAsync(int catId);

        Task<List<Transaction>> GetTransactionsAsync(int userId, int month, int year);
        Task AddTransactionAsync(Transaction tx);
        Task DeleteTransactionAsync(int id);

        // Для бюджета и перерасхода
        Task<decimal> GetTotalIncomeAsync(int userId, int month, int year);
        Task<decimal> GetTotalExpenseAsync(int userId, int month, int year);
        Task<List<(string Category, decimal Limit, decimal Expense)>> GetOverspentCategoriesAsync(int userId, int month, int year);
    }
}