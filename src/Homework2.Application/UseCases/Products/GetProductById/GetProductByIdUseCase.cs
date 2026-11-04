using Homework2.Domain.Common;
using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.GetProductById;

public class GetProductByIdUseCase(IProductRepository productRepository) : IGetProductByIdUseCase
{
    public async Task<Result<ProductDto>> ExecuteAsync(
        GetProductByIdDto getProductByIdDto, 
        CancellationToken cancellationToken = default)
    {
        if (!ProductId.TryCreate(getProductByIdDto.ProductId, out var productId))
            return Error.Validation("Product.ProductId", "ProductId cannot be empty.");

        var productResult = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (productResult is null)
            return Error.NotFound("Product.NotFound", $"Product with '{productId}' id not found.");

        return new ProductDto(
            productResult.Id.Value,
            productResult.Title,
            productResult.Price.Amount,
            productResult.Weight.Grams,
            productResult.Type,
            productResult.CreatedAt,
            productResult.StorageId.Value);
    }
}
