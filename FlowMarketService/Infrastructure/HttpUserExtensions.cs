using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FlowMarketService.Infrastructure;

public static class HttpUserExtensions
{
    public static Guid? GetUserId(this HttpContext httpContext)
    {
        var sub = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
