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

    public partial class TransactionsViewModel : ObservableObject
    {
        private readonly IDataService data;

        public TransactionsViewModel(IDataService ds)
        {
            data = ds;

            Categories = new ObservableCollection<Category>();
            Transactions = new ObservableCollection<TxDisplay>();

            AddCommand = new AsyncRelayCommand(AddAsync, CanAdd);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedTransaction != null);
        }

        // коллекции
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<TxDisplay> Transactions { get; }

        // выбранные / вводимые данные
        [ObservableProperty] private Category selectedCategory;
        [ObservableProperty] private string rubles = string.Empty;
        [ObservableProperty] private string kopeks = string.Empty;   // ← пусто вместо «0»
        [ObservableProperty] private TxDisplay selectedTransaction;

        // команды
        public IAsyncRelayCommand AddCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }

        // ---------- загрузка ----------
        public async Task LoadAsync()
        {
            var userId = Preferences.Get("userId", 0);
            var now = DateTime.Now;

            // категории
            Categories.Clear();
            foreach (var c in await data.GetCategoriesAsync(userId))
                Categories.Add(c);

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
        }

        // ---------- валидация для кнопки ----------
        private bool CanAdd()
        {
            // копейки могут быть пустыми -> 0
            var kopValid = string.IsNullOrWhiteSpace(Kopeks) || int.TryParse(Kopeks, out var k) && k >= 0 && k < 100;
            var rubValid = !string.IsNullOrWhiteSpace(Rubles) && decimal.TryParse(Rubles, out _);
            return SelectedCategory != null && rubValid && kopValid;
        }

        // обновляем доступность кнопок
        partial void OnSelectedCategoryChanged(Category _, Category __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnRublesChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnKopeksChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
        partial void OnSelectedTransactionChanged(TxDisplay _, TxDisplay __) => DeleteCommand.NotifyCanExecuteChanged();

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

                // очистка формы
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
        private async Task DeleteAsync()
        {
            if (SelectedTransaction == null) return;

            await data.DeleteTransactionAsync(SelectedTransaction.Id);
            await LoadAsync();
        }
    }
}