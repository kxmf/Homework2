using Homework2.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Homework2.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess != null ? onSuccess(result.Value) : controller.Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => controller.NotFound(new { error = result.Error.Message, code = result.Error.Code }),
            ErrorType.Validation => controller.BadRequest(new { error = result.Error.Message, code = result.Error.Code }),
            ErrorType.Conflict => controller.Conflict(new { error = result.Error.Message, code = result.Error.Code }),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.Message, code = result.Error.Code })
        };
    }
}
