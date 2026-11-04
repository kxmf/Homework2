using Homework2.Domain.Common;

namespace Homework2.Domain.Product;

public interface IProductRepository : IRepository<Product, ProductId>
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(DateOnly? createdAt,
                                                                       ProductType? type,
                                                                       StorageId? storageId,
                                                                       int pageNumber,
                                                                       int pageSize,
                                                                       CancellationToken cancellationToken = default);
}
