using System;

namespace GCAT.Models;

// One of the three top cards: total balance / total income / total expenses.
public class SummaryCard
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double ChangePercent { get; set; }
    public bool IsPositiveChange { get; set; }

    public string DisplayAmount => $"{CurrencySettings.Symbol}{Amount:N2}";
    public string ChangeArrow => IsPositiveChange ? "\u2197" : "\u2198";
    public string DisplayChange => $"{ChangeArrow} {Math.Abs(ChangePercent):N0}% from last month";
}
