using Homework2.Api.Extensions;
using Homework2.Application.UseCases.Products.CreateProducts;
using Homework2.Application.UseCases.Products.GetProductById;
using Homework2.Application.UseCases.Products.GetProducts;
using Homework2.Domain.Product;
using Microsoft.AspNetCore.Mvc;

namespace Homework2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Products — REST")]
public class ProductsController(
    ICreateProductUseCase createProductUseCase,
    IGetProductByIdUseCase getProductByIdUseCase,
    IGetProductsUseCase getProductsUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProductIdDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        var result = await createProductUseCase.ExecuteAsync(dto, cancellationToken);

        return result.ToActionResult(this, idDto =>
            CreatedAtAction(nameof(GetProductById), new { id = idDto.Id }, idDto));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await getProductByIdUseCase.ExecuteAsync(
            new GetProductByIdDto(id),
            cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProductsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] DateOnly? createdAt,
        [FromQuery] ProductType? type,
        [FromQuery] Guid? storageId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsDto(createdAt, type, storageId, pageNumber, pageSize);
        var result = await getProductsUseCase.ExecuteAsync(query, cancellationToken);

        return result.ToActionResult(this);
    }
}
