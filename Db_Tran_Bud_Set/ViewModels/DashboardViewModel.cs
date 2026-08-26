using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using GCAT.Models;

namespace GCAT.ViewModels;

// Which button opened the month picker, so we know which label to update when a month is chosen.
public enum MonthPickerTarget
{
    None,
    Header,
    ExpensesPeriod
}

public class DashboardViewModel : ViewModelBase
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

    private string _expensesPeriodLabel = "this month";
    public string ExpensesPeriodLabel
    {
        get => _expensesPeriodLabel;
        set => SetField(ref _expensesPeriodLabel, value);
    }

    // ----- Placeholder data -----
    public ObservableCollection<SummaryCard> SummaryCards { get; }
    public ObservableCollection<ExpenseSlice> ExpensesOverview { get; }
    public ObservableCollection<MonthlyPoint> IncomeVsExpenses { get; }
    public ObservableCollection<TransactionItem> RecentTransactions { get; }
    public ObservableCollection<BudgetItem> BudgetOverview { get; }

    // ----- Month picker popup (image 3) -----
    private bool _isMonthPickerOpen;
    public bool IsMonthPickerOpen
    {
        get => _isMonthPickerOpen;
        set => SetField(ref _isMonthPickerOpen, value);
    }

    private MonthPickerViewModel _monthPicker;
    public MonthPickerViewModel MonthPicker
    {
        get => _monthPicker;
        set => SetField(ref _monthPicker, value);
    }

    private MonthPickerTarget _activeTarget = MonthPickerTarget.None;

    public RelayCommand OpenHeaderMonthPickerCommand { get; }
    public RelayCommand OpenExpensesPeriodPickerCommand { get; }
    public RelayCommand ClosePickerCommand { get; }

    public DashboardViewModel()
    {
        SummaryCards = new ObservableCollection<SummaryCard>
        {
            new() { Title = "total balance", Amount = 77777.77m, ChangePercent = 0, IsPositiveChange = true },
            new() { Title = "total income", Amount = 77777.77m, ChangePercent = 0, IsPositiveChange = false },
            new() { Title = "total expenses", Amount = 77777.77m, ChangePercent = 0, IsPositiveChange = true },
        };

        ExpensesOverview = new ObservableCollection<ExpenseSlice>
        {
            new() { Category = "Food", Amount = 10000m, Color = new SolidColorBrush(Color.Parse("#FFB0D6")) },
            new() { Category = "Food", Amount = 10000m, Color = new SolidColorBrush(Color.Parse("#F06FA6")) },
            new() { Category = "Food", Amount = 10000m, Color = new SolidColorBrush(Color.Parse("#C2185B")) },
            new() { Category = "Food", Amount = 10000m, Color = new SolidColorBrush(Color.Parse("#7A0E3D")) },
        };
        RecalculateSlicePercents();

        IncomeVsExpenses = new ObservableCollection<MonthlyPoint>
        {
            new() { Month = "May", Income = 20000, Expenses = 8000 },
            new() { Month = "June", Income = 22000, Expenses = 9000 },
            new() { Month = "July", Income = 68000, Expenses = 12000 },
            new() { Month = "August", Income = 70000, Expenses = 15000 },
        };
        RecalculateBarRatios();

        RecentTransactions = new ObservableCollection<TransactionItem>
        {
            new() { Category = "food", IconPath = "Assets/Icons/category-food.png", Date = new DateTime(2026, 8, 1), Amount = -10000m },
            new() { Category = "food", IconPath = "Assets/Icons/category-food.png", Date = new DateTime(2026, 8, 1), Amount = 10000m },
            new() { Category = "food", IconPath = "Assets/Icons/category-food.png", Date = new DateTime(2026, 8, 1), Amount = -10000m },
            new() { Category = "food", IconPath = "Assets/Icons/category-food.png", Date = new DateTime(2026, 8, 1), Amount = 10000m },
        };

        BudgetOverview = new ObservableCollection<BudgetItem>
        {
            new() { Category = "food", Spent = 7777, Limit = 8000 },
            new() { Category = "food", Spent = 7777, Limit = 8000 },
            new() { Category = "food", Spent = 7777, Limit = 8000 },
            new() { Category = "food", Spent = 7777, Limit = 8000 },
        };

        _monthPicker = new MonthPickerViewModel(2026, 8);
        _monthPicker.MonthChosen += OnMonthChosen;

        OpenHeaderMonthPickerCommand = new RelayCommand(_ => OpenPicker(MonthPickerTarget.Header));
        OpenExpensesPeriodPickerCommand = new RelayCommand(_ => OpenPicker(MonthPickerTarget.ExpensesPeriod));
        ClosePickerCommand = new RelayCommand(_ => IsMonthPickerOpen = false);
    }

    private void OpenPicker(MonthPickerTarget target)
    {
        _activeTarget = target;
        // Reset the picker to reflect whichever label is currently active for this target.
        IsMonthPickerOpen = true;
    }

    private void OnMonthChosen(int year, int month)
    {
        var label = $"{MonthNames[month - 1]} {year}";

        if (_activeTarget == MonthPickerTarget.Header)
            HeaderMonthLabel = label;
        else if (_activeTarget == MonthPickerTarget.ExpensesPeriod)
            ExpensesPeriodLabel = label.ToLowerInvariant();

        IsMonthPickerOpen = false;
    }

    private void RecalculateSlicePercents()
    {
        var total = ExpensesOverview.Sum(s => s.Amount);
        foreach (var s in ExpensesOverview)
            s.Percent = total == 0 ? 0 : (double)(s.Amount / total);
    }

    private void RecalculateBarRatios()
    {
        if (IncomeVsExpenses.Count == 0) return;

        var max = Math.Max(IncomeVsExpenses.Max(p => p.Income), IncomeVsExpenses.Max(p => p.Expenses));
        foreach (var p in IncomeVsExpenses)
        {
            p.IncomeRatio = max == 0 ? 0 : (double)(p.Income / max);
            p.ExpensesRatio = max == 0 ? 0 : (double)(p.Expenses / max);
        }
    }
}
