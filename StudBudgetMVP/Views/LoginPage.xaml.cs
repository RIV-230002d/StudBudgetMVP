using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;
using System.IO;

namespace StudBudgetMVP.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
            var ds = new SqliteDataService(dbPath);

            BindingContext = new LoginViewModel(ds, Navigation);
        }
    }
}