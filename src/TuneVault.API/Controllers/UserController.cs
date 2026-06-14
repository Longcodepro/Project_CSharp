// API/Controllers/UserController.cs
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.User.Commands.FollowUser;
using TuneVault.Application.Features.User.Commands.UnfollowUser;
using TuneVault.Application.Features.User.Commands.UpdateProfile;
using TuneVault.Application.Features.User.Commands.UpdateSecurity;
using TuneVault.Application.Features.User.Commands.VerifyAsArtist;
using TuneVault.Application.Features.User.Queries.CheckFollowStatus;
using TuneVault.Application.Features.User.Queries.GetAllArtists;
using TuneVault.Application.Features.User.Queries.GetFollowers;
using TuneVault.Application.Features.User.Queries.GetFollowing;
using TuneVault.Application.Features.User.Queries.GetUserById;
using TuneVault.Application.Features.User.Queries.GetProfile;
using TuneVault.Application.Features.User.Queries.GetUserByIdDisplay;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator) => _mediator = mediator;

    // Lấy UserId từ JWT claim (sub)
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub");

    #region Queries

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return result is not null
            ? Ok(new { success = true, data = result, message = (string?)null })
            : NotFound(new { success = false, data = (object?)null, message = $"Không tìm thấy user '{id}'." });
    }

    [HttpGet("artists")]
    public async Task<IActionResult> GetAllArtists(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllArtistsQuery(), ct);
        return Ok(new { success = true, data = result, message = (string?)null });
    }

    [HttpGet("{id}/followers")]
    public async Task<IActionResult> GetFollowers(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFollowersQuery(id), ct);
        return Ok(new { success = true, data = result, message = (string?)null });
    }

    [HttpGet("{id}/following")]
    public async Task<IActionResult> GetFollowing(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFollowingQuery(id), ct);
        return Ok(new { success = true, data = result, message = (string?)null });
    }

    [HttpGet("{followerId}/is-following/{followeeId}")]
    public async Task<IActionResult> CheckStatus(string followerId, string followeeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckFollowStatusQuery(followerId, followeeId), ct);
        return Ok(new { success = true, data = result, message = (string?)null });
    }

    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetProfile(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(id), ct);
        return result is not null
            ? Ok(new { success = true, data = result, message = (string?)null })
            : NotFound(new { success = false, data = (object?)null, message = $"Không tìm thấy profile '{id}'." });
    }

    [HttpGet("display/{idDisplay}")]
    public async Task<IActionResult> GetByIdDisplay(string idDisplay, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdDisplayQuery(idDisplay), ct);
        return result is not null
            ? Ok(new { success = true, data = result, message = (string?)null })
            : NotFound(new { success = false, data = (object?)null, message = $"Không tìm thấy user '{idDisplay}'." });
    }

    #endregion

    #region Commands

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            return Ok(new { success = true, data = result, message = (string?)null });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    [HttpPut("security")]
    [Authorize]
    public async Task<IActionResult> UpdateSecurity([FromBody] UpdateSecurityCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            return result
                ? Ok(new { success = true, data = (object?)null, message = "Cập nhật bảo mật thành công." })
                : BadRequest(new { success = false, data = (object?)null, message = "Không thể cập nhật bảo mật." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    [HttpPost("follow")]
    [Authorize]
    public async Task<IActionResult> Follow([FromBody] FollowUserCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            return result
                ? Ok(new { success = true, data = (object?)null, message = "Đã follow thành công." })
                : BadRequest(new { success = false, data = (object?)null, message = "Không thể thực hiện follow." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    [HttpDelete("unfollow")]
    [Authorize]
    public async Task<IActionResult> Unfollow([FromBody] UnfollowUserCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            return result
                ? Ok(new { success = true, data = (object?)null, message = "Đã unfollow thành công." })
                : BadRequest(new { success = false, data = (object?)null, message = "Không thể thực hiện unfollow." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    [HttpPatch("{id}/verify-artist")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> VerifyArtist(string id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new VerifyAsArtistCommand(id), ct);
            return Ok(new { success = true, data = result, message = (string?)null });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    #endregion
}