using FluentAssertions;
using Homework2.Application.UseCases.Products.GetProducts;
using Homework2.Domain.Product;
using NSubstitute;
using Xunit;

namespace Homework2.UnitTests.Application.UseCases;

public class GetProductsUseCaseTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly GetProductsUseCase _sut;

    public GetProductsUseCaseTests() => _sut = new(_repository);

    [Fact]
    public async Task GetPaged_ValidPagination_ReturnsDtoWithItems()
    {
        var product = CreateProduct("milk", 89.90m, ProductType.Groceries);
        _repository.GetPagedAsync(null, null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(([product], 1));

        var result = await _sut.ExecuteAsync(new GetProductsDto(null, null, null, 1, 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].Title.Should().Be("milk");
        result.Value.Items[0].ProductType.Should().Be(ProductType.Groceries);
    }

    [Theory]
    [InlineData(0, 10, "Pagination.PageNumber")]
    [InlineData(-1, 10, "Pagination.PageNumber")]
    [InlineData(1, 0, "Pagination.PageSize")]
    [InlineData(1, -5, "Pagination.PageSize")]
    [InlineData(1, 101, "Pagination.PageSize")]
    public async Task GetPaged_InvalidPagingParams_FailsValidation(int pageNumber, int pageSize, string expectedCode)
    {
        var query = new GetProductsDto(null, null, null, pageNumber, pageSize);

        var result = await _sut.ExecuteAsync(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedCode);
        await _repository.DidNotReceive().GetPagedAsync(
            Arg.Any<DateOnly?>(), Arg.Any<ProductType?>(), Arg.Any<StorageId?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPaged_EmptyStorageGuid_FailsValidation()
    {
        var query = new GetProductsDto(null, null, Guid.Empty, 1, 10);

        var result = await _sut.ExecuteAsync(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Filter.StorageId");
    }

    [Fact]
    public async Task GetPaged_WithFilters_PassesAllArgumentsToRepository()
    {
        var date = new DateOnly(2026, 11, 20);
        var type = ProductType.Electronics;
        var storageGuid = Guid.NewGuid();

        _repository.GetPagedAsync(date, type, Arg.Is<StorageId>(s => s.Value == storageGuid), 2, 20, Arg.Any<CancellationToken>())
            .Returns(([], 0));

        var query = new GetProductsDto(date, type, storageGuid, 2, 20);

        var result = await _sut.ExecuteAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Items.Should().BeEmpty();
    }

    private static Product CreateProduct(string title, decimal price, ProductType type) =>
        Product.Restore(
            ProductId.Unique(),
            title,
            ProductPrice.Create(price).Value,
            ProductWeight.FromGrams(500).Value,
            type,
            new DateOnly(2026, 11, 24),
            StorageId.Create(Guid.NewGuid()));
}
