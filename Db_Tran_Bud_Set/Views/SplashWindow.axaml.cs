using System.Threading.Tasks;
using Avalonia.Controls;

namespace GCAT.Views;

public partial class SplashWindow : Window
{
    // Bound by the Image in SplashWindow.axaml. Drop app-icon.png into Assets/Icons.
    public string IconPath => "Assets/Icons/app-icon.png";

    public SplashWindow()
    {
        InitializeComponent();
        DataContext = this;
        Opened += async (_, _) => await RunSplashSequenceAsync();
    }

    private async Task RunSplashSequenceAsync()
    {
        // Small delay so the transition (defined in XAML) actually animates rather
        // than snapping straight to Opacity=1 on first layout pass.
        await Task.Delay(100);
        RootPanel.Opacity = 1;

        await Task.Delay(1600); // hold on screen

        RootPanel.Opacity = 0; // fade out
        await Task.Delay(700);

        var main = new MainWindow();
        main.Show();
        Close();
    }
}
