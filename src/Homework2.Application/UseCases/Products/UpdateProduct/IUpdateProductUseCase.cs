using Homework2.Domain.Common;

namespace Homework2.Application.UseCases.Products.UpdateProduct;

public interface IUpdateProductUseCase
{
    Task<Result> ExecuteAsync(
        UpdateProductDto updateProductDto, 
        CancellationToken cancellationToken = default);
}