using FinanceFlow.Modules.Transactions.Domain;

namespace FinanceFlow.Modules.Transactions.Application;

internal static class Mapping
{
    public static CategoryDto ToDto(this Category c) =>
        new(c.Id, c.Name, (int)c.Kind, c.Color, c.Icon);

    public static TransactionDto ToDto(this Transaction t) =>
        new(t.Id, t.AccountId, t.CategoryId, (int)t.Type, (int)t.Direction,
            t.Amount, t.Currency, t.OccurredOn, t.Description);
}
