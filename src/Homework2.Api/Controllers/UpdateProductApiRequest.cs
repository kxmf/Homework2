using Homework2.Domain.Product;

namespace Homework2.Api.Controllers;

public sealed record UpdateProductApiRequest(
    string Title,
    decimal Price,
    int Weight,
    ProductType Type,
    Guid StorageId);
