using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Infrastructure.Security;

public class JwtOptions
{
    public const string Section = "Jwt";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string SigningKey { get; set; } = "";          // >= 32 bytes, from env / secret store only
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 30;
}

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _o = options.Value;
    public TimeSpan RefreshLifetime => TimeSpan.FromDays(_o.RefreshDays);

    public string CreateAccessToken(User user, out DateTimeOffset expiresAt)
    {
        expiresAt = DateTimeOffset.UtcNow.AddMinutes(_o.AccessMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _o.Issuer, Audience = _o.Audience, Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity([new Claim("sub", user.Id.ToString())]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.SigningKey)), SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public string NewRefreshTokenValue() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    public string HashRefreshToken(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
