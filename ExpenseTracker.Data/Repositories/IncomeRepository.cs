using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Core.Interfaces;
using ExpenseTracker.Core.Models;

namespace ExpenseTracker.Data.Repositories;

public class IncomeRepository : IIncomeRepository
{
    private readonly AppDbContext _context;

    public IncomeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Income>> GetAllAsync()
    {
        return await _context.Incomes
            .OrderByDescending(i => i.Date)
            .ToListAsync();
    }

    public async Task<List<Income>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Incomes
            .Where(i => i.Date >= start && i.Date <= end)
            .ToListAsync();
    }

    public async Task<Income?> GetByIdAsync(int id)
    {
        return await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddAsync(Income income)
    {
        _context.Incomes.Add(income);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Income income)
    {
        _context.Incomes.Update(income);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income is not null)
        {
            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();
        }
    }
}