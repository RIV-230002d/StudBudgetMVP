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

            // регистрируем маршрут страницы регистрации
            Routing.RegisterRoute("register", typeof(RegisterPage));
        }

        // обработчик пункта "Выход"
        private void OnLogoutClicked(object sender, System.EventArgs e)
        {
            Preferences.Remove("userId");

            // возвращаем пользователя к странице входа
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}