using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.API.DTOs.Playlists;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Playlist.Commands.AddTrackToPlaylist;
using TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;
using TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;
using TuneVault.Application.Features.Playlist.Commands.RemoveTrackFromPlaylist;
using TuneVault.Application.Features.Playlist.Commands.UpdatePlaylist;
using TuneVault.Application.Features.Playlist.Commands.UpdateTrackOrder;
using TuneVault.Domain.Interfaces;
using TuneVault.Application.Features.Playlist.DTOs;
using TuneVault.Application.Features.Playlist.Queries.GetPlaylistById;
using TuneVault.Application.Features.Playlist.Queries.GetPublicPlaylists;
using TuneVault.Application.Features.Playlist.Queries.GetPlaylists;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý playlist của người dùng: CRUD playlist, public/private và track trong playlist.
/// </summary>
[ApiController]
[Route("api/playlists")]
[Authorize]
public sealed class PlaylistController : BaseApiController
{
    private static readonly HashSet<string> AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private readonly ISender _mediator;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo controller playlist với MediatR sender.
    /// </summary>
    /// <param name="mediator">Sender dùng để gửi command/query sang Application layer.</param>
    /// <param name="fileStorage">Service lưu file vào wwwroot/uploads.</param>
    /// <param name="currentUserContext">Service để lấy thông tin người dùng hiện tại.</param>
    public PlaylistController(ISender mediator, IFileStorageService fileStorage, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Trả về mã người dùng hiện tại hoặc response 401 nếu request chưa xác thực.
    /// </summary>
    private IActionResult GetUserIdOrUnauthorizedResult()
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }
        return Ok(userId);
    }

    /// <summary>
    /// Lấy danh sách playlist của người dùng hiện tại.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách playlist của user đang đăng nhập.</returns>
    private async Task<IActionResult> GetMyPlaylistsInternal(CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        if (userIdResult is UnauthorizedObjectResult)
        {
            return userIdResult;
        }
        var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");

        var result = await _mediator.Send(new GetPlaylistsQuery(userId), ct);
        return Ok(ApiResponse<IEnumerable<PlaylistDto>>.Ok(result, "Lấy danh sách playlist thành công."));
    }

    /// <summary>
    /// Lấy danh sách playlist công khai cho trang khám phá.
    /// </summary>
    /// <param name="limit">Số lượng playlist tối đa, mặc định 10 và tối đa 50.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách playlist public còn hoạt động.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PlaylistPublicDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic([FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPublicPlaylistsQuery(limit), ct);
        return Ok(ApiResponse<IReadOnlyCollection<PlaylistPublicDto>>.Ok(
            result,
            "Lấy danh sách playlist công khai thành công."));
    }

    /// <summary>
    /// Lấy danh sách playlist của người dùng hiện tại theo route rõ nghĩa.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách playlist của user đang đăng nhập.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlaylistDto>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMine(CancellationToken ct) => GetMyPlaylistsInternal(ct);

    /// <summary>
    /// Lấy chi tiết playlist theo id.
    /// </summary>
    /// <param name="id">Mã playlist cần lấy.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thông tin playlist kèm danh sách track.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PlaylistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PlaylistPublicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        PlaylistPublicDto? publicDto = null;

        if (userIdResult is UnauthorizedObjectResult)
        {
            var publicResult = await _mediator.Send(new GetPlaylistByIdQuery(id, null), ct);
            if (publicResult is null)
            {
                return NotFound(ApiResponse<object?>.Fail($"Không tìm thấy playlist '{id}' hoặc playlist đã bị xóa."));
            }
            publicDto = new PlaylistPublicDto(
                publicResult.Id,
                publicResult.OwnerId,
                publicResult.Title,
                publicResult.Description,
                publicResult.CoverImgUrl,
                publicResult.IsPublic,
                publicResult.ContentType,
                publicResult.ReleaseDate,
                publicResult.CreatedAt,
                publicResult.Tracks);
        }
        else
        {
            var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");
            if (string.IsNullOrWhiteSpace(userId)) // This check might be redundant with ?? throw, but kept for clarity/safety
            {
                return Unauthorized(ApiResponse<object?>.Fail("Invalid user ID."));
            }

            var result = await _mediator.Send(new GetPlaylistByIdQuery(id, userId), ct);
            if (result is null)
            {
                return NotFound(ApiResponse<object?>.Fail($"Không tìm thấy playlist '{id}' hoặc playlist đã bị xóa."));
            }

            if (string.Equals(result.OwnerId, userId, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(ApiResponse<PlaylistDto>.Ok(result, "Lấy thông tin playlist thành công."));
            }

            publicDto = new PlaylistPublicDto(
                result.Id,
                result.OwnerId,
                result.Title,
                result.Description,
                result.CoverImgUrl,
                result.IsPublic,
                result.ContentType,
                result.ReleaseDate,
                result.CreatedAt,
                result.Tracks);
        }

        return Ok(ApiResponse<PlaylistPublicDto>.Ok(publicDto, "Lấy thông tin playlist công khai thành công."));
    }

    /// <summary>
    /// Tạo playlist mới cho người dùng hiện tại.
    /// </summary>
    /// <param name="request">Thông tin playlist cần tạo.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Playlist vừa được tạo.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<PlaylistDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromForm] CreatePlaylistFormRequestDto request, CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        if (userIdResult is UnauthorizedObjectResult) return userIdResult;
        var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Invalid user ID."));
        }

        string? savedCoverPath = null;

        try
        {
            ValidateImageFile(request.CoverImage, "Ảnh bìa playlist");
            var coverUrl = ResolveDefaultCoverUrl(request.CoverImageUrl);

            if (request.CoverImage is not null)
                coverUrl = await SaveCoverAsync(request.CoverImage, "playlist-covers", ct);

            savedCoverPath = ResolvePhysicalUploadPath(coverUrl);

            var commandRequest = new CreatePlaylistRequestDto(
                request.Title,
                request.Description,
                request.IsPublic,
                coverUrl,
                request.ContentType,
                request.ReleaseDate);

            var result = await _mediator.Send(new CreatePlaylistCommand(userId, commandRequest), ct);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                ApiResponse<PlaylistDto>.Ok(result, "Tạo playlist thành công."));
        }
        catch
        {
            await DeleteIfExistsAsync(savedCoverPath, ct);
            throw;
        }
    }

    /// <summary>
    /// Cập nhật thông tin và trạng thái public/private của playlist.
    /// </summary>
    /// <param name="id">Mã playlist cần cập nhật.</param>
    /// <param name="request">Payload cập nhật playlist.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Playlist sau khi cập nhật.</returns>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<PlaylistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string id, [FromForm] UpdatePlaylistFormRequestDto request, CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        if (userIdResult is UnauthorizedObjectResult) return userIdResult;
        var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");

        string? savedCoverPath = null;
        string? previousCoverPath = null;

        try
        {
            ValidateImageFile(request.CoverImage, "Ảnh bìa playlist");

            var currentPlaylist = await _mediator.Send(new GetPlaylistByIdQuery(id, userId), ct);
            var coverUrl = request.KeepCurrentCover ? currentPlaylist?.CoverImgUrl : null;

            if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
            {
                coverUrl = ResolveDefaultCoverUrl(request.CoverImageUrl);
                previousCoverPath = ResolvePhysicalUploadPath(currentPlaylist?.CoverImgUrl);
            }

            if (request.CoverImage is not null)
            {
                coverUrl = await SaveCoverAsync(request.CoverImage, "playlist-covers", ct);
                savedCoverPath = ResolvePhysicalUploadPath(coverUrl);
                previousCoverPath = ResolvePhysicalUploadPath(currentPlaylist?.CoverImgUrl);
            }

            var commandRequest = new UpdatePlaylistRequestDto(
                request.Title,
                request.Description,
                request.IsPublic,
                coverUrl,
                request.ContentType,
                request.ReleaseDate);

            var result = await _mediator.Send(new UpdatePlaylistCommand(id, userId, commandRequest), ct);
            await DeleteIfExistsAsync(previousCoverPath, ct);

            return Ok(ApiResponse<PlaylistDto>.Ok(result, "Cập nhật playlist thành công."));
        }
        catch
        {
            await DeleteIfExistsAsync(savedCoverPath, ct);
            throw;
        }
    }

    /// <summary>
    /// Xóa mềm playlist của người dùng hiện tại.
    /// </summary>
    /// <param name="id">Mã playlist cần xóa.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả xóa playlist.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        if (userIdResult is UnauthorizedObjectResult) return userIdResult;
        var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");

        await _mediator.Send(new DeletePlaylistCommand(id, userId), ct);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa playlist thành công."));
    }

    /// <summary>
    /// Thêm một bài hát vào cuối playlist.
    /// </summary>
    /// <param name="id">Mã playlist cần thêm bài hát.</param>
    /// <param name="request">Payload chứa media item cần thêm.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả thêm bài hát vào playlist.</returns>
    [HttpPost("{id}/tracks")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTrack(string id, [FromBody] AddTrackToPlaylistRequestDto request, CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        if (userIdResult is UnauthorizedObjectResult) return userIdResult;
        var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");

        await _mediator.Send(new AddTrackToPlaylistCommand(id, userId, request), ct);
        return Ok(ApiResponse<bool>.Ok(true, "Thêm bài hát vào playlist thành công."));
    }

    /// <summary>
    /// Xóa một bài hát khỏi playlist.
    /// </summary>
    /// <param name="id">Mã playlist cần xóa bài hát.</param>
    /// <param name="mediaId">Mã media item cần xóa khỏi playlist.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả xóa bài hát khỏi playlist.</returns>
    [HttpDelete("{id}/tracks/{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveTrack(string id, string mediaId, CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        if (userIdResult is UnauthorizedObjectResult) return userIdResult;
        var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");

        await _mediator.Send(new RemoveTrackFromPlaylistCommand(id, userId, mediaId), ct);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa bài hát khỏi playlist thành công."));
    }

    /// <summary>
    /// Cập nhật thứ tự phát của một bài hát trong playlist.
    /// </summary>
    /// <param name="playlistId">Mã playlist chứa bài hát.</param>
    /// <param name="mediaItemId">Mã media item cần cập nhật thứ tự.</param>
    /// <param name="newOrder">Thứ tự mới trong playlist.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả cập nhật thứ tự bài hát.</returns>
    [HttpPatch("{playlistId}/tracks/{mediaItemId}/order")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTrackOrder(string playlistId, string mediaItemId, [FromQuery] int newOrder, CancellationToken ct)
    {
        var userIdResult = GetUserIdOrUnauthorizedResult();
        if (userIdResult is UnauthorizedObjectResult) return userIdResult;
        var userId = ((OkObjectResult)userIdResult).Value as string ?? throw new InvalidOperationException("User ID is unexpectedly null after authentication.");

        await _mediator.Send(new UpdateTrackOrderCommand(playlistId, userId, mediaItemId, newOrder), ct);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thứ tự bài hát thành công."));
    }

    /// <summary>
    /// Kiểm tra file ảnh bìa trước khi lưu vào wwwroot.
    /// </summary>
    private static void ValidateImageFile(IFormFile? file, string displayName)
    {
        if (file is null)
            return;

        if (file.Length <= 0)
            throw new DomainException($"{displayName} không được rỗng.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            throw new DomainException($"{displayName} chỉ hỗ trợ .jpg, .jpeg, .png hoặc .webp.");

        var contentType = file.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!AllowedImageContentTypes.Contains(contentType))
            throw new DomainException($"{displayName} không đúng định dạng ảnh hợp lệ.");
    }

    /// <summary>
    /// Lưu ảnh bìa và trả về URL public.
    /// </summary>
    private async Task<string?> SaveCoverAsync(IFormFile? file, string folderName, CancellationToken ct)
    {
        if (file is null)
            return null;

        var physicalPath = await _fileStorage.SaveAsync(file, folderName, ct);
        return $"/uploads/{folderName}/{Path.GetFileName(physicalPath)}";
    }

    /// <summary>
    /// Đổi URL public trong wwwroot/uploads về path vật lý để cleanup file cũ.
    /// </summary>
    private static string? ResolvePhysicalUploadPath(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl) ||
            !publicUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (publicUrl.StartsWith("/uploads/default-cover/", StringComparison.OrdinalIgnoreCase))
            return null;

        var relativePath = publicUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
    }

    private static string? ResolveDefaultCoverUrl(string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(coverImageUrl))
            return null;

        var normalizedUrl = coverImageUrl.Trim();
        const string prefix = "/uploads/default-cover/";
        if (!normalizedUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Ảnh bìa mặc định không hợp lệ.");

        var fileName = Path.GetFileName(normalizedUrl);
        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(normalizedUrl, prefix + fileName, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Ảnh bìa mặc định không hợp lệ.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            throw new DomainException("Ảnh bìa mặc định chỉ hỗ trợ .jpg, .jpeg, .png hoặc .webp.");

        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "default-cover", fileName);
        if (!System.IO.File.Exists(physicalPath))
            throw new DomainException("Ảnh bìa mặc định không tồn tại.");

        return prefix + fileName;
    }

    /// <summary>
    /// Xóa file vật lý nếu có path hợp lệ.
    /// </summary>
    private async Task DeleteIfExistsAsync(string? physicalPath, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(physicalPath))
            await _fileStorage.DeleteAsync(physicalPath, ct);
    }
}
