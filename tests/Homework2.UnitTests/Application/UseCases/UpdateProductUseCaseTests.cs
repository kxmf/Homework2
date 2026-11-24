using FluentAssertions;
using Homework2.Application.UseCases.Products.UpdateProduct;
using Homework2.Domain.Product;
using NSubstitute;
using Xunit;

namespace Homework2.UnitTests.Application.UseCases;

public class UpdateProductUseCaseTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly UpdateProductUseCase _sut;

    public UpdateProductUseCaseTests() => _sut = new(_repository);

    [Fact]
    public async Task Update_ExistingProduct_UpdatesStateAndPersists()
    {
        var product = CreateProduct("old-title", 100m);
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var newStorageId = Guid.NewGuid();
        var dto = new UpdateProductDto(product.Id.Value, "new-title", 200m, 750, ProductType.Homecare, newStorageId);

        var result = await _sut.ExecuteAsync(dto);

        result.IsSuccess.Should().BeTrue();
        product.Title.Should().Be("new-title");
        product.Price.Amount.Should().Be(200m);
        product.Weight.Grams.Should().Be(750);
        product.Type.Should().Be(ProductType.Homecare);
        product.StorageId.Value.Should().Be(newStorageId);

        await _repository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_EmptyProductId_FailsValidation()
    {
        var dto = new UpdateProductDto(Guid.Empty, "title", 100m, 100, ProductType.General, Guid.NewGuid());

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.ProductId");
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<ProductId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_NonExistentProduct_ReturnsNotFound()
    {
        var missingId = Guid.NewGuid();
        _repository.GetByIdAsync(Arg.Any<ProductId>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var dto = new UpdateProductDto(missingId, "title", 100m, 100, ProductType.General, Guid.NewGuid());

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task Update_InvalidPrice_FailsValidation(decimal invalidPrice)
    {
        var product = CreateProduct();
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var dto = new UpdateProductDto(product.Id.Value, "title", invalidPrice, 100, ProductType.General, Guid.NewGuid());

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Price");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_EmptyStorageId_FailsValidation()
    {
        var product = CreateProduct();
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var dto = new UpdateProductDto(product.Id.Value, "title", 100m, 100, ProductType.General, Guid.Empty);

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.StorageId");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    private static Product CreateProduct(string title = "item", decimal price = 100m) =>
        Product.Restore(
            ProductId.Unique(),
            title,
            ProductPrice.Create(price).Value,
            ProductWeight.FromGrams(500).Value,
            ProductType.General,
            new DateOnly(2026, 11, 1),
            StorageId.Create(Guid.NewGuid()));
}
