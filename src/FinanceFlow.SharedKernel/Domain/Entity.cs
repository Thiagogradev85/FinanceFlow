namespace FinanceFlow.SharedKernel;

/// <summary>
/// Base de toda entidade de domínio. Identidade é por <see cref="Id"/> (não por valor).
/// Setter é protegido: ninguém de fora troca o Id de uma entidade existente.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity() { }

    protected Entity(Guid id) => Id = id;

    public override bool Equals(object? obj) =>
        obj is Entity other
        && GetType() == other.GetType()
        && Id != default
        && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
