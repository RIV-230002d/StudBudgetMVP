using StudBudgetMVP.ViewModels;
using StudBudgetMVP.Services;
using Microsoft.Maui.Storage;
using System.IO;

namespace StudBudgetMVP.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel vm;

        public HomePage()
        {
            InitializeComponent();

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
            var dataService = new SqliteDataService(dbPath);
            vm = new HomeViewModel(dataService);

            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await vm.LoadAsync();
        }
    }
}
