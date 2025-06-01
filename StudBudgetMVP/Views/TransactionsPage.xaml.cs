using Microsoft.Maui.Controls;
using StudBudgetMVP.ViewModels;
using StudBudgetMVP.Services;
using System.IO;

namespace StudBudgetMVP.Views;

public partial class TransactionsPage : ContentPage
{
    private readonly TransactionsViewModel vm;

    public TransactionsPage()
    {
        InitializeComponent();
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
        vm = new TransactionsViewModel(new SqliteDataService(dbPath));
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.LoadAsync();
    }
}