namespace ExpenseTracker.Core.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? MonthlyBudget { get; set; }

    public List<Expense> Expenses { get; set; } = new();
}