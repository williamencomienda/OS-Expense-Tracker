namespace GCAT.ViewModels;

// A single row in the "Display currency" popup. Mirrors DateFormatOption: the row shows a
// symbol and a code/name, and is selected via a per-row command wired by the picker VM.
public class CurrencyOption : ViewModelBase
{
    public string Symbol { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;

    // The value stored in SettingsViewModel.DisplayCurrency, e.g. "₱ PHP".
    public string Label => $"{Symbol} {Code}";

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public RelayCommand? SelectCommand { get; set; }
}
