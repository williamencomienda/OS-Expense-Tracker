using System.Collections.Generic;
using Avalonia.Media;

namespace GCAT.Models;

// Central place for category -> pill color mapping, shared by the transactions table,
// the "Filters" popup category chips, and the import/edit form's category dropdown.
public static class CategoryPalette
{
    public static readonly IReadOnlyList<string> ExpenseCategories = new[]
    {
        "food", "transport", "shopping", "utilities", "entertainment"
    };

    private static readonly Dictionary<string, IBrush> Colors = new()
    {
        ["income"] = new SolidColorBrush(Color.Parse("#3DDC84")),
        ["food"] = new SolidColorBrush(Color.Parse("#FF6FB1")),
        ["transport"] = new SolidColorBrush(Color.Parse("#B06FFF")),
        ["shopping"] = new SolidColorBrush(Color.Parse("#FFD76F")),
        ["utilities"] = new SolidColorBrush(Color.Parse("#6FD1FF")),
        ["entertainment"] = new SolidColorBrush(Color.Parse("#FFA36F")),
    };

    public static IBrush BrushFor(string category) =>
        Colors.TryGetValue(category.ToLowerInvariant(), out var brush) ? brush : Brushes.Gray;
}
