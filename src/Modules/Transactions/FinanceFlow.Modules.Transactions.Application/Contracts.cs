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
    string Description);

public sealed record TransactionsSummaryDto(
    int Year,
    int Month,
    decimal MonthIncome,
    decimal MonthExpense,
    decimal MonthNet,
    decimal AllTimeNet);
