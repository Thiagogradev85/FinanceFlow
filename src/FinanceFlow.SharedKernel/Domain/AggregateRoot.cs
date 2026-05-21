namespace FinanceFlow.SharedKernel;

/// <summary>
/// Raiz de um agregado: a única porta de entrada para alterar o que está dentro dele.
/// Acumula eventos de domínio que serão publicados após o commit (padrão outbox simplificado).
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }

    protected AggregateRoot(Guid id) : base(id) { }

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
