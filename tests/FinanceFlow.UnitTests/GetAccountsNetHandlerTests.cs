using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.Modules.Transactions.Application.Transactions;

namespace FinanceFlow.UnitTests;

public class GetAccountsNetHandlerTests
{
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Category = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task Handle_MultipleAccountsWithMixedTransactions_ReturnsCorrectNetPerAccount()
    {
        var accountA = Guid.NewGuid();
        var accountB = Guid.NewGuid();
        var income = Transaction.CreateIncomeOrExpense(User, accountA, Category, TransactionType.Income, 1000m, "BRL", Today, "Salário").Value;
        var expense = Transaction.CreateIncomeOrExpense(User, accountA, Category, TransactionType.Expense, 300m, "BRL", Today, "Mercado").Value;
        var (outLeg, inLeg) = Transaction.CreateTransferPair(User, accountA, accountB, 200m, "BRL", Today, "Transferência").Value;
        var repo = new FakeTransactionRepository([income, expense, outLeg, inLeg]);
        var handler = new GetAccountsNetHandler(repo);

        var result = await handler.Handle(new GetAccountsNetQuery(User, Today), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(500m, result.Value[accountA]); // 1000 - 300 - 200
        Assert.Equal(200m, result.Value[accountB]); // +200 recebido na transferência
    }

    [Fact]
    public async Task Handle_AccountWithNoTransactions_IsAbsentFromResult()
    {
        var accountWithTx = Guid.NewGuid();
        var accountWithout = Guid.NewGuid();
        var tx = Transaction.CreateIncomeOrExpense(User, accountWithTx, Category, TransactionType.Income, 100m, "BRL", Today, "x").Value;
        var repo = new FakeTransactionRepository([tx]);
        var handler = new GetAccountsNetHandler(repo);

        var result = await handler.Handle(new GetAccountsNetQuery(User, Today), CancellationToken.None);

        Assert.False(result.Value.ContainsKey(accountWithout));
    }

    [Fact]
    public async Task Handle_TransactionAfterAsOfDate_IsExcludedFromNet()
    {
        var account = Guid.NewGuid();
        var future = Today.AddMonths(1);
        var installments = Transaction.CreateInstallmentPurchase(User, account, Category, 200m, 2, "BRL", Today, "Notebook").Value;
        var repo = new FakeTransactionRepository(installments);
        var handler = new GetAccountsNetHandler(repo);

        var result = await handler.Handle(new GetAccountsNetQuery(User, Today), CancellationToken.None);

        Assert.Equal(-200m, result.Value[account]); // só a 1ª parcela (Today); a 2ª (mês seguinte) fica de fora
        Assert.NotEqual(Today, future); // sanity check da massa de teste
    }

    [Fact]
    public async Task Handle_SoftDeletedTransaction_IsExcludedFromNet()
    {
        var account = Guid.NewGuid();
        var income = Transaction.CreateIncomeOrExpense(User, account, Category, TransactionType.Income, 1000m, "BRL", Today, "Salário").Value;
        var expense = Transaction.CreateIncomeOrExpense(User, account, Category, TransactionType.Expense, 300m, "BRL", Today, "Estorno").Value;
        expense.SoftDelete();
        var repo = new FakeTransactionRepository([income, expense]);
        var handler = new GetAccountsNetHandler(repo);

        var result = await handler.Handle(new GetAccountsNetQuery(User, Today), CancellationToken.None);

        Assert.Equal(1000m, result.Value[account]); // a despesa deletada não deve entrar na conta
    }

    [Fact]
    public async Task Handle_NoTransactionsAtAll_ReturnsEmptyDictionary()
    {
        var repo = new FakeTransactionRepository([]);
        var handler = new GetAccountsNetHandler(repo);

        var result = await handler.Handle(new GetAccountsNetQuery(User, Today), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
