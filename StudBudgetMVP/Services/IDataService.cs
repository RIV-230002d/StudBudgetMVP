using System.Collections.Generic;
using System.Threading.Tasks;
using StudBudgetMVP.Models;

namespace StudBudgetMVP.Services;
public interface IDataService
{
    Task<bool> RegisterAsync(string username, string password);
    Task<User?> LoginAsync(string username, string password);
    Task InitializeAsync();
    Task<List<Transaction>> GetTransactionsAsync(int userId);
    Task AddTransactionAsync(Transaction tx);
    Task UpdateTransactionAsync(Transaction tx);
    Task DeleteTransactionAsync(int id);
    Task<BudgetSummary> GetMonthlySummaryAsync(int userId, int year, int month);
}