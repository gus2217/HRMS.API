namespace Jacana.SharedKernel.Domain;

/// <summary>Marker contract for soft-delete support (EF global query filter target).</summary>
public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTime? DeletedAtUtc { get; }
    Guid? DeletedByUserId { get; }
}

/// <summary>Audit-stamping contract. Values are written by the persistence interceptor only.</summary>
public interface IAuditable
{
    DateTime CreatedAtUtc { get; }
    Guid CreatedByUserId { get; }
    DateTime? ModifiedAtUtc { get; }
    Guid? ModifiedByUserId { get; }
}

/// <summary>
/// Audit + soft-delete base for persistent entities. Setters are private — the
/// SaveChanges interceptor stamps these through EF's change tracker, never app code.
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable, ISoftDelete
    where TId : notnull
{
    protected AuditableEntity() { }
    protected AuditableEntity(TId id) : base(id) { }

    public DateTime CreatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public Guid? ModifiedByUserId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public Guid? DeletedByUserId { get; private set; }
}
