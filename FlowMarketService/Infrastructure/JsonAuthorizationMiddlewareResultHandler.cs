using System.Net.Mime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace FlowMarketService.Infrastructure;

/// <summary>
/// 403 javoblarini bo‘sh o‘rniga JSON (ApiErrorResponse) bilan qaytaradi — frontend integratsiyasi uchun.
/// </summary>
public sealed class JsonAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy? policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        var authenticateResult = await context.AuthenticateAsync();
        if (!authenticateResult.Succeeded)
        {
            await context.ChallengeAsync();
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
            "Ushbu amal uchun ruxsat yo‘q.",
            context.TraceIdentifier,
            "FORBIDDEN"));
    }
}
