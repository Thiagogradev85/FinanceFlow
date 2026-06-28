using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record GetUpcomingCommitmentsQuery(Guid UserId, int Months = 6)
    : IRequest<Result<UpcomingCommitmentsDto>>;

public sealed class GetUpcomingCommitmentsHandler(ITransactionRepository transactions, IClock clock)
    : IRequestHandler<GetUpcomingCommitmentsQuery, Result<UpcomingCommitmentsDto>>
{
    public async Task<Result<UpcomingCommitmentsDto>> Handle(GetUpcomingCommitmentsQuery query, CancellationToken ct)
    {
        var months = query.Months < 1 ? 6 : query.Months;
        var today = DateOnly.FromDateTime(clock.UtcNow);

        // Conta a partir do 1º dia do PRÓXIMO mês, por N meses. As parcelas do mês corrente já
        // entram nas despesas do mês; "comprometido" é o que ainda vem pela frente.
        var from = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
        var to = from.AddMonths(months);

        var monthly = await transactions.GetMonthlyCommittedInstallmentsAsync(query.UserId, from, to, ct);
        var total = monthly.Sum(m => m.Amount);

        return new UpcomingCommitmentsDto(total, monthly);
    }
}
