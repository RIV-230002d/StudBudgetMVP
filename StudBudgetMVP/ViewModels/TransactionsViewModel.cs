using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudBudgetMVP.Services;
using StudBudgetMVP.Models;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace StudBudgetMVP.ViewModels;
public partial class TransactionsViewModel : ObservableObject
{
    [ObservableProperty] public int userId;
    [ObservableProperty] public string category;
    [ObservableProperty] public decimal amount;
    public ObservableCollection<Transaction> Transactions { get; } = new();

    public TransactionsViewModel(IDataService ds)
    {
        Service = ds;
        UserId = Preferences.Get("userId", 0);
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    public IDataService Service { get; }
    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    async Task LoadAsync()
    {
        var list = await Service.GetTransactionsAsync(UserId);
        Transactions.Clear();
        foreach (var tx in list) Transactions.Add(tx);
    }

    async Task AddAsync()
    {
        var tx = new Transaction { UserId = UserId, Category = Category, Amount = Amount, Date = DateTime.Now };
        await Service.AddTransactionAsync(tx);
        await LoadAsync();
    }

    async Task DeleteAsync()
    {
        if (Selected != null)
        {
            await Service.DeleteTransactionAsync(Selected.Id);
            await LoadAsync();
        }
    }

    [ObservableProperty] Transaction selected;
}