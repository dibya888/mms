using MoneyApp.Application.Common;
using MoneyApp.Domain;
using MoneyApp.Tests.Infra;
using Xunit;

namespace MoneyApp.Tests.Accounts;

public class AccessPolicyTests
{
    [Fact]
    public async Task PersonalWalletOwnerHasAllPermissions()
    {
        using var db = TestDb.Create();
        var user = db.AddUser();
        var wallet = db.AddAccount(AccountKind.Wallet, user.Id);
        var policy = new AccessPolicy(db);

        var perms = await policy.GetAsync(wallet, user.Id, default);

        Assert.True(perms.HasFlag(ModulePermissions.View));
        Assert.True(perms.HasFlag(ModulePermissions.Take));
        Assert.True(perms.HasFlag(ModulePermissions.Deposit));
        Assert.True(perms.HasFlag(ModulePermissions.Manage));
    }

    [Fact]
    public async Task StrangerHasNoPermissionsOnSomeoneElsesWallet()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var stranger = db.AddUser("stranger@example.com");
        var wallet = db.AddAccount(AccountKind.Wallet, owner.Id);
        var policy = new AccessPolicy(db);

        var perms = await policy.GetAsync(wallet, stranger.Id, default);

        Assert.Equal(ModulePermissions.None, perms);
    }

    [Fact]
    public async Task ModuleInvisibleToFamilyMemberWithNoGrant()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var member = db.AddUser("member@example.com");
        var family = new Family { Name = "F", OwnerUserId = owner.Id, PublicId = "ABC123" };
        db.Families.Add(family);
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = owner.Id, Status = MembershipStatus.Active });
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = member.Id, Status = MembershipStatus.Active });
        var module = db.AddAccount(AccountKind.Module, owner.Id, familyId: family.Id);
        db.SaveChanges();
        var policy = new AccessPolicy(db);

        var perms = await policy.GetAsync(module, member.Id, default);
        var canView = await policy.CanViewAsync(module, member.Id, default);

        Assert.Equal(ModulePermissions.None, perms);
        Assert.False(canView);
    }

    [Fact]
    public async Task ModuleVisibleOnceTakeIsGrantedButDepositIsNot()
    {
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var member = db.AddUser("member@example.com");
        var family = new Family { Name = "F", OwnerUserId = owner.Id, PublicId = "ABC124" };
        db.Families.Add(family);
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = owner.Id, Status = MembershipStatus.Active });
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = member.Id, Status = MembershipStatus.Active });
        var module = db.AddAccount(AccountKind.Module, owner.Id, familyId: family.Id);
        db.ModulePermissions.Add(new AccountModuleAccess { AccountId = module.Id, UserId = member.Id, Permissions = ModulePermissions.View | ModulePermissions.Take });
        db.SaveChanges();
        var policy = new AccessPolicy(db);

        Assert.True(await policy.CanTakeAsync(module, member.Id, default));
        Assert.False(await policy.CanDepositAsync(module, member.Id, default));
    }

    [Fact]
    public async Task KickedMembersGrantsNoLongerApplyEvenIfRowRemains()
    {
        // Simulates FamilyService.KickAsync having removed the FamilyMember row already;
        // AccessPolicy must treat a non-active membership as no access.
        using var db = TestDb.Create();
        var owner = db.AddUser("owner@example.com");
        var kicked = db.AddUser("kicked@example.com");
        var family = new Family { Name = "F", OwnerUserId = owner.Id, PublicId = "ABC125" };
        db.Families.Add(family);
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = owner.Id, Status = MembershipStatus.Active });
        // kicked user has no FamilyMember row at all (as FamilyService.KickAsync leaves it)
        var module = db.AddAccount(AccountKind.Module, owner.Id, familyId: family.Id);
        db.ModulePermissions.Add(new AccountModuleAccess { AccountId = module.Id, UserId = kicked.Id, Permissions = ModulePermissions.View | ModulePermissions.Take });
        db.SaveChanges();
        var policy = new AccessPolicy(db);

        var perms = await policy.GetAsync(module, kicked.Id, default);

        Assert.Equal(ModulePermissions.None, perms);
    }

    [Fact]
    public async Task FamilyOwnerControlsModuleWhoseOwnerWasKicked()
    {
        using var db = TestDb.Create();
        var famOwner = db.AddUser("famowner@example.com");
        var moduleOwner = db.AddUser("modowner@example.com"); // kicked: no FamilyMember row
        var family = new Family { Name = "F", OwnerUserId = famOwner.Id, PublicId = "ABC126" };
        db.Families.Add(family);
        db.FamilyMembers.Add(new FamilyMember { FamilyId = family.Id, UserId = famOwner.Id, Status = MembershipStatus.Active });
        var module = db.AddAccount(AccountKind.Module, moduleOwner.Id, familyId: family.Id);
        db.SaveChanges();
        var policy = new AccessPolicy(db);

        var perms = await policy.GetAsync(module, famOwner.Id, default);

        Assert.True(perms.HasFlag(ModulePermissions.Manage));
        Assert.True(perms.HasFlag(ModulePermissions.Take));
    }
}
