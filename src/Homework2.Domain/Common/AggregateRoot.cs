namespace Homework2.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : struct, IStronglyTypedId<TId>
{
    protected AggregateRoot(TId id) : base(id)
    {
    }
}