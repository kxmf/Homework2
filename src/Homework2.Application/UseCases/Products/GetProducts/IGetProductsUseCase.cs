using Homework2.Domain.Common;

namespace Homework2.Application.UseCases.Products.GetProducts;

public interface IGetProductsUseCase
{
    Task<Result<ProductsDto>> ExecuteAsync(GetProductsDto getProductsDto, CancellationToken cancellationToken = default);
}