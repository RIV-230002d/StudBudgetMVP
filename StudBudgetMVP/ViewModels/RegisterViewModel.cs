using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudBudgetMVP.Services;
using Microsoft.Maui.Controls;

namespace StudBudgetMVP.ViewModels;
public partial class RegisterViewModel : ObservableObject
{
    [ObservableProperty] string username;
    [ObservableProperty] string password;
    private readonly IDataService ds;

    public RegisterViewModel(IDataService dataService)
    {
        ds = dataService;
        RegisterCommand = new AsyncRelayCommand(RegisterAsync);
    }

    public IAsyncRelayCommand RegisterCommand { get; }

    async Task RegisterAsync()
    {
        var ok = await ds.RegisterAsync(Username, Password);
        var msg = ok ? "Регистрация прошла" : "Пользователь существует";
        await Application.Current.MainPage.DisplayAlert("СтудБюджет", msg, "OK");
    }
}