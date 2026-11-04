using Homework2.Domain.Common;

namespace Homework2.Domain.Product;

public readonly record struct StorageId : IStronglyTypedId<Guid, StorageId>
{
    public Guid Value { get; }

    private StorageId(Guid value) => Value = value;

    public static StorageId Create(Guid value)
    {
        if (!TryCreate(value, out var result))
            throw new ArgumentException("StorageId cannot be empty.", nameof(value));

        return result;
    }

    public static bool TryCreate(Guid value, out StorageId result)
    {
        if (value == Guid.Empty)
        {
            result = default;
            return false;
        }

        result = new StorageId(value);
        return true;
    }

    public static StorageId Unique() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
