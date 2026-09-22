using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Application.Auth;

public sealed record RegisterRequest(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, StringLength(128, MinimumLength = 10)] string Password,
    [property: Required, StringLength(100, MinimumLength = 1)] string DisplayName,
    [property: StringLength(10)] string? LanguageCode);
public sealed record LoginRequest(
    [property: Required, StringLength(254)] string Email,
    [property: Required, StringLength(128)] string Password);
public sealed record RefreshRequest([property: Required, StringLength(200)] string RefreshToken);
public sealed record AuthResult(Guid UserId, string DisplayName, string AccessToken, DateTimeOffset AccessExpiresAt, string RefreshToken);

public class AuthService(IAppDbContext db, IPasswordService passwords, ITokenService tokens)
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan LockoutSpan = TimeSpan.FromMinutes(15);

    public async Task<AuthResult> RegisterAsync(RegisterRequest r, CancellationToken ct)
    {
        var norm = Normalize(r.Email);
        if (await db.Users.AnyAsync(u => u.NormalizedEmail == norm, ct))
            throw AppException.Conflict("Email is already registered.");
        var user = new User
        {
            Email = r.Email.Trim(), NormalizedEmail = norm, DisplayName = r.DisplayName.Trim(),
            PasswordHash = passwords.Hash(r.Password), LanguageCode = string.IsNullOrWhiteSpace(r.LanguageCode) ? "en" : r.LanguageCode!
        };
        db.Users.Add(user);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw AppException.Conflict("Email is already registered."); }
        return await IssueAsync(user, Guid.NewGuid(), ct);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest r, CancellationToken ct)
    {
        var norm = Normalize(r.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == norm, ct);
        if (user is null) { passwords.Verify(null, r.Password); throw AppException.Unauthorized(); } // timing equalisation
        if (user.LockoutEnd is { } end && end > DateTimeOffset.UtcNow) throw new AppException(429, "locked", "Account temporarily locked.");
        if (!passwords.Verify(user.PasswordHash, r.Password))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailures) { user.LockoutEnd = DateTimeOffset.UtcNow + LockoutSpan; user.FailedLoginCount = 0; }
            await db.SaveChangesAsync(ct);
            throw AppException.Unauthorized();
        }
        user.FailedLoginCount = 0; user.LockoutEnd = null;
        return await IssueAsync(user, Guid.NewGuid(), ct);
    }

    public async Task<AuthResult> RefreshAsync(RefreshRequest r, CancellationToken ct)
    {
        var hash = tokens.HashRefreshToken(r.RefreshToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct) ?? throw AppException.Unauthorized();
        if (token.RevokedAt is not null)
        {   // reuse of a rotated token: revoke the whole chain
            await RevokeChainAsync(token.FamilyId, ct);
            throw AppException.Unauthorized();
        }
        if (token.ExpiresAt <= DateTimeOffset.UtcNow) throw AppException.Unauthorized();
        token.RevokedAt = DateTimeOffset.UtcNow;
        var user = await db.Users.FindAsync([token.UserId], ct) ?? throw AppException.Unauthorized();
        return await IssueAsync(user, token.FamilyId, ct);
    }

    public async Task LogoutAsync(RefreshRequest r, CancellationToken ct)
    {
        var hash = tokens.HashRefreshToken(r.RefreshToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (token is not null) await RevokeChainAsync(token.FamilyId, ct);
    }

    private async Task RevokeChainAsync(Guid chain, CancellationToken ct)
    {
        var live = await db.RefreshTokens.Where(t => t.FamilyId == chain && t.RevokedAt == null).ToListAsync(ct);
        foreach (var t in live) t.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<AuthResult> IssueAsync(User user, Guid chain, CancellationToken ct)
    {
        var access = tokens.CreateAccessToken(user, out var exp);
        var refresh = tokens.NewRefreshTokenValue();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, FamilyId = chain, TokenHash = tokens.HashRefreshToken(refresh),
            ExpiresAt = DateTimeOffset.UtcNow + tokens.RefreshLifetime
        });
        await db.SaveChangesAsync(ct);
        return new AuthResult(user.Id, user.DisplayName, access, exp, refresh);
    }

    private static string Normalize(string email) => email.Trim().ToUpperInvariant();
}
