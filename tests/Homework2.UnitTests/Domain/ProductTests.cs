using FluentAssertions;
using Homework2.Domain.Product;
using Xunit;

namespace Homework2.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnProduct()
    {
        var id = ProductId.Unique();
        var price = ProductPrice.Create(199.99m).Value;
        var weight = ProductWeight.FromGrams(300).Value;
        var storageId = StorageId.Create(Guid.NewGuid());

        var result = Product.Create(id, "Мыло", price, weight, ProductType.Homecare, storageId);

        result.IsSuccess.Should().BeTrue();
        var product = result.Value;
        product.Id.Should().Be(id);
        product.Title.Should().Be("Мыло");
        product.Price.Should().Be(price);
        product.Weight.Should().Be(weight);
        product.Type.Should().Be(ProductType.Homecare);
        product.StorageId.Should().Be(storageId);
        product.CreatedAt.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldFail(string emptyTitle)
    {
        var price = ProductPrice.Create(100m).Value;
        var weight = ProductWeight.FromGrams(100).Value;

        var result = Product.Create(ProductId.Unique(), emptyTitle, price, weight, ProductType.General, StorageId.Create(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Title");
    }

    [Fact]
    public void Create_WithDefaultStorageId_ShouldFail()
    {
        var price = ProductPrice.Create(100m).Value;
        var weight = ProductWeight.FromGrams(100).Value;

        var result = Product.Create(ProductId.Unique(), "Товар", price, weight, ProductType.General, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.StorageId");
    }

    [Fact]
    public void StorageId_Create_WithEmptyGuid_ShouldThrowArgumentException()
    {
        var act = () => StorageId.Create(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Restore_ShouldPreserveAllAttributes()
    {
        var id = ProductId.Unique();
        var price = ProductPrice.Create(500m).Value;
        var weight = ProductWeight.FromGrams(1500).Value;
        var storageId = StorageId.Create(Guid.NewGuid());
        var createdAt = new DateOnly(2025, 1, 15);

        var product = Product.Restore(id, "Чайник", price, weight, ProductType.Electronics, createdAt, storageId);

        product.Id.Should().Be(id);
        product.Title.Should().Be("Чайник");
        product.Price.Should().Be(price);
        product.Weight.Should().Be(weight);
        product.Type.Should().Be(ProductType.Electronics);
        product.CreatedAt.Should().Be(createdAt);
        product.StorageId.Should().Be(storageId);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateState()
    {
        var id = ProductId.Unique();
        var product = Product.Restore(
            id,
            "Старое имя",
            ProductPrice.Create(100m).Value,
            ProductWeight.FromGrams(500).Value,
            ProductType.General,
            new DateOnly(2026, 1, 1),
            StorageId.Create(Guid.NewGuid()));

        var newPrice = ProductPrice.Create(250m).Value;
        var newWeight = ProductWeight.FromGrams(750).Value;
        var newStorageId = StorageId.Create(Guid.NewGuid());

        var updateResult = product.Update("Новое имя", newPrice, newWeight, ProductType.Homecare, newStorageId);

        updateResult.IsSuccess.Should().BeTrue();
        product.Title.Should().Be("Новое имя");
        product.Price.Should().Be(newPrice);
        product.Weight.Should().Be(newWeight);
        product.Type.Should().Be(ProductType.Homecare);
        product.StorageId.Should().Be(newStorageId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyTitle_ShouldFail(string emptyTitle)
    {
        var product = Product.Restore(
            ProductId.Unique(),
            "Имя",
            ProductPrice.Create(100m).Value,
            ProductWeight.FromGrams(500).Value,
            ProductType.General,
            new DateOnly(2026, 1, 1),
            StorageId.Create(Guid.NewGuid()));

        var updateResult = product.Update(
            emptyTitle,
            ProductPrice.Create(100m).Value,
            ProductWeight.FromGrams(500).Value,
            ProductType.General,
            StorageId.Create(Guid.NewGuid()));

        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Code.Should().Be("Product.Title");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void ProductPrice_WithZeroOrNegativeAmount_ShouldFail(decimal amount)
    {
        var result = ProductPrice.Create(amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Price");
    }

    [Fact]
    public void ProductPrice_WithMoreThanTwoDecimals_ShouldFail()
    {
        var result = ProductPrice.Create(19.999m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Price");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void ProductWeight_WithZeroOrNegativeGrams_ShouldFail(int grams)
    {
        var result = ProductWeight.FromGrams(grams);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Weight");
    }

    [Fact]
    public void ProductWeight_FromKilograms_ShouldConvertCorrectly()
    {
        var weight = ProductWeight.FromKilograms(1.25m).Value;

        weight.Grams.Should().Be(1250);
        weight.Kilograms.Should().Be(1.25m);
    }
}
