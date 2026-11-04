using Homework2.Domain.Common;
using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Products.CreateProducts;

public class CreateProductUseCase(IProductRepository productRepository) : ICreateProductUseCase
{
    public async Task<Result<ProductIdDto>> ExecuteAsync(
        CreateProductDto createProductDto, 
        CancellationToken cancellationToken = default)
    {
        var priceResult = ProductPrice.Create(createProductDto.Price);
        if (priceResult.IsFailure)
            return priceResult.Error;

        var weightResult = ProductWeight.FromGrams(createProductDto.Weight);
        if (weightResult.IsFailure)
            return weightResult.Error;

        if (!StorageId.TryCreate(createProductDto.StorageId, out var storageId))
            return Error.Validation("Product.StorageId", "StorageId cannot be empty.");

        var productResult = Product.Create(
            ProductId.Unique(),
            createProductDto.Title,
            priceResult.Value,
            weightResult.Value,
            createProductDto.Type,
            storageId);

        if (productResult.IsFailure)
            return productResult.Error;

        await productRepository.AddAsync(productResult.Value, cancellationToken);

        return new ProductIdDto(productResult.Value.Id.Value);
    }
}
