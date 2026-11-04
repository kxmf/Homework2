using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.GetProductById;

public readonly record struct ProductDto(
    Guid ProductId,
    string Title,
    decimal Price,
    int Weight,
    ProductType ProductType,
    DateOnly CreatedAt,
    Guid StorageId);
