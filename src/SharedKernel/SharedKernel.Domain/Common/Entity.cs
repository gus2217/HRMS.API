namespace Jacana.SharedKernel.Domain;

/// <summary>
/// Base for all entities. Identity is defined by the Id; two entities are equal
/// if they share the same Id and the same type. Never use reference equality.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity() { }

    protected Entity(TId id) => Id = id;

    public TId Id { get; protected set; } = default!;

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) => obj is Entity<TId> e && Equals(e);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
