using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Application.Ledger;

public sealed record PostTransactionRequest(
    Guid? FromAccountId, Guid? ToAccountId, Guid? CategoryId,
    [param: Range(0.0001, 999999999999.0)] decimal Amount,
    [param: StringLength(500)] string? Note);
public sealed record TransactionDto(Guid Id, TransactionType Type, Guid? FromAccountId, Guid? ToAccountId, Guid? CategoryId,
    decimal Amount, string CurrencyCode, string? Note, Guid CreatedByUserId, DateTimeOffset CreatedAt);

public class LedgerService(IAppDbContext db, AccessPolicy policy)
{
    /// Atomically moves money. Accounts are row-locked in id order (no deadlocks, no double-spend).
    public async Task<TransactionDto> PostAsync(Guid userId, PostTransactionRequest r, CancellationToken ct)
    {
        if (decimal.Round(r.Amount, 4) != r.Amount) throw AppException.Invalid("Amount has too many decimal places.");
        if (r.FromAccountId is null && r.ToAccountId is null) throw AppException.Invalid("A source or destination is required.");
        if (r.FromAccountId == r.ToAccountId) throw AppException.Invalid("Source and destination must differ.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var ids = new[] { r.FromAccountId, r.ToAccountId }.Where(i => i.HasValue).Select(i => i!.Value).Distinct().OrderBy(i => i).ToArray();
        // Parameterised raw SQL (interpolated => DbParameter). Locks rows until commit.
        var locked = await db.Accounts.FromSql($"SELECT * FROM accounts WHERE id = ANY({ids}) ORDER BY id FOR UPDATE").ToListAsync(ct);
        if (locked.Count != ids.Length) throw AppException.NotFound("Account");
        var from = locked.FirstOrDefault(a => a.Id == r.FromAccountId);
        var to = locked.FirstOrDefault(a => a.Id == r.ToAccountId);

        var (type, currency) = await ValidateAsync(userId, from, to, r.CategoryId, ct);
        var amount = r.Amount;

        switch (type)
        {
            case TransactionType.Income: to!.Balance += amount; break;
            case TransactionType.Transfer:
                if (from!.Balance < amount) throw AppException.Invalid("Insufficient balance.");
                from.Balance -= amount; to!.Balance += amount; break;
            case TransactionType.Expense:
                if (from!.Kind == AccountKind.CreditCard) from.Balance += amount;      // increases amount due
                else { if (from.Balance < amount) throw AppException.Invalid("Insufficient balance."); from.Balance -= amount; }
                break;
            case TransactionType.CreditCardPayment:
                if (from!.Balance < amount) throw AppException.Invalid("Insufficient balance.");
                if (to!.Balance < amount) throw AppException.Invalid("Payment exceeds amount due.");
                from.Balance -= amount; to.Balance -= amount; break;
        }

        var t = new LedgerTransaction
        {
            Type = type, FromAccountId = from?.Id, ToAccountId = to?.Id, CategoryId = r.CategoryId, Amount = amount,
            CurrencyCode = currency, Note = r.Note?.Trim(), CreatedByUserId = userId
        };
        db.Transactions.Add(t);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return ToDto(t);
    }

    private async Task<(TransactionType, string)> ValidateAsync(Guid userId, Account? from, Account? to, Guid? categoryId, CancellationToken ct)
    {
        if (from is not null && to is not null && from.CurrencyCode != to.CurrencyCode)
            throw AppException.Invalid("Accounts must use the same currency.");
        var currency = (from ?? to)!.CurrencyCode;
        // Hide accounts the user cannot see behind NotFound.
        if (from is not null && !await policy.CanViewAsync(from, userId, ct)) throw AppException.NotFound("Account");
        if (to is not null && !await policy.CanViewAsync(to, userId, ct)) throw AppException.NotFound("Account");

        static bool IsPool(Account a) => a.Kind is AccountKind.Wallet or AccountKind.Module;
        TransactionType type;
        if (from?.Kind == AccountKind.Source && to is not null && IsPool(to)) type = TransactionType.Income;
        else if (from is not null && to is not null && IsPool(from) && IsPool(to)) type = TransactionType.Transfer;
        else if (from is not null && to is null && (IsPool(from) || from.Kind == AccountKind.CreditCard)) type = TransactionType.Expense;
        else if (from is not null && to?.Kind == AccountKind.CreditCard && IsPool(from)) type = TransactionType.CreditCardPayment;
        else throw AppException.Invalid("Unsupported transaction combination.");

        if (type == TransactionType.Expense)
        {
            if (categoryId is null) throw AppException.Invalid("Expense category is required.");
            var ok = await db.ExpenseCategories.AnyAsync(c => c.Id == categoryId && (c.OwnerUserId == null || c.OwnerUserId == userId), ct);
            if (!ok) throw AppException.Invalid("Unknown category.");
        }
        else if (categoryId is not null) throw AppException.Invalid("Category applies to expenses only.");

        // Debit side authorisation
        if (from is not null)
        {
            var p = await policy.GetAsync(from, userId, ct);
            var needed = from.Kind == AccountKind.Module ? ModulePermissions.Take : ModulePermissions.View;
            if (from.Kind != AccountKind.Module && from.OwnerUserId != userId) throw AppException.Forbidden();
            if (!p.HasFlag(needed)) throw AppException.Forbidden();
        }
        // Credit side authorisation
        if (to is not null)
        {
            var p = await policy.GetAsync(to, userId, ct);
            var needed = to.Kind == AccountKind.Module ? ModulePermissions.Deposit : ModulePermissions.View;
            if (to.Kind != AccountKind.Module && to.OwnerUserId != userId) throw AppException.Forbidden();
            if (!p.HasFlag(needed)) throw AppException.Forbidden();
        }
        return (type, currency);
    }

    public async Task<List<TransactionDto>> HistoryAsync(Guid userId, Guid accountId, int page, int pageSize, CancellationToken ct)
    {
        var a = await db.Accounts.FirstOrDefaultAsync(x => x.Id == accountId, ct) ?? throw AppException.NotFound("Account");
        if (!await policy.CanViewAsync(a, userId, ct)) throw AppException.NotFound("Account");
        pageSize = Math.Clamp(pageSize, 1, 100); page = Math.Max(page, 1);
        return await db.Transactions.Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new TransactionDto(t.Id, t.Type, t.FromAccountId, t.ToAccountId, t.CategoryId, t.Amount, t.CurrencyCode, t.Note, t.CreatedByUserId, t.CreatedAt))
            .ToListAsync(ct);
    }

    private static TransactionDto ToDto(LedgerTransaction t) =>
        new(t.Id, t.Type, t.FromAccountId, t.ToAccountId, t.CategoryId, t.Amount, t.CurrencyCode, t.Note, t.CreatedByUserId, t.CreatedAt);
}


