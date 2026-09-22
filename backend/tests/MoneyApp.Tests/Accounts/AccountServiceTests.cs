using MoneyApp.Application.Accounts;
using MoneyApp.Application.Common;
using MoneyApp.Application.Families;
using MoneyApp.Domain;
using MoneyApp.Tests.Infra;
using Xunit;

namespace MoneyApp.Tests.Accounts;

public class AccountServiceTests
{
    private static (FamilyService fam, AccountService acc) Services(Infrastructure.Persistence.AppDbContext db) =>
        (new FamilyService(db), new AccountService(db, new AccessPolicy(db)));

    [Fact]
    public async Task ListNeverReturnsAModuleTheUserCannotView()
    {
        using var db = TestDb.Create();
        var (fam, acc) = Services(db);
        var owner = db.AddUser("owner@example.com");
        var outsider = db.AddUser("outsider@example.com");
        var f = await fam.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await acc.CreateAsync(owner.Id, new CreateAccountRequest(AccountKind.Module, "Family Fund", "BDT"), default);

        var visible = await acc.ListAsync(outsider.Id, default);

        Assert.Empty(visible);
    }

    [Fact]
    public async Task OwnerCanGrantTakeOnlyAndItIsReflectedInPermissionsList()
    {
        using var db = TestDb.Create();
        var (fam, acc) = Services(db);
        var owner = db.AddUser("owner@example.com");
        var member = db.AddUser("member@example.com");
        var f = await fam.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await fam.RequestJoinAsync(member.Id, f.PublicId, default);
        await fam.DecideAsync(owner.Id, member.Id, true, default);
        var module = await acc.CreateAsync(owner.Id, new CreateAccountRequest(AccountKind.Module, "Cash", "BDT"), default);

        await acc.SetPermissionsAsync(owner.Id, module.Id, member.Id, new SetPermissionsRequest(ModulePermissions.Take), default);
        var perms = await acc.GetPermissionsAsync(owner.Id, module.Id, default);

        var row = perms.Single(p => p.UserId == member.Id);
        Assert.Equal(ModulePermissions.View | ModulePermissions.Take, row.Permissions);
    }

    [Fact]
    public async Task NonManagerCannotReadOrSetPermissions()
    {
        using var db = TestDb.Create();
        var (fam, acc) = Services(db);
        var owner = db.AddUser("owner@example.com");
        var member = db.AddUser("member@example.com");
        var f = await fam.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        await fam.RequestJoinAsync(member.Id, f.PublicId, default);
        await fam.DecideAsync(owner.Id, member.Id, true, default);
        var module = await acc.CreateAsync(owner.Id, new CreateAccountRequest(AccountKind.Module, "Cash", "BDT"), default);

        await Assert.ThrowsAsync<AppException>(() => acc.GetPermissionsAsync(member.Id, module.Id, default));
        await Assert.ThrowsAsync<AppException>(() =>
            acc.SetPermissionsAsync(member.Id, module.Id, owner.Id, new SetPermissionsRequest(ModulePermissions.View), default));
    }

    [Fact]
    public async Task OwnersOwnAccessCannotBeChanged()
    {
        using var db = TestDb.Create();
        var (fam, acc) = Services(db);
        var owner = db.AddUser();
        var f = await fam.CreateAsync(owner.Id, new CreateFamilyRequest("F"), default);
        var module = await acc.CreateAsync(owner.Id, new CreateAccountRequest(AccountKind.Module, "Cash", "BDT"), default);

        await Assert.ThrowsAsync<AppException>(() =>
            acc.SetPermissionsAsync(owner.Id, module.Id, owner.Id, new SetPermissionsRequest(ModulePermissions.None), default));
    }
}
