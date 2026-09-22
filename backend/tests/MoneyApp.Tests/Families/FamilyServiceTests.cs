using Microsoft.EntityFrameworkCore;
using MoneyApp.Application.Common;
using MoneyApp.Application.Families;
using MoneyApp.Domain;
using MoneyApp.Tests.Infra;
using Xunit;

namespace MoneyApp.Tests.Families;

public class FamilyServiceTests
{
    [Fact]
    public async Task CreateMakesTheCreatorAnActiveOwner()
    {
        using var db = TestDb.Create();
        var user = db.AddUser();
        var svc = new FamilyService(db);

        var f = await svc.CreateAsync(user.Id, new CreateFamilyRequest("Our Family"), default);

        Assert.True(f.IsOwner);
        Assert.Equal(MembershipStatus.Active, f.MyStatus);
        Assert.InRange(f.PublicId.Length, 6, 10);
    }

    [Fact]
    public async Task RequestJoinLeavesTheRequesterPendingNotActive()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var applicant = db.AddUser("applicant@example.com");
        var svc = new FamilyService(db);
        var f = await svc.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);

        await svc.RequestJoinAsync(applicant.Id, f.PublicId, default);
        var members = await svc.MembersAsync(owner.Id, default);

        var applicantRow = Assert.Single(members, m => m.UserId == applicant.Id);
        Assert.Equal(MembershipStatus.Pending, applicantRow.Status);
    }

    [Fact]
    public async Task PendingApplicantCannotTakeFamilyActionsUntilApproved()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var applicant = db.AddUser("applicant@example.com");
        var svc = new FamilyService(db);
        var f = await svc.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await svc.RequestJoinAsync(applicant.Id, f.PublicId, default);

        // MembersAsync requires an ACTIVE membership; a pending applicant is forbidden.
        await Assert.ThrowsAsync<AppException>(() => svc.MembersAsync(applicant.Id, default));
    }

    [Fact]
    public async Task ApproveActivatesTheMember()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var applicant = db.AddUser("applicant@example.com");
        var svc = new FamilyService(db);
        var f = await svc.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await svc.RequestJoinAsync(applicant.Id, f.PublicId, default);

        await svc.DecideAsync(owner.Id, applicant.Id, approve: true, default);
        var members = await svc.MembersAsync(applicant.Id, default); // now active, so this must succeed

        Assert.Contains(members, m => m.UserId == applicant.Id && m.Status == MembershipStatus.Active);
    }

    [Fact]
    public async Task RejectRemovesThePendingRequest()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var applicant = db.AddUser("applicant@example.com");
        var svc = new FamilyService(db);
        var f = await svc.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await svc.RequestJoinAsync(applicant.Id, f.PublicId, default);

        await svc.DecideAsync(owner.Id, applicant.Id, approve: false, default);

        Assert.False(await db.FamilyMembers.AnyAsync(m => m.UserId == applicant.Id));
    }

    [Fact]
    public async Task NonOwnerCannotApproveOrKick()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var member = db.AddUser("member@example.com");
        var applicant = db.AddUser("applicant@example.com");
        var svc = new FamilyService(db);
        var f = await svc.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await svc.RequestJoinAsync(member.Id, f.PublicId, default);
        await svc.DecideAsync(owner.Id, member.Id, approve: true, default);
        await svc.RequestJoinAsync(applicant.Id, f.PublicId, default);

        await Assert.ThrowsAsync<AppException>(() => svc.DecideAsync(member.Id, applicant.Id, true, default));
        await Assert.ThrowsAsync<AppException>(() => svc.KickAsync(member.Id, applicant.Id, default));
    }

    [Fact]
    public async Task KickRemovesMembershipAndTheMembersModulePermissions()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var member = db.AddUser("member@example.com");
        var svc = new FamilyService(db);
        var f = await svc.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await svc.RequestJoinAsync(member.Id, f.PublicId, default);
        await svc.DecideAsync(owner.Id, member.Id, true, default);
        var module = db.AddAccount(AccountKind.Module, owner.Id, familyId: f.Id);
        db.ModulePermissions.Add(new AccountModuleAccess { AccountId = module.Id, UserId = member.Id, Permissions = ModulePermissions.View | ModulePermissions.Take });
        db.SaveChanges();

        await svc.KickAsync(owner.Id, member.Id, default);

        Assert.False(await db.FamilyMembers.AnyAsync(m => m.UserId == member.Id));
        Assert.False(await db.ModulePermissions.AnyAsync(p => p.UserId == member.Id));
    }

    [Fact]
    public async Task OwnerCannotBeKicked()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser();
        var svc = new FamilyService(db);
        var f = await svc.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);

        await Assert.ThrowsAsync<AppException>(() => svc.KickAsync(owner.Id, owner.Id, default));
    }

    [Fact]
    public async Task AUserCannotBelongToTwoFamilies()
    {
        using var db = TestDb.Create();
        var user = db.AddUser();
        var other = db.AddUser("other@example.com");
        var svc = new FamilyService(db);
        await svc.CreateAsync(user.Id, new CreateFamilyRequest("F1"), default);
        var f2 = await svc.CreateAsync(other.Id, new CreateFamilyRequest("F2"), default);

        await Assert.ThrowsAsync<AppException>(() => svc.RequestJoinAsync(user.Id, f2.PublicId, default));
    }
}
