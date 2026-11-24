using FluentAssertions;
using Homework2.Domain.Product;
using Homework2.Infrastructure.Repositories;
using Homework2.IntegrationTests.Fixtures;
using Xunit;

namespace Homework2.IntegrationTests.Infrastructure;

[Collection("Database")]
public class PostgresProductRepositoryTests(PostgresDatabaseFixture fixture) : IAsyncLifetime
{
    private readonly PostgresProductRepository _repository = new(fixture.DataSource);

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Add_And_GetById_PersistsAndReadsAllProperties()
    {
        var id = ProductId.Unique();
        var storageId = StorageId.Create(Guid.NewGuid());
        var createdDate = new DateOnly(2026, 11, 24);
        var product = CreateProduct(id, "laptop", 1500m, 2200, ProductType.Electronics, createdDate, storageId);

        await _repository.AddAsync(product);
        var retrieved = await _repository.GetByIdAsync(id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(id);
        retrieved.Title.Should().Be("laptop");
        retrieved.Price.Amount.Should().Be(1500m);
        retrieved.Weight.Grams.Should().Be(2200);
        retrieved.Type.Should().Be(ProductType.Electronics);
        retrieved.CreatedAt.Should().Be(createdDate);
        retrieved.StorageId.Should().Be(storageId);
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNull()
    {
        var missingId = ProductId.Unique();

        var result = await _repository.GetByIdAsync(missingId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_ExistingProduct_UpdatesDatabaseRow()
    {
        var id = ProductId.Unique();
        var originalStorage = StorageId.Create(Guid.NewGuid());
        var product = CreateProduct(id, "old-name", 100m, 500, ProductType.General, new DateOnly(2026, 11, 1), originalStorage);
        await _repository.AddAsync(product);

        var newStorage = StorageId.Create(Guid.NewGuid());
        product.Update("new-name", ProductPrice.Create(299.99m).Value, ProductWeight.FromGrams(1200).Value, ProductType.Homecare, newStorage);

        await _repository.UpdateAsync(product);
        var updated = await _repository.GetByIdAsync(id);

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("new-name");
        updated.Price.Amount.Should().Be(299.99m);
        updated.Weight.Grams.Should().Be(1200);
        updated.Type.Should().Be(ProductType.Homecare);
        updated.StorageId.Should().Be(newStorage);
        updated.CreatedAt.Should().Be(new DateOnly(2026, 11, 1));
    }

    [Fact]
    public async Task Remove_ExistingProduct_DeletesFromDatabase()
    {
        var product = CreateProduct(title: "to-delete");
        await _repository.AddAsync(product);

        await _repository.RemoveAsync(product);
        var retrieved = await _repository.GetByIdAsync(product.Id);

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetPaged_WithoutFilters_PaginatesAndSortsCorrectly()
    {
        var storageId = StorageId.Create(Guid.NewGuid());
        var baseDate = new DateOnly(2026, 11, 1);

        for (var i = 1; i <= 5; i++)
        {
            var product = CreateProduct(
                title: $"item-{i}",
                price: 10m * i,
                weight: 100 * i,
                type: ProductType.General,
                createdAt: baseDate.AddDays(i),
                storageId: storageId);

            await _repository.AddAsync(product);
        }

        var (page1, total1) = await _repository.GetPagedAsync(null, null, null, 1, 2);
        var (page2, total2) = await _repository.GetPagedAsync(null, null, null, 2, 2);
        var (page3, total3) = await _repository.GetPagedAsync(null, null, null, 3, 2);

        total1.Should().Be(5);
        total2.Should().Be(5);
        total3.Should().Be(5);

        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
        page3.Should().HaveCount(1);

        page1[0].CreatedAt.Should().BeOnOrAfter(page1[1].CreatedAt);
        page1[1].CreatedAt.Should().BeOnOrAfter(page2[0].CreatedAt);
    }

    [Fact]
    public async Task GetPaged_FilterByType_ReturnsOnlyMatching()
    {
        var storage = StorageId.Create(Guid.NewGuid());
        var item1 = CreateProduct(title: "tablet", type: ProductType.Electronics, storageId: storage);
        var item2 = CreateProduct(title: "apples", type: ProductType.Groceries, storageId: storage);
        var item3 = CreateProduct(title: "mouse", type: ProductType.Electronics, storageId: storage);

        await _repository.AddAsync(item1);
        await _repository.AddAsync(item2);
        await _repository.AddAsync(item3);

        var (items, total) = await _repository.GetPagedAsync(null, ProductType.Electronics, null, 1, 10);

        total.Should().Be(2);
        items.Should().HaveCount(2).And.OnlyContain(p => p.Type == ProductType.Electronics);
    }

    [Fact]
    public async Task GetPaged_FilterByCreatedAt_ReturnsOnlyMatchingDate()
    {
        var targetDate = new DateOnly(2026, 11, 20);
        var match = CreateProduct(title: "on-date", createdAt: targetDate);
        var other = CreateProduct(title: "other-date", createdAt: new DateOnly(2026, 11, 21));

        await _repository.AddAsync(match);
        await _repository.AddAsync(other);

        var (items, total) = await _repository.GetPagedAsync(targetDate, null, null, 1, 10);

        total.Should().Be(1);
        items.Should().ContainSingle();
        items[0].Id.Should().Be(match.Id);
    }

    [Fact]
    public async Task GetPaged_FilterByStorageId_ReturnsOnlySpecificStorage()
    {
        var targetStorage = StorageId.Create(Guid.NewGuid());
        var otherStorage = StorageId.Create(Guid.NewGuid());

        var match = CreateProduct(title: "store-a", storageId: targetStorage);
        var other = CreateProduct(title: "store-b", storageId: otherStorage);

        await _repository.AddAsync(match);
        await _repository.AddAsync(other);

        var (items, total) = await _repository.GetPagedAsync(null, null, targetStorage, 1, 10);

        total.Should().Be(1);
        items.Should().ContainSingle();
        items[0].StorageId.Should().Be(targetStorage);
    }

    [Fact]
    public async Task GetPaged_CombinedFilter_MatchesAllConditions()
    {
        var targetStorage = StorageId.Create(Guid.NewGuid());
        var targetDate = new DateOnly(2026, 11, 22);

        var match = CreateProduct(title: "target", type: ProductType.Groceries, createdAt: targetDate, storageId: targetStorage);
        var wrongType = CreateProduct(title: "wrong-type", type: ProductType.General, createdAt: targetDate, storageId: targetStorage);

        await _repository.AddAsync(match);
        await _repository.AddAsync(wrongType);

        var (items, total) = await _repository.GetPagedAsync(targetDate, ProductType.Groceries, targetStorage, 1, 10);

        total.Should().Be(1);
        items.Should().ContainSingle();
        items[0].Id.Should().Be(match.Id);
    }

    [Fact]
    public async Task GetPaged_WhenNoMatch_ReturnsEmptyListAndZeroCount()
    {
        var storage = StorageId.Create(Guid.NewGuid());
        await _repository.AddAsync(CreateProduct(storageId: storage));

        var (items, total) = await _repository.GetPagedAsync(null, null, StorageId.Create(Guid.NewGuid()), 1, 10);

        total.Should().Be(0);
        items.Should().BeEmpty();
    }

    private static Product CreateProduct(
        ProductId? id = null,
        string title = "item",
        decimal price = 100m,
        int weight = 500,
        ProductType type = ProductType.General,
        DateOnly? createdAt = null,
        StorageId? storageId = null) =>
        Product.Restore(
            id ?? ProductId.Unique(),
            title,
            ProductPrice.Create(price).Value,
            ProductWeight.FromGrams(weight).Value,
            type,
            createdAt ?? new DateOnly(2026, 11, 24),
            storageId ?? StorageId.Create(Guid.NewGuid()));
}
