using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MoneyApp.Domain;

namespace MoneyApp.Application.Common;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<Family> Families { get; }
    DbSet<FamilyMember> FamilyMembers { get; }
    DbSet<Account> Accounts { get; }
    DbSet<AccountModuleAccess> ModulePermissions { get; }
    DbSet<ExpenseCategory> ExpenseCategories { get; }
    DbSet<LedgerTransaction> Transactions { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string? hash, string password); // null hash = timing-equalising dummy check
}

public sealed record TokenPair(string AccessToken, DateTimeOffset AccessExpiresAt, string RefreshToken);

public interface ITokenService
{
    string CreateAccessToken(User user, out DateTimeOffset expiresAt);
    string NewRefreshTokenValue();
    string HashRefreshToken(string value);
    TimeSpan RefreshLifetime { get; }
}

public class AppException(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public static AppException NotFound(string what = "Resource") => new(404, "not_found", $"{what} not found.");
    public static AppException Forbidden() => new(403, "forbidden", "You do not have access.");
    public static AppException Invalid(string msg) => new(400, "invalid", msg);
    public static AppException Conflict(string msg) => new(409, "conflict", msg);
    public static AppException Unauthorized() => new(401, "unauthorized", "Invalid credentials.");
}
