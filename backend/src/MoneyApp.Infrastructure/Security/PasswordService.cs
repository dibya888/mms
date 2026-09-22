using Microsoft.AspNetCore.Identity;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Infrastructure.Security;

public class PasswordService : IPasswordService
{
    private static readonly PasswordHasher<User> Hasher = new();
    private static readonly User Dummy = new();
    private static readonly string DummyHash = Hasher.HashPassword(Dummy, Guid.NewGuid().ToString("N"));

    public string Hash(string password) => Hasher.HashPassword(Dummy, password);

    public bool Verify(string? hash, string password)
    {
        var r = Hasher.VerifyHashedPassword(Dummy, hash ?? DummyHash, password);
        return hash is not null && r != PasswordVerificationResult.Failed;
    }
}
