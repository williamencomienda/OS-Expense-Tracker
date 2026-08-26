using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using GCAT.ViewModels;

namespace GCAT.Views;

public partial class SplashWindow : Window
{
    public string IconPath => "Assets/Icons/app-icon.png";

    public SplashWindow()
    {
        InitializeComponent();
        DataContext = this;
        Opened += async (_, _) => await RunSplashSequenceAsync();
    }

    private async Task RunSplashSequenceAsync()
    {
        await Task.Delay(100);
        RootPanel.Opacity = 1;

        await Task.Delay(1600);

        RootPanel.Opacity = 0;
        await Task.Delay(700);

        // Resolve the MainWindowViewModel (and its DashboardViewModel dependency) via DI
        var mainWindowViewModel = App.Services.GetRequiredService<MainWindowViewModel>();

        // Load real data into the dashboard before showing it
        await mainWindowViewModel.Dashboard.LoadDashboardDataAsync();

        var main = new MainWindow
        {
            DataContext = mainWindowViewModel
        };
        main.Show();
        Close();
    }
}