using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GCAT.Models;

namespace GCAT.ViewModels;

public class BudgetViewModel : ViewModelBase
{
    private static readonly string[] MonthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    // ----- Header -----
    private string _headerMonthLabel = "August 2026";
    public string HeaderMonthLabel
    {
        get => _headerMonthLabel;
        set => SetField(ref _headerMonthLabel, value);
    }

    // ----- Top stat cards -----
    public ObservableCollection<BudgetSummaryCard> SummaryCards { get; }

    // ----- "all budgets" list -----
    public ObservableCollection<BudgetCategory> Budgets { get; }

    // ----- "alerts" list -----
    public ObservableCollection<BudgetAlert> Alerts { get; }

    // ----- Popup host - same pattern as TransactionsViewModel -----
    private object? _activePopup;
    public object? ActivePopup
    {
        get => _activePopup;
        set
        {
            if (SetField(ref _activePopup, value))
                OnPropertyChanged(nameof(IsPopupOpen));
        }
    }

    public bool IsPopupOpen => ActivePopup != null;

    public RelayCommand OpenHeaderMonthPickerCommand { get; }
    public RelayCommand OpenProfileCommand { get; }
    public RelayCommand SetBudgetCommand { get; }
    public RelayCommand ClosePopupCommand { get; }

    // Reuses the exact same month picker control/view model as Dashboard and Transactions.
    private readonly MonthPickerViewModel _monthPicker;

    public BudgetViewModel()
    {
        SummaryCards = new ObservableCollection<BudgetSummaryCard>
        {
            new() { Title = "total budget", DisplayAmount = "\u20B132,000.00", Subtitle = "across 4 categories" },
            new() { Title = "total spent", DisplayAmount = "\u20B126,944.00", Subtitle = "84.2% of budget used" },
            new() { Title = "remaining", DisplayAmount = "\u20B15,056.00", Subtitle = "on track, 9 days left" },
        };

        Budgets = new ObservableCollection<BudgetCategory>
        {
            new() { Name = "food", Spent = 7777, Limit = 8000 },
            new() { Name = "entertainment", Spent = 8367, Limit = 8000 },
            new() { Name = "transportation", Spent = 5600, Limit = 8000 },
            new() { Name = "utilities", Spent = 5200, Limit = 8000 },
        };

        Alerts = new ObservableCollection<BudgetAlert>
        {
            new() { Label = "food nearly maxed", BadgeText = "97%", Severity = AlertSeverity.Warning },
            new() { Label = "entertainment over limit", BadgeText = "+\u20B1367", Severity = AlertSeverity.OverLimit },
            new() { Label = "transportation sufficient", BadgeText = "healthy", Severity = AlertSeverity.Healthy },
            new() { Label = "utilities on space", BadgeText = "healthy", Severity = AlertSeverity.Healthy },
        };

        _monthPicker = new MonthPickerViewModel(2026, 8);
        _monthPicker.MonthChosen += (year, month) =>
        {
            HeaderMonthLabel = $"{MonthNames[month - 1]} {year}";
            ActivePopup = null;
        };

        OpenHeaderMonthPickerCommand = new RelayCommand(_ => ActivePopup = _monthPicker);

        // TODO: hook up whatever the "meow" button should actually do (profile menu, settings, etc.)
        OpenProfileCommand = new RelayCommand(_ => { });

        SetBudgetCommand = new RelayCommand(_ => OpenSetBudget());

        ClosePopupCommand = new RelayCommand(_ => ActivePopup = null);
    }

    private void OpenSetBudget()
    {
        var form = new SetBudgetViewModel(Budgets);
        form.Submitted += amounts =>
        {
            foreach (var (key, amount) in amounts)
            {
                var category = Budgets.FirstOrDefault(b => b.Name == key);
                if (category != null) category.Limit = amount;
            }
            ActivePopup = null;
        };
        form.CloseRequested += () => ActivePopup = null;
        ActivePopup = form;
    }
}
