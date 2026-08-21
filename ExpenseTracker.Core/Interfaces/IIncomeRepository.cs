using ExpenseTracker.Core.Models;

namespace ExpenseTracker.Core.Interfaces;

public interface IIncomeRepository
{
    Task<List<Income>> GetAllAsync();
    Task<List<Income>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<Income?> GetByIdAsync(int id);
    Task AddAsync(Income income);
    Task UpdateAsync(Income income);
    Task DeleteAsync(int id);
}