using Microsoft.AspNetCore.Mvc;
using RiskApp.Application.Auth;

namespace RiskApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthProvider _auth;
    public AuthController(IAuthProvider auth) => _auth = auth;

    /// <summary>Register a new user and assign a role ("Reader" or "Writer").</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var errors = await _auth.RegisterAsync(req, ct);
        return errors is null ? Ok(new { ok = true }) : BadRequest(new { ok = false, errors });
    }

    /// <summary>Exchange email/password for a JWT access token.</summary>
    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest req, CancellationToken ct)
    {
        var result = await _auth.IssueTokenAsync(req, ct);
        return result.Succeeded ? Ok(result.Token) : Unauthorized(new { errors = result.Errors });
    }
}
