using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;
using System.IO;
using Microsoft.Maui.Storage;

namespace StudBudgetMVP.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
        var vm = new RegisterViewModel(new SqliteDataService(dbPath));
        BindingContext = vm;
    }
}