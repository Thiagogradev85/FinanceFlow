using FinanceFlow.SharedKernel;

namespace FinanceFlow.Api.Common;

/// <summary>Traduz Result/Result&lt;T&gt; do domínio para respostas HTTP. Um lugar só decide o status code.</summary>
public static class ResultExtensions
{
    public static IResult ToHttp<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error!);

    public static IResult ToHttp(this Result result) =>
        result.IsSuccess ? Results.NoContent() : ToProblem(result.Error!);

    private static IResult ToProblem(AppError error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(detail: error.Message, statusCode: status, title: error.Code);
    }
}
