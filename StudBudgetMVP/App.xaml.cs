using Microsoft.Maui.Controls;

namespace StudBudgetMVP;
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
    System.Diagnostics.Debug.WriteLine($"[Unhandled] {e.ExceptionObject}");
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[UnobservedTask] {e.Exception}");
            e.SetObserved();
        };
        MainPage = new AppShell();
    }
}