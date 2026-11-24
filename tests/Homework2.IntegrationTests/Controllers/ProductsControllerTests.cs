using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Homework2.Api.Controllers;
using Homework2.Api.Converters;
using Homework2.Application.UseCases.Products.CreateProducts;
using Homework2.Application.UseCases.Products.GetProductById;
using Homework2.Application.UseCases.Products.GetProducts;
using Homework2.Domain.Product;
using Homework2.IntegrationTests.Fixtures;
using Xunit;

namespace Homework2.IntegrationTests.Controllers;

[Collection("Database")]
public class ProductsControllerTests(PostgresDatabaseFixture dbFixture) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new(dbFixture);
    private HttpClient _client = null!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new ProductTypeJsonConverter() }
    };

    public async Task InitializeAsync()
    {
        await dbFixture.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Create_ValidPayload_Returns201AndPersistsData()
    {
        var request = CreateDto("coffee-machine", 45000m, 9000, ProductType.Electronics);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var createdId = await response.Content.ReadFromJsonAsync<ProductIdDto>(JsonOptions);
        createdId.Id.Should().NotBeEmpty();

        var getResponse = await _client.GetAsync($"/api/products/{createdId.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        dto.ProductId.Should().Be(createdId.Id);
        dto.Title.Should().Be("coffee-machine");
        dto.Price.Should().Be(45000m);
        dto.ProductType.Should().Be(ProductType.Electronics);
    }

    [Theory]
    [InlineData(-100, 100)]
    [InlineData(100, -10)]
    public async Task Create_InvalidNumbers_Returns400BadRequest(decimal price, int weight)
    {
        var request = CreateDto(price: price, weight: weight);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_MissingId_Returns404NotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaged_Returns200WithCreatedItems()
    {
        var storageId = Guid.NewGuid();
        await _client.PostAsJsonAsync("/api/products", CreateDto("item-1", 100m, storageId: storageId));
        await _client.PostAsJsonAsync("/api/products", CreateDto("item-2", 200m, storageId: storageId));

        var response = await _client.GetAsync("/api/products?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductsDto>(JsonOptions);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Update_ExistingProduct_UpdatesFieldsAndReturns200()
    {
        var storageId = Guid.NewGuid();
        var createResponse = await _client.PostAsJsonAsync("/api/products", CreateDto("old-item", 150m, storageId: storageId));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductIdDto>(JsonOptions);

        var updateRequest = new UpdateProductApiRequest("new-item", 350m, 450, ProductType.Homecare, storageId);

        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{created.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);

        updated.Title.Should().Be("new-item");
        updated.Price.Should().Be(350m);
        updated.ProductType.Should().Be(ProductType.Homecare);
    }

    [Fact]
    public async Task Update_NonExistentProduct_Returns404NotFound()
    {
        var request = new UpdateProductApiRequest("item", 100m, 100, ProductType.General, Guid.NewGuid());

        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static CreateProductDto CreateDto(
        string title = "item",
        decimal price = 100m,
        int weight = 500,
        ProductType type = ProductType.General,
        Guid? storageId = null) =>
        new(title, price, weight, type, storageId ?? Guid.NewGuid());
}
