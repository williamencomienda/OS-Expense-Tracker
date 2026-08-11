using ExpenseTracker.Core.Models;

namespace ExpenseTracker.Core.Interfaces;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllAsync();
    Task<List<Expense>> GetByCategoryAsync(int categoryId);
    Task<List<Expense>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<Expense?> GetByIdAsync(int id);
    Task AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(int id);
}