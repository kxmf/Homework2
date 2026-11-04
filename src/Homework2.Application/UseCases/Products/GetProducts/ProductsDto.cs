using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.GetProducts;

public readonly record struct ProductItemDto(Guid ProductId,
                                             string Title,
                                             decimal Price,
                                             int Weight,
                                             ProductType ProductType,
                                             DateOnly CreatedAt,
                                             Guid StorageId);

public readonly record struct ProductsDto(IReadOnlyList<ProductItemDto> Items,
                                          int TotalCount,
                                          int PageNumber,
                                          int PageSize)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}