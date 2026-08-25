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
    modelBuilder.Entity<Expense>(entity =>
    {
        entity.Property(e => e.Amount)
              .HasColumnType("decimal(18,2)");

        entity.HasOne(e => e.Category)
              .WithMany(c => c.Expenses)
              .HasForeignKey(e => e.CategoryId);
    });

    modelBuilder.Entity<Category>(entity =>
    {
        entity.Property(c => c.MonthlyBudget)
              .HasColumnType("decimal(18,2)");
    });

    modelBuilder.Entity<Income>(entity =>
    {
        entity.Property(i => i.Amount)
              .HasColumnType("decimal(18,2)");
    });
}
}