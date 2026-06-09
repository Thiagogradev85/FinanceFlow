using FinanceFlow.Api.Common;
using FinanceFlow.Api.Common.Assistant;

namespace FinanceFlow.Api.Endpoints;

public static class AssistantEndpoints
{
    public sealed record InterpretRequest(string Message);

    public static void MapAssistantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assistant").WithTags("Assistant");

        // Interpreta a frase e devolve uma PROPOSTA (não grava). O front confirma e chama POST /api/transactions.
        group.MapPost("/interpret", async (InterpretRequest req, IFinancialAssistant assistant, CancellationToken ct) =>
            (await assistant.InterpretAsync(req.Message ?? "", DemoUser.Id, ct)).ToHttp());
    }
}
