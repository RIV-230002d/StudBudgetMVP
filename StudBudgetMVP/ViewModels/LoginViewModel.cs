using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Services;
using System.Threading.Tasks;

namespace StudBudgetMVP.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IDataService data;

        public LoginViewModel(IDataService ds, INavigation nav)
        {
            data = ds;
            navigation = nav;

            LoginCommand = new AsyncRelayCommand(LoginAsync);
            GoRegisterCommand = new AsyncRelayCommand(OpenRegisterAsync);
        }

        [ObservableProperty] private string username;
        [ObservableProperty] private string password;

        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand GoRegisterCommand { get; }

        // ————— private —————
        private readonly INavigation navigation;

        private async Task LoginAsync()
        {
            var user = await data.LoginAsync(Username?.Trim(), Password);
            if (user is null)
            {
                await Application.Current.MainPage.DisplayAlert("Login failed", "Wrong credentials", "OK");
                return;
            }

            Preferences.Set("userId", user.Id);
            Application.Current.MainPage = new AppShell();       // теперь внутри Shell
        }

        private Task OpenRegisterAsync()
            => navigation.PushAsync(new Views.RegisterPage());   // навигация через NavigationPage
    }
}