using FinanceFlow.SharedKernel;

namespace FinanceFlow.Modules.Transactions.Domain;

/// <summary>
/// Entidade central. Criada por factory methods nomeados (não por construtor público),
/// porque receita/despesa e transferência têm regras diferentes.
/// </summary>
public sealed class Transaction : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CategoryId { get; private set; }        // null em transferências
    public TransactionType Type { get; private set; }
    public TransactionDirection Direction { get; private set; }
    public decimal Amount { get; private set; }          // SEMPRE positivo
    public string Currency { get; private set; } = "BRL";
    public DateOnly OccurredOn { get; private set; }     // data que aconteceu (sem hora → sem bug de fuso)
    public string Description { get; private set; } = string.Empty;
    public Guid? TransferGroupId { get; private set; }   // liga as duas pernas de uma transferência
    public Guid? InstallmentGroupId { get; private set; } // liga as N parcelas de uma compra parcelada
    public int? InstallmentNumber { get; private set; }   // 1..N — qual parcela é esta
    public int? InstallmentCount { get; private set; }    // N — total de parcelas
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Transaction() { }

    public static Result<Transaction> CreateIncomeOrExpense(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        decimal amount,
        string currency,
        DateOnly occurredOn,
        string description)
    {
        if (type == TransactionType.Transfer)
            return AppError.Validation("transaction.use_transfer", "Use CreateTransferPair para transferências.");
        if (amount <= 0)
            return AppError.Validation("transaction.amount_positive", "Valor deve ser positivo.");
        if (string.IsNullOrWhiteSpace(currency))
            return AppError.Validation("transaction.currency_required", "Moeda é obrigatória.");

        var now = DateTime.UtcNow;
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            Direction = type == TransactionType.Income ? TransactionDirection.Inflow : TransactionDirection.Outflow,
            Amount = amount,
            Currency = currency,
            OccurredOn = occurredOn,
            Description = description?.Trim() ?? string.Empty,
            TransferGroupId = null,
            IsDeleted = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        transaction.Raise(new TransactionCreatedDomainEvent(
            transaction.Id, userId, accountId, transaction.Direction, amount, currency, occurredOn));

        return transaction;
    }

    /// <summary>
    /// Cria as DUAS pernas de uma transferência, já ligadas pelo mesmo TransferGroupId.
    /// Quem chama deve salvar as duas na MESMA transação SQL (UnitOfWork), senão fica
    /// "saldo fantasma": saiu de A mas nunca entrou em B.
    /// </summary>
    public static Result<(Transaction outLeg, Transaction inLeg)> CreateTransferPair(
        Guid userId,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        string currency,
        DateOnly occurredOn,
        string description)
    {
        if (fromAccountId == toAccountId)
            return AppError.Validation("transfer.same_account", "Contas de origem e destino devem ser diferentes.");
        if (amount <= 0)
            return AppError.Validation("transfer.amount_positive", "Valor deve ser positivo.");

        var groupId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var outLeg = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = fromAccountId,
            CategoryId = null,
            Type = TransactionType.Transfer,
            Direction = TransactionDirection.Outflow,
            Amount = amount,
            Currency = currency,
            OccurredOn = occurredOn,
            Description = description?.Trim() ?? string.Empty,
            TransferGroupId = groupId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var inLeg = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = toAccountId,
            CategoryId = null,
            Type = TransactionType.Transfer,
            Direction = TransactionDirection.Inflow,
            Amount = amount,
            Currency = currency,
            OccurredOn = occurredOn,
            Description = description?.Trim() ?? string.Empty,
            TransferGroupId = groupId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        return (outLeg, inLeg);
    }

    /// <summary>
    /// Cria as N parcelas de uma compra parcelada (sempre despesa), uma por mês, ligadas pelo
    /// mesmo InstallmentGroupId. O valor é o da PARCELA, não o total: cada mês recebe exatamente
    /// esse valor (total = installmentAmount × installmentCount). A 1ª parcela cai em
    /// firstOccurredOn; as seguintes em firstOccurredOn.AddMonths(i) — o AddMonths já corrige
    /// fim de mês (31/jan → 28/fev). Como na transferência, quem chama deve salvar TODAS na
    /// mesma transação SQL (UnitOfWork).
    /// </summary>
    public static Result<IReadOnlyList<Transaction>> CreateInstallmentPurchase(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        decimal installmentAmount,
        int installmentCount,
        string currency,
        DateOnly firstOccurredOn,
        string description)
    {
        if (installmentCount < 2)
            return AppError.Validation("installment.count_min", "Parcelamento exige ao menos 2 parcelas.");
        if (installmentAmount <= 0)
            return AppError.Validation("installment.amount_positive", "Valor da parcela deve ser positivo.");
        if (string.IsNullOrWhiteSpace(currency))
            return AppError.Validation("transaction.currency_required", "Moeda é obrigatória.");

        var groupId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var baseDescription = description?.Trim() ?? string.Empty;

        var installments = new List<Transaction>(installmentCount);
        for (var i = 0; i < installmentCount; i++)
        {
            var number = i + 1;
            var label = $"({number}/{installmentCount})";
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = accountId,
                CategoryId = categoryId,
                Type = TransactionType.Expense,
                Direction = TransactionDirection.Outflow,
                Amount = installmentAmount,
                Currency = currency,
                OccurredOn = firstOccurredOn.AddMonths(i),
                Description = baseDescription.Length == 0 ? label : $"{baseDescription} {label}",
                TransferGroupId = null,
                InstallmentGroupId = groupId,
                InstallmentNumber = number,
                InstallmentCount = installmentCount,
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            transaction.Raise(new TransactionCreatedDomainEvent(
                transaction.Id, userId, accountId, transaction.Direction,
                installmentAmount, currency, transaction.OccurredOn));

            installments.Add(transaction);
        }

        return Result<IReadOnlyList<Transaction>>.Success(installments);
    }

    /// <summary>
    /// Edita uma receita/despesa existente. Transferência não se edita por aqui
    /// (ela é um par de pernas — mudaria as duas). Recalcula a Direction a partir do Type.
    /// </summary>
    public Result EditIncomeOrExpense(
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        decimal amount,
        string currency,
        DateOnly occurredOn,
        string description)
    {
        if (Type == TransactionType.Transfer)
            return Result.Failure(AppError.Validation("transaction.cannot_edit_transfer", "Transferências não podem ser editadas por aqui."));
        if (type == TransactionType.Transfer)
            return Result.Failure(AppError.Validation("transaction.use_transfer", "Use transferência para Type=Transfer."));
        if (amount <= 0)
            return Result.Failure(AppError.Validation("transaction.amount_positive", "Valor deve ser positivo."));

        AccountId = accountId;
        CategoryId = categoryId;
        Type = type;
        Direction = type == TransactionType.Income ? TransactionDirection.Inflow : TransactionDirection.Outflow;
        Amount = amount;
        Currency = currency;
        OccurredOn = occurredOn;
        Description = description?.Trim() ?? string.Empty;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
