using System.Windows;
using KhelrunAI.Services;

namespace KhelrunAI;

public partial class App : Application
{
    public static AppSettings Settings { get; } = AppSettings.Load();

    public App()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            MessageBox.Show(e.Exception.Message, "KHELRUN AI error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        ThemeService.Apply(Settings.Theme);
        base.OnStartup(e);
    }
}
