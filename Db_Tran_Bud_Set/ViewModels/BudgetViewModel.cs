using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GCAT.Models;
using CoreModels = ExpenseTracker.Core.Models;
using ExpenseTracker.Core.Interfaces;
using System.Collections.Generic;

namespace GCAT.ViewModels;

public class BudgetViewModel : ViewModelBase
{
    private static readonly string[] MonthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    private readonly ICategoryRepository _categoryRepository;
    private readonly IBudgetService _budgetService;

    private int _currentMonth = DateTime.Today.Month;
    private int _currentYear = DateTime.Today.Year;

    private string _headerMonthLabel = $"{MonthNames[DateTime.Today.Month - 1]} {DateTime.Today.Year}";
    public string HeaderMonthLabel
    {
        get => _headerMonthLabel;
        set => SetField(ref _headerMonthLabel, value);
    }

    public ObservableCollection<BudgetSummaryCard> SummaryCards { get; } = new();
    public ObservableCollection<BudgetCategory> Budgets { get; } = new();
    public ObservableCollection<BudgetAlert> Alerts { get; } = new();

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

    // Fires whenever a budget is changed, so Dashboard/Transactions can react if needed
    public event Action? DataChanged;

    private readonly MonthPickerViewModel _monthPicker;

    public BudgetViewModel(ICategoryRepository categoryRepository, IBudgetService budgetService)
    {
        _categoryRepository = categoryRepository;
        _budgetService = budgetService;

        _monthPicker = new MonthPickerViewModel(_currentYear, _currentMonth);
        _monthPicker.MonthChosen += async (year, month) =>
        {
            HeaderMonthLabel = $"{MonthNames[month - 1]} {year}";
            ActivePopup = null;
            _currentMonth = month;
            _currentYear = year;
            await LoadBudgetDataAsync();
        };

        OpenHeaderMonthPickerCommand = new RelayCommand(_ => ActivePopup = _monthPicker);
        OpenProfileCommand = new RelayCommand(_ => { });
        SetBudgetCommand = new RelayCommand(_ => OpenSetBudget());
        ClosePopupCommand = new RelayCommand(_ => ActivePopup = null);
    }

    // Call this after construction (constructors can't be async)
    public async Task LoadBudgetDataAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var budgetedCategories = categories.Where(c => c.MonthlyBudget.HasValue).ToList();

        Budgets.Clear();
        Alerts.Clear();

        decimal totalBudget = 0;
        decimal totalSpent = 0;

        foreach (var category in budgetedCategories)
        {
            var spent = await _budgetService.GetTotalSpentAsync(category.Id, _currentMonth, _currentYear);
            var limit = category.MonthlyBudget!.Value;

            totalBudget += limit;
            totalSpent += spent;

            Budgets.Add(new BudgetCategory
            {
                Name = category.Name,
                Spent = spent,
                Limit = limit
            });

            var ratio = limit == 0 ? 0 : spent / limit;
            if (ratio > 1m)
            {
                Alerts.Add(new BudgetAlert
                {
                    Label = $"{category.Name} over limit",
                    BadgeText = $"+{CurrencySettings.Symbol}{(spent - limit):N0}",
                    Severity = AlertSeverity.OverLimit
                });
            }
            else if (ratio >= 0.9m)
            {
                Alerts.Add(new BudgetAlert
                {
                    Label = $"{category.Name} nearly maxed",
                    BadgeText = $"{ratio:P0}",
                    Severity = AlertSeverity.Warning
                });
            }
            else
            {
                Alerts.Add(new BudgetAlert
                {
                    Label = $"{category.Name} sufficient",
                    BadgeText = "healthy",
                    Severity = AlertSeverity.Healthy
                });
            }
        }

        var remaining = totalBudget - totalSpent;
        var percentUsed = totalBudget == 0 ? 0 : totalSpent / totalBudget;

        SummaryCards.Clear();
        SummaryCards.Add(new BudgetSummaryCard
        {
            Title = "total budget",
            DisplayAmount = $"{CurrencySettings.Symbol}{totalBudget:N2}",
            Subtitle = $"across {budgetedCategories.Count} categories"
        });
        SummaryCards.Add(new BudgetSummaryCard
        {
            Title = "total spent",
            DisplayAmount = $"{CurrencySettings.Symbol}{totalSpent:N2}",
            Subtitle = $"{percentUsed:P1} of budget used"
        });
        SummaryCards.Add(new BudgetSummaryCard
        {
            Title = "remaining",
            DisplayAmount = $"{CurrencySettings.Symbol}{remaining:N2}",
            Subtitle = remaining >= 0 ? "on track" : "over budget"
        });
    }

    private void OpenSetBudget()
    {
        var form = new SetBudgetViewModel(Budgets);
        form.Submitted += async amounts => await OnBudgetsSubmitted(amounts);
        form.CloseRequested += () => ActivePopup = null;
        ActivePopup = form;
    }

    private async Task OnBudgetsSubmitted(Dictionary<string, decimal> amounts)
    {
        var categories = await _categoryRepository.GetAllAsync();

        foreach (var (key, amount) in amounts)
        {
            var category = categories.FirstOrDefault(c =>
                c.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (category != null)
            {
                category.MonthlyBudget = amount;
                await _categoryRepository.UpdateAsync(category);
            }
        }

        ActivePopup = null;
        await LoadBudgetDataAsync();
        DataChanged?.Invoke();
    }
}