namespace Homework2.Domain.Common;

public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : struct, IStronglyTypedId<TId>
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    Task RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}