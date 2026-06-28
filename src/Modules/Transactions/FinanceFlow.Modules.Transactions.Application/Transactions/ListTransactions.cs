using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

// Sem Year/Month → últimas N transações de TODOS os meses (default da aba Transações,
// pra você ver e corrigir o que lançou). Com Year+Month → filtra aquele mês.
public sealed record ListTransactionsQuery(Guid UserId, int? Year = null, int? Month = null, int Take = 200)
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;

public sealed class ListTransactionsHandler(ITransactionRepository transactions)
    : IRequestHandler<ListTransactionsQuery, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(ListTransactionsQuery query, CancellationToken ct)
    {
        var items = query.Year is int year && query.Month is int month
            ? await transactions.ListByUserAndMonthAsync(query.UserId, year, month, ct)
            : await transactions.ListRecentByUserAsync(query.UserId, query.Take, ct);

        IReadOnlyList<TransactionDto> dtos = items.Select(t => t.ToDto()).ToList();
        return Result<IReadOnlyList<TransactionDto>>.Success(dtos);
    }
}
