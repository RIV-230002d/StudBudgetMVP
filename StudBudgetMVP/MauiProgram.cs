using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StudBudgetMVP.Services;
using StudBudgetMVP.ViewModels;

namespace StudBudgetMVP;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // DI
        builder.Services.AddSingleton<IDataService, SqliteDataService>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<TransactionsViewModel>();
        builder.Services.AddTransient<BudgetViewModel>();
        builder.Services.AddTransient<HomeViewModel>();      // новый
        builder.Services.AddTransient<ProfileViewModel>();   // новый

        return builder.Build();
    }
}
