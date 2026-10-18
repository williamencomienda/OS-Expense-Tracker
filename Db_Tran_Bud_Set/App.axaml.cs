using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Data.Repositories;
using ExpenseTracker.Core.Interfaces;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Models;
using GCAT.ViewModels;
using GCAT.Views;
using GCAT.Models;

namespace GCAT;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DateDisplay.Load();
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Make sure the database exists and is up to date before anything tries to use it
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            if (!db.Categories.Any())
            {
                db.Categories.AddRange(
                    new Category { Name = "Food", MonthlyBudget = 2000 },
                    new Category { Name = "Transport", MonthlyBudget = 2000 },
                    new Category { Name = "Shopping", MonthlyBudget = 2000 },
                    new Category { Name = "Utilities", MonthlyBudget = 2000 },
                    new Category { Name = "Entertainment", MonthlyBudget = 2000 }
                );
                db.SaveChanges();
            }
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // App boots into the splash screen first; splash hands off to MainWindow.
            desktop.MainWindow = new SplashWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={AppContext.BaseDirectory}expensetracker.db"));

        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IIncomeRepository, IncomeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBudgetService, BudgetCalculationService>();
        services.AddTransient<TransactionsViewModel>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<BudgetViewModel>();
    }
}