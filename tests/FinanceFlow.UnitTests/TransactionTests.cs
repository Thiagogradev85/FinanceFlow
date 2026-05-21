using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;

namespace FinanceFlow.UnitTests;

public class TransactionTests
{
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Account = Guid.NewGuid();
    private static readonly Guid Category = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void CreateIncomeOrExpense_WithTransferType_ReturnsFailure()
    {
        var result = Transaction.CreateIncomeOrExpense(
            User, Account, Category, TransactionType.Transfer, 100m, "BRL", Today, "x");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public void CreateIncomeOrExpense_WithNonPositiveAmount_ReturnsFailure()
    {
        var result = Transaction.CreateIncomeOrExpense(
            User, Account, Category, TransactionType.Expense, 0m, "BRL", Today, "x");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CreateIncomeOrExpense_Income_SetsInflowDirection()
    {
        var result = Transaction.CreateIncomeOrExpense(
            User, Account, Category, TransactionType.Income, 4500m, "BRL", Today, "Salário");

        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionDirection.Inflow, result.Value.Direction);
    }

    [Fact]
    public void CreateIncomeOrExpense_Expense_RaisesTransactionCreatedEvent()
    {
        var result = Transaction.CreateIncomeOrExpense(
            User, Account, Category, TransactionType.Expense, 80m, "BRL", Today, "Cinema");

        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionDirection.Outflow, result.Value.Direction);
        Assert.Single(result.Value.DomainEvents);
        Assert.IsType<TransactionCreatedDomainEvent>(result.Value.DomainEvents.First());
    }

    [Fact]
    public void CreateTransferPair_SameAccount_ReturnsFailure()
    {
        var result = Transaction.CreateTransferPair(
            User, Account, Account, 100m, "BRL", Today, "x");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CreateTransferPair_Valid_LinksBothLegsWithSameGroup()
    {
        var to = Guid.NewGuid();

        var result = Transaction.CreateTransferPair(
            User, Account, to, 250m, "BRL", Today, "Transferência");

        Assert.True(result.IsSuccess);
        var (outLeg, inLeg) = result.Value;

        Assert.Equal(TransactionDirection.Outflow, outLeg.Direction);
        Assert.Equal(TransactionDirection.Inflow, inLeg.Direction);
        Assert.Equal(outLeg.TransferGroupId, inLeg.TransferGroupId);
        Assert.NotNull(outLeg.TransferGroupId);
    }
}
