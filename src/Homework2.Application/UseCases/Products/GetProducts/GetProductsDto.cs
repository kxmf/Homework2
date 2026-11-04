using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.GetProducts;

public readonly record struct GetProductsDto(DateOnly? CreatedAt = null,
                                             ProductType? Type = null,
                                             Guid? StorageId = null,
                                             int PageNumber = 1,
                                             int PageSize = 10);