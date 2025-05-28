using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;

namespace StudBudgetMVP.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        var dataService = new SqliteDataService();
        var vm = new LoginViewModel(dataService);

        BindingContext = vm;
    }
}