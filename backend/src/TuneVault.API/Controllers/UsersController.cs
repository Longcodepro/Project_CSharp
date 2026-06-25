using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.API.DTOs.Users;
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
using TuneVault.Application.Abstractions;

namespace TuneVault.API.Controllers;

/// <summary>
/// API endpoints for managing users and their relationships.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : BaseApiController
{
    private static readonly HashSet<string> AllowedAvatarContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private static readonly HashSet<string> AllowedAvatarExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFileStorageService _fileStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance for sending commands and queries.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT.</param>
    /// <param name="fileStorage">Service lưu file vật lý vào thư mục public của API.</param>
    public UsersController(
        IMediator mediator,
        ICurrentUserContext currentUserContext,
        IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
        _fileStorage = fileStorage;
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
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateProfileFormRequestDto request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để cập nhật thông tin cá nhân."));
        }

        if (request.RemoveAvatar && request.AvatarFile is not null)
        {
            return BadRequest(ApiResponse<object?>.Fail("Không thể vừa tải ảnh đại diện mới vừa yêu cầu xóa ảnh hiện tại trong cùng một lần cập nhật."));
        }

        ValidateAvatarFile(request.AvatarFile);

        var currentProfile = await _mediator.Send(new GetUserProfileQuery(currentUserId), ct);
        var previousAvatarUrl = currentProfile.AvatarUrl;
        var nextAvatarUrl = currentProfile.AvatarUrl;
        var shouldUpdateAvatar = false;
        string? savedAvatarPhysicalPath = null;

        try
        {
            if (request.RemoveAvatar)
            {
                nextAvatarUrl = null;
                shouldUpdateAvatar = true;
            }
            else if (request.AvatarFile is not null)
            {
                savedAvatarPhysicalPath = await _fileStorage.SaveAsync(request.AvatarFile, "avatars", ct);
                nextAvatarUrl = BuildPublicUploadUrl("avatars", Path.GetFileName(savedAvatarPhysicalPath));
                shouldUpdateAvatar = true;
            }

            var command = new UpdateProfileCommand(
                currentUserId,
                request.IdDisplay,
                request.DisplayName,
                request.Bio,
                nextAvatarUrl,
                shouldUpdateAvatar);

            var result = await _mediator.Send(command, ct);

            if (shouldUpdateAvatar &&
                !string.IsNullOrWhiteSpace(previousAvatarUrl) &&
                !string.Equals(previousAvatarUrl, nextAvatarUrl, StringComparison.OrdinalIgnoreCase))
            {
                await TryDeleteAvatarFileAsync(previousAvatarUrl, ct);
            }

            return Ok(ApiResponse<UserProfileDto>.Ok(result, "Cập nhật thông tin cá nhân thành công."));
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(savedAvatarPhysicalPath))
            {
                await _fileStorage.DeleteAsync(savedAvatarPhysicalPath, ct);
            }

            throw;
        }
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
    [AllowAnonymous]
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
    [ProducesResponseType(typeof(ApiResponse<UserPublicDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(string id, CancellationToken ct)
    {
        // Bước 1: Gửi query GetUserByIdQuery tới MediatR.
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, ct);

        // Bước 2: Trả về kết quả hoặc 404 nếu không tìm thấy.
        return result == null
            ? NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng hoặc tài khoản này hiện không còn hoạt động."))
            : Ok(ApiResponse<UserPublicDetailDto>.Ok(result, "Lấy thông tin người dùng thành công."));
    }

    /// <summary>
    /// Follows a user.
    /// </summary>
    /// <param name="request">Dữ liệu chỉ chứa mã người dùng cần theo dõi.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse indicating success or failure.</returns>
    [HttpPost("follow")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FollowUser([FromBody] FollowUserRequestDto request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác theo dõi."));
        }

        if (string.IsNullOrWhiteSpace(request.FolloweeId))
        {
            return BadRequest(ApiResponse<object?>.Fail("Mã người dùng cần theo dõi không được để trống."));
        }

        await _mediator.Send(new FollowUserCommand(currentUserId, request.FolloweeId), ct);

        return Ok(ApiResponse<object?>.Ok(null, "Theo dõi người dùng thành công."));
    }

    /// <summary>
    /// Unfollows a user.
    /// </summary>
    /// <param name="request">Dữ liệu chỉ chứa mã người dùng cần bỏ theo dõi.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse indicating success or failure.</returns>
    [HttpDelete("unfollow")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfollowUser([FromBody] UnfollowUserRequestDto request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác bỏ theo dõi."));
        }

        if (string.IsNullOrWhiteSpace(request.FolloweeId))
        {
            return BadRequest(ApiResponse<object?>.Fail("Mã người dùng cần bỏ theo dõi không được để trống."));
        }

        await _mediator.Send(new UnfollowUserCommand(currentUserId, request.FolloweeId), ct);

        return Ok(ApiResponse<object?>.Ok(null, "Bỏ theo dõi người dùng thành công."));
    }

    /// <summary>
    /// Gets the follow status between two users.
    /// </summary>
    /// <param name="followeeId">The ID of the user who is being followed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ApiResponse containing the follow status.</returns>
    [HttpGet("is-following/{followeeId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowStatus(string followeeId, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để kiểm tra trạng thái theo dõi."));
        }

        if (string.IsNullOrWhiteSpace(followeeId))
        {
            return BadRequest(ApiResponse<object?>.Fail("Mã người dùng cần kiểm tra không được để trống."));
        }

        var targetUser = await _mediator.Send(new GetUserByIdQuery(followeeId), ct);
        if (targetUser is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng cần kiểm tra trạng thái theo dõi."));
        }

        var result = await _mediator.Send(new CheckFollowStatusQuery(currentUserId, followeeId), ct);

        return Ok(ApiResponse<bool>.Ok(result, "Kiểm tra trạng thái theo dõi thành công."));
    }

    /// <summary>
    /// Route cũ được giữ lại để tránh phá client hiện tại, nhưng vẫn chỉ cho phép kiểm tra theo tài khoản đang đăng nhập.
    /// </summary>
    /// <param name="followerId">Mã follower từ route cũ.</param>
    /// <param name="followeeId">Mã người dùng đang được xem.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>ApiResponse chứa trạng thái theo dõi của tài khoản hiện tại.</returns>
    [HttpGet("{followerId}/is-following/{followeeId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowStatusLegacy(string followerId, string followeeId, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để kiểm tra trạng thái theo dõi."));
        }

        if (!currentUserId.Equals(followerId, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object?>.Fail("Bạn không có quyền kiểm tra trạng thái theo dõi của người dùng khác."));
        }

        return await GetFollowStatus(followeeId, ct);
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
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowers(string id, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để xem danh sách người theo dõi."));
        }

        if (!currentUserId.Equals(id, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object?>.Fail("Bạn không có quyền xem danh sách người theo dõi của tài khoản khác."));
        }

        var targetUser = await _mediator.Send(new GetUserByIdQuery(id), ct);
        if (targetUser is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng hoặc tài khoản này hiện không còn hoạt động."));
        }

        var query = new GetFollowersQuery(id);
        var result = await _mediator.Send(query, ct);

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
        var targetUser = await _mediator.Send(new GetUserByIdQuery(id), ct);
        if (targetUser is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy người dùng hoặc tài khoản này hiện không còn hoạt động."));
        }

        var result = await _mediator.Send(new CountFollowersQuery(id), ct);
        return Ok(ApiResponse<int>.Ok(result, "Lấy số lượng người theo dõi thành công."));
    }

    /// <summary>
    /// Chỉ chấp nhận các định dạng ảnh phổ biến để tránh người dùng upload nhầm file không hiển thị được.
    /// </summary>
    private static void ValidateAvatarFile(IFormFile? avatarFile)
    {
        if (avatarFile is null)
        {
            return;
        }

        if (avatarFile.Length <= 0)
        {
            throw new DomainException("File ảnh đại diện không được rỗng.");
        }

        var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
        if (!AllowedAvatarExtensions.Contains(extension))
        {
            throw new DomainException("Ảnh đại diện chỉ hỗ trợ các định dạng .jpg, .jpeg, .png hoặc .webp.");
        }

        var contentType = avatarFile.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!AllowedAvatarContentTypes.Contains(contentType))
        {
            throw new DomainException("File tải lên không phải là ảnh hợp lệ cho avatar.");
        }
    }

    /// <summary>
    /// Database chỉ lưu URL public để frontend có thể dùng trực tiếp mà không phụ thuộc path vật lý của máy chạy API.
    /// </summary>
    private static string BuildPublicUploadUrl(string folderName, string fileName)
        => $"/uploads/{folderName}/{fileName}";

    /// <summary>
    /// Chỉ xóa avatar cũ nằm trong thư mục public của API để tránh tác động nhầm tới URL ngoài hệ thống.
    /// </summary>
    private async Task TryDeleteAvatarFileAsync(string avatarUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl) ||
            !avatarUrl.StartsWith("/uploads/avatars/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(avatarUrl);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars", fileName);
        await _fileStorage.DeleteAsync(physicalPath, ct);
    }
}
