using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GCAT.Models;

namespace GCAT.ViewModels;

// Drives the "Set budget" popup. Pre-fills one row per existing budget category
// (same categories shown in the "all budgets" list) with their current limit.
public class SetBudgetViewModel : ViewModelBase
{
    private static readonly string[] Months =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    private string? _selectedMonth;
    public string? SelectedMonth
    {
        get => _selectedMonth;
        private set
        {
            if (SetField(ref _selectedMonth, value))
                OnPropertyChanged(nameof(SelectedMonthYearLabel));
        }
    }

    private int? _selectedYear;
    public int? SelectedYear
    {
        get => _selectedYear;
        private set
        {
            if (SetField(ref _selectedYear, value))
                OnPropertyChanged(nameof(SelectedMonthYearLabel));
        }
    }

    // Shown on the "DATE" select button, e.g. "August 2026" (or "select" before anything is chosen).
    public string SelectedMonthYearLabel =>
        SelectedMonth != null && SelectedYear != null ? $"{SelectedMonth} {SelectedYear}" : "select";

    public ObservableCollection<BudgetInputRow> CategoryInputs { get; }

    public RelayCommand SubmitCommand { get; }
    public RelayCommand CancelCommand { get; }

    // Fires with { categoryKey -> new limit } for every row on successful submit.
    public event Action<Dictionary<string, decimal>>? Submitted;
    public event Action? CloseRequested;

    // ----- Nested month picker popup (swaps in for the form content while open) -----
    private readonly MonthPickerViewModel _monthPicker;

    private MonthPickerViewModel? _activeMonthPicker;
    public MonthPickerViewModel? ActiveMonthPicker
    {
        get => _activeMonthPicker;
        private set
        {
            if (SetField(ref _activeMonthPicker, value))
                OnPropertyChanged(nameof(IsMonthPickerOpen));
        }
    }

    public bool IsMonthPickerOpen => ActiveMonthPicker != null;

    public RelayCommand OpenMonthPickerCommand { get; }
    public RelayCommand CloseMonthPickerCommand { get; }

    public SetBudgetViewModel(IEnumerable<BudgetCategory> existingBudgets)
    {
        var today = DateTime.Today;
        SelectedMonth = Months[today.Month - 1];
        SelectedYear = today.Year;

        CategoryInputs = new ObservableCollection<BudgetInputRow>(
            existingBudgets.Select(b => new BudgetInputRow
            {
                CategoryKey = b.Name,
                CategoryLabel = b.Name.ToUpperInvariant(),
                AmountText = b.Limit.ToString("0.##")
            }));

        _monthPicker = new MonthPickerViewModel(today.Year, today.Month);
        _monthPicker.MonthChosen += (year, month) =>
        {
            SelectedMonth = Months[month - 1];
            SelectedYear = year;
            ActiveMonthPicker = null;
        };

        OpenMonthPickerCommand = new RelayCommand(_ => ActiveMonthPicker = _monthPicker);
        CloseMonthPickerCommand = new RelayCommand(_ => ActiveMonthPicker = null);

        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        SubmitCommand = new RelayCommand(_ => Submit());
    }

    private void Submit()
    {
        var result = new Dictionary<string, decimal>();

        foreach (var row in CategoryInputs)
        {
            if (!decimal.TryParse(row.AmountText, out var amount) || amount < 0) return;
            result[row.CategoryKey] = amount;
        }

        Submitted?.Invoke(result);
        CloseRequested?.Invoke();
    }
}

