using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace StudBudgetMVP.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly IDataService data;

        public HomeViewModel(IDataService dataService)
        {
            data = dataService;
            ExpenseCategories = new ObservableCollection<CategoryDisplay>();
        }

        [ObservableProperty] private string currentMonthYear;
        [ObservableProperty] private decimal totalIncome;
        [ObservableProperty] private decimal totalExpense;
        [ObservableProperty] private bool hasOverspend;

        public ObservableCollection<CategoryDisplay> ExpenseCategories { get; }

        public async Task LoadAsync()
        {
            var userId = Preferences.Get("userId", 0);
            var now = DateTime.Now;

            // Месяц и год для шапки
            CurrentMonthYear = now.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

            var cats = await data.GetCategoriesAsync(userId);
            var txs = await data.GetTransactionsAsync(userId, now.Year, now.Month);

            ExpenseCategories.Clear();

            foreach (var c in cats.Where(x => !x.IsIncome))
            {
                decimal sum = txs.Where(t => t.CategoryId == c.Id).Sum(t => t.Amount);

                var item = new CategoryDisplay
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsIncome = c.IsIncome,
                    Limit = c.Limit,
                    Total = sum
                };

                ExpenseCategories.Add(item);
            }

            TotalIncome = cats.Where(x => x.IsIncome)
                .Select(cat => txs.Where(t => t.CategoryId == cat.Id).Sum(t => t.Amount))
                .Sum();

            TotalExpense = ExpenseCategories.Sum(e => e.Total);
            HasOverspend = TotalExpense > TotalIncome;
        }
    }
}
