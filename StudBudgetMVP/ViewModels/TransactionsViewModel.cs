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

public partial class TransactionsViewModel : ObservableObject
{
    private readonly IDataService dataService;

    public TransactionsViewModel(IDataService ds)
    {
        dataService = ds;
        Transactions = new ObservableCollection<Transaction>();
        Categories = new ObservableCollection<Category>();
        AddCommand = new AsyncRelayCommand(AddTransactionAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteTransactionAsync);
    }

    public ObservableCollection<Transaction> Transactions { get; }
    public ObservableCollection<Category> Categories { get; }

    [ObservableProperty]
    private Category selectedCategory;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private Transaction selectedTransaction;

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public async Task LoadAsync()
    {
        var userId = Preferences.Get("userId", 0);
        var now = DateTime.Now;

        var cats = await dataService.GetCategoriesAsync(userId);
        Categories.Clear();
        foreach (var cat in cats)
            Categories.Add(cat);

        var txs = await dataService.GetTransactionsAsync(userId, now.Year, now.Month);
        Transactions.Clear();
        foreach (var tx in txs)
            Transactions.Add(tx);
    }

    private async Task AddTransactionAsync()
    {
        if (SelectedCategory == null || Amount == 0)
            return;

        var userId = Preferences.Get("userId", 0);

        var tx = new Transaction
        {
            UserId = userId,
            CategoryId = SelectedCategory.Id,
            Amount = Amount,
            Date = DateTime.Now
        };

        await dataService.AddTransactionAsync(tx);
        Amount = 0;
        await LoadAsync();
    }

    private async Task DeleteTransactionAsync()
    {
        if (SelectedTransaction == null)
            return;

        await dataService.DeleteTransactionAsync(SelectedTransaction.Id);
        await LoadAsync();
    }
}