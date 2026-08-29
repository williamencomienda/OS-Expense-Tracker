using System;
using System.Collections.ObjectModel;
using System.Linq;
using GCAT.Models;

namespace GCAT.ViewModels;

// Drives the "Filters" popup (image 5): category chips, a date range, an amount range,
// and sort-by chips, plus Reset / Apply.
public class FiltersViewModel : ViewModelBase
{
    public ObservableCollection<ListOption> Categories { get; }
    public ObservableCollection<ListOption> SortOptions { get; }

    private string _dateFrom = string.Empty;
    public string DateFrom
    {
        get => _dateFrom;
        set => SetField(ref _dateFrom, value);
    }

    private string _dateTo = string.Empty;
    public string DateTo
    {
        get => _dateTo;
        set => SetField(ref _dateTo, value);
    }

    private string _amountMin = string.Empty;
    public string AmountMin
    {
        get => _amountMin;
        set => SetField(ref _amountMin, value);
    }

    private string _amountMax = string.Empty;
    public string AmountMax
    {
        get => _amountMax;
        set => SetField(ref _amountMax, value);
    }

    public RelayCommand SelectCategoryCommand { get; }
    public RelayCommand SelectSortCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand ApplyCommand { get; }
    public RelayCommand CloseCommand { get; }

    // Raised when "Apply filters" is clicked - TransactionsViewModel reads the current
    // selections straight off this instance rather than needing them passed as args.
    public event Action? Applied;
    public event Action? CloseRequested;

    public string SelectedCategory => Categories.FirstOrDefault(c => c.IsSelected)?.Label ?? "All";
    public string SelectedSort => SortOptions.FirstOrDefault(s => s.IsSelected)?.Label ?? "Newest";

    public FiltersViewModel()
    {
        var categoryLabels = new[] { "All" }.Concat(CategoryPalette.ExpenseCategories.Select(Capitalize));
        Categories = new ObservableCollection<ListOption>(
            categoryLabels.Select((c, i) => new ListOption { Label = c, IsSelected = i == 0 }));

        var sortLabels = new[] { "Newest", "Oldest", "Highest amount", "Lowest amount" };
        SortOptions = new ObservableCollection<ListOption>(
            sortLabels.Select((s, i) => new ListOption { Label = s, IsSelected = i == 0 }));

        SelectCategoryCommand = new RelayCommand(param =>
        {
            if (param is ListOption option)
                foreach (var c in Categories) c.IsSelected = ReferenceEquals(c, option);
        });

        SelectSortCommand = new RelayCommand(param =>
        {
            if (param is ListOption option)
                foreach (var s in SortOptions) s.IsSelected = ReferenceEquals(s, option);
        });

        ResetCommand = new RelayCommand(_ =>
        {
            foreach (var c in Categories) c.IsSelected = c.Label == "All";
            foreach (var s in SortOptions) s.IsSelected = s.Label == "Newest";
            DateFrom = string.Empty;
            DateTo = string.Empty;
            AmountMin = string.Empty;
            AmountMax = string.Empty;
        });

        ApplyCommand = new RelayCommand(_ => Applied?.Invoke());
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    private static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
