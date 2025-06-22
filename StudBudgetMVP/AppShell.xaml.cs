using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StudBudgetMVP.Views;
using System.IO;

namespace StudBudgetMVP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Регистрируем маршруты для Shell (на случай переходов)
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("home", typeof(HomePage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));
            // Остальные уже зарегистрированы через ContentTemplate
        }

        // обработчик пункта "Выход"
        private void OnLogoutClicked(object sender, System.EventArgs e)
        {
            Preferences.Remove("userId");
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
