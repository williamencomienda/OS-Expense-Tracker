using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using GCAT.Models;
using CoreModels = ExpenseTracker.Core.Models;
using ExpenseTracker.Core.Interfaces;

namespace GCAT.ViewModels;

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

    // ----- Injected dependencies -----
    private readonly IExpenseRepository _expenseRepository;
    private readonly IIncomeRepository _incomeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBudgetService _budgetService;

    // ----- Header -----
    private string _headerMonthLabel = $"{MonthNames[DateTime.Today.Month - 1]} {DateTime.Today.Year}";
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

    // ----- Currently selected period -----
    private int _currentMonth = DateTime.Today.Month;
    private int _currentYear = DateTime.Today.Year;

    // ----- Data collections -----
    public ObservableCollection<SummaryCard> SummaryCards { get; } = new();
    public ObservableCollection<ExpenseSlice> ExpensesOverview { get; } = new();
    public ObservableCollection<MonthlyPoint> IncomeVsExpenses { get; } = new();
    public ObservableCollection<TransactionItem> RecentTransactions { get; } = new();
    public ObservableCollection<BudgetItem> BudgetOverview { get; } = new();

    // ----- Month picker popup -----
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

    public DashboardViewModel(
        IExpenseRepository expenseRepository,
        IIncomeRepository incomeRepository,
        ICategoryRepository categoryRepository,
        IBudgetService budgetService)
    {
        _expenseRepository = expenseRepository;
        _incomeRepository = incomeRepository;
        _categoryRepository = categoryRepository;
        _budgetService = budgetService;

        _monthPicker = new MonthPickerViewModel(_currentYear, _currentMonth);
        _monthPicker.MonthChosen += OnMonthChosen;

        OpenHeaderMonthPickerCommand = new RelayCommand(_ => OpenPicker(MonthPickerTarget.Header));
        OpenExpensesPeriodPickerCommand = new RelayCommand(_ => OpenPicker(MonthPickerTarget.ExpensesPeriod));
        ClosePickerCommand = new RelayCommand(_ => IsMonthPickerOpen = false);
    }

    // Call this right after creating the ViewModel (constructors can't be async)
    public async Task LoadDashboardDataAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] Working directory: {System.IO.Directory.GetCurrentDirectory()}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] Base directory: {AppContext.BaseDirectory}");

        var allExpenses = await _expenseRepository.GetAllAsync();
        var allIncome = await _incomeRepository.GetAllAsync();
        var allCategories = await _categoryRepository.GetAllAsync();

        System.Diagnostics.Debug.WriteLine($"[DEBUG] Loaded {allExpenses.Count} expenses, {allIncome.Count} income, {allCategories.Count} categories");
        foreach (var c in allCategories)
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Category: {c.Name}, Budget: {c.MonthlyBudget}");

        var monthExpenses = allExpenses
            .Where(e => e.Date.Month == _currentMonth && e.Date.Year == _currentYear)
            .ToList();

        var monthIncome = allIncome
            .Where(i => i.Date.Month == _currentMonth && i.Date.Year == _currentYear)
            .ToList();

        PopulateSummaryCards(monthExpenses, monthIncome, allExpenses, allIncome);
        PopulateExpensesOverview(monthExpenses);
        await PopulateIncomeVsExpensesAsync(allExpenses, allIncome);
        PopulateRecentTransactions(monthExpenses, monthIncome);
        await PopulateBudgetOverviewAsync(allCategories);
    }

    // ----- Conversion / mapping logic -----

    private void PopulateSummaryCards(
        System.Collections.Generic.List<CoreModels.Expense> monthExpenses,
        System.Collections.Generic.List<CoreModels.Income> monthIncome,
        System.Collections.Generic.List<CoreModels.Expense> allExpenses,
        System.Collections.Generic.List<CoreModels.Income> allIncome)
    {
        var totalIncome = monthIncome.Sum(i => i.Amount);
        var totalExpenses = monthExpenses.Sum(e => e.Amount);
        var balance = allIncome.Sum(i => i.Amount) - allExpenses.Sum(e => e.Amount);

        SummaryCards.Clear();
        SummaryCards.Add(new SummaryCard { Title = "total balance", Amount = balance, ChangePercent = 0, IsPositiveChange = true });
        SummaryCards.Add(new SummaryCard { Title = "total income", Amount = totalIncome, ChangePercent = 0, IsPositiveChange = true });
        SummaryCards.Add(new SummaryCard { Title = "total expenses", Amount = totalExpenses, ChangePercent = 0, IsPositiveChange = false });
    }

    private void PopulateExpensesOverview(System.Collections.Generic.List<CoreModels.Expense> monthExpenses)
    {
        ExpensesOverview.Clear();

        var grouped = monthExpenses
            .GroupBy(e => e.Category?.Name ?? "Uncategorized")
            .Select(g => new ExpenseSlice
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount),
                Color = CategoryPalette.BrushFor(g.Key)
            });

        foreach (var slice in grouped)
            ExpensesOverview.Add(slice);

        RecalculateSlicePercents();
    }

    private async Task PopulateIncomeVsExpensesAsync(
        System.Collections.Generic.List<CoreModels.Expense> allExpenses,
        System.Collections.Generic.List<CoreModels.Income> allIncome)
    {
        IncomeVsExpenses.Clear();

        // Show the current month plus the 3 preceding months
        for (int i = 3; i >= 0; i--)
        {
            var targetDate = new DateTime(_currentYear, _currentMonth, 1).AddMonths(-i);

            var incomeTotal = allIncome
                .Where(x => x.Date.Month == targetDate.Month && x.Date.Year == targetDate.Year)
                .Sum(x => x.Amount);

            var expenseTotal = allExpenses
                .Where(x => x.Date.Month == targetDate.Month && x.Date.Year == targetDate.Year)
                .Sum(x => x.Amount);

            IncomeVsExpenses.Add(new MonthlyPoint
            {
                Month = MonthNames[targetDate.Month - 1],
                Income = incomeTotal,
                Expenses = expenseTotal
            });
        }

        RecalculateBarRatios();
        await Task.CompletedTask;
    }

    private void PopulateRecentTransactions(
        System.Collections.Generic.List<CoreModels.Expense> monthExpenses,
        System.Collections.Generic.List<CoreModels.Income> monthIncome)
    {
        RecentTransactions.Clear();

        var expenseItems = monthExpenses.Select(e => new TransactionItem
        {
            Category = e.Category?.Name ?? "Uncategorized",
            IconPath = "Assets/Icons/category-default.png", // placeholder — icons TODO
            Date = e.Date,
            Amount = -e.Amount // negative: money going out
        });

        var incomeItems = monthIncome.Select(i => new TransactionItem
        {
            Category = i.Source,
            IconPath = "Assets/Icons/category-income.png", // placeholder
            Date = i.Date,
            Amount = i.Amount // positive: money coming in
        });

        foreach (var t in expenseItems.Concat(incomeItems).OrderByDescending(t => t.Date))
            RecentTransactions.Add(t);
    }

    private async Task PopulateBudgetOverviewAsync(System.Collections.Generic.List<CoreModels.Category> allCategories)
    {
        BudgetOverview.Clear();

        foreach (var category in allCategories.Where(c => c.MonthlyBudget.HasValue))
        {
            var spent = await _budgetService.GetTotalSpentAsync(category.Id, _currentMonth, _currentYear);

            BudgetOverview.Add(new BudgetItem
            {
                Category = category.Name,
                Spent = spent,
                Limit = category.MonthlyBudget!.Value
            });
        }
    }

    // ----- Month picker handling -----

    private void OpenPicker(MonthPickerTarget target)
    {
        _activeTarget = target;
        IsMonthPickerOpen = true;
    }

    private async void OnMonthChosen(int year, int month)
    {
        var label = $"{MonthNames[month - 1]} {year}";

        if (_activeTarget == MonthPickerTarget.Header)
            HeaderMonthLabel = label;
        else if (_activeTarget == MonthPickerTarget.ExpensesPeriod)
            ExpensesPeriodLabel = label.ToLowerInvariant();

        IsMonthPickerOpen = false;

        _currentMonth = month;
        _currentYear = year;

        await LoadDashboardDataAsync();
    }

    // ----- Existing helper calculations (unchanged) -----

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