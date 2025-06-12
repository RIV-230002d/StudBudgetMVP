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

public partial class BudgetViewModel : ObservableObject
{
    private readonly IDataService data;

    public BudgetViewModel(IDataService service)
    {
        data = service;

        IncomeCategories = new ObservableCollection<Category>();
        ExpenseCategories = new ObservableCollection<Category>();

        AddCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedCategory != null);
    }

    // ---------- коллекции ----------
    public ObservableCollection<Category> IncomeCategories { get; }
    public ObservableCollection<Category> ExpenseCategories { get; }

    // ---------- поля ввода ----------
    [ObservableProperty] private string newCategoryName;
    [ObservableProperty] private string newLimit;      // для расхода
    [ObservableProperty] private bool newIsIncome;   // true = доход
    public bool IsExpense => !NewIsIncome;

    // ---------- выбранная ----------
    [ObservableProperty] private Category selectedCategory;

    // ---------- итоги ----------
    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalExpense;

    // ---------- команды ----------
    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    // ========== события для CanExecute ==========
    partial void OnNewCategoryNameChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
    partial void OnNewLimitChanged(string _, string __) => AddCommand.NotifyCanExecuteChanged();
    partial void OnNewIsIncomeChanged(bool _, bool __)
    {
        OnPropertyChanged(nameof(IsExpense));
        AddCommand.NotifyCanExecuteChanged();
    }
    partial void OnSelectedCategoryChanged(Category _, Category __) => DeleteCommand.NotifyCanExecuteChanged();

    // ========== PUBLIC: загрузка ==========
    public async Task LoadAsync() => await RefreshAllAsync();

    // ---------------------------------------------------------------------
    // --------------------  ВНУТРЕННЯЯ  РАБОТА  ----------------------------
    // ---------------------------------------------------------------------

    private async Task RefreshAllAsync()
    {
        var userId = Preferences.Get("userId", 0);

        // --- категории
        var cats = await data.GetCategoriesAsync(userId);
        IncomeCategories.Clear();
        ExpenseCategories.Clear();

        foreach (var c in cats)
        {
            if (c.IsIncome) IncomeCategories.Add(c);
            else ExpenseCategories.Add(c);
        }

        // --- суммы
        var dt = DateTime.Now;
        TotalIncome = await data.GetTotalIncomeAsync(userId, dt.Month, dt.Year);
        TotalExpense = await data.GetTotalExpenseAsync(userId, dt.Month, dt.Year);
    }

    // ---------- сохранить / добавить ----------
    private bool CanSave()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return false;

        if (IsExpense)
            return decimal.TryParse(NewLimit, out var l) && l >= 0;

        return true;
    }

    private async Task SaveAsync()
    {
        var userId = Preferences.Get("userId", 0);

        Category c;
        if (SelectedCategory != null && SelectedCategory.Id > 0)
        {
            c = SelectedCategory;
            c.Name = NewCategoryName;
            c.IsIncome = NewIsIncome;
            c.Limit = IsExpense ? decimal.Parse(NewLimit) : (decimal?)null;
        }
        else
        {
            c = new Category
            {
                UserId = userId,
                Name = NewCategoryName,
                IsIncome = NewIsIncome,
                Limit = IsExpense ? decimal.Parse(NewLimit) : (decimal?)null
            };
        }

        await data.AddCategoryAsync(c);

        // сброс формы
        NewCategoryName = string.Empty;
        NewLimit = string.Empty;
        NewIsIncome = false;

        await RefreshAllAsync();
    }

    // ---------- удалить ----------
    private async Task DeleteAsync()
    {
        if (SelectedCategory == null) return;

        await data.DeleteCategoryAsync(SelectedCategory.Id);
        await RefreshAllAsync();
    }
}