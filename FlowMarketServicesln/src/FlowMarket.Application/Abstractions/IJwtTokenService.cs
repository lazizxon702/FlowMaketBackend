using FlowMarket.Domain.Entities;

namespace FlowMarket.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
