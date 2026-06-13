using MediatR;
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

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý các tác vụ liên quan đến User trong TuneVault.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Khởi tạo Controller với MediatR.
    /// </summary>
    public UsersController(IMediator mediator) => _mediator = mediator;

    #region Queries

    /// <summary> Lấy thông tin chi tiết user theo Id. </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetUserByIdQuery(id), ct));

    /// <summary> Lấy danh sách tất cả các nghệ sĩ. </summary>
    [HttpGet("artists")]
    public async Task<IActionResult> GetAllArtists(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllArtistsQuery(), ct));

    /// <summary> Lấy danh sách người theo dõi của một user. </summary>
    [HttpGet("{id}/followers")]
    public async Task<IActionResult> GetFollowers(string id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetFollowersQuery(id), ct));

    /// <summary> Lấy danh sách những người mà user đang theo dõi. </summary>
    [HttpGet("{id}/following")]
    public async Task<IActionResult> GetFollowing(string id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetFollowingQuery(id), ct));

    /// <summary> Kiểm tra trạng thái theo dõi giữa follower và followee. </summary>
    [HttpGet("{followerId}/is-following/{followeeId}")]
    public async Task<IActionResult> CheckStatus(string followerId, string followeeId, CancellationToken ct)
        => Ok(await _mediator.Send(new CheckFollowStatusQuery(followerId, followeeId), ct));

    #endregion

    #region Commands

    /// <summary> Cập nhật thông tin profile (DisplayName, Bio, Avatar). </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result is not null ? Ok(result) : BadRequest("Không thể cập nhật profile.");
    }

    /// <summary> Cập nhật bảo mật (Email, Password). </summary>
    [HttpPut("security")]
    public async Task<IActionResult> UpdateSecurity([FromBody] UpdateSecurityCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result ? Ok() : BadRequest("Không thể cập nhật bảo mật.");
    }

    /// <summary> Thực hiện follow một user khác. </summary>
    [HttpPost("follow")]
    public async Task<IActionResult> Follow([FromBody] FollowUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result ? Ok() : BadRequest("Không thể thực hiện hành động follow.");
    }

    /// <summary> Hủy follow một user khác. </summary>
    [HttpDelete("unfollow")]
    public async Task<IActionResult> Unfollow([FromBody] UnfollowUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result ? Ok() : BadRequest("Không thể thực hiện hành động unfollow.");
    }

    /// <summary> Xác thực user là nghệ sĩ. </summary>
    [HttpPatch("{id}/verify-artist")]
    public async Task<IActionResult> VerifyArtist(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyAsArtistCommand(id), ct);
        return result is not null ? Ok(result) : BadRequest("Không thể xác thực nghệ sĩ.");
    }

    /// <summary> Lấy profile đầy đủ của user theo Id (avatar, bio, followers, ngày tạo...). </summary>
    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetProfile(string id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetUserProfileQuery(id), ct));

    /// <summary> Tìm user theo handle hiển thị công khai (vd: /display/john_doe). </summary>
    [HttpGet("display/{idDisplay}")]
    public async Task<IActionResult> GetByIdDisplay(string idDisplay, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdDisplayQuery(idDisplay), ct);
        return result is not null ? Ok(result) : NotFound($"Không tìm thấy user với handle '{idDisplay}'.");
    }

    #endregion
}