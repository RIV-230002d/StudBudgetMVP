using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;
using System.IO;
using Microsoft.Maui.Storage;

namespace StudBudgetMVP.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
        var vm = new LoginViewModel(new SqliteDataService(dbPath));
        BindingContext = vm;
    }
}