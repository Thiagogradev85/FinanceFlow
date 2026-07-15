using FinanceFlow.Modules.Transactions.Application.Transactions;
using FinanceFlow.Modules.Transactions.Domain;

namespace FinanceFlow.UnitTests;

public class ListTransactionsHandlerTests
{
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Account = Guid.NewGuid();
    private static readonly Guid Category = Guid.NewGuid();

    private static Transaction Income(decimal amount, DateOnly date) =>
        Transaction.CreateIncomeOrExpense(User, Account, Category, TransactionType.Income, amount, "BRL", date, "Renda").Value;

    private static Transaction Expense(decimal amount, DateOnly date) =>
        Transaction.CreateIncomeOrExpense(User, Account, Category, TransactionType.Expense, amount, "BRL", date, "Gasto").Value;

    [Fact]
    public async Task Handle_WithMonthFilter_WhenPrevMonthHasPositiveBalance_PrependsCarryForwardAsInflow()
    {
        var repo = new FakeTransactionRepository([Income(1000m, new DateOnly(2026, 1, 15))]);
        var handler = new ListTransactionsHandler(repo);

        var result = await handler.Handle(new ListTransactionsQuery(User, 2026, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var first = result.Value[0];
        Assert.True(first.IsCarryForward);
        Assert.Equal("Saldo mês anterior", first.Description);
        Assert.Equal(1000m, first.Amount);
        Assert.Equal((int)TransactionDirection.Inflow, first.Direction);
        Assert.Equal(new DateOnly(2026, 2, 1), first.OccurredOn);
        Assert.Equal(Guid.Empty, first.Id);
    }

    [Fact]
    public async Task Handle_WithMonthFilter_WhenPrevMonthHasNegativeBalance_PrependsCarryForwardAsOutflow()
    {
        var repo = new FakeTransactionRepository([Expense(500m, new DateOnly(2026, 1, 15))]);
        var handler = new ListTransactionsHandler(repo);

        var result = await handler.Handle(new ListTransactionsQuery(User, 2026, 2), CancellationToken.None);

        var first = result.Value[0];
        Assert.True(first.IsCarryForward);
        Assert.Equal(500m, first.Amount);
        Assert.Equal((int)TransactionDirection.Outflow, first.Direction);
        Assert.Equal((int)TransactionType.Expense, first.Type);
    }

    [Fact]
    public async Task Handle_WithMonthFilter_WhenPrevMonthNetIsZero_DoesNotPrependCarryForward()
    {
        var jan = new DateOnly(2026, 1, 15);
        var repo = new FakeTransactionRepository([Income(500m, jan), Expense(500m, jan)]);
        var handler = new ListTransactionsHandler(repo);

        var result = await handler.Handle(new ListTransactionsQuery(User, 2026, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_WithoutMonthFilter_DoesNotInjectCarryForward()
    {
        var repo = new FakeTransactionRepository([Income(1000m, new DateOnly(2026, 1, 15))]);
        var handler = new ListTransactionsHandler(repo);

        var result = await handler.Handle(new ListTransactionsQuery(User), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value, t => Assert.False(t.IsCarryForward));
    }

    [Fact]
    public async Task Handle_WithMonthFilter_CurrentMonthTransactionsAppearAfterCarryForward()
    {
        var jan = new DateOnly(2026, 1, 15);
        var feb = new DateOnly(2026, 2, 10);
        var repo = new FakeTransactionRepository([Income(1000m, jan), Expense(200m, feb)]);
        var handler = new ListTransactionsHandler(repo);

        var result = await handler.Handle(new ListTransactionsQuery(User, 2026, 2), CancellationToken.None);

        Assert.Equal(2, result.Value.Count);
        Assert.True(result.Value[0].IsCarryForward);
        Assert.False(result.Value[1].IsCarryForward);
    }
}
