using Microsoft.EntityFrameworkCore;
using MoneyApp.Domain;
using MoneyApp.Infrastructure.Persistence;

namespace MoneyApp.Tests.Infra;

/// A fresh in-memory AppDbContext per test, seeded with the currencies and categories the tests need.
/// Note: EF's InMemory provider does not enforce FK/check constraints or SELECT ... FOR UPDATE locking,
/// so LedgerService's row-lock statement is exercised against Postgres only (see docs/MIGRATIONS.md) —
/// these tests cover business rules, not database-level integrity.
public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new AppDbContext(options);
        db.Currencies.AddRange(
            new Currency { Code = "BDT", Name = "Bangladeshi Taka", Symbol = "৳", DecimalPlaces = 2 },
            new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 });
        db.ExpenseCategories.Add(new ExpenseCategory { Id = Guid.NewGuid(), Key = "food" });
        db.SaveChanges();
        return db;
    }

    public static User AddUser(this AppDbContext db, string email = "a@example.com")
    {
        var u = new User { Email = email, NormalizedEmail = email.ToUpperInvariant(), DisplayName = email, PasswordHash = "x" };
        db.Users.Add(u); db.SaveChanges();
        return u;
    }

    public static Account AddAccount(this AppDbContext db, AccountKind kind, Guid owner, decimal balance = 0, string currency = "BDT", Guid? familyId = null)
    {
        var a = new Account { Kind = kind, Name = kind.ToString(), OwnerUserId = owner, Balance = balance, CurrencyCode = currency, FamilyId = familyId };
        db.Accounts.Add(a); db.SaveChanges();
        return a;
    }
}
