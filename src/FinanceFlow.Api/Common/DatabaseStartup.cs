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

        var wallet = Account.Create(DemoUser.Id, "Carteira", AccountType.Cash, "BRL", 0m).Value;
        var checking = Account.Create(DemoUser.Id, "Conta Corrente", AccountType.Checking, "BRL", 0m).Value;
        accountsDb.Accounts.AddRange(wallet, checking);
        await accountsDb.SaveChangesAsync();

        // Receitas
        var salario      = Category.Create(DemoUser.Id, "Salário",      CategoryKind.Income,  "#22c55e", "wallet").Value;
        var freelance    = Category.Create(DemoUser.Id, "Freelance",    CategoryKind.Income,  "#3b82f6", "laptop").Value;
        var investimento = Category.Create(DemoUser.Id, "Investimentos",CategoryKind.Income,  "#f59e0b", "trending-up").Value;
        var outrasRec    = Category.Create(DemoUser.Id, "Outras receitas",CategoryKind.Income, "#888888","plus-circle").Value;

        // Despesas — principais gastos de família
        var moradia      = Category.Create(DemoUser.Id, "Moradia",       CategoryKind.Expense, "#6366f1", "home").Value;
        var mercado      = Category.Create(DemoUser.Id, "Mercado",       CategoryKind.Expense, "#f59e0b", "shopping-cart").Value;
        var restaurante  = Category.Create(DemoUser.Id, "Restaurante",   CategoryKind.Expense, "#f97316", "utensils").Value;
        var transporte   = Category.Create(DemoUser.Id, "Transporte",    CategoryKind.Expense, "#3b82f6", "car").Value;
        var saude        = Category.Create(DemoUser.Id, "Saúde",         CategoryKind.Expense, "#22c55e", "heart-pulse").Value;
        var farmacia     = Category.Create(DemoUser.Id, "Farmácia",      CategoryKind.Expense, "#ef4444", "pill").Value;
        var educacao     = Category.Create(DemoUser.Id, "Educação",      CategoryKind.Expense, "#a855f7", "graduation-cap").Value;
        var lazer        = Category.Create(DemoUser.Id, "Lazer",         CategoryKind.Expense, "#ec4899", "popcorn").Value;
        var vestuario    = Category.Create(DemoUser.Id, "Vestuário",     CategoryKind.Expense, "#14b8a6", "shirt").Value;
        var assinaturas  = Category.Create(DemoUser.Id, "Assinaturas",   CategoryKind.Expense, "#8b5cf6", "tv").Value;
        var academia     = Category.Create(DemoUser.Id, "Academia",      CategoryKind.Expense, "#06b6d4", "dumbbell").Value;
        var pets         = Category.Create(DemoUser.Id, "Pets",          CategoryKind.Expense, "#fb923c", "paw-print").Value;
        var outrasDesp   = Category.Create(DemoUser.Id, "Outras despesas",CategoryKind.Expense,"#888888","tag").Value;

        transactionsDb.Categories.AddRange(
            salario, freelance, investimento, outrasRec,
            moradia, mercado, restaurante, transporte, saude, farmacia,
            educacao, lazer, vestuario, assinaturas, academia, pets, outrasDesp);
        await transactionsDb.SaveChangesAsync();
        // Sem transações de demo — o usuário preenche do zero.
    }
}
