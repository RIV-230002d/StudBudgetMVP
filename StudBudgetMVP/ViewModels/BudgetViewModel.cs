using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Models;
using StudBudgetMVP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StudBudgetMVP.ViewModels
{
    public partial class BudgetViewModel : ObservableObject
    {
        private readonly IDataService data;

        public BudgetViewModel(IDataService service)
        {
            data = service;

            IncomeCategories = new ObservableCollection<CategoryDisplay>();
            ExpenseCategories = new ObservableCollection<CategoryDisplay>();

            AddCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            DeleteCategoryCommand = new AsyncRelayCommand<CategoryDisplay>(DeleteCategoryAsync);

            IsIncomeTab = true;
            OverspentFirst = true;
            SortModes = new[] { "По имени", "Сначала новые", "Сначала старые" };
            SelectedSortMode = SortModes[0];

            NewIsIncome = true;
        }

        public ObservableCollection<CategoryDisplay> IncomeCategories { get; }
        public ObservableCollection<CategoryDisplay> ExpenseCategories { get; }

        [ObservableProperty] private string newCategoryName;
        [ObservableProperty] private string newLimit;
        [ObservableProperty] private bool newIsIncome;
        public bool IsExpense => !NewIsIncome;

        [ObservableProperty] private decimal totalIncome;
        [ObservableProperty] private decimal totalExpense;
        [ObservableProperty] private bool hasOverspend;

        [ObservableProperty] private bool isIncomeTab;
        public bool IsExpenseTab => !IsIncomeTab;
        partial void OnIsIncomeTabChanged(bool _, bool __) => OnPropertyChanged(nameof(IsExpenseTab));

        public string[] SortModes { get; }
        [ObservableProperty] private string selectedSortMode;
        [ObservableProperty] private bool overspentFirst;

        partial void OnSelectedSortModeChanged(string _, string __) => ApplySorting();
        partial void OnOverspentFirstChanged(bool _, bool __) => ApplySorting();

        public IAsyncRelayCommand AddCommand { get; }
        public IAsyncRelayCommand<CategoryDisplay> DeleteCategoryCommand { get; }

        partial void OnNewCategoryNameChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnNewLimitChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnNewIsIncomeChanged(bool _, bool __)
        {
            OnPropertyChanged(nameof(IsExpense));
            AddCommand.NotifyCanExecuteChanged();
        }

        public async Task LoadAsync() => await RefreshAllAsync();

        private async Task RefreshAllAsync()
        {
            var userId = Preferences.Get("userId", 0);
            var now = DateTime.Now;

            var cats = await data.GetCategoriesAsync(userId);
            var txs = await data.GetTransactionsAsync(userId, now.Year, now.Month);

            IncomeCategories.Clear();
            ExpenseCategories.Clear();

            foreach (var c in cats)
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

                if (c.IsIncome)
                    IncomeCategories.Add(item);
                else
                    ExpenseCategories.Add(item);
            }

            ApplySorting();

            TotalIncome = IncomeCategories.Sum(i => i.Total);
            TotalExpense = ExpenseCategories.Sum(e => e.Total);
            HasOverspend = TotalExpense > TotalIncome;
        }

        private void ApplySorting()
        {
            IEnumerable<CategoryDisplay> SortCore(IEnumerable<CategoryDisplay> src) =>
                SelectedSortMode switch
                {
                    "Сначала новые" => src.OrderByDescending(c => c.Id),
                    "Сначала старые" => src.OrderBy(c => c.Id),
                    _ => src.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
                };

            var incOrdered = SortCore(IncomeCategories).ToList();
            IncomeCategories.Clear();
            foreach (var i in incOrdered) IncomeCategories.Add(i);

            var baseExpenses = ExpenseCategories.ToList();
            IEnumerable<CategoryDisplay> orderedExp =
                OverspentFirst
                    ? SortCore(baseExpenses.Where(e => e.IsOverspent))
                        .Concat(SortCore(baseExpenses.Where(e => !e.IsOverspent)))
                    : SortCore(baseExpenses);

            ExpenseCategories.Clear();
            foreach (var e in orderedExp) ExpenseCategories.Add(e);
        }

        private bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
                return false;

            if (IsExpense)
            {
                var txt = (NewLimit ?? string.Empty).Replace('.', ',');
                return decimal.TryParse(txt, out var l) && l >= 0;
            }

            return true;
        }

        private async Task SaveAsync()
        {
            var userId = Preferences.Get("userId", 0);

            decimal? limitValue = null;
            if (IsExpense)
            {
                var txt = (NewLimit ?? string.Empty).Replace('.', ',');
                limitValue = decimal.Parse(txt);
            }

            var cat = new Category
            {
                UserId = userId,
                Name = NewCategoryName,
                IsIncome = NewIsIncome,
                Limit = limitValue
            };

            await data.AddCategoryAsync(cat);

            NewCategoryName = string.Empty;
            NewLimit = string.Empty;
            NewIsIncome = true;

            await RefreshAllAsync();
        }

        private async Task DeleteCategoryAsync(CategoryDisplay cat)
        {
            if (cat == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удалить категорию?",
                "Вы действительно хотите удалить эту категорию и все связанные с ней транзакции?",
                "Да", "Нет");

            if (!confirm) return;

            await data.DeleteCategoryAsync(cat.Id);
            await RefreshAllAsync();
        }
    }
}
