namespace GCAT.ViewModels;

// A single row in the "Date format" popup - unlike ListOption, this needs a second
// value (the live preview, e.g. "Aug 31, 2026") alongside the pattern label.
public class DateFormatOption : ViewModelBase
{
    public string Pattern { get; init; } = string.Empty;
    public string Preview { get; init; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    // Assigned by DateFormatPickerViewModel, same WireRowCommands-style pattern
    // TransactionsViewModel uses for per-row Edit/Remove commands.
    public RelayCommand? SelectCommand { get; set; }
}
