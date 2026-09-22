using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Application.Accounts;

public sealed record CreateAccountRequest(
    [param: Required] AccountKind Kind,
    [param: Required, StringLength(100, MinimumLength = 1)] string Name,
    [param: Required, StringLength(3, MinimumLength = 3)] string CurrencyCode);
public sealed record RenameRequest([property: Required, StringLength(100, MinimumLength = 1)] string Name);
public sealed record SetPermissionsRequest([param: Required] ModulePermissions Permissions);
public sealed record AccountDto(Guid Id, AccountKind Kind, string Name, string CurrencyCode, decimal Balance,
    Guid OwnerUserId, ModulePermissions MyPermissions);
public sealed record PermissionDto(Guid UserId, string DisplayName, ModulePermissions Permissions);

public class AccountService(IAppDbContext db, AccessPolicy policy)
{
    public async Task<AccountDto> CreateAsync(Guid userId, CreateAccountRequest r, CancellationToken ct)
    {
        var currencyCode = r.CurrencyCode.Trim().ToUpperInvariant();

        if (!await db.Currencies.AnyAsync(c => c.Code == currencyCode, ct))
            throw AppException.Invalid("Unsupported currency.");
        Guid? familyId = null;
        if (r.Kind == AccountKind.Module)
        {
            familyId = await db.FamilyMembers.Where(m => m.UserId == userId && m.Status == MembershipStatus.Active)
                .Select(m => (Guid?)m.FamilyId).FirstOrDefaultAsync(ct) ?? throw AppException.Invalid("Join a family first.");
        }
        var a = new Account { Kind = r.Kind, Name = r.Name.Trim(), CurrencyCode = currencyCode, OwnerUserId = userId, FamilyId = familyId };
        db.Accounts.Add(a);
        await db.SaveChangesAsync(ct);
        return ToDto(a, ModulePermissions.View | ModulePermissions.Take | ModulePermissions.Deposit | ModulePermissions.Manage);
    }

    /// Returns only accounts the user can View. Inaccessible modules never appear.
    public async Task<List<AccountDto>> ListAsync(Guid userId, CancellationToken ct)
    {
        var familyId = await db.FamilyMembers.Where(m => m.UserId == userId && m.Status == MembershipStatus.Active)
            .Select(m => (Guid?)m.FamilyId).FirstOrDefaultAsync(ct);
        var candidates = await db.Accounts.Where(a => !a.IsArchived &&
            ((a.Kind != AccountKind.Module && a.OwnerUserId == userId) || (familyId != null && a.Kind == AccountKind.Module && a.FamilyId == familyId)))
            .ToListAsync(ct);
        var result = new List<AccountDto>();
        foreach (var a in candidates)
        {
            var p = await policy.GetAsync(a, userId, ct);
            if (p.HasFlag(ModulePermissions.View)) result.Add(ToDto(a, p));
        }
        return result;
    }

    public async Task<AccountDto> RenameAsync(Guid userId, Guid id, RenameRequest r, CancellationToken ct)
    {
        var a = await LoadAsync(id, ct);
        var p = await policy.GetAsync(a, userId, ct);
        if (!p.HasFlag(ModulePermissions.Manage)) throw AppException.NotFound("Account"); // no existence leak
        a.Name = r.Name.Trim();
        await db.SaveChangesAsync(ct);
        return ToDto(a, p);
    }

    public async Task<List<PermissionDto>> GetPermissionsAsync(Guid userId, Guid moduleId, CancellationToken ct)
    {
        var a = await LoadModuleForOwnerAsync(userId, moduleId, ct);
        return await (from p in db.ModulePermissions.Where(x => x.AccountId == a.Id)
                      join u in db.Users on p.UserId equals u.Id
                      select new PermissionDto(u.Id, u.DisplayName, p.Permissions)).ToListAsync(ct);
    }

    public async Task SetPermissionsAsync(Guid actor, Guid moduleId, Guid targetUserId, SetPermissionsRequest r, CancellationToken ct)
    {
        var a = await LoadModuleForOwnerAsync(actor, moduleId, ct);
        if (targetUserId == a.OwnerUserId) throw AppException.Invalid("The owner's access cannot be changed.");
        if (!await policy.IsActiveMemberAsync(a.FamilyId!.Value, targetUserId, ct)) throw AppException.Invalid("User is not an active family member.");
        var perms = AccessPolicy.Normalize(r.Permissions);
        var existing = await db.ModulePermissions.FirstOrDefaultAsync(p => p.AccountId == a.Id && p.UserId == targetUserId, ct);
        if (perms == ModulePermissions.None) { if (existing is not null) db.ModulePermissions.Remove(existing); }
        else if (existing is null) db.ModulePermissions.Add(new AccountModuleAccess { AccountId = a.Id, UserId = targetUserId, Permissions = perms });
        else existing.Permissions = perms;
        await db.SaveChangesAsync(ct);
    }

    private async Task<Account> LoadAsync(Guid id, CancellationToken ct) =>
        await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct) ?? throw AppException.NotFound("Account");

    private async Task<Account> LoadModuleForOwnerAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var a = await LoadAsync(id, ct);
        if (a.Kind != AccountKind.Module || !await policy.IsEffectiveOwnerAsync(a, userId, ct)) throw AppException.NotFound("Module");
        return a;
    }

    private static AccountDto ToDto(Account a, ModulePermissions p) => new(a.Id, a.Kind, a.Name, a.CurrencyCode, a.Balance, a.OwnerUserId, p);
}
