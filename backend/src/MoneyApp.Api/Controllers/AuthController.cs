using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MoneyApp.Application.Auth;

namespace MoneyApp.Api.Controllers;

[ApiController, Route("api/v1/auth"), AllowAnonymous, EnableRateLimiting("auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")] public async Task<AuthResult> Register(RegisterRequest r, CancellationToken ct) => await auth.RegisterAsync(r, ct);
    [HttpPost("login")] public async Task<AuthResult> Login(LoginRequest r, CancellationToken ct) => await auth.LoginAsync(r, ct);
    [HttpPost("refresh")] public async Task<AuthResult> Refresh(RefreshRequest r, CancellationToken ct) => await auth.RefreshAsync(r, ct);
    [HttpPost("logout")] public async Task<IActionResult> Logout(RefreshRequest r, CancellationToken ct) { await auth.LogoutAsync(r, ct); return NoContent(); }
}
