using FinanceFlow.Modules.Accounts.Domain;
using FinanceFlow.SharedKernel;

namespace FinanceFlow.UnitTests;

public class AccountTests
{
    private static readonly Guid User = Guid.NewGuid();

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        var result = Account.Create(User, "  ", AccountType.Checking);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public void Create_WithLowercaseCurrency_NormalizesToUpper()
    {
        var result = Account.Create(User, "Carteira", AccountType.Cash, "brl", 100m);

        Assert.True(result.IsSuccess);
        Assert.Equal("BRL", result.Value.Currency);
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ReturnsFailure()
    {
        var result = Account.Create(User, "Carteira", AccountType.Cash, "REAL");

        Assert.True(result.IsFailure);
    }
}
