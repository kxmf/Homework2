using Homework2.Domain.Common;

namespace Homework2.Domain.Product;

public readonly record struct ProductId : IStronglyTypedId<Guid, ProductId>
{
    public Guid Value { get; }

    private ProductId(Guid value) => Value = value;

    public static ProductId Create(Guid value)
    {
        if (!TryCreate(value, out var result))
            throw new ArgumentException("ProductId cannot be empty.", nameof(value));

        return result;
    }

    public static bool TryCreate(Guid value, out ProductId result)
    {
        if (value == Guid.Empty)
        {
            result = default;
            return false;
        }

        result = new ProductId(value);
        return true;
    }

    public static ProductId Unique() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}