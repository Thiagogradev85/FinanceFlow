namespace FinanceFlow.Api.Common;

/// <summary>
/// Proteção mínima para o MVP: rejeita requests sem X-Api-Key válido.
/// Ativado apenas quando API_KEY está configurado (produção).
/// Em desenvolvimento (sem a var), todos os requests passam.
/// </summary>
public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
{
    private const string HeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext ctx)
    {
        var expected = config["API_KEY"];

        // Sem API_KEY configurado (dev local) → libera tudo
        if (string.IsNullOrWhiteSpace(expected))
        {
            await next(ctx);
            return;
        }

        // Só a API exige chave. UI estática (/, /assets/*, manifest, sw.js), o SPA
        // fallback e o /health passam livres — senão o navegador nem carregaria a página.
        if (!ctx.Request.Path.StartsWithSegments("/api"))
        {
            await next(ctx);
            return;
        }

        if (!ctx.Request.Headers.TryGetValue(HeaderName, out var received) || received != expected)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { error = "API key inválida ou ausente." });
            return;
        }

        await next(ctx);
    }
}
