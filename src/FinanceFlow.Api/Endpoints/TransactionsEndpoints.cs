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
    public sealed record UpdateCategoryRequest(string Name, string Color = "#888888", string Icon = "tag");

    public sealed record CreateTransactionRequest(
        Guid AccountId,
        Guid CategoryId,
        TransactionType Type,
        decimal Amount,
        DateOnly OccurredOn,
        string Description,
        string Currency = "BRL");

    public sealed record UpdateTransactionRequest(
        Guid AccountId,
        Guid CategoryId,
        TransactionType Type,
        decimal Amount,
        DateOnly OccurredOn,
        string Description,
        string Currency = "BRL");

    public sealed record CreateInstallmentPurchaseRequest(
        Guid AccountId,
        Guid CategoryId,
        decimal InstallmentAmount,
        int InstallmentCount,
        DateOnly FirstOccurredOn,
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

        categories.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(
                new UpdateCategoryCommand(id, DemoUser.Id, req.Name, req.Color, req.Icon), ct)).ToHttp());

        categories.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteCategoryCommand(id, DemoUser.Id), ct)).ToHttp());

        var transactions = app.MapGroup("/api/transactions").WithTags("Transactions");

        // Sem year/month: últimas transações de todos os meses (pra ver e corrigir lançamentos).
        transactions.MapGet("/", async (int? year, int? month, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListTransactionsQuery(DemoUser.Id, year, month), ct)).ToHttp());

        transactions.MapPost("/", async (CreateTransactionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateTransactionCommand(
                DemoUser.Id, req.AccountId, req.CategoryId, req.Type,
                req.Amount, req.OccurredOn, req.Description, req.Currency), ct)).ToHttp());

        // Compra parcelada: gera N despesas mensais (uma por mês) numa só operação.
        transactions.MapPost("/installment", async (CreateInstallmentPurchaseRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateInstallmentPurchaseCommand(
                DemoUser.Id, req.AccountId, req.CategoryId,
                req.InstallmentAmount, req.InstallmentCount, req.FirstOccurredOn, req.Description, req.Currency), ct)).ToHttp());

        // "Comprometido": total parcelado já agendado para os próximos meses.
        transactions.MapGet("/commitments", async (int? months, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetUpcomingCommitmentsQuery(DemoUser.Id, months ?? 6), ct)).ToHttp());

        transactions.MapPut("/{id:guid}", async (Guid id, UpdateTransactionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateTransactionCommand(
                id, DemoUser.Id, req.AccountId, req.CategoryId, req.Type,
                req.Amount, req.OccurredOn, req.Description, req.Currency), ct)).ToHttp());

        transactions.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteTransactionCommand(id, DemoUser.Id), ct)).ToHttp());
    }
}
