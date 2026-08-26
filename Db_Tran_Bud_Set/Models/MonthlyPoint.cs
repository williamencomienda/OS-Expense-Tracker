namespace GCAT.Models;

// One column of the "income vs expenses" bar chart.
public class MonthlyPoint
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }

    // 0.0 - 1.0 height ratios relative to the max value in the series, filled in by the view model.
    public double IncomeRatio { get; set; }
    public double ExpensesRatio { get; set; }
}
