using FluentAssertions;
using Homework2.Application.UseCases.Products.GetProductById;
using Homework2.Domain.Product;
using NSubstitute;
using Xunit;

namespace Homework2.UnitTests.Application.UseCases;

public class GetProductByIdUseCaseTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly GetProductByIdUseCase _sut;

    public GetProductByIdUseCaseTests() => _sut = new(_repository);

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsMappedDto()
    {
        var product = CreateProduct("headphones", 250m);
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var result = await _sut.ExecuteAsync(new GetProductByIdDto(product.Id.Value));

        result.IsSuccess.Should().BeTrue();
        result.Value.ProductId.Should().Be(product.Id.Value);
        result.Value.Title.Should().Be("headphones");
        result.Value.Price.Should().Be(250m);
        result.Value.ProductType.Should().Be(product.Type);
    }

    [Fact]
    public async Task GetById_NotFoundInRepo_ReturnsNotFoundError()
    {
        var missingId = Guid.NewGuid();
        _repository.GetByIdAsync(Arg.Any<ProductId>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var result = await _sut.ExecuteAsync(new GetProductByIdDto(missingId));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }

    [Fact]
    public async Task GetById_EmptyGuid_FailsValidationWithoutQueryingRepo()
    {
        var result = await _sut.ExecuteAsync(new GetProductByIdDto(Guid.Empty));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.ProductId");
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<ProductId>(), Arg.Any<CancellationToken>());
    }

    private static Product CreateProduct(string title = "item", decimal price = 100m) =>
        Product.Restore(
            ProductId.Unique(),
            title,
            ProductPrice.Create(price).Value,
            ProductWeight.FromGrams(300).Value,
            ProductType.Electronics,
            new DateOnly(2026, 11, 24),
            StorageId.Create(Guid.NewGuid()));
}
