using Homework2.Api.Extensions;
using Homework2.Application.UseCases.Storages.CreateStorage;
using Microsoft.AspNetCore.Mvc;

namespace Homework2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Storages — REST")]
public class StoragesController(ICreateStorageUseCase createStorageUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(StorageIdDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStorage(
        [FromBody] CreateStorageDto dto,
        CancellationToken cancellationToken)
    {
        var result = await createStorageUseCase.ExecuteAsync(dto, cancellationToken);

        return result.ToActionResult(this, idDto =>
            StatusCode(StatusCodes.Status201Created, idDto));
    }
}
