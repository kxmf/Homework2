namespace Homework2.Domain.Common;

public interface IStronglyTypedId<TSelf> : IEquatable<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf>;

public interface IStronglyTypedId<TValue, TSelf> : IStronglyTypedId<TSelf>
    where TValue : notnull, IEquatable<TValue>
    where TSelf : struct, IStronglyTypedId<TValue, TSelf>
{
    TValue Value { get; }

    static abstract TSelf Create(TValue value);

    static abstract bool TryCreate(TValue value, out TSelf result);
}