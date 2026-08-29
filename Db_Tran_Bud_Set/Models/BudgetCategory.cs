using GCAT.ViewModels;

namespace GCAT.Models;

// One row in the "all budgets" list (e.g. food, entertainment).
public class BudgetCategory : ViewModelBase
{
    public string Name { get; set; } = string.Empty;

    private decimal _spent;
    public decimal Spent
    {
        get => _spent;
        set => SetField(ref _spent, value);
    }

    private decimal _limit;
    public decimal Limit
    {
        get => _limit;
        set => SetField(ref _limit, value);
    }

    // Raw percent used, uncapped (can exceed 100 when over budget).
    public double PercentUsed => Limit <= 0 ? 0 : (double)(Spent / Limit) * 100;

    // Value fed to the ProgressBar (capped at 100 so the bar never overflows visually).
    public double ProgressValue => PercentUsed > 100 ? 100 : PercentUsed;

    public bool IsOverBudget => Spent > Limit;

    public string DisplayAmount => $"\u20B1{Spent:N0}/\u20B1{Limit:N0}";

    public string DisplayPercent => IsOverBudget
        ? $"{PercentUsed:N0}% - over budget"
        : $"{PercentUsed:N0}%";
}
