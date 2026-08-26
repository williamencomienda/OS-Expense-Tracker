using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GCAT.Models;

namespace GCAT.ViewModels;

public class TransactionsViewModel : ViewModelBase
{
    private static readonly string[] MonthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    public static readonly string[] Accounts = { "Bank Savings", "GCash", "Credit Card", "Cash" };

    public ObservableCollection<Transaction> AllTransactions { get; } = new();
    public ObservableCollection<Transaction> FilteredTransactions { get; } = new();

    // ----- Header -----
    private string _headerMonthLabel = "August 2026";
    public string HeaderMonthLabel
    {
        get => _headerMonthLabel;
        set => SetField(ref _headerMonthLabel, value);
    }

    // ----- Summary cards (placeholders, matches image 1) -----
    public ObservableCollection<SummaryCard> SummaryCards { get; }

    // ----- Search + quick filters -----
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                RecomputeFilteredTransactions();
        }
    }

    private string _typeFilterLabel = "all types";
    public string TypeFilterLabel
    {
        get => _typeFilterLabel;
        set => SetField(ref _typeFilterLabel, value);
    }

    private string _accountFilterLabel = "all accounts";
    public string AccountFilterLabel
    {
        get => _accountFilterLabel;
        set => SetField(ref _accountFilterLabel, value);
    }

    // ----- Popup host - exactly one of these object instances is "active" at a time -----
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
    public RelayCommand OpenImportCommand { get; }
    public RelayCommand OpenTypeFilterCommand { get; }
    public RelayCommand OpenAccountFilterCommand { get; }
    public RelayCommand OpenFiltersCommand { get; }
    public RelayCommand ClosePopupCommand { get; }

    private readonly MonthPickerViewModel _monthPicker;
    private readonly FiltersViewModel _filters;

    public TransactionsViewModel()
    {
        SummaryCards = new ObservableCollection<SummaryCard>
        {
            new() { Title = "net savings", Amount = 77777.77m, ChangePercent = 0, IsPositiveChange = true },
            new() { Title = "total income", Amount = 77777.77m, ChangePercent = 0, IsPositiveChange = false },
            new() { Title = "total expenses", Amount = 77777.77m, ChangePercent = 0, IsPositiveChange = true },
        };

        SeedPlaceholderData();

        // The header month picker reuses the exact same control/view model as the Dashboard.
        _monthPicker = new MonthPickerViewModel(2026, 8);
        _monthPicker.MonthChosen += (year, month) =>
        {
            HeaderMonthLabel = $"{MonthNames[month - 1]} {year}";
            ActivePopup = null;
        };

        _filters = new FiltersViewModel();
        _filters.Applied += () =>
        {
            RecomputeFilteredTransactions();
            ActivePopup = null;
        };
        _filters.CloseRequested += () => ActivePopup = null;

        OpenHeaderMonthPickerCommand = new RelayCommand(_ => ActivePopup = _monthPicker);
        OpenImportCommand = new RelayCommand(_ => OpenImport(null));
        OpenTypeFilterCommand = new RelayCommand(_ => OpenTypeFilter());
        OpenAccountFilterCommand = new RelayCommand(_ => OpenAccountFilter());
        OpenFiltersCommand = new RelayCommand(_ => ActivePopup = _filters);
        ClosePopupCommand = new RelayCommand(_ => ActivePopup = null);

        RecomputeFilteredTransactions();
    }

    private void OpenTypeFilter()
    {
        var current = TypeFilterLabel switch { "income" => "Income", "expense" => "Expense", _ => "All types" };
        var picker = new ListPickerViewModel("Filter by type", new[] { "All types", "Income", "Expense" }, current);
        picker.OptionChosen += label =>
        {
            TypeFilterLabel = label.ToLowerInvariant();
            RecomputeFilteredTransactions();
            ActivePopup = null;
        };
        picker.CloseRequested += () => ActivePopup = null;
        ActivePopup = picker;
    }

    private void OpenAccountFilter()
    {
        var options = new List<string> { "All accounts" };
        options.AddRange(Accounts);
        var current = Accounts.FirstOrDefault(a => a.Equals(AccountFilterLabel, StringComparison.OrdinalIgnoreCase)) ?? "All accounts";

        var picker = new ListPickerViewModel("Filter by account", options, current);
        picker.OptionChosen += label =>
        {
            AccountFilterLabel = label.ToLowerInvariant();
            RecomputeFilteredTransactions();
            ActivePopup = null;
        };
        picker.CloseRequested += () => ActivePopup = null;
        ActivePopup = picker;
    }

    private void OpenImport(Transaction? editing)
    {
        var form = editing == null
            ? new ImportTransactionViewModel(Accounts)
            : new ImportTransactionViewModel(editing, Accounts);

        form.Submitted += OnTransactionSubmitted;
        form.CloseRequested += () => ActivePopup = null;
        ActivePopup = form;
    }

    private void OnTransactionSubmitted(Transaction transaction)
    {
        if (!AllTransactions.Contains(transaction))
        {
            WireRowCommands(transaction);
            AllTransactions.Insert(0, transaction);
        }

        RecomputeFilteredTransactions();
    }

    private void RemoveTransaction(Transaction transaction)
    {
        AllTransactions.Remove(transaction);
        RecomputeFilteredTransactions();
    }

    private void WireRowCommands(Transaction transaction)
    {
        transaction.EditCommand = new RelayCommand(_ => OpenImport(transaction));
        transaction.RemoveCommand = new RelayCommand(_ => RemoveTransaction(transaction));
    }

    private void RecomputeFilteredTransactions()
    {
        IEnumerable<Transaction> query = AllTransactions;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(t =>
                t.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                t.Category.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (TypeFilterLabel == "income") query = query.Where(t => t.IsIncome);
        else if (TypeFilterLabel == "expense") query = query.Where(t => !t.IsIncome);

        if (AccountFilterLabel != "all accounts")
            query = query.Where(t => t.Account.Equals(AccountFilterLabel, StringComparison.OrdinalIgnoreCase));

        if (_filters.SelectedCategory != "All")
            query = query.Where(t => t.Category.Equals(_filters.SelectedCategory, StringComparison.OrdinalIgnoreCase));

        if (DateTime.TryParse(_filters.DateFrom, out var from))
            query = query.Where(t => t.Date.Date >= from.Date);
        if (DateTime.TryParse(_filters.DateTo, out var to))
            query = query.Where(t => t.Date.Date <= to.Date);

        if (decimal.TryParse(_filters.AmountMin, out var min))
            query = query.Where(t => t.Amount >= min);
        if (decimal.TryParse(_filters.AmountMax, out var max))
            query = query.Where(t => t.Amount <= max);

        query = _filters.SelectedSort switch
        {
            "Oldest" => query.OrderBy(t => t.Date),
            "Highest amount" => query.OrderByDescending(t => t.Amount),
            "Lowest amount" => query.OrderBy(t => t.Amount),
            _ => query.OrderByDescending(t => t.Date),
        };

        FilteredTransactions.Clear();
        foreach (var t in query) FilteredTransactions.Add(t);
    }

    private void SeedPlaceholderData()
    {
        void Add(DateTime date, string description, string category, string account, bool isIncome, decimal amount)
        {
            var t = new Transaction
            {
                Date = date,
                Description = description,
                Category = category,
                Account = account,
                IsIncome = isIncome,
                Amount = amount,
                CategoryBrush = CategoryPalette.BrushFor(category),
            };
            WireRowCommands(t);
            AllTransactions.Add(t);
        }

        var d = new DateTime(2026, 8, 1);
        Add(d, "Salary", "income", "Bank Savings", true, 100000m);
        Add(d, "Salary", "income", "GCash", true, 100000m);
        Add(d, "Groceries", "food", "GCash", false, 5000m);
        Add(d, "Transportation", "transport", "GCash", false, 100m);
        Add(d, "Online Shopping", "shopping", "Credit Card", false, 15000m);
        Add(d.AddDays(-2), "Electric Bill", "utilities", "Bank Savings", false, 3200m);
        Add(d.AddDays(-3), "Movie Night", "entertainment", "Cash", false, 900m);
    }
}
