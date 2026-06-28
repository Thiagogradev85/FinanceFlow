using System.Globalization;
using FinanceFlow.Modules.Accounts.Application;
using FinanceFlow.Modules.Transactions.Application;
using FinanceFlow.Modules.Transactions.Application.Categories;
using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Api.Common.Assistant;

/// <summary>Campos crus que o LLM extraiu da frase, antes de resolver contra o domínio.</summary>
internal sealed record ParsedProposal(decimal Valor, string Categoria, string? Data, string? Descricao);

/// <summary>Categorias + conta padrão do usuário, prontas para montar a proposta.</summary>
internal sealed record AssistantContext(IReadOnlyList<CategoryDto> Categories, AccountDto Account);

/// <summary>
/// Lógica de domínio compartilhada entre as implementações do assistente (Copilot, Claude…).
/// Cada provedor só faz a parte de IA (extrair os campos); a resolução vive aqui, num lugar só.
/// </summary>
internal static class AssistantProposalFactory
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    /// <summary>Busca categorias e conta padrão via MediatR (sem o host depender dos módulos).</summary>
    public static async Task<Result<AssistantContext>> LoadContextAsync(ISender sender, Guid userId, CancellationToken ct)
    {
        var categoriesResult = await sender.Send(new ListCategoriesQuery(userId), ct);
        if (categoriesResult.IsFailure) return categoriesResult.Error!;
        var categories = categoriesResult.Value;
        if (categories.Count == 0)
            return AppError.Domain("assistant.no_categories", "Cadastre uma categoria antes de usar o chat.");

        var accountsResult = await sender.Send(new ListAccountsQuery(userId), ct);
        if (accountsResult.IsFailure) return accountsResult.Error!;
        var account = accountsResult.Value.FirstOrDefault(a => a.IsPrimary)
                      ?? accountsResult.Value.FirstOrDefault();
        if (account is null)
            return AppError.Domain("assistant.no_accounts", "Cadastre uma conta antes de usar o chat.");

        return new AssistantContext(categories, account);
    }

    /// <summary>Lista "- Nome (Receita/Despesa)" para injetar na instrução de sistema do LLM.</summary>
    public static string BuildCategoryList(IReadOnlyList<CategoryDto> categories) =>
        string.Join("\n", categories.Select(c =>
            $"- {c.Name} ({(c.Kind == (int)CategoryKind.Income ? "Receita" : "Despesa")})"));

    /// <summary>
    /// Resolve os campos crus contra o domínio: casa a categoria pelo nome exato, deriva o tipo
    /// (Receita/Despesa) do Kind — batendo com a regra category_kind_mismatch do handler — e monta a proposta.
    /// </summary>
    public static Result<AssistantReply> Build(ParsedProposal parsed, AssistantContext ctx, IClock clock)
    {
        var category = ctx.Categories.FirstOrDefault(c =>
            string.Equals(c.Name, parsed.Categoria, StringComparison.OrdinalIgnoreCase));
        if (category is null)
            return AppError.Domain("assistant.category_not_found",
                $"Não encontrei a categoria \"{parsed.Categoria}\". Categorias: {string.Join(", ", ctx.Categories.Select(c => c.Name))}.");

        var occurredOn = DateOnly.TryParse(parsed.Data, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d
            : DateOnly.FromDateTime(clock.UtcNow);
        var type = (TransactionType)category.Kind; // Kind 1=Income/2=Expense ⇒ TransactionType 1/2
        var description = string.IsNullOrWhiteSpace(parsed.Descricao) ? category.Name : parsed.Descricao!.Trim();

        var proposal = new TransactionProposal(
            ctx.Account.Id, ctx.Account.Name, category.Id, category.Name,
            type, parsed.Valor, occurredOn, description);

        var verbo = type == TransactionType.Income ? "Entrada" : "Saída";
        var reply = $"{verbo} de R$ {parsed.Valor.ToString("N2", PtBr)} em {category.Name} " +
                    $"em {occurredOn:dd/MM} na conta {ctx.Account.Name}. Confirmar?";
        return new AssistantReply(reply, proposal);
    }
}
