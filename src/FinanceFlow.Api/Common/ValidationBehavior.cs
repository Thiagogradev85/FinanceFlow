using FluentValidation;
using MediatR;

namespace FinanceFlow.Api.Common;

/// <summary>
/// Pipeline do MediatR: roda os validators do FluentValidation ANTES de cada handler.
/// Se a entrada for inválida, lança ValidationException (traduzida para HTTP 422 pelo
/// ValidationExceptionHandler). Regras de NEGÓCIO continuam via Result/AppError no domínio —
/// aqui é só validação de ENTRADA, o único lugar onde permitimos exceção (centralizada).
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
