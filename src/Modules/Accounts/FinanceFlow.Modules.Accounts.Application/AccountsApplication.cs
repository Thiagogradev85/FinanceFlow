using FinanceFlow.Modules.Accounts.Domain;
using FinanceFlow.SharedKernel;

namespace FinanceFlow.Modules.Accounts.Application;

/// <summary>Âncora para escanear este assembly (MediatR + validators).</summary>
public sealed class AccountsApplicationAssemblyMarker;

public sealed record AccountDto(
    Guid Id,
    string Name,
    int Type,
    string Currency,
    decimal OpeningBalance);

public interface IAccountsUnitOfWork : IUnitOfWork;

public interface IAccountRepository : IRepository<Account>
{
    Task<IReadOnlyList<Account>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<decimal> GetOpeningTotalAsync(Guid userId, CancellationToken ct = default);
}

internal static class AccountMapping
{
    public static AccountDto ToDto(this Account a) =>
        new(a.Id, a.Name, (int)a.Type, a.Currency, a.OpeningBalance);
}
