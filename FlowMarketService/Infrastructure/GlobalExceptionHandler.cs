using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;

namespace FlowMarketService.Infrastructure;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        var message = environment.IsDevelopment()
            ? exception.Message
            : "Server xatosi. Iltimos, keyinroq urinib ko‘ring.";

        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(message, httpContext.TraceIdentifier, "INTERNAL_ERROR"),
            cancellationToken);

        return true;
    }
}
