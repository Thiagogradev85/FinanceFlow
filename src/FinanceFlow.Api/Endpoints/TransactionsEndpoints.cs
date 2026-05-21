using FinanceFlow.Api.Common;
using FinanceFlow.Modules.Transactions.Application.Categories;
using FinanceFlow.Modules.Transactions.Application.Transactions;
using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Api.Endpoints;

public static class TransactionsEndpoints
{
    public sealed record CreateCategoryRequest(string Name, CategoryKind Kind, string Color = "#888888", string Icon = "tag");

    public sealed record CreateTransactionRequest(
        Guid AccountId,
        Guid CategoryId,
        TransactionType Type,
        decimal Amount,
        DateOnly OccurredOn,
        string Description,
        string Currency = "BRL");

    public static void MapTransactionsEndpoints(this IEndpointRouteBuilder app)
    {
        var categories = app.MapGroup("/api/categories").WithTags("Categories");

        categories.MapGet("/", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListCategoriesQuery(DemoUser.Id), ct)).ToHttp());

        categories.MapPost("/", async (CreateCategoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(
                new CreateCategoryCommand(DemoUser.Id, req.Name, req.Kind, req.Color, req.Icon), ct)).ToHttp());

        var transactions = app.MapGroup("/api/transactions").WithTags("Transactions");

        transactions.MapGet("/", async (int? year, int? month, IClock clock, ISender sender, CancellationToken ct) =>
        {
            var now = clock.UtcNow;
            return (await sender.Send(
                new ListTransactionsQuery(DemoUser.Id, year ?? now.Year, month ?? now.Month), ct)).ToHttp();
        });

        transactions.MapPost("/", async (CreateTransactionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateTransactionCommand(
                DemoUser.Id, req.AccountId, req.CategoryId, req.Type,
                req.Amount, req.OccurredOn, req.Description, req.Currency), ct)).ToHttp());
    }
}
