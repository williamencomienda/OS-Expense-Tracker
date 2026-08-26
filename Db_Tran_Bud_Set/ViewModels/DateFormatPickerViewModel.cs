using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GCAT.ViewModels;

// Drives the "Date format" popup. Same shape as ListPickerViewModel (title, close, single-select
// list) but each row also shows a live-rendered preview date, which ListOption can't hold.
public class DateFormatPickerViewModel : ViewModelBase
{
    private static readonly (string Pattern, string NetFormat)[] Formats =
    {
        ("MMM DD, YYYY", "MMM dd, yyyy"),
        ("DD/MM/YYYY", "dd/MM/yyyy"),
        ("MM/DD/YYYY", "MM/dd/yyyy"),
        ("YYYY-MM-DD", "yyyy-MM-dd"),
    };

    public string Title => "Date format";
    public ObservableCollection<DateFormatOption> Options { get; }

    public RelayCommand CloseCommand { get; }

    public event Action<string>? FormatChosen;
    public event Action? CloseRequested;

    public DateFormatPickerViewModel(string selectedPattern)
    {
        var today = DateTime.Today;

        Options = new ObservableCollection<DateFormatOption>(
            Formats.Select(f => new DateFormatOption
            {
                Pattern = f.Pattern,
                Preview = today.ToString(f.NetFormat),
                IsSelected = f.Pattern == selectedPattern
            }));

        foreach (var option in Options)
            option.SelectCommand = new RelayCommand(_ => ChooseFormat(option));

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    private void ChooseFormat(DateFormatOption chosen)
    {
        foreach (var o in Options) o.IsSelected = ReferenceEquals(o, chosen);
        FormatChosen?.Invoke(chosen.Pattern);
    }
}
