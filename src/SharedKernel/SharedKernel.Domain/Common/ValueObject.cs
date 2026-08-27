namespace Jacana.SharedKernel.Domain;

/// <summary>
/// Value object base: structural equality over the equality components returned by
/// <see cref="GetEqualityComponents"/>. Two value objects are equal when all
/// components are equal.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => obj is ValueObject vo && Equals(vo);

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(17, (hash, c) => HashCode.Combine(hash, c));
}
