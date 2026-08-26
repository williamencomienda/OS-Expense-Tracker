namespace GCAT.Models;

public class BudgetItem
{
    public string Category { get; set; } = string.Empty;
    public decimal Spent { get; set; }
    public decimal Limit { get; set; }

    public double Progress => Limit <= 0 ? 0 : (double)(Spent / Limit);
    public string DisplayLabel => $"\u20B1{Spent:N0}/\u20B1{Limit:N0}";
}
