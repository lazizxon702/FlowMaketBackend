namespace FlowMarket.Application.Contracts;

public sealed record RegisterRequest(string FullName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
