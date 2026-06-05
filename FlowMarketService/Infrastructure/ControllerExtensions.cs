using FlowMarketService.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Infrastructure;

public static class ControllerExtensions
{
    /// <summary>
    /// Muvaffaqiyatli javoblar o‘zgartirilmaydi (Figma / mobil klientlar bilan moslik).
    /// Xatolarda <see cref="ApiErrorResponse"/> — traceId bilan bir xil format.
    /// </summary>
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        var traceId = controller.HttpContext.TraceIdentifier;

        if (result.Success)
        {
            return result.StatusCode switch
            {
                StatusCodes.Status204NoContent => new NoContentResult(),
                StatusCodes.Status201Created => new ObjectResult(result.Value)
                    { StatusCode = StatusCodes.Status201Created },
                _ => new OkObjectResult(result.Value)
            };
        }

        var body = new ApiErrorResponse(
            result.Error ?? "Xato",
            traceId,
            MapStatusToCode(result.StatusCode));

        return result.StatusCode switch
        {
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(body),
            StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(body),
            StatusCodes.Status403Forbidden => new ObjectResult(body) { StatusCode = StatusCodes.Status403Forbidden },
            StatusCodes.Status404NotFound => new NotFoundObjectResult(body),
            StatusCodes.Status409Conflict => new ConflictObjectResult(body),
            StatusCodes.Status429TooManyRequests => new ObjectResult(body)
                { StatusCode = StatusCodes.Status429TooManyRequests },
            _ => new BadRequestObjectResult(body)
        };
    }

    private static string MapStatusToCode(int status) =>
        status switch
        {
            StatusCodes.Status400BadRequest => "BAD_REQUEST",
            StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
            StatusCodes.Status403Forbidden => "FORBIDDEN",
            StatusCodes.Status404NotFound => "NOT_FOUND",
            StatusCodes.Status409Conflict => "CONFLICT",
            StatusCodes.Status429TooManyRequests => "TOO_MANY_REQUESTS",
            _ => "ERROR"
        };
}
