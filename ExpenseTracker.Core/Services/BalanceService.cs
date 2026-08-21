using ExpenseTracker.Core.Interfaces;

namespace ExpenseTracker.Core.Services;

public class BalanceService : IBalanceService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IIncomeRepository _incomeRepository;

    public BalanceService(
        IExpenseRepository expenseRepository,
        IIncomeRepository incomeRepository)
    {
        _expenseRepository = expenseRepository;
        _incomeRepository = incomeRepository;
    }

    public async Task<decimal> GetCurrentBalanceAsync()
    {
        var allExpenses = await _expenseRepository.GetAllAsync();
        var allIncome = await _incomeRepository.GetAllAsync();

        var totalIncome = allIncome.Sum(i => i.Amount);
        var totalExpenses = allExpenses.Sum(e => e.Amount);

        return totalIncome - totalExpenses;
    }

    public async Task<decimal> GetTotalIncomeAsync(int month, int year)
    {
        var income = await _incomeRepository.GetAllAsync();
        return income
            .Where(i => i.Date.Month == month && i.Date.Year == year)
            .Sum(i => i.Amount);
    }

    public async Task<decimal> GetTotalExpensesAsync(int month, int year)
    {
        var expenses = await _expenseRepository.GetAllAsync();
        return expenses
            .Where(e => e.Date.Month == month && e.Date.Year == year)
            .Sum(e => e.Amount);
    }

    public async Task<decimal> GetBalanceForMonthAsync(int month, int year)
    {
        var income = await GetTotalIncomeAsync(month, year);
        var expenses = await GetTotalExpensesAsync(month, year);
        return income - expenses;
    }
}