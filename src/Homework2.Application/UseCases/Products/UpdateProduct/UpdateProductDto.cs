using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.UpdateProduct;

public readonly record struct UpdateProductDto(
    Guid ProductId,
    string Title,
    decimal Price,
    int Weight,
    ProductType Type,
    Guid StorageId);