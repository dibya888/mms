using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyApp.Api.Extensions;
using MoneyApp.Application.Families;

namespace MoneyApp.Api.Controllers;

[ApiController, Route("api/v1/families"), Authorize]
public class FamiliesController(FamilyService svc) : ControllerBase
{
    [HttpPost] public async Task<FamilyDto> Create(CreateFamilyRequest r, CancellationToken ct) => await svc.CreateAsync(User.UserId(), r, ct);
    [HttpGet("me")] public async Task<ActionResult<FamilyDto>> Mine(CancellationToken ct) => await svc.MineAsync(User.UserId(), ct) is { } f ? f : NoContent();
    [HttpGet("search/{publicId}")]
    public async Task<FamilySearchResult> Search(string publicId, CancellationToken ct) => await svc.SearchAsync(publicId, ct);
    [HttpPost("join/{publicId}")]
    public async Task<IActionResult> RequestJoin(string publicId, CancellationToken ct) { await svc.RequestJoinAsync(User.UserId(), publicId, ct); return Accepted(); }
    [HttpGet("me/members")] public async Task<List<MemberDto>> Members(CancellationToken ct) => await svc.MembersAsync(User.UserId(), ct);
    [HttpPost("me/requests/{userId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid userId, CancellationToken ct) { await svc.DecideAsync(User.UserId(), userId, true, ct); return NoContent(); }
    [HttpPost("me/requests/{userId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid userId, CancellationToken ct) { await svc.DecideAsync(User.UserId(), userId, false, ct); return NoContent(); }
    [HttpDelete("me/members/{userId:guid}")]
    public async Task<IActionResult> Kick(Guid userId, CancellationToken ct) { await svc.KickAsync(User.UserId(), userId, ct); return NoContent(); }
}
