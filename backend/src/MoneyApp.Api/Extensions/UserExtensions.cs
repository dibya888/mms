using System.Security.Claims;
namespace MoneyApp.Api.Extensions;

public static class UserExtensions
{
    public static Guid UserId(this ClaimsPrincipal p) =>
        Guid.TryParse(p.FindFirstValue("sub") ?? p.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedAccessException();
}
