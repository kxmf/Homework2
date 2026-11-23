using Homework2.Domain.Common;

namespace Homework2.Domain.Product;

public sealed class Product : AggregateRoot<ProductId>
{
    public string Title { get; private set; }
    public ProductPrice Price { get; private set; }
    public ProductWeight Weight { get; private set; }
    public ProductType Type { get; private set; }
    public DateOnly CreatedAt { get; init; }
    public StorageId StorageId { get; private set; }

    private Product(ProductId id, string title, ProductPrice price, ProductWeight weight, ProductType type, StorageId storageId, DateOnly createdAt) : base(id)
    {
        Title = title;
        Price = price;
        Weight = weight;
        Type = type;
        StorageId = storageId;
        CreatedAt = createdAt;
    }

    private Product(ProductId id, string title, ProductPrice price, ProductWeight weight, ProductType type, StorageId storageId)
        : this(id, title, price, weight, type, storageId, DateOnly.FromDateTime(DateTime.UtcNow))
    {
    }

    public static Result<Product> Create(ProductId id,
                                         string title,
                                         ProductPrice price,
                                         ProductWeight weight,
                                         ProductType type,
                                         StorageId storageId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("Product.Title", "Product title cannot be empty.");

        if (storageId.Value == Guid.Empty)
            return Error.Validation("Product.StorageId", "StorageId cannot be empty.");

        return new Product(id, title.Trim(), price, weight, type, storageId);
    }

    public static Product Restore(ProductId id,
                                  string title,
                                  ProductPrice price,
                                  ProductWeight weight,
                                  ProductType type,
                                  DateOnly createdAt,
                                  StorageId storageId)
    {
        return new Product(id, title, price, weight, type, storageId, createdAt);
    }

    public Result Update(string title,
                         ProductPrice price,
                         ProductWeight weight,
                         ProductType type,
                         StorageId storageId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Error.Validation("Product.Title", "Product title cannot be empty.");
        }

        if (storageId.Value == Guid.Empty)
        {
            return Error.Validation("Product.StorageId", "StorageId cannot be empty.");
        }

        Title = title.Trim();
        Price = price;
        Weight = weight;
        Type = type;
        StorageId = storageId;

        return Result.Success();
    }
}
