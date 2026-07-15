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

    [Fact]
    public void CreateTransferPair_Valid_RaisesTransactionCreatedEventOnBothLegs()
    {
        var to = Guid.NewGuid();

        var result = Transaction.CreateTransferPair(
            User, Account, to, 250m, "BRL", Today, "Transferência");

        var (outLeg, inLeg) = result.Value;
        Assert.Single(outLeg.DomainEvents);
        Assert.IsType<TransactionCreatedDomainEvent>(outLeg.DomainEvents.First());
        Assert.Single(inLeg.DomainEvents);
        Assert.IsType<TransactionCreatedDomainEvent>(inLeg.DomainEvents.First());
    }

    [Fact]
    public void CreateInstallmentPurchase_WithLessThanTwoInstallments_ReturnsFailure()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 1, "BRL", Today, "Notebook");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public void CreateInstallmentPurchase_WithNonPositiveAmount_ReturnsFailure()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 0m, 12, "BRL", Today, "Notebook");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CreateInstallmentPurchase_Valid_CreatesOneTransactionPerInstallment()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 12, "BRL", Today, "Notebook");

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value.Count);
    }

    [Fact]
    public void CreateInstallmentPurchase_Valid_LinksAllInstallmentsWithSameGroup()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 12, "BRL", Today, "Notebook");

        var groups = result.Value.Select(t => t.InstallmentGroupId).Distinct().ToList();

        Assert.Single(groups);
        Assert.NotNull(groups[0]);
    }

    [Fact]
    public void CreateInstallmentPurchase_Valid_NumbersInstallmentsSequentially()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 3, "BRL", Today, "Notebook");

        var numbers = result.Value.Select(t => t.InstallmentNumber).ToList();

        Assert.Equal(new int?[] { 1, 2, 3 }, numbers);
        Assert.All(result.Value, t => Assert.Equal(3, t.InstallmentCount));
    }

    [Fact]
    public void CreateInstallmentPurchase_Valid_SpreadsAcrossConsecutiveMonths()
    {
        var first = new DateOnly(2026, 1, 15);

        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 3, "BRL", first, "Notebook");

        var dates = result.Value.Select(t => t.OccurredOn).ToList();

        Assert.Equal(first, dates[0]);
        Assert.Equal(first.AddMonths(1), dates[1]);
        Assert.Equal(first.AddMonths(2), dates[2]);
    }

    [Fact]
    public void CreateInstallmentPurchase_Valid_EachInstallmentIsOutflowExpenseWithInstallmentAmount()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 12, "BRL", Today, "Notebook");

        Assert.All(result.Value, t =>
        {
            Assert.Equal(TransactionType.Expense, t.Type);
            Assert.Equal(TransactionDirection.Outflow, t.Direction);
            Assert.Equal(200m, t.Amount);
        });
    }

    [Fact]
    public void CreateInstallmentPurchase_Valid_LabelsDescriptionWithInstallmentNumber()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 12, "BRL", Today, "Notebook");

        Assert.Equal("Notebook (1/12)", result.Value[0].Description);
        Assert.Equal("Notebook (12/12)", result.Value[11].Description);
    }

    [Fact]
    public void CreateInstallmentPurchase_Valid_RaisesOneTransactionCreatedEventPerInstallment()
    {
        var result = Transaction.CreateInstallmentPurchase(
            User, Account, Category, 200m, 12, "BRL", Today, "Notebook");

        Assert.All(result.Value, t =>
        {
            Assert.Single(t.DomainEvents);
            Assert.IsType<TransactionCreatedDomainEvent>(t.DomainEvents.First());
        });
    }
}
