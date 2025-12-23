using Microsoft.AspNetCore.Identity;
using RiskApp.Application.Auth;
using RiskApp.Infrastructure.Users;

namespace RiskApp.Infrastructure.Auth;

public class IdentityAuthProvider : IAuthProvider
{
    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signin;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly IJwtTokenService _tokens;

    public IdentityAuthProvider(UserManager<AppUser> users, SignInManager<AppUser> signin, RoleManager<IdentityRole> roles, IJwtTokenService tokens)
    {
        _users = users; _signin = signin; _roles = roles; _tokens = tokens;
    }

    public async Task<IReadOnlyList<AuthError>?> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        // Ensure role exists
        if (!await _roles.RoleExistsAsync(req.Role))
            await _roles.CreateAsync(new IdentityRole(req.Role));

        var user = new AppUser { UserName = req.Email, Email = req.Email, DisplayName = req.DisplayName };
        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return result.Errors.Select(e => new AuthError(e.Code, e.Description)).ToList();

        await _users.AddToRoleAsync(user, req.Role);
        return null; // no errors
    }

    public async Task<AuthResult> IssueTokenAsync(TokenRequest req, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(req.Email);
        if (user is null)
            return new AuthResult(false, null, new[] { new AuthError("user_not_found", "User not found.") });

        var pass = await _signin.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if (!pass.Succeeded)
            return new AuthResult(false, null, new[] { new AuthError("invalid_credentials", "Invalid credentials.") });

        var roles = await _users.GetRolesAsync(user);
        var (token, exp) = _tokens.CreateToken(user, roles);
        return new AuthResult(true, new TokenResponse(token, exp), null);
    }
}
