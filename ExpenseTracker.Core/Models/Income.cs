namespace ExpenseTracker.Core.Models;

public class Income
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;  // e.g. "Salary", "Freelance", "Gift"
    public DateTime Date { get; set; }
}