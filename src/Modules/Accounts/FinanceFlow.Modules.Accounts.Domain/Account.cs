using FinanceFlow.SharedKernel;

namespace FinanceFlow.Modules.Accounts.Domain;

/// <summary>
/// Conta onde o dinheiro mora. O saldo ATUAL não é guardado aqui: ele é
/// OpeningBalance + (entradas − saídas) das transações. Manter saldo materializado
/// é justamente o caso de uso do Kafka na Fase 2 (um consumer atualiza um agregado).
/// </summary>
public sealed class Account : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public decimal OpeningBalance { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Account() { }

    public static Result<Account> Create(
        Guid userId,
        string name,
        AccountType type,
        string currency = "BRL",
        decimal openingBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return AppError.Validation("account.name_required", "Nome da conta é obrigatório.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return AppError.Validation("account.currency_invalid", "Moeda deve ter 3 letras (ex.: BRL).");

        var now = DateTime.UtcNow;
        return new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            Currency = currency.ToUpperInvariant(),
            OpeningBalance = openingBalance,
            IsArchived = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da conta é obrigatório.", nameof(name));

        Name = name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Result UpdateDetails(string name, decimal openingBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
            return AppError.Validation("account.name_required", "Nome da conta é obrigatório.");

        Name = name.Trim();
        OpeningBalance = openingBalance;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    public void Archive()
    {
        IsArchived = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
