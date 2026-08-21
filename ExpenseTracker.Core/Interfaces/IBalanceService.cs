namespace ExpenseTracker.Core.Interfaces;

public interface IBalanceService
{
    Task<decimal> GetCurrentBalanceAsync();
    Task<decimal> GetBalanceForMonthAsync(int month, int year);
    Task<decimal> GetTotalIncomeAsync(int month, int year);
    Task<decimal> GetTotalExpensesAsync(int month, int year);
}