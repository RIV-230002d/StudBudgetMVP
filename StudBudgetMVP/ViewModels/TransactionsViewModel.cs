using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Models;
using StudBudgetMVP.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StudBudgetMVP.ViewModels
{
    // проекция для вывода
    public class TxDisplay
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }
        public decimal Amount { get; init; }
        public string CategoryName { get; init; }
    }

    public class CategoryFilterItem : ObservableObject
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }

        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set => SetProperty(ref isChecked, value);
        }
    }

    public partial class TransactionsViewModel : ObservableObject
    {
        private readonly IDataService data;

        public TransactionsViewModel(IDataService ds)
        {
            data = ds;

            Categories = new ObservableCollection<Category>();
            Transactions = new ObservableCollection<TxDisplay>();
            CategoryFilters = new ObservableCollection<CategoryFilterItem>();

            AddCommand = new AsyncRelayCommand(AddAsync, CanAdd);
            DeleteTransactionCommand = new AsyncRelayCommand<TxDisplay>(DeleteAsync);

            ToggleFiltersCommand = new RelayCommand(ToggleFilters);
            ToggleCategoryFilterCommand = new RelayCommand(ToggleCategoryFilters);
            ResetFiltersCommand = new RelayCommand(ResetFilters);

            // Фильтры по умолчанию:
            var now = DateTime.Now;
            DateFrom = new DateTime(now.Year, now.Month, 1);
            DateTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        }

        // коллекции
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<TxDisplay> Transactions { get; }
        public ObservableCollection<CategoryFilterItem> CategoryFilters { get; }

        // выбранные / вводимые данные
        [ObservableProperty] private Category selectedCategory;
        [ObservableProperty] private string rubles = string.Empty;
        [ObservableProperty] private string kopeks = string.Empty;   // ← пусто вместо «0»
        [ObservableProperty] private TxDisplay selectedTransaction;

        // фильтры
        [ObservableProperty] private bool areFiltersVisible;
        [ObservableProperty] private bool areCategoryFiltersVisible;

        [ObservableProperty] private DateTime dateFrom;
        [ObservableProperty] private DateTime dateTo;

        // команды
        public IAsyncRelayCommand AddCommand { get; }
        public IAsyncRelayCommand<TxDisplay> DeleteTransactionCommand { get; }
        public RelayCommand ToggleFiltersCommand { get; }
        public RelayCommand ToggleCategoryFilterCommand { get; }
        public RelayCommand ResetFiltersCommand { get; }

        // Отфильтрованный список транзакций
        public ObservableCollection<TxDisplay> FilteredTransactions { get; } = new();

        // ---------- загрузка ----------
        public async Task LoadAsync()
        {
            var userId = Preferences.Get("userId", 0);
            var now = DateTime.Now;

            // категории
            Categories.Clear();
            foreach (var c in await data.GetCategoriesAsync(userId))
                Categories.Add(c);

            // категории для фильтра
            CategoryFilters.Clear();
            foreach (var c in Categories)
                CategoryFilters.Add(new CategoryFilterItem { Name = c.Name, CategoryId = c.Id, IsChecked = true });

            // транзакции -> проекция + сортировка DESC
            var catDict = Categories.ToDictionary(c => c.Id, c => c.Name);
            var txs = await data.GetTransactionsAsync(userId, now.Year, now.Month);

            Transactions.Clear();
            foreach (var t in txs.OrderByDescending(t => t.Date))
            {
                Transactions.Add(new TxDisplay
                {
                    Id = t.Id,
                    Date = t.Date,
                    Amount = t.Amount,
                    CategoryName = catDict.TryGetValue(t.CategoryId, out var n) ? n : $"ID {t.CategoryId}"
                });
            }

            ApplyFilters();
        }

        // ---------- валидация для кнопки ----------
        private bool CanAdd()
        {
            var kopValid = string.IsNullOrWhiteSpace(Kopeks) || int.TryParse(Kopeks, out var k) && k >= 0 && k < 100;
            var rubValid = !string.IsNullOrWhiteSpace(Rubles) && decimal.TryParse(Rubles, out _);
            return SelectedCategory != null && rubValid && kopValid;
        }

        partial void OnSelectedCategoryChanged(Category _, Category __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnRublesChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnKopeksChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnSelectedTransactionChanged(TxDisplay _, TxDisplay __) => DeleteTransactionCommand.NotifyCanExecuteChanged();

        // ---------- добавить ----------
        private async Task AddAsync()
        {
            try
            {
                var userId = Preferences.Get("userId", 0);

                decimal rub = decimal.Parse(Rubles);
                int kop = string.IsNullOrWhiteSpace(Kopeks) ? 0 : int.Parse(Kopeks);

                var amount = rub + kop / 100m;

                var tx = new Transaction
                {
                    UserId = userId,
                    CategoryId = SelectedCategory.Id,
                    Amount = amount,
                    Date = DateTime.Now
                };

                await data.AddTransactionAsync(tx);

                Rubles = string.Empty;
                Kopeks = string.Empty;

                await LoadAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AddTx] " + ex);
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        // ---------- удалить ----------
        private async Task DeleteAsync(TxDisplay tx)
        {
            if (tx == null) return;
            await data.DeleteTransactionAsync(tx.Id);
            await LoadAsync();
        }

        // ---------- фильтры ----------
        private void ToggleFilters() => AreFiltersVisible = !AreFiltersVisible;
        private void ToggleCategoryFilters() => AreCategoryFiltersVisible = !AreCategoryFiltersVisible;

        private void ResetFilters()
        {
            foreach (var c in CategoryFilters)
                c.IsChecked = true;

            var now = DateTime.Now;
            DateFrom = new DateTime(now.Year, now.Month, 1);
            DateTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            ApplyFilters();
        }

        partial void OnDateFromChanged(DateTime oldValue, DateTime newValue) => ApplyFilters();
        partial void OnDateToChanged(DateTime oldValue, DateTime newValue) => ApplyFilters();

        partial void OnAreCategoryFiltersVisibleChanged(bool oldValue, bool newValue) { /* Nothing */ }
        partial void OnAreFiltersVisibleChanged(bool oldValue, bool newValue)
        {
            if (!newValue) ApplyFilters();
        }

        private void ApplyFilters()
        {
            var activeCategories = CategoryFilters.Where(cf => cf.IsChecked).Select(cf => cf.CategoryId).ToHashSet();
            var from = DateFrom.Date;
            var to = DateTo.Date.AddDays(1).AddTicks(-1); // включительно до конца дня

            var filtered = Transactions.Where(t =>
                activeCategories.Contains(Categories.FirstOrDefault(c => c.Name == t.CategoryName)?.Id ?? -1) &&
                t.Date >= from && t.Date <= to
            );

            FilteredTransactions.Clear();
            foreach (var tx in filtered)
                FilteredTransactions.Add(tx);
        }
    }
}
