using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudBudgetMVP.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace StudBudgetMVP.ViewModels;
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] string username;
    [ObservableProperty] string password;
    private readonly IDataService ds;

    public LoginViewModel(IDataService dataService)
    {
        ds = dataService;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        NavigateRegisterCommand = new RelayCommand(async () =>
            await Shell.Current.GoToAsync(nameof(Views.RegisterPage)));
    }

    public IAsyncRelayCommand LoginCommand { get; }
    public IRelayCommand NavigateRegisterCommand { get; }

    async Task LoginAsync()
    {
        var user = await ds.LoginAsync(Username, Password);
        if (user != null)
            await Shell.Current.GoToAsync($"//TransactionsPage?userId={user.Id}");
        else
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Неверные данные", "OK");
        Preferences.Set("userId", user.Id);
        await Shell.Current.GoToAsync("//TransactionsPage");
    }
}