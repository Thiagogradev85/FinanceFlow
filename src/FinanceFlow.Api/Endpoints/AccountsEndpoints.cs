using FinanceFlow.Api.Common;
using FinanceFlow.Modules.Accounts.Application;
using FinanceFlow.Modules.Accounts.Domain;
using MediatR;

namespace FinanceFlow.Api.Endpoints;

public static class AccountsEndpoints
{
    public sealed record CreateAccountRequest(string Name, AccountType Type, string Currency = "BRL", decimal OpeningBalance = 0);

    public static void MapAccountsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListAccountsQuery(DemoUser.Id), ct)).ToHttp());

        group.MapPost("/", async (CreateAccountRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(
                new CreateAccountCommand(DemoUser.Id, req.Name, req.Type, req.Currency, req.OpeningBalance), ct)).ToHttp());
    }
}
