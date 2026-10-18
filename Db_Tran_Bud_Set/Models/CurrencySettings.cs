namespace GCAT.Models;

// Holds the currency symbol chosen in Settings so plain model classes (like Transaction)
// can format amounts without a reference to SettingsViewModel.
public static class CurrencySettings
{
    public static string Symbol { get; private set; } = "\u20B1"; // ₱ by default

    // Accepts a stored label such as "₱ PHP" or "$ USD/CAD" and keeps just the symbol.
    public static void SetFromLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label)) return;
        Symbol = label.Trim().Split(' ')[0];
    }
}
