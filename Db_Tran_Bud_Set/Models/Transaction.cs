using System;
using System.Windows.Input;
using Avalonia.Media;

namespace GCAT.Models;

// A single row on the Transactions page (image 1). Richer than the Dashboard's
// TransactionItem since this page needs account/category/type columns and row actions.
public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;

    // "food", "transport", "shopping", "utilities", "entertainment", or "income".
    public string Category { get; set; } = string.Empty;

    public string Account { get; set; } = string.Empty;
    public bool IsIncome { get; set; }
    public decimal Amount { get; set; } // always a positive magnitude; sign comes from IsIncome

    public IBrush CategoryBrush { get; set; } = Brushes.HotPink;

    // Wired up by TransactionsViewModel right after the row is created, so the row's own
    // "..." menu (Edit / Remove) can bind straight to these without reaching back up to
    // the parent view model - keeps it working correctly even from inside a Flyout/popup.
    public ICommand? EditCommand { get; set; }
    public ICommand? RemoveCommand { get; set; }

    public string DisplayDate => Date.ToString("MMMM d, yyyy");
    public string DisplayAmount => $"\u20B1{Amount:N2}";
    public string TypeLabel => IsIncome ? "income" : "expense";
    public string TypeGlyph => IsIncome ? "^" : "v";
    public bool IsExpense => !IsIncome;
}
