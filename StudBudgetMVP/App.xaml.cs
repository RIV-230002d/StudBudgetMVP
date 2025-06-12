using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Views;
using System.Globalization;

namespace StudBudgetMVP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var ru = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = ru;
            CultureInfo.DefaultThreadCurrentUICulture = ru;

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                System.Diagnostics.Debug.WriteLine($"[Unhandled] {e.ExceptionObject}");
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[UnobservedTask] {e.Exception}");
                e.SetObserved();
            };

            // Если userId сохранён ― показываем основную оболочку,
            // иначе стартуем со страницы логина
            var userId = Preferences.Get("userId", 0);
            MainPage = userId > 0
                ? new AppShell()
                : new NavigationPage(new LoginPage());
        }
    }
}