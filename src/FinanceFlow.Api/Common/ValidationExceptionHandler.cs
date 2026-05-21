using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace FinanceFlow.Api.Common;

/// <summary>Converte FluentValidation.ValidationException em uma resposta 422 com os erros por campo.</summary>
public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not ValidationException validationException)
            return false;

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            title = "Falha de validação",
            status = 422,
            errors
        }, ct);

        return true;
    }
}
