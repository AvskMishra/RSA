namespace RiskApp.Application.Auth;

public record RegisterRequest(string Email, string Password, string? DisplayName, string Role); // "Reader" or "Writer"
public record TokenRequest(string Email, string Password);
public record TokenResponse(string AccessToken, DateTime ExpiresAtUtc);
public record AuthError(string Code, string Description);
public record AuthResult(bool Succeeded, TokenResponse? Token, IReadOnlyList<AuthError>? Errors);
