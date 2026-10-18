using Avalonia.Media;

namespace GCAT.Models;

// One wedge of the "expenses overview" donut chart + its legend row.
public class ExpenseSlice
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // 0.0 - 1.0 share of the total. Set by the view model after all slices are known.
    public double Percent { get; set; }

    public IBrush Color { get; set; } = Brushes.HotPink;

    public string DisplayAmount => $"{CurrencySettings.Symbol}{Amount:N0} ({Percent:P0})";
}
