namespace FinanceFlow.SharedKernel;

/// <summary>
/// Categoria do erro — o Api traduz cada tipo para um status HTTP.
/// </summary>
public enum ErrorType
{
    Validation = 1,    // 422 — dados de entrada inválidos
    NotFound = 2,      // 404 — entidade não encontrada
    Unauthorized = 3,  // 403 — sem permissão
    Conflict = 4,      // 409 — violação de unicidade
    Domain = 5         // 400 — regra de negócio quebrada
}

/// <summary>
/// Erro de aplicação tipado. Substitui o lançamento de Exception genérica:
/// um Command devolve <see cref="Result"/> com um AppError em vez de "throw".
/// </summary>
public sealed class AppError
{
    public ErrorType Type { get; }
    public string Code { get; }
    public string Message { get; }

    private AppError(ErrorType type, string code, string message)
    {
        Type = type;
        Code = code;
        Message = message;
    }

    public static AppError Validation(string code, string message) => new(ErrorType.Validation, code, message);
    public static AppError NotFound(string code, string message) => new(ErrorType.NotFound, code, message);
    public static AppError Unauthorized(string code, string message) => new(ErrorType.Unauthorized, code, message);
    public static AppError Conflict(string code, string message) => new(ErrorType.Conflict, code, message);
    public static AppError Domain(string code, string message) => new(ErrorType.Domain, code, message);

    public override string ToString() => $"{Type}:{Code} — {Message}";
}
