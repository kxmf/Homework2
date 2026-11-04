using System.Collections.Concurrent;
using Homework2.Domain.Product;

namespace Homework2.Infrastructure.Repositories;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<ProductId, Product> _products;

    public InMemoryProductRepository()
    {
        _products = new();
    }

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task AddAsync(Product aggregate, CancellationToken cancellationToken = default)
    {
        _products.TryAdd(aggregate.Id, aggregate);
        return Task.CompletedTask;
    }

    public void Update(Product aggregate) => _products[aggregate.Id] = aggregate;

    public void Remove(Product aggregate) => _products.TryRemove(aggregate.Id, out _);

    public Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        DateOnly? createdAt,
        ProductType? type,
        StorageId? storageId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Product> query = _products.Values;

        if (createdAt.HasValue)
            query = query.Where(p => p.CreatedAt == createdAt.Value);

        if (type.HasValue)
            query = query.Where(p => p.Type == type.Value);

        if (storageId.HasValue)
            query = query.Where(p => p.StorageId == storageId.Value);

        // Сортировка по CreatedAt с детерминированным вторичным ключом Id
        var list = query
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id.Value)
            .ToList();

        var totalCount = list.Count;

        var items = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IReadOnlyList<Product> Items, int TotalCount)>((items, totalCount));
    }
}
