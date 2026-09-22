using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Application.Families;

public sealed record CreateFamilyRequest(string Name);
public sealed record FamilySearchResult(string PublicId, string Name);
public sealed record MemberDto(Guid UserId, string DisplayName, MembershipStatus Status, bool IsOwner);
public sealed record FamilyDto(Guid Id, string PublicId, string Name, bool IsOwner, MembershipStatus MyStatus);

public class FamilyService(IAppDbContext db)
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public async Task<FamilyDto> CreateAsync(Guid userId, CreateFamilyRequest r, CancellationToken ct)
    {
        if (await db.FamilyMembers.AnyAsync(m => m.UserId == userId, ct))
            throw AppException.Conflict("You already belong to, or have requested, a family.");
        var family = new Family { Name = r.Name.Trim(), OwnerUserId = userId, PublicId = NewPublicId() };
        db.Families.Add(family);
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = userId, Status = MembershipStatus.Active, DecidedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(ct);
        return new FamilyDto(family.Id, family.PublicId, family.Name, true, MembershipStatus.Active);
    }

    public async Task<FamilyDto?> MineAsync(Guid userId, CancellationToken ct) =>
        await (from m in db.FamilyMembers.Where(x => x.UserId == userId)
               join f in db.Families on m.FamilyId equals f.Id
               select new FamilyDto(f.Id, f.PublicId, f.Name, f.OwnerUserId == userId, m.Status)).FirstOrDefaultAsync(ct);

    // Exposes only name + public id; no member data.
    public async Task<FamilySearchResult> SearchAsync(string publicId, CancellationToken ct)
    {
        var id = publicId.Trim().ToUpperInvariant();
        return await db.Families.Where(f => f.PublicId == id).Select(f => new FamilySearchResult(f.PublicId, f.Name)).FirstOrDefaultAsync(ct)
               ?? throw AppException.NotFound("Family");
    }

    public async Task RequestJoinAsync(Guid userId, string publicId, CancellationToken ct)
    {
        var id = publicId.Trim().ToUpperInvariant();
        var family = await db.Families.FirstOrDefaultAsync(f => f.PublicId == id, ct) ?? throw AppException.NotFound("Family");
        if (await db.FamilyMembers.AnyAsync(m => m.UserId == userId, ct))
            throw AppException.Conflict("You already belong to, or have requested, a family.");
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = userId, Status = MembershipStatus.Pending });
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { throw AppException.Conflict("Request already exists."); }
    }

    public async Task<List<MemberDto>> MembersAsync(Guid actor, CancellationToken ct)
    {
        var f = await OwnedFamilyAsync(actor, ct, allowMember: true);
        var isOwner = f.OwnerUserId == actor;
        var q = from m in db.FamilyMembers.Where(x => x.FamilyId == f.Id)
                join u in db.Users on m.UserId equals u.Id
                select new MemberDto(u.Id, u.DisplayName, m.Status, u.Id == f.OwnerUserId);
        if (!isOwner) q = q.Where(m => m.Status == MembershipStatus.Active); // pending list is owner-only
        return await q.ToListAsync(ct);
    }

    public async Task DecideAsync(Guid actor, Guid userId, bool approve, CancellationToken ct)
    {
        var f = await OwnedFamilyAsync(actor, ct);
        var m = await db.FamilyMembers.FirstOrDefaultAsync(x => x.FamilyId == f.Id && x.UserId == userId && x.Status == MembershipStatus.Pending, ct)
                ?? throw AppException.NotFound("Join request");
        if (approve) { m.Status = MembershipStatus.Active; m.DecidedAt = DateTimeOffset.UtcNow; }
        else db.FamilyMembers.Remove(m);
        await db.SaveChangesAsync(ct);
    }

    public async Task KickAsync(Guid actor, Guid userId, CancellationToken ct)
    {
        var f = await OwnedFamilyAsync(actor, ct);
        if (userId == f.OwnerUserId) throw AppException.Invalid("The family owner cannot be removed.");
        var m = await db.FamilyMembers.FirstOrDefaultAsync(x => x.FamilyId == f.Id && x.UserId == userId, ct) ?? throw AppException.NotFound("Member");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var moduleIds = db.Accounts.Where(a => a.FamilyId == f.Id).Select(a => a.Id);
        db.ModulePermissions.RemoveRange(db.ModulePermissions.Where(p => p.UserId == userId && moduleIds.Contains(p.AccountId)));
        db.FamilyMembers.Remove(m);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task<Family> OwnedFamilyAsync(Guid actor, CancellationToken ct, bool allowMember = false)
    {
        var m = await db.FamilyMembers.FirstOrDefaultAsync(x => x.UserId == actor && x.Status == MembershipStatus.Active, ct) ?? throw AppException.Forbidden();
        var f = await db.Families.FirstAsync(x => x.Id == m.FamilyId, ct);
        if (!allowMember && f.OwnerUserId != actor) throw AppException.Forbidden();
        return f;
    }

    private static string NewPublicId() => string.Create(10, 0, (span, _) =>
    { for (var i = 0; i < span.Length; i++) span[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]; });
}
