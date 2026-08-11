namespace ExpenseTracker.Core.Models;

public class Expense
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    // Foreign key relationship to Category
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}