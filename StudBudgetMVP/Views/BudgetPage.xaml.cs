using Microsoft.Maui.Controls;
using StudBudgetMVP.ViewModels;
using StudBudgetMVP.Services;
using System.IO;

namespace StudBudgetMVP.Views;

public partial class BudgetPage : ContentPage
{
    private readonly BudgetViewModel vm;

    public BudgetPage()
    {
        InitializeComponent();
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
        vm = new BudgetViewModel(new SqliteDataService(dbPath));
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.LoadCategoriesAsync();
    }
}