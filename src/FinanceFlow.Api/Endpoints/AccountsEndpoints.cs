using FinanceFlow.Api.Common;
using FinanceFlow.Modules.Accounts.Application;
using FinanceFlow.Modules.Accounts.Domain;
using FinanceFlow.Modules.Transactions.Application.Categories;
using FinanceFlow.Modules.Transactions.Application.Transactions;
using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Api.Endpoints;

public static class AccountsEndpoints
{
    public sealed record CreateAccountRequest(string Name, AccountType Type, string Currency = "BRL", decimal OpeningBalance = 0);
    public sealed record UpdateAccountRequest(string Name, decimal OpeningBalance);
    public sealed record AccountBalanceDto(Guid AccountId, decimal OpeningBalance, decimal TransactionsNet, decimal Balance);

    public static void MapAccountsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListAccountsQuery(DemoUser.Id), ct)).ToHttp());

        // Compõe dois módulos no nível da API (Accounts + Transactions) — mesmo padrão do
        // DashboardEndpoints: nenhum módulo chama o outro diretamente. Uma query batida em
        // lote (não uma por conta) para não virar N+1 quando o usuário tem várias contas.
        group.MapGet("/balances", async (IClock clock, ISender sender, CancellationToken ct) =>
        {
            var accountsResult = await sender.Send(new ListAccountsQuery(DemoUser.Id), ct);
            if (accountsResult.IsFailure) return accountsResult.ToHttp();

            var asOf = DateOnly.FromDateTime(clock.UtcNow);
            var netsResult = await sender.Send(new GetAccountsNetQuery(DemoUser.Id, asOf), ct);
            if (netsResult.IsFailure) return netsResult.ToHttp();

            var nets = netsResult.Value!;
            var dtos = accountsResult.Value!
                .Select(a =>
                {
                    var net = nets.GetValueOrDefault(a.Id);
                    return new AccountBalanceDto(a.Id, a.OpeningBalance, net, a.OpeningBalance + net);
                })
                .ToList();

            return Results.Ok(dtos);
        });

        group.MapPost("/", async (CreateAccountRequest req, ISender sender, CancellationToken ct) =>
        {
            // openingBalance fica em 0 na entidade; o saldo inicial vira uma transação de Receita.
            var result = await sender.Send(
                new CreateAccountCommand(DemoUser.Id, req.Name, req.Type, req.Currency, 0m), ct);
            if (result.IsFailure) return result.ToHttp();

            if (req.OpeningBalance > 0)
            {
                var cats = await sender.Send(new ListCategoriesQuery(DemoUser.Id), ct);
                var incomeCat = cats.Value?.FirstOrDefault(c => c.Kind == (int)CategoryKind.Income);
                if (incomeCat is not null)
                    await sender.Send(new CreateTransactionCommand(
                        DemoUser.Id, result.Value.Id, incomeCat.Id,
                        TransactionType.Income, req.OpeningBalance,
                        DateOnly.FromDateTime(DateTime.UtcNow),
                        "Saldo inicial", req.Currency), ct);
            }

            return result.ToHttp();
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(
                new UpdateAccountCommand(id, DemoUser.Id, req.Name, 0m), ct)).ToHttp());

        group.MapPost("/{id:guid}/primary", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SetPrimaryAccountCommand(id, DemoUser.Id), ct)).ToHttp());

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteAccountCommand(id, DemoUser.Id), ct)).ToHttp());
    }
}
