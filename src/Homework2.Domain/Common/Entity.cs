namespace Homework2.Domain.Common;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : struct, IStronglyTypedId<TId>
{
    public TId Id { get; protected init; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public bool Equals(Entity<TId>? other)
    {
        if (other is null || GetType() != other.GetType())
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (EqualityComparer<TId>.Default.Equals(Id, default) ||
            EqualityComparer<TId>.Default.Equals(other.Id, default))
        {
            return false;
        }

        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}