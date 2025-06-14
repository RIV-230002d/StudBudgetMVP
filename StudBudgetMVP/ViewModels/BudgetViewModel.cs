using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Models;
using StudBudgetMVP.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StudBudgetMVP.ViewModels;

/// <summary>Запись для отображения в списке.</summary>
public class CategoryDisplay
{
    public int Id { get; init; }
    public string Name { get; init; }
    public bool IsIncome { get; init; }
    public decimal? Limit { get; init; }      // null → доход или нет лимита
    public decimal Total { get; init; }      // накоплено в текущем месяце

    public bool IsOverspent => !IsIncome && Limit is decimal l && Total > l;
}

public partial class BudgetViewModel : ObservableObject
{
    private readonly IDataService data;

    public BudgetViewModel(IDataService service)
    {
        data = service;

        IncomeCategories = new ObservableCollection<CategoryDisplay>();
        ExpenseCategories = new ObservableCollection<CategoryDisplay>();

        AddCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedCategory != null);

        IsIncomeTab = true;                     // показать список доходов
    }

    // ---------- коллекции для UI ----------
    public ObservableCollection<CategoryDisplay> IncomeCategories { get; }
    public ObservableCollection<CategoryDisplay> ExpenseCategories { get; }

    // ---------- поля формы ----------
    [ObservableProperty] private string newCategoryName;
    [ObservableProperty] private string newLimit;
    [ObservableProperty] private bool newIsIncome;
    public bool IsExpense => !NewIsIncome;

    // ---------- выбранная ----------
    [ObservableProperty] private Category selectedCategory;

    // ---------- итоговые суммы ----------
    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalExpense;

    // ---------- переключатель списков ----------
    [ObservableProperty] private bool isIncomeTab;
    public bool IsExpenseTab => !IsIncomeTab;
    partial void OnIsIncomeTabChanged(bool _, bool __) =>
        OnPropertyChanged(nameof(IsExpenseTab));

    // ---------- команды ----------
    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    // ----- обновление CanExecute -----
    partial void OnNewCategoryNameChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
    partial void OnNewLimitChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
    partial void OnNewIsIncomeChanged(bool _, bool __)
    {
        OnPropertyChanged(nameof(IsExpense));
        AddCommand.NotifyCanExecuteChanged();
    }
    partial void OnSelectedCategoryChanged(Category _, Category __) => DeleteCommand.NotifyCanExecuteChanged();

    // ================= публичное =================
    public async Task LoadAsync() => await RefreshAllAsync();

    // -----------------------------------------------------------------
    private async Task RefreshAllAsync()
    {
        var userId = Preferences.Get("userId", 0);
        var now = DateTime.Now;

        // --- исходные данные ---
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

        TotalIncome = IncomeCategories.Sum(i => i.Total);
        TotalExpense = ExpenseCategories.Sum(e => e.Total);
    }

    // ================= создание / редактирование =================
    private bool CanSave()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return false;

        return IsExpense
            ? decimal.TryParse(NewLimit, out var l) && l >= 0
            : true;
    }

    private async Task SaveAsync()
    {
        var userId = Preferences.Get("userId", 0);

        var cat = new Category
        {
            UserId = userId,
            Name = NewCategoryName,
            IsIncome = NewIsIncome,
            Limit = IsExpense ? decimal.Parse(NewLimit) : (decimal?)null
        };

        await data.AddCategoryAsync(cat);

        // очистка формы
        NewCategoryName = string.Empty;
        NewLimit = string.Empty;
        NewIsIncome = false;

        await RefreshAllAsync();
    }

    // ================= удаление =================
    private async Task DeleteAsync()
    {
        if (SelectedCategory == null) return;

        await data.DeleteCategoryAsync(SelectedCategory.Id);
        await RefreshAllAsync();
    }
}