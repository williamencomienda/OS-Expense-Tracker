using ExpenseTracker.Core.Models;

namespace ExpenseTracker.Core.Interfaces;

public interface IBudgetService
{
    Task<decimal> GetTotalSpentAsync(int categoryId, int month, int year);
    Task<decimal> GetRemainingBudgetAsync(int categoryId, int month, int year);
    Task<bool> IsOverBudgetAsync(int categoryId, int month, int year);
    Task<Dictionary<Category, decimal>> GetSpendingSummaryAsync(int month, int year);
}