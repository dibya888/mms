using Microsoft.EntityFrameworkCore;
using MoneyApp.Domain;

namespace MoneyApp.Application.Common;

/// Single source of truth for authorization. Every read/write of an account goes through here.
public class AccessPolicy(IAppDbContext db)
{
    public async Task<bool> IsActiveMemberAsync(Guid familyId, Guid userId, CancellationToken ct) =>
        await db.FamilyMembers.AnyAsync(m => m.FamilyId == familyId && m.UserId == userId && m.Status == MembershipStatus.Active, ct);

    /// Effective permissions of a user on an account (View|Take|Deposit|Manage for personal owners).
    public async Task<ModulePermissions> GetAsync(Account a, Guid userId, CancellationToken ct)
    {
        const ModulePermissions All = ModulePermissions.View | ModulePermissions.Take | ModulePermissions.Deposit | ModulePermissions.Manage;
        if (a.Kind != AccountKind.Module) return a.OwnerUserId == userId ? All : ModulePermissions.None;

        var familyId = a.FamilyId!.Value;
        if (!await IsActiveMemberAsync(familyId, userId, ct)) return ModulePermissions.None;
        if (a.OwnerUserId == userId) return All;

        // Module whose owner is no longer an active member (kicked): family owner controls it.
        var ownerActive = await IsActiveMemberAsync(familyId, a.OwnerUserId, ct);
        if (!ownerActive)
        {
            var familyOwner = await db.Families.Where(f => f.Id == familyId).Select(f => f.OwnerUserId).FirstAsync(ct);
            if (familyOwner == userId) return All;
        }
        return await db.ModulePermissions.Where(p => p.AccountId == a.Id && p.UserId == userId)
            .Select(p => p.Permissions).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsEffectiveOwnerAsync(Account a, Guid userId, CancellationToken ct)
    {
        if (a.Kind != AccountKind.Module) return a.OwnerUserId == userId;
        if (!await IsActiveMemberAsync(a.FamilyId!.Value, userId, ct)) return false;
        if (a.OwnerUserId == userId) return true;
        if (await IsActiveMemberAsync(a.FamilyId.Value, a.OwnerUserId, ct)) return false;
        return await db.Families.AnyAsync(f => f.Id == a.FamilyId && f.OwnerUserId == userId, ct);
    }

    public async Task<bool> CanViewAsync(Account a, Guid u, CancellationToken ct) => (await GetAsync(a, u, ct)).HasFlag(ModulePermissions.View);
    public async Task<bool> CanTakeAsync(Account a, Guid u, CancellationToken ct) => (await GetAsync(a, u, ct)).HasFlag(ModulePermissions.Take);
    public async Task<bool> CanDepositAsync(Account a, Guid u, CancellationToken ct) => (await GetAsync(a, u, ct)).HasFlag(ModulePermissions.Deposit);

    /// Normalises so that any grant implies View.
    public static ModulePermissions Normalize(ModulePermissions p) => p == ModulePermissions.None ? p : p | ModulePermissions.View;
}
