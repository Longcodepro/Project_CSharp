using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.User.Commands.FollowUser;
using TuneVault.Application.Features.User.Commands.UnfollowUser;
using TuneVault.Application.Features.User.Commands.UpdateProfile;
using TuneVault.Application.Features.User.Commands.VerifyAsArtist;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Application.Features.User.Queries.CountFollowers;
using TuneVault.Application.Features.User.Queries.CheckFollowStatus;
using TuneVault.Application.Features.User.Queries.GetAllArtists;
using TuneVault.Application.Features.User.Queries.GetFollowers;
using TuneVault.Application.Features.User.Queries.GetFollowing;
using TuneVault.Application.Features.User.Queries.GetProfile;
using TuneVault.Application.Features.User.Queries.GetUserById;
using TuneVault.Application.Features.User.Queries.GetUserByIdDisplay;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

/// <summary>
/// API endpoints for managing users and their relationships.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance for sending commands and queries.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT.</param>
    public UsersController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Lấy thông tin hồ sơ đầy đủ của chính người dùng đang đăng nhập.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>ApiResponse chứa thông tin hồ sơ hiện tại.</returns>
    [HttpGet("me/profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để xem thông tin cá nhân."));
        }

        var result = await _mediator.Send(new GetUserProfileQuery(currentUserId), ct);

        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Lấy thông tin cá nhân thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ của chính người dùng đang đăng nhập.
    /// </summary>
    /// <param name="request">Thông tin hồ sơ mới.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>ApiResponse chứa hồ sơ sau khi cập nhật.</returns>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequestDto request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để cập nhật thông tin cá nhân."));
        }

        var command = new UpdateProfileCommand(currentUserId, request.DisplayName, request.Bio, request.AvatarUrl);
        var result = await _mediator.Send(command, ct);

        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Cập nhật thông tin cá nhân thành công."));
    }

    /// <summary>
    /// Xác thực một tài khoản người dùng thành nghệ sĩ.
    /// </summary>
    /// <param name="id">Mã định danh hệ thống của người dùng cần xác thực.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>ApiResponse chứa hồ sơ người dùng sau khi được xác thực nghệ sĩ.</returns>
    [HttpPatch("{id}/verify-artist")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyArtist(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(ApiResponse<object?>.Fail("Mã người dùng cần xác thực nghệ sĩ không được để trống."));
        }

        var result = await _mediator.Send(new VerifyAsArtistCommand(id), ct);

        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Xác thực tài khoản nghệ sĩ thành công."));
    }

    /// <summary>
    /// Lấy danh sách tài khoản nghệ sĩ đang hoạt động.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>ApiResponse chứa danh sách nghệ sĩ.</returns>
    [HttpGet("artists")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetArtists(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllArtistsQuery(), ct);

        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result, "Lấy danh sách nghệ sĩ thành công."));
    }

    /// <summary>
    /// Tìm người dùng theo handle hiển thị công khai.
    /// </summary>
    /// <param name="idDisplay">Handle công khai của người dùng.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>ApiResponse chứa thông tin người dùng công khai nếu tìm thấy.</returns>
    [HttpGet("by-handle/{idDisplay}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByIdDisplay(string idDisplay, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idDisplay))
        {
            return BadRequest(ApiResponse<object?>.Fail("Handle người dùng không được để trống."));
        }

        var result = await _mediator.Send(new GetUserByIdDisplayQuery(idDisplay), ct);

        return result is null
            ? NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng với handle này."))
            : Ok(ApiResponse<UserDto>.Ok(result, "Lấy thông tin người dùng theo handle thành công."));
    }

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse containing the user details or an error message.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous] // Allow anonymous access for public user profiles
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(string id, CancellationToken ct)
    {
        // Bước 1: Gửi query GetUserByIdQuery tới MediatR.
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, ct);

        // Bước 2: Trả về kết quả hoặc 404 nếu không tìm thấy.
        return result == null
            ? NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng."))
            : Ok(ApiResponse<UserDto>.Ok(result, "Lấy thông tin người dùng thành công."));
    }

    /// <summary>
    /// Follows a user.
    /// </summary>
    /// <param name="followUserCommand">The command containing the ID of the user to follow.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse indicating success or failure.</returns>
    [HttpPost("follow")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FollowUser([FromBody] FollowUserCommand followUserCommand, CancellationToken ct)
    {
        // Bước 1: Gửi command FollowUserCommand tới MediatR.
        // SenderId sẽ được lấy tự động từ JWT của người dùng hiện tại.
        await _mediator.Send(followUserCommand, ct);

        // Bước 2: Trả về response thành công.
        return Ok(ApiResponse<object?>.Ok(null, "Theo dõi người dùng thành công."));
    }

    /// <summary>
    /// Unfollows a user.
    /// </summary>
    /// <param name="unfollowUserCommand">The command containing the ID of the user to unfollow.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse indicating success or failure.</returns>
    [HttpDelete("unfollow")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfollowUser([FromBody] UnfollowUserCommand unfollowUserCommand, CancellationToken ct)
    {
        // Bước 1: Gửi command UnfollowUserCommand tới MediatR.
        // SenderId sẽ được lấy tự động từ JWT của người dùng hiện tại.
        await _mediator.Send(unfollowUserCommand, ct);

        // Bước 2: Trả về response thành công.
        return Ok(ApiResponse<object?>.Ok(null, "Bỏ theo dõi người dùng thành công."));
    }

    /// <summary>
    /// Gets the follow status between two users.
    /// </summary>
    /// <param name="followerId">The ID of the user who is following.</param>
    /// <param name="followeeId">The ID of the user who is being followed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse containing the follow status.</returns>
    [HttpGet("{followerId}/is-following/{followeeId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowStatus(string followerId, string followeeId, CancellationToken ct)
    {
        // Bước 1: Gửi query CheckFollowStatusQuery tới MediatR.
        var query = new CheckFollowStatusQuery(followerId, followeeId);
        var result = await _mediator.Send(query, ct);

        // Bước 2: Trả về kết quả.
        return Ok(ApiResponse<bool>.Ok(result, "Kiểm tra trạng thái theo dõi thành công."));
    }

    /// <summary>
    /// Gets a list of users that a given user is following.
    /// </summary>
    /// <param name="id">The ID of the user whose following list to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse containing a list of users being followed.</returns>
    [HttpGet("{id}/following")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowing(string id, CancellationToken ct)
    {
        // Bước 1: Gửi query GetFollowingQuery tới MediatR.
        var query = new GetFollowingQuery(id);
        var result = await _mediator.Send(query, ct);

        // Bước 2: Trả về kết quả.
        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result, "Lấy danh sách đang theo dõi thành công."));
    }

    /// <summary>
    /// Gets a list of users who are following a given user.
    /// </summary>
    /// <param name="id">The ID of the user whose followers list to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse containing a list of followers.</returns>
    [HttpGet("{id}/followers")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowers(string id, CancellationToken ct)
    {
        // Bước 1: Gửi query GetFollowersQuery tới MediatR.
        var query = new GetFollowersQuery(id);
        var result = await _mediator.Send(query, ct);

        // Bước 2: Trả về kết quả.
        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result, "Lấy danh sách người theo dõi thành công."));
    }

    /// <summary>
    /// Đếm số lượng follower hiện tại của một người dùng/nghệ sĩ.
    /// </summary>
    /// <param name="id">Mã định danh người dùng cần đếm follower.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>ApiResponse chứa số lượng follower (int).</returns>
    [HttpGet("{id}/followers-count")]
    [AllowAnonymous] // Followers count can be public
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CountFollowers(string id, CancellationToken ct)
    {
        // Bước 1: Gửi query CountFollowersQuery tới MediatR, truyền vào userId cần đếm.
        var query = new CountFollowersQuery(id);
        var result = await _mediator.Send(query, ct);

        // Bước 2: Handler sẽ gọi repository đếm số bản ghi Follow có FolloweeId = id.
        // Bước 3: Trả về kết quả hoặc 404 nếu user không tồn tại.
        return result == -1 // Assuming -1 indicates user not found, adjust if repository returns null or throws
            ? NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng."))
            : Ok(ApiResponse<int>.Ok(result, "Lấy số lượng người theo dõi thành công."));
    }
}
