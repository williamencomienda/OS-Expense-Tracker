using ExpenseTracker.Core.Interfaces;
using ExpenseTracker.Core.Models;

namespace ExpenseTracker.Core.Services;

public class BudgetCalculationService : IBudgetService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BudgetCalculationService(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<decimal> GetTotalSpentAsync(int categoryId, int month, int year)
    {
        var expenses = await _expenseRepository.GetByCategoryAsync(categoryId);

        return expenses
            .Where(e => e.Date.Month == month && e.Date.Year == year)
            .Sum(e => e.Amount);
    }

    public async Task<decimal> GetRemainingBudgetAsync(int categoryId, int month, int year)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category?.MonthlyBudget is null)
            return 0;

        var spent = await GetTotalSpentAsync(categoryId, month, year);
        return category.MonthlyBudget.Value - spent;
    }

    public async Task<bool> IsOverBudgetAsync(int categoryId, int month, int year)
    {
        var remaining = await GetRemainingBudgetAsync(categoryId, month, year);
        return remaining < 0;
    }

    public async Task<Dictionary<Category, decimal>> GetSpendingSummaryAsync(int month, int year)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var summary = new Dictionary<Category, decimal>();

        foreach (var category in categories)
        {
            var spent = await GetTotalSpentAsync(category.Id, month, year);
            summary[category] = spent;
        }

        return summary;
    }
}