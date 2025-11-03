namespace RiskApp.Application.Auth;

/// <summary>
/// High-level auth surface that controllers depend on. Backed by ASP.NET Identity today,
/// we can swap to a custom DB (Option B) or external IdP (Option C) later by changing DI.
/// </summary>
public interface IAuthProvider
{
    Task<IReadOnlyList<AuthError>?> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
    Task<AuthResult> IssueTokenAsync(TokenRequest req, CancellationToken ct = default);
}
