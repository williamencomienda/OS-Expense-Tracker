using System.Collections.ObjectModel;
using GCAT.Models;

namespace GCAT.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string AppIconPath => "Assets/Icons/app-icon.png";

    public ObservableCollection<NavItem> NavItems { get; } = new()
    {
        new NavItem { Key = "dashboard", Label = "dashboard", IconPath = "Assets/Icons/nav-dashboard.png" },
        new NavItem { Key = "transactions", Label = "transactions", IconPath = "Assets/Icons/nav-transactions.png" },
        new NavItem { Key = "budget", Label = "budget", IconPath = "Assets/Icons/nav-budget.png" },
        new NavItem { Key = "settings", Label = "settings", IconPath = "Assets/Icons/nav-settings.png" },
    };

    private NavItem? _selectedNavItem;
    public NavItem? SelectedNavItem
    {
        get => _selectedNavItem;
        set
        {
            if (SetField(ref _selectedNavItem, value))
                UpdateCurrentPage();
        }
    }

    // "dashboard" and "transactions" have real pages. "budget"/"settings" aren't built yet -
    // they fall back to showing the dashboard for now.
    public DashboardViewModel Dashboard { get; }
    public TransactionsViewModel Transactions { get; } = new();
    public BudgetViewModel Budget { get; } = new();
    public SettingsViewModel Settings { get; } = new();

    private object _currentPage;
    public object CurrentPage
    {
        get => _currentPage;
        private set => SetField(ref _currentPage, value);
    }

    // Dashboard is now injected via DI instead of being created with "new"
    public MainWindowViewModel(DashboardViewModel dashboardViewModel)
    {
        Dashboard = dashboardViewModel;

        _currentPage = Dashboard;
        SelectedNavItem = NavItems[0];
    }

    private void UpdateCurrentPage()
    {
        CurrentPage = SelectedNavItem?.Key switch
        {
            "transactions" => Transactions,
            "budget" => Budget,
            "settings" => Settings,
            _ => Dashboard,
        };
    }
}