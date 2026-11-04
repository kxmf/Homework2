using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.CreateProducts;

public readonly record struct CreateProductDto(string Title,
                                               decimal Price,
                                               int Weight,
                                               ProductType Type,
                                               Guid StorageId)
{
}
