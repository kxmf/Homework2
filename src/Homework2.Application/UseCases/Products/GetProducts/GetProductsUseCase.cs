using Homework2.Domain.Common;
using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.GetProducts;

public class GetProductsUseCase(IProductRepository productRepository) : IGetProductsUseCase
{
    public async Task<Result<ProductsDto>> ExecuteAsync(GetProductsDto getProductsDto,
                                                        CancellationToken cancellationToken = default)
    {
        if (getProductsDto.PageNumber < 1)
            return Error.Validation("Pagination.PageNumber", "PageNumber must be greater than or equal to 1.");

        if (getProductsDto.PageSize is < 1 or > 100)
            return Error.Validation("Pagination.PageSize", "PageSize must be between 1 and 100.");

        StorageId? domainStorageId = null;
        if (getProductsDto.StorageId.HasValue)
        {
            if (!StorageId.TryCreate(getProductsDto.StorageId.Value, out var storageId))
                return Error.Validation("Filter.StorageId", "Invalid StorageId filter value.");

            domainStorageId = storageId;
        }

        var (products, totalCount) = await productRepository.GetPagedAsync(
            getProductsDto.CreatedAt,
            getProductsDto.Type,
            domainStorageId,
            getProductsDto.PageNumber,
            getProductsDto.PageSize,
            cancellationToken);

        var items = products.Select(p => new ProductItemDto(
            p.Id.Value,
            p.Title,
            p.Price.Amount,
            p.Weight.Grams,
            p.Type,
            p.CreatedAt,
            p.StorageId.Value
        )).ToList();

        return new ProductsDto(items, totalCount, getProductsDto.PageNumber, getProductsDto.PageSize);
    }
}