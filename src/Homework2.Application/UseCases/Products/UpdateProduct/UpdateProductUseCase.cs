using Homework2.Domain.Common;
using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.UpdateProduct;

public class UpdateProductUseCase(IProductRepository productRepository) : IUpdateProductUseCase
{
    public async Task<Result> ExecuteAsync(
        UpdateProductDto updateProductDto,
        CancellationToken cancellationToken = default)
    {
        if (!ProductId.TryCreate(updateProductDto.ProductId, out var productId))
        {
            return Error.Validation("Product.ProductId", "ProductId cannot be empty.");
        }

        if (!Enum.IsDefined(updateProductDto.Type))
        {
            return Error.Validation("Product.Type", "Invalid product type specified.");
        }

        var product = await productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Error.NotFound("Product.NotFound", $"Product with '{productId}' id not found.");
        }

        var priceResult = ProductPrice.Create(updateProductDto.Price);
        if (priceResult.IsFailure)
        {
            return priceResult.Error;
        }

        var weightResult = ProductWeight.FromGrams(updateProductDto.Weight);
        if (weightResult.IsFailure)
        {
            return weightResult.Error;
        }

        if (!StorageId.TryCreate(updateProductDto.StorageId, out var storageId))
        {
            return Error.Validation("Product.StorageId", "StorageId cannot be empty.");
        }

        var updateResult = product.Update(
            updateProductDto.Title,
            priceResult.Value,
            weightResult.Value,
            updateProductDto.Type,
            storageId);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        await productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }
}
