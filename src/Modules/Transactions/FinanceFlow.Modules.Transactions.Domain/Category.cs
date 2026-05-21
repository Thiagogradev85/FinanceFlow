using FinanceFlow.SharedKernel;

namespace FinanceFlow.Modules.Transactions.Domain;

/// <summary>
/// Categoria de uma transação ("Mercado", "Salário"). Tem um Kind (Receita/Despesa):
/// uma categoria de despesa nunca deve classificar uma receita — essa regra é validada
/// na criação da Transaction, não aqui (a entidade só cuida do próprio estado).
/// </summary>
public sealed class Category : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CategoryKind Kind { get; private set; }
    public string Color { get; private set; } = "#888888";
    public string Icon { get; private set; } = "tag";
    public bool IsSystem { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    // Construtor sem parâmetros e privado: é só para o EF Core materializar do banco.
    private Category() { }

    public static Result<Category> Create(
        Guid userId,
        string name,
        CategoryKind kind,
        string color = "#888888",
        string icon = "tag")
    {
        if (string.IsNullOrWhiteSpace(name))
            return AppError.Validation("category.name_required", "Nome da categoria é obrigatório.");

        var now = DateTime.UtcNow;
        return new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Kind = kind,
            Color = color,
            Icon = icon,
            IsSystem = false,
            IsArchived = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da categoria é obrigatório.", nameof(name));

        Name = name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Update(string name, string color, string icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da categoria é obrigatório.", nameof(name));

        Name = name.Trim();
        Color = color;
        Icon = icon;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
