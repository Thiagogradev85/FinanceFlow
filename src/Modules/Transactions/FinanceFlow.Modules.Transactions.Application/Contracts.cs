namespace FinanceFlow.Modules.Transactions.Application;

// DTOs = a "casca" pública que sai pela API. Nunca expomos a entidade de domínio direto.
public sealed record CategoryDto(Guid Id, string Name, int Kind, string Color, string Icon);

public sealed record TransactionDto(
    Guid Id,
    Guid AccountId,
    Guid? CategoryId,
    int Type,
    int Direction,
    decimal Amount,
    string Currency,
    DateOnly OccurredOn,
    string Description,
    Guid? InstallmentGroupId = null,
    int? InstallmentNumber = null,
    int? InstallmentCount = null,
    bool IsCarryForward = false);

public sealed record TransactionsSummaryDto(
    int Year,
    int Month,
    decimal MonthIncome,
    decimal MonthExpense,
    decimal MonthNet,
    decimal AllTimeNet);

// "Comprometido": quanto de parcela já está agendado para os próximos meses.
public sealed record MonthCommitmentDto(int Year, int Month, decimal Amount);

public sealed record UpcomingCommitmentsDto(decimal Total, IReadOnlyList<MonthCommitmentDto> Months);

// Dados brutos vindos do banco para o breakdown de categorias.
// O PercentOfExpense é calculado no handler, não aqui — é regra de apresentação, não de persistência.
public sealed record CategoryBreakdownRawDto(
    Guid CategoryId,
    string CategoryName,
    string Color,
    string Icon,
    decimal Total);
