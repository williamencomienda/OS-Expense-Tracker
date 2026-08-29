using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GCAT.Models;
using CoreModels = ExpenseTracker.Core.Models;
using ExpenseTracker.Core.Interfaces;

namespace GCAT.ViewModels;

public class TransactionsViewModel : ViewModelBase
{
    private static readonly string[] MonthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    public static readonly string[] Accounts = { "Bank Savings", "GCash", "Credit Card", "Cash" };

    private readonly IExpenseRepository _expenseRepository;
    private readonly IIncomeRepository _incomeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private List<CoreModels.Category> _categoriesCache = new();

    public ObservableCollection<Transaction> AllTransactions { get; } = new();
    public ObservableCollection<Transaction> FilteredTransactions { get; } = new();

    private string _headerMonthLabel = "August 2026";
    public string HeaderMonthLabel
    {
        get => _headerMonthLabel;
        set => SetField(ref _headerMonthLabel, value);
    }

    public ObservableCollection<SummaryCard> SummaryCards { get; } = new();

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
    public RelayCommand OpenFiltersCommand { get; }
    public RelayCommand ClosePopupCommand { get; }

    private readonly MonthPickerViewModel _monthPicker;
    private readonly FiltersViewModel _filters;

    public TransactionsViewModel(
        IExpenseRepository expenseRepository,
        IIncomeRepository incomeRepository,
        ICategoryRepository categoryRepository)
    {
        _expenseRepository = expenseRepository;
        _incomeRepository = incomeRepository;
        _categoryRepository = categoryRepository;

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
        OpenFiltersCommand = new RelayCommand(_ => ActivePopup = _filters);
        ClosePopupCommand = new RelayCommand(_ => ActivePopup = null);
    }

    // Call this right after construction (constructors can't be async)
    public async Task LoadTransactionsAsync()
    {
        var expenses = await _expenseRepository.GetAllAsync();
        var income = await _incomeRepository.GetAllAsync();
        _categoriesCache = await _categoryRepository.GetAllAsync();

        AllTransactions.Clear();

        foreach (var e in expenses)
        {
            var t = new Transaction
            {
                DbId = e.Id,
                Date = e.Date,
                Description = e.Description,
                Category = e.Category?.Name ?? "Uncategorized",
                Account = "Unknown", // TODO: no Account concept in DB yet
                IsIncome = false,
                Amount = e.Amount,
                CategoryBrush = CategoryPalette.BrushFor(e.Category?.Name ?? "Uncategorized"),
            };
            WireRowCommands(t);
            AllTransactions.Add(t);
        }

        foreach (var i in income)
        {
            var t = new Transaction
            {
                DbId = i.Id,
                Date = i.Date,
                Description = i.Source,
                Category = "income",
                Account = "Unknown",
                IsIncome = true,
                Amount = i.Amount,
                CategoryBrush = CategoryPalette.BrushFor("income"),
            };
            WireRowCommands(t);
            AllTransactions.Add(t);
        }

        RecomputeFilteredTransactions();

        var totalIncome = income.Sum(x => x.Amount);
        var totalExpenses = expenses.Sum(x => x.Amount);
        SummaryCards.Clear();
        SummaryCards.Add(new SummaryCard { Title = "net savings", Amount = totalIncome - totalExpenses, ChangePercent = 0, IsPositiveChange = true });
        SummaryCards.Add(new SummaryCard { Title = "total income", Amount = totalIncome, ChangePercent = 0, IsPositiveChange = true });
        SummaryCards.Add(new SummaryCard { Title = "total expenses", Amount = totalExpenses, ChangePercent = 0, IsPositiveChange = false });
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

    private void OpenImport(Transaction? editing)
    {
        var form = editing == null
            ? new ImportTransactionViewModel()
            : new ImportTransactionViewModel(editing);

        form.Submitted += OnTransactionSubmitted;
        form.CloseRequested += () => ActivePopup = null;
        ActivePopup = form;
    }

    private async void OnTransactionSubmitted(Transaction transaction)
    {
        var isEditing = AllTransactions.Any(t => t.Id == transaction.Id);

        if (transaction.IsIncome)
        {
            if (isEditing)
            {
                var income = await _incomeRepository.GetByIdAsync(transaction.DbId);
                if (income != null)
                {
                    income.Amount = transaction.Amount;
                    income.Source = transaction.Description;
                    income.Date = transaction.Date;
                    await _incomeRepository.UpdateAsync(income);
                }
            }
            else
            {
                var income = new CoreModels.Income
                {
                    Amount = transaction.Amount,
                    Source = transaction.Description,
                    Date = transaction.Date
                };
                await _incomeRepository.AddAsync(income);
            }
        }
        else
        {
            var category = await GetOrCreateCategoryAsync(transaction.Category);

            if (isEditing)
            {
                var expense = await _expenseRepository.GetByIdAsync(transaction.DbId);
                if (expense != null)
                {
                    expense.Amount = transaction.Amount;
                    expense.Description = transaction.Description;
                    expense.Date = transaction.Date;
                    expense.CategoryId = category.Id;
                    await _expenseRepository.UpdateAsync(expense);
                }
            }
            else
            {
                var expense = new CoreModels.Expense
                {
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    Date = transaction.Date,
                    CategoryId = category.Id
                };
                await _expenseRepository.AddAsync(expense);
            }
        }

        await LoadTransactionsAsync();
        DataChanged?.Invoke();
    }

    private async Task<CoreModels.Category> GetOrCreateCategoryAsync(string categoryName)
    {
        var existing = _categoriesCache.FirstOrDefault(c =>
            c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

        if (existing != null) return existing;

        var newCategory = new CoreModels.Category { Name = categoryName };
        await _categoryRepository.AddAsync(newCategory);
        _categoriesCache.Add(newCategory);
        return newCategory;
    }

    private async void RemoveTransaction(Transaction transaction)
    {
        if (transaction.IsIncome)
            await _incomeRepository.DeleteAsync(transaction.DbId);
        else
            await _expenseRepository.DeleteAsync(transaction.DbId);

        AllTransactions.Remove(transaction);
        RecomputeFilteredTransactions();
        DataChanged?.Invoke();
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

    // Updates dashboard analytics when new transaction is added
    public event Action? DataChanged;
    
}