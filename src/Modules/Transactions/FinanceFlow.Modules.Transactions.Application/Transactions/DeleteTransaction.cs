using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record DeleteTransactionCommand(Guid Id, Guid UserId) : IRequest<Result>;

public sealed class DeleteTransactionHandler(
    ITransactionRepository transactions,
    ITransactionsUnitOfWork uow) : IRequestHandler<DeleteTransactionCommand, Result>
{
    public async Task<Result> Handle(DeleteTransactionCommand cmd, CancellationToken ct)
    {
        var transaction = await transactions.GetByIdAsync(cmd.Id, ct);
        if (transaction is null || transaction.UserId != cmd.UserId)
            return Result.Failure(AppError.NotFound("transaction.not_found", "Transação não encontrada."));

        transaction.SoftDelete(); // soft delete: some das queries, mas fica no histórico
        transactions.Update(transaction);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
