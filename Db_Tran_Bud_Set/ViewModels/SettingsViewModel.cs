using GCAT.Models;

namespace GCAT.ViewModels;
using GCAT.Models;

public class SettingsViewModel : ViewModelBase
{
    // ----- Preferences -----
    private string _displayCurrency = "\u20B1 PHP";
    public string DisplayCurrency
    {
        get => _displayCurrency;
        set
        {
            if (SetField(ref _displayCurrency, value))
                CurrencySettings.SetFromLabel(value);
        }
    }

    private string _dateFormat = DateDisplay.Pattern;
    public string DateFormat
    {
        get => _dateFormat;
        set => SetField(ref _dateFormat, value);
    }

    // ----- Popup host -----
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

    public RelayCommand OpenProfileCommand { get; }
    public RelayCommand OpenDateFormatPickerCommand { get; }
    public RelayCommand OpenCurrencyPickerCommand { get; }
    public RelayCommand ClosePopupCommand { get; }

    public SettingsViewModel()
    {
        // TODO: hook up whatever the "meow" button should actually do (profile menu, settings, etc.)
        OpenProfileCommand = new RelayCommand(_ => { });

        OpenDateFormatPickerCommand = new RelayCommand(_ => OpenDateFormatPicker());
        OpenCurrencyPickerCommand = new RelayCommand(_ => OpenCurrencyPicker());

        ClosePopupCommand = new RelayCommand(_ => ActivePopup = null);
    }

    private void OpenDateFormatPicker()
    {
        var picker = new DateFormatPickerViewModel(DateFormat);
        picker.FormatChosen += pattern =>
        {
            DateFormat = pattern;
            DateDisplay.Set(pattern);
            ActivePopup = null;
        };
        picker.CloseRequested += () => ActivePopup = null;
        ActivePopup = picker;
    }

    private void OpenCurrencyPicker()
    {
        var picker = new CurrencyPickerViewModel(DisplayCurrency);
        picker.CurrencyChosen += label =>
        {
            DisplayCurrency = label;
            ActivePopup = null;
        };
        picker.CloseRequested += () => ActivePopup = null;
        ActivePopup = picker;
    }
}
