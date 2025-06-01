using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudBudgetMVP.Models;
using StudBudgetMVP.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace StudBudgetMVP.ViewModels;

public partial class BudgetViewModel : ObservableObject
{
    private readonly IDataService dataService;

    public BudgetViewModel(IDataService ds)
    {
        dataService = ds;
        Categories = new ObservableCollection<Category>();
        OverspentCategories = new ObservableCollection<string>();
        AddCommand = new AsyncRelayCommand(AddCategoryAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteCategoryAsync);
    }

    public ObservableCollection<Category> Categories { get; }
    public ObservableCollection<string> OverspentCategories { get; }

    [ObservableProperty]
    private string newCategoryName;

    [ObservableProperty]
    private decimal newLimit;

    [ObservableProperty]
    private bool newIsIncome;

    [ObservableProperty]
    private Category selectedCategory;

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public async Task LoadCategoriesAsync()
    {
        var userId = Preferences.Get("userId", 0);
        var cats = await dataService.GetCategoriesAsync(userId);
        Categories.Clear();
        foreach (var cat in cats)
            Categories.Add(cat);

        await CalculateOverspentAsync();
    }

    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return;

        var userId = Preferences.Get("userId", 0);

        var cat = new Category
        {
            UserId = userId,
            Name = NewCategoryName,
            IsIncome = NewIsIncome,
            Limit = NewIsIncome ? 0 : NewLimit
        };

        await dataService.AddCategoryAsync(cat);
        NewCategoryName = string.Empty;
        NewLimit = 0;
        NewIsIncome = false;
        await LoadCategoriesAsync();
    }

    private async Task DeleteCategoryAsync()
    {
        if (SelectedCategory == null) return;

        await dataService.DeleteCategoryAsync(SelectedCategory.Id);
        await LoadCategoriesAsync();
    }

    public async Task CalculateOverspentAsync()
    {
        OverspentCategories.Clear();

        var userId = Preferences.Get("userId", 0);
        var now = DateTime.Now;
        var transactions = await dataService.GetTransactionsAsync(userId, now.Year, now.Month);

        foreach (var cat in Categories.Where(c => !c.IsIncome))
        {
            var spent = transactions.Where(t => t.CategoryId == cat.Id).Sum(t => t.Amount);
            if (cat.Limit > 0 && spent > cat.Limit)
            {
                OverspentCategories.Add($"{cat.Name}: overspent {(spent - cat.Limit):C}");
            }
        }
    }
}