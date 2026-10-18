using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GCAT.ViewModels;

// Drives the "Display currency" popup. Same shape as DateFormatPickerViewModel:
// title, close, and a single-select list of options.
public class CurrencyPickerViewModel : ViewModelBase
{
    private static readonly (string Symbol, string Code)[] Currencies =
    {
        ("\u20B1", "PHP"),        // ₱
        ("$", "USD/CAD"),
        ("\u00A5", "YEN/YUAN"),   // ¥
        ("\u20AC", "EUR"),        // €
    };

    public string Title => "Display currency";
    public ObservableCollection<CurrencyOption> Options { get; }

    public RelayCommand CloseCommand { get; }

    public event Action<string>? CurrencyChosen;
    public event Action? CloseRequested;

    public CurrencyPickerViewModel(string selectedLabel)
    {
        Options = new ObservableCollection<CurrencyOption>(
            Currencies.Select(c =>
            {
                var option = new CurrencyOption { Symbol = c.Symbol, Code = c.Code };
                option.IsSelected = option.Label == selectedLabel;
                return option;
            }));

        foreach (var option in Options)
            option.SelectCommand = new RelayCommand(_ => ChooseCurrency(option));

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    private void ChooseCurrency(CurrencyOption chosen)
    {
        foreach (var o in Options) o.IsSelected = ReferenceEquals(o, chosen);
        CurrencyChosen?.Invoke(chosen.Label);
    }
}
