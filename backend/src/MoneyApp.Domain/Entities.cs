namespace MoneyApp.Domain;

public enum AccountKind { Wallet = 1, Module = 2, Source = 3, CreditCard = 4 }
public enum MembershipStatus { Pending = 1, Active = 2 }
public enum TransactionType { Income = 1, Transfer = 2, Expense = 3, CreditCardPayment = 4 }

[Flags]
public enum ModulePermissions { None = 0, View = 1, Take = 2, Deposit = 4, Manage = 8 }

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string NormalizedEmail { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string LanguageCode { get; set; } = "en";
    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; }          // token-rotation chain id, for reuse detection
    public string TokenHash { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Currency
{
    public string Code { get; set; } = "";     // ISO 4217
    public string Name { get; set; } = "";
    public string Symbol { get; set; } = "";
    public int DecimalPlaces { get; set; } = 2;
}

public class Family
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PublicId { get; set; } = "";  // unique, searchable ID
    public string Name { get; set; } = "";
    public Guid OwnerUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class FamilyMember
{
    public Guid FamilyId { get; set; }
    public Guid UserId { get; set; }
    public MembershipStatus Status { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DecidedAt { get; set; }
}

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AccountKind Kind { get; set; }
    public string Name { get; set; } = "";      // user-defined, any language
    public string CurrencyCode { get; set; } = "";
    public decimal Balance { get; set; }        // CreditCard: outstanding due
    public Guid OwnerUserId { get; set; }
    public Guid? FamilyId { get; set; }         // set only for Module
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class AccountModuleAccess
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public ModulePermissions Permissions { get; set; }
}

public class ExpenseCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Key { get; set; }            // predefined: i18n key
    public string? Name { get; set; }           // custom: user text
    public Guid? OwnerUserId { get; set; }      // null = predefined
}

public class LedgerTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TransactionType Type { get; set; }
    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "";
    public string? Note { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
