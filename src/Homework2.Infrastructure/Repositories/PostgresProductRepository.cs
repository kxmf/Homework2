using System.Data;
using Dapper;
using Homework2.Domain.Product;
using Npgsql;

namespace Homework2.Infrastructure.Repositories;

public sealed class PostgresProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, title, price, weight, type, created_at AS CreatedAt, storage_id AS StorageId
            FROM products
            WHERE id = @Id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { Id = id.Value },
            cancellationToken: cancellationToken);

        var dao = await connection.QuerySingleOrDefaultAsync<ProductDao>(command);

        return dao?.ToDomain();
    }

    public async Task AddAsync(Product aggregate, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO products (id, title, price, weight, type, created_at, storage_id)
            VALUES (@Id, @Title, @Price, @Weight, @Type, @CreatedAt, @StorageId);
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = aggregate.Id.Value,
                Title = aggregate.Title,
                Price = aggregate.Price.Amount,
                Weight = aggregate.Weight.Grams,
                Type = (int)aggregate.Type,
                CreatedAt = aggregate.CreatedAt,
                StorageId = aggregate.StorageId.Value
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task UpdateAsync(Product aggregate, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE products
            SET title = @Title,
                price = @Price,
                weight = @Weight,
                type = @Type,
                storage_id = @StorageId
            WHERE id = @Id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = aggregate.Id.Value,
                Title = aggregate.Title,
                Price = aggregate.Price.Amount,
                Weight = aggregate.Weight.Grams,
                Type = (int)aggregate.Type,
                StorageId = aggregate.StorageId.Value
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task RemoveAsync(Product aggregate, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM products WHERE id = @Id;";

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { Id = aggregate.Id.Value },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        DateOnly? createdAt,
        ProductType? type,
        StorageId? storageId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (createdAt.HasValue)
        {
            whereConditions.Add("created_at = @CreatedAt");
            parameters.Add("CreatedAt", createdAt.Value, DbType.Date);
        }

        if (type.HasValue)
        {
            whereConditions.Add("type = @Type");
            parameters.Add("Type", (int)type.Value, DbType.Int32);
        }

        if (storageId.HasValue)
        {
            whereConditions.Add("storage_id = @StorageId");
            parameters.Add("StorageId", storageId.Value.Value, DbType.Guid);
        }

        var whereClause = whereConditions.Count > 0
            ? "WHERE " + string.Join(" AND ", whereConditions)
            : string.Empty;

        parameters.Add("Offset", (pageNumber - 1) * pageSize, DbType.Int32);
        parameters.Add("Limit", pageSize, DbType.Int32);

        var sql = $"""
            SELECT COUNT(*)
            FROM products
            {whereClause};

            SELECT id, title, price, weight, type, created_at AS CreatedAt, storage_id AS StorageId
            FROM products
            {whereClause}
            ORDER BY created_at DESC, id
            OFFSET @Offset LIMIT @Limit;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalCount = await multi.ReadSingleAsync<int>();
        var daos = await multi.ReadAsync<ProductDao>();

        var items = daos.Select(d => d.ToDomain()).ToList();

        return (items, totalCount);
    }

    private sealed class ProductDao
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public int Weight { get; init; }
        public int Type { get; init; }
        public DateOnly CreatedAt { get; init; }
        public Guid StorageId { get; init; }

        public Product ToDomain()
        {
            return Product.Restore(
                ProductId.Create(Id),
                Title,
                ProductPrice.Create(Price).Value,
                ProductWeight.FromGrams(Weight).Value,
                (ProductType)Type,
                CreatedAt,
                Domain.Product.StorageId.Create(StorageId));
        }
    }
}
