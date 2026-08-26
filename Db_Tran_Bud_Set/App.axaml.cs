using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GCAT.Views;

namespace GCAT;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // App boots into the splash screen first; splash hands off to MainWindow.
            desktop.MainWindow = new SplashWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
