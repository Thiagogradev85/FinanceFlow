using FinanceFlow.Modules.Accounts.Domain;
using FinanceFlow.Modules.Accounts.Infrastructure;
using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.Modules.Transactions.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Api.Common;

public static class DatabaseStartup
{
    /// <summary>Aplica as migrations dos dois módulos e, se vazio, semeia dados de demonstração.</summary>
    public static async Task UseDatabaseStartupAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var accountsDb = sp.GetRequiredService<AccountsDbContext>();
        var transactionsDb = sp.GetRequiredService<TransactionsDbContext>();

        await accountsDb.Database.MigrateAsync();
        await transactionsDb.Database.MigrateAsync();

        await SeedAsync(accountsDb, transactionsDb);
    }

    private static async Task SeedAsync(AccountsDbContext accountsDb, TransactionsDbContext transactionsDb)
    {
        if (await accountsDb.Accounts.AnyAsync(a => a.UserId == DemoUser.Id))
            return;

        var wallet = Account.Create(DemoUser.Id, "Carteira", AccountType.Cash, "BRL", 500m).Value;
        var checking = Account.Create(DemoUser.Id, "Conta Corrente", AccountType.Checking, "BRL", 2500m).Value;
        accountsDb.Accounts.AddRange(wallet, checking);
        await accountsDb.SaveChangesAsync();

        var salario = Category.Create(DemoUser.Id, "Salário", CategoryKind.Income, "#22c55e", "wallet").Value;
        var mercado = Category.Create(DemoUser.Id, "Mercado", CategoryKind.Expense, "#ef4444", "shopping-cart").Value;
        var lazer = Category.Create(DemoUser.Id, "Lazer", CategoryKind.Expense, "#f59e0b", "popcorn").Value;
        transactionsDb.Categories.AddRange(salario, mercado, lazer);
        await transactionsDb.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var t1 = Transaction.CreateIncomeOrExpense(DemoUser.Id, checking.Id, salario.Id, TransactionType.Income, 4500m, "BRL", today.AddDays(-10), "Salário do mês").Value;
        var t2 = Transaction.CreateIncomeOrExpense(DemoUser.Id, checking.Id, mercado.Id, TransactionType.Expense, 320.50m, "BRL", today.AddDays(-3), "Compras no mercado").Value;
        var t3 = Transaction.CreateIncomeOrExpense(DemoUser.Id, wallet.Id, lazer.Id, TransactionType.Expense, 80m, "BRL", today.AddDays(-1), "Cinema").Value;
        transactionsDb.Transactions.AddRange(t1, t2, t3);
        await transactionsDb.SaveChangesAsync();
    }
}
