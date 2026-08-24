using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Core.Models;

namespace ExpenseTracker.Data;

public class AppDbContext : DbContext
{
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Income> Incomes => Set<Income>();
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId);

        modelBuilder.Entity<Income>()
            .Property(i => i.Amount)
            .HasColumnType("decimal(18,2)");
    }
}