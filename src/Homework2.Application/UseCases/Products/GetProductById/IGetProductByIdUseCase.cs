using Homework2.Domain.Common;

namespace Homework2.Application.UseCases.Products.GetProductById;

public interface IGetProductByIdUseCase
{
    public Task<Result<ProductDto>> ExecuteAsync(GetProductByIdDto getProductByIdDto, CancellationToken cancellationToken = default);
}
