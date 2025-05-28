using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;

namespace StudBudgetMVP.Views;
public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();

        var dataService = new SqliteDataService();
        var vm = new RegisterViewModel(dataService);

        BindingContext = vm;
    }
}