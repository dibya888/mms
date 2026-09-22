using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyApp.Api.Extensions;
using MoneyApp.Application.Accounts;
using MoneyApp.Application.Ledger;

namespace MoneyApp.Api.Controllers;

[ApiController, Route("api/v1/accounts"), Authorize]
public class AccountsController(AccountService svc, LedgerService ledger) : ControllerBase
{
    [HttpGet] public async Task<List<AccountDto>> List(CancellationToken ct) => await svc.ListAsync(User.UserId(), ct);
    [HttpPost] public async Task<AccountDto> Create(CreateAccountRequest r, CancellationToken ct) => await svc.CreateAsync(User.UserId(), r, ct);
    [HttpPut("{id:guid}/name")] public async Task<AccountDto> Rename(Guid id, RenameRequest r, CancellationToken ct) => await svc.RenameAsync(User.UserId(), id, r, ct);
    [HttpGet("{id:guid}/transactions")]
    public async Task<List<TransactionDto>> History(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default) =>
        await ledger.HistoryAsync(User.UserId(), id, page, pageSize, ct);
    [HttpGet("{id:guid}/permissions")] public async Task<List<PermissionDto>> Permissions(Guid id, CancellationToken ct) => await svc.GetPermissionsAsync(User.UserId(), id, ct);
    [HttpPut("{id:guid}/permissions/{userId:guid}")]
    public async Task<IActionResult> SetPermissions(Guid id, Guid userId, SetPermissionsRequest r, CancellationToken ct) { await svc.SetPermissionsAsync(User.UserId(), id, userId, r, ct); return NoContent(); }
}
