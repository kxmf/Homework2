using Homework2.Domain.Common;

namespace Homework2.Application.UseCases.Products.CreateProducts;

public interface ICreateProductUseCase
{
    public Task<Result<ProductIdDto>> ExecuteAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default);
}
