using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GCAT.Models;

namespace GCAT.ViewModels;

public enum TransactionFormMode
{
    Add,
    Edit
}

// Drives the "Import transaction" popup (image 2). Used both for the header "import"
// button (Add mode, blank form) and for the row-level "Edit" action (Edit mode, pre-filled
// from the existing Transaction) - the same view is reused either way.
public class ImportTransactionViewModel : ViewModelBase
{
    private static readonly string[] Months =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    public TransactionFormMode Mode { get; private set; }
    public string SubmitLabel => Mode == TransactionFormMode.Add ? "Add" : "Save";

    public ObservableCollection<string> MonthOptions { get; } = new(Months);
    public ObservableCollection<int> DayOptions { get; } = new(Enumerable.Range(1, 31));
    public ObservableCollection<int> YearOptions { get; } = new(Enumerable.Range(DateTime.Now.Year - 3, 6));

    public ObservableCollection<string> CategoryOptions { get; } =
        new(CategoryPalette.ExpenseCategories.Select(Capitalize));

    private string? _selectedMonth;
    public string? SelectedMonth
    {
        get => _selectedMonth;
        set => SetField(ref _selectedMonth, value);
    }

    private int? _selectedDay;
    public int? SelectedDay
    {
        get => _selectedDay;
        set => SetField(ref _selectedDay, value);
    }

    private int? _selectedYear;
    public int? SelectedYear
    {
        get => _selectedYear;
        set => SetField(ref _selectedYear, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    private string? _selectedCategory;
    public string? SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    private bool _isIncome;
    public bool IsIncome
    {
        get => _isIncome;
        set
        {
            if (SetField(ref _isIncome, value))
            {
                OnPropertyChanged(nameof(IsExpense));
                if (value) SelectedCategory = null; // category is implied ("income") once Type = Income
            }
        }
    }

    public bool IsExpense => !IsIncome;

    private string _amountText = string.Empty;
    public string AmountText
    {
        get => _amountText;
        set => SetField(ref _amountText, value);
    }

    public RelayCommand SelectIncomeCommand { get; }
    public RelayCommand SelectExpenseCommand { get; }
    public RelayCommand SubmitCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event Action<Transaction>? Submitted;
    public event Action? CloseRequested;

    private readonly Transaction? _editingTransaction;

    // Add mode: blank form.
    public ImportTransactionViewModel()
    {
        Mode = TransactionFormMode.Add;

        var today = DateTime.Today;
        SelectedMonth = Months[today.Month - 1];
        SelectedDay = today.Day;
        SelectedYear = today.Year;
        IsIncome = false;

        SelectIncomeCommand = new RelayCommand(_ => IsIncome = true);
        SelectExpenseCommand = new RelayCommand(_ => IsIncome = false);
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        SubmitCommand = new RelayCommand(_ => Submit());
    }

    // Edit mode: pre-fills every field from an existing transaction, and mutates it in place on submit.
    public ImportTransactionViewModel(Transaction existing) : this()
    {
        Mode = TransactionFormMode.Edit;
        _editingTransaction = existing;

        SelectedMonth = Months[existing.Date.Month - 1];
        SelectedDay = existing.Date.Day;
        SelectedYear = existing.Date.Year;
        Description = existing.Description;
        IsIncome = existing.IsIncome;
        SelectedCategory = existing.IsIncome ? null : Capitalize(existing.Category);
        AmountText = existing.Amount.ToString("0.##");
    }

    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(Description)) return;
        if (SelectedMonth == null || SelectedDay == null || SelectedYear == null) return;
        if (!decimal.TryParse(AmountText, out var amount) || amount <= 0) return;

        var month = Array.IndexOf(Months, SelectedMonth) + 1;
        var day = Math.Min(SelectedDay.Value, DateTime.DaysInMonth(SelectedYear.Value, month));
        var date = new DateTime(SelectedYear.Value, month, day);
        var category = IsIncome ? "income" : (SelectedCategory ?? "food").ToLowerInvariant();

        var transaction = _editingTransaction ?? new Transaction();
        transaction.Date = date;
        transaction.Description = Description;
        transaction.Category = category;
        transaction.IsIncome = IsIncome;
        transaction.Amount = amount;
        transaction.CategoryBrush = CategoryPalette.BrushFor(category);

        Submitted?.Invoke(transaction);
        CloseRequested?.Invoke();
    }

    private static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
