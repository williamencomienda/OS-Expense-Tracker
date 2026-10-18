using System;

namespace GCAT.Models;

public class TransactionItem
{
    public string Category { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    // Negative = expense, positive/zero = income.
    public decimal Amount { get; set; }

    public bool IsExpense => Amount < 0;
    public bool IsIncome => Amount >= 0;

    public string DisplayAmount => (IsExpense ? "- " : "+ ") + CurrencySettings.Symbol + System.Math.Abs(Amount).ToString("N2");
    public string DisplayDate => DateDisplay.Format(Date);
}
