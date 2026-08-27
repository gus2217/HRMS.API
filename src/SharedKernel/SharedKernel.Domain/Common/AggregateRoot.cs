namespace Jacana.SharedKernel.Domain;

/// <summary>
/// Aggregate root. Carries a RowVersion for optimistic concurrency and a collection
/// of domain events that the infrastructure flushes into the outbox on commit.
/// Inherits audit + soft-delete support from <see cref="AuditableEntity{TId}"/>.
/// </summary>
public abstract class AggregateRoot<TId> : AuditableEntity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>Optimistic concurrency token (byte[] rowversion / Postgres xmin).</summary>
    public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>Stamps a fresh concurrency token. Called by the persistence layer before save.</summary>
    public void StampRowVersion() => RowVersion = Guid.NewGuid().ToByteArray();
}
