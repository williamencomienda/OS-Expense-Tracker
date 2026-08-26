namespace GCAT.ViewModels;

// One editable row inside the "Set budget" popup (e.g. FOOD -> amount textbox).
public class BudgetInputRow : ViewModelBase
{
    public string CategoryKey { get; init; } = string.Empty;   // lowercase, matches BudgetCategory.Name
    public string CategoryLabel { get; init; } = string.Empty; // uppercase display label, e.g. "FOOD"

    private string _amountText = string.Empty;
    public string AmountText
    {
        get => _amountText;
        set => SetField(ref _amountText, value);
    }
}
