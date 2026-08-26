using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GCAT.ViewModels;

// One month button inside the "Select month" popup.
public class MonthOption : ViewModelBase
{
    public string Label { get; set; } = string.Empty;
    public int MonthNumber { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }
}

// Drives the "Select month" popup shown in image 3. It's shared by both trigger points
// (the header month button and the "this month" filter) - DashboardViewModel decides
// which label to update when a month is chosen.
public class MonthPickerViewModel : ViewModelBase
{
    private static readonly string[] MonthLabels =
        { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    private int _year;
    public int Year
    {
        get => _year;
        set => SetField(ref _year, value);
    }

    public ObservableCollection<MonthOption> Months { get; }

    public RelayCommand PrevYearCommand { get; }
    public RelayCommand NextYearCommand { get; }
    public RelayCommand ChooseMonthCommand { get; }

    public event Action<int, int>? MonthChosen; // (year, monthNumber)

    public MonthPickerViewModel(int year, int selectedMonth)
    {
        _year = year;

        Months = new ObservableCollection<MonthOption>(
            MonthLabels.Select((label, i) => new MonthOption { Label = label, MonthNumber = i + 1 }));

        SelectMonth(selectedMonth);

        PrevYearCommand = new RelayCommand(_ => Year--);
        NextYearCommand = new RelayCommand(_ => Year++);
        ChooseMonthCommand = new RelayCommand(param =>
        {
            if (param is MonthOption option)
            {
                SelectMonth(option.MonthNumber);
                MonthChosen?.Invoke(Year, option.MonthNumber);
            }
        });
    }

    public void SelectMonth(int monthNumber)
    {
        foreach (var m in Months)
            m.IsSelected = m.MonthNumber == monthNumber;
    }
}
