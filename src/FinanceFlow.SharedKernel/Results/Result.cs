namespace FinanceFlow.SharedKernel;

/// <summary>
/// Resultado de uma operação que pode falhar — sem usar exceções para fluxo de negócio.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public AppError? Error { get; }

    protected Result(bool isSuccess, AppError? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Um Result de sucesso não pode carregar erro.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Um Result de falha precisa de um erro.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(AppError error) => new(false, error);
}

/// <summary>
/// Result que carrega um valor em caso de sucesso. Tem conversões implícitas:
/// <c>return account;</c> ou <c>return AppError.NotFound(...);</c> bastam no handler.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não é possível ler Value de um Result com falha.");

    private Result(T? value, bool isSuccess, AppError? error) : base(isSuccess, error) => _value = value;

    public static Result<T> Success(T value) => new(value, true, null);
    public static new Result<T> Failure(AppError error) => new(default, false, error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(AppError error) => Failure(error);
}
