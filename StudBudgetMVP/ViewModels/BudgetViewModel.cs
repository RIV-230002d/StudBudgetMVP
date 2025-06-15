using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Models;
using StudBudgetMVP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StudBudgetMVP.ViewModels;

public class CategoryDisplay
{
    public int Id { get; init; }
    public string Name { get; init; }
    public bool IsIncome { get; init; }
    public decimal? Limit { get; init; }
    public decimal Total { get; init; }
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
        DeleteCategoryCommand = new AsyncRelayCommand<CategoryDisplay>(DeleteCategoryAsync);

        IsIncomeTab = true;
        OverspentFirst = true;
        SortModes = new[] { "По имени", "Сначала новые", "Сначала старые" };
        SelectedSortMode = SortModes[0];
    }

    // ---------- коллекции ----------
    public ObservableCollection<CategoryDisplay> IncomeCategories { get; }
    public ObservableCollection<CategoryDisplay> ExpenseCategories { get; }

    // ---------- поля формы ----------
    [ObservableProperty] private string newCategoryName;
    [ObservableProperty] private string newLimit;
    [ObservableProperty] private bool newIsIncome;
    public bool IsExpense => !NewIsIncome;

    // —–– уведомляем команду при изменениях поля/лимита/типа
    partial void OnNewCategoryNameChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
    partial void OnNewLimitChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
    partial void OnNewIsIncomeChanged(bool _, bool __)
    {
        OnPropertyChanged(nameof(IsExpense));
        AddCommand.NotifyCanExecuteChanged();
    }

    // ---------- итоги ----------
    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalExpense;
    [ObservableProperty] private bool hasOverspend;

    // ---------- табы ----------
    [ObservableProperty] private bool isIncomeTab;
    public bool IsExpenseTab => !IsIncomeTab;
    partial void OnIsIncomeTabChanged(bool _, bool __) => OnPropertyChanged(nameof(IsExpenseTab));

    // ---------- управление сортировкой ----------
    public string[] SortModes { get; }
    [ObservableProperty] private string selectedSortMode;
    [ObservableProperty] private bool overspentFirst;

    partial void OnSelectedSortModeChanged(string _, string __) => ApplySorting();
    partial void OnOverspentFirstChanged(bool _, bool __) => ApplySorting();

    // ---------- команды ----------
    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand<CategoryDisplay> DeleteCategoryCommand { get; }

    // ================= LOAD =================
    public async Task LoadAsync() => await RefreshAllAsync();

    // -----------------------------------------------------------------
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

    // ---------- сортировка ----------
    private void ApplySorting()
    {
        IEnumerable<CategoryDisplay> SortCore(IEnumerable<CategoryDisplay> src) =>
            SelectedSortMode switch
            {
                "Сначала новые" => src.OrderByDescending(c => c.Id),
                "Сначала старые" => src.OrderBy(c => c.Id),
                _ => src.OrderBy(c => c.Name,
                                                StringComparer.CurrentCultureIgnoreCase)
            };

        // ---------- ДОХОДЫ ----------
        var incOrdered = SortCore(IncomeCategories).ToList();
        IncomeCategories.Clear();
        foreach (var i in incOrdered) IncomeCategories.Add(i);

        // ---------- РАСХОДЫ ----------
        // копию делаем СРАЗУ, чтобы дальнейшее Clear() не влиял на перечисление
        var srcExpenses = ExpenseCategories.ToList();

        IEnumerable<CategoryDisplay> ordered;

        if (OverspentFirst)
        {
            var overs = SortCore(srcExpenses.Where(c => c.IsOverspent));
            var rest = SortCore(srcExpenses.Where(c => !c.IsOverspent));
            ordered = overs.Concat(rest);
        }
        else
        {
            ordered = SortCore(srcExpenses);
        }

        ExpenseCategories.Clear();
        foreach (var e in ordered) ExpenseCategories.Add(e);
    }

    // ---------- add ----------
    private bool CanSave()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return false;

        if (!IsExpense)            // это доход
            return true;

        var txt = NewLimit?.Trim();
        return !string.IsNullOrWhiteSpace(txt) &&
               decimal.TryParse(txt, out var l) &&
               l >= 0;
    }

    private async Task SaveAsync()
    {
        var userId = Preferences.Get("userId", 0);

        var cat = new Category
        {
            UserId = userId,
            Name = NewCategoryName.Trim(),
            IsIncome = NewIsIncome,
            Limit = IsExpense ? decimal.Parse(NewLimit.Trim()) : (decimal?)null
        };

        await data.AddCategoryAsync(cat);

        NewCategoryName = string.Empty;
        NewLimit = string.Empty;
        NewIsIncome = false;

        await RefreshAllAsync();
    }

    // ---------- delete ----------
    private async Task DeleteCategoryAsync(CategoryDisplay cat)
    {
        if (cat == null) return;

        await data.DeleteCategoryAsync(cat.Id);
        await RefreshAllAsync();
    }
}