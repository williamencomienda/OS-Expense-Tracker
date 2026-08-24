using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Core.Models;
using ExpenseTracker.Data;
using ExpenseTracker.Data.Repositories;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlite("Data Source=expensetracker.db");

using var context = new AppDbContext(optionsBuilder.Options);

// Make sure the database exists and migrations are applied
context.Database.Migrate();

var categoryRepo = new CategoryRepository(context);
var expenseRepo = new ExpenseRepository(context);
var incomeRepo = new IncomeRepository(context);

// --- Add a category ---
var groceries = new Category { Name = "Groceries", MonthlyBudget = 5000 };
await categoryRepo.AddAsync(groceries);
Console.WriteLine($"Created category: {groceries.Name} (Id: {groceries.Id})");

// --- Add an expense under it ---
var expense = new Expense
{
    Amount = 350.75m,
    Description = "Weekly grocery run",
    Date = DateTime.Now,
    CategoryId = groceries.Id
};
await expenseRepo.AddAsync(expense);
Console.WriteLine($"Created expense: {expense.Description} - {expense.Amount:C}");

// --- Add income ---
var income = new Income
{
    Amount = 20000,
    Source = "Salary",
    Date = DateTime.Now
};
await incomeRepo.AddAsync(income);
Console.WriteLine($"Created income: {income.Source} - {income.Amount:C}");

// --- Read everything back ---
Console.WriteLine("\n--- All Categories ---");
foreach (var c in await categoryRepo.GetAllAsync())
    Console.WriteLine($"{c.Id}: {c.Name} (Budget: {c.MonthlyBudget:C})");

Console.WriteLine("\n--- All Expenses ---");
foreach (var e in await expenseRepo.GetAllAsync())
    Console.WriteLine($"{e.Id}: {e.Description} - {e.Amount:C} on {e.Date:d}");

Console.WriteLine("\n--- All Income ---");
foreach (var i in await incomeRepo.GetAllAsync())
    Console.WriteLine($"{i.Id}: {i.Source} - {i.Amount:C} on {i.Date:d}");