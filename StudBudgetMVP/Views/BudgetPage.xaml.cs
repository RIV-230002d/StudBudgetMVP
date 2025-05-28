using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;

namespace StudBudgetMVP.Views;

public partial class BudgetPage : ContentPage
{
    public BudgetPage()
    {
        InitializeComponent();
        var vm = new BudgetViewModel(new SqliteDataService());
        BindingContext = vm;
    }
}