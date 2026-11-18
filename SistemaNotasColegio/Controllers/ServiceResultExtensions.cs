using Microsoft.AspNetCore.Mvc;
using SistemaNotasColegio.Services.Common;

namespace SistemaNotasColegio.Controllers;

public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        ServiceResult<T> result)
    {
        if (result.Success)
        {
            return controller.Ok(result.Data);
        }

        var body = new { mensaje = result.Error };

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => controller.NotFound(body),
            ServiceErrorType.Conflict => controller.Conflict(body),
            ServiceErrorType.Unauthorized => controller.Unauthorized(body),
            ServiceErrorType.Forbidden => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                body),
            _ => controller.BadRequest(body)
        };
    }
}
