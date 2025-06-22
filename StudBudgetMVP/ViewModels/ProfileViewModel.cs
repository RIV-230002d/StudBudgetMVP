using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudBudgetMVP.Services;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using System.IO;

namespace StudBudgetMVP.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty]
        private string username = "Имя пользователя";

        public IRelayCommand OpenSettingsCommand { get; }

        private readonly IDataService dataService;

        public ProfileViewModel()
            : this(new SqliteDataService(Path.Combine(FileSystem.AppDataDirectory, "app.db"))) { }

        public ProfileViewModel(IDataService dataService)
        {
            this.dataService = dataService;
            OpenSettingsCommand = new RelayCommand(() => { });

            _ = LoadUserAsync();
        }

        private async Task LoadUserAsync()
        {
            var userId = Preferences.Get("userId", 0);
            if (userId > 0)
            {
                var user = await dataService.GetUserByIdAsync(userId);
                if (user != null)
                    Username = user.Username;
            }
        }
    }
}
