namespace ExpenseTracker.Core.Models;

public class Budget
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int Month { get; set; }   // 1-12
    public int Year { get; set; }
}