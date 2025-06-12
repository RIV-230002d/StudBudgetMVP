using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using StudBudgetMVP.Services;
using System.Threading.Tasks;

namespace StudBudgetMVP.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IDataService data;
        private readonly INavigation navigation;

        public RegisterViewModel(IDataService ds, INavigation nav)
        {
            data = ds;
            navigation = nav;

            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
        }

        [ObservableProperty] private string username;
        [ObservableProperty] private string password;

        public IAsyncRelayCommand RegisterCommand { get; }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Enter user & pass", "OK");
                return;
            }

            var ok = await data.RegisterAsync(Username.Trim(), Password);
            if (!ok)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "User exists", "OK");
                return;
            }

            await Application.Current.MainPage.DisplayAlert("Success", "Account created", "OK");
            await navigation.PopAsync();               // возвращаемся к LoginPage
        }
    }
}