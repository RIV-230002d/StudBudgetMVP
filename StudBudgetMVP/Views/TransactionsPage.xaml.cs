using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;

namespace StudBudgetMVP.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage()
    {
        InitializeComponent();
        var vm = new TransactionsViewModel(new SqliteDataService());
        BindingContext = vm;
        vm.LoadCommand.Execute(null);
    }
}