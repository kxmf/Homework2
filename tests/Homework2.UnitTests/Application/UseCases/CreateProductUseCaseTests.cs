using FluentAssertions;
using Homework2.Application.UseCases.Products.CreateProducts;
using Homework2.Domain.Product;
using NSubstitute;
using Xunit;

namespace Homework2.UnitTests.Application.UseCases;

public class CreateProductUseCaseTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly CreateProductUseCase _sut;

    public CreateProductUseCaseTests() => _sut = new(_repository);

    [Fact]
    public async Task Create_ValidDto_SavesToRepositoryAndReturnsId()
    {
        var dto = CreateDto();

        var result = await _sut.ExecuteAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p =>
                p.Title == dto.Title &&
                p.Price.Amount == dto.Price &&
                p.Weight.Grams == dto.Weight &&
                p.Type == dto.Type &&
                p.StorageId.Value == dto.StorageId),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 500, "Product.Price")]
    [InlineData(-10, 500, "Product.Price")]
    [InlineData(100, 0, "Product.Weight")]
    [InlineData(100, -100, "Product.Weight")]
    public async Task Create_InvalidPriceOrWeight_FailsValidation(decimal price, int weight, string expectedError)
    {
        var dto = CreateDto(price: price, weight: weight);

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedError);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_UndefinedType_ReturnsValidationError()
    {
        var dto = CreateDto(type: (ProductType)999);

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Type");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_EmptyStorageId_ReturnsValidationError()
    {
        var dto = CreateDto(storageId: Guid.Empty);

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.StorageId");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyTitle_ReturnsValidationError(string emptyTitle)
    {
        var dto = CreateDto(title: emptyTitle);

        var result = await _sut.ExecuteAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Title");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    private static CreateProductDto CreateDto(
        string title = "laptop",
        decimal price = 1000m,
        int weight = 1500,
        ProductType type = ProductType.Electronics,
        Guid? storageId = null) =>
        new(title, price, weight, type, storageId ?? Guid.NewGuid());
}
