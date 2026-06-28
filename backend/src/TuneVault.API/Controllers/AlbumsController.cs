using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.API.DTOs.Albums;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Album.Commands.AddTrackToAlbum;
using TuneVault.Application.Features.Album.Commands.CreateAlbum;
using TuneVault.Application.Features.Album.Commands.DeleteAlbum;
using TuneVault.Application.Features.Album.Commands.RemoveTrackFromAlbum;
using TuneVault.Application.Features.Album.Commands.UpdateAlbum;
using TuneVault.Application.Features.Album.Commands.UpdateAlbumTrackOrder;
using TuneVault.Application.Features.Album.DTOs;
using TuneVault.Application.Features.Album.Queries.GetAlbumById;
using TuneVault.Application.Features.Album.Queries.GetMyAlbums;
using TuneVault.Application.Features.Album.Queries.GetPublicAlbums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý album của nghệ sĩ.
/// </summary>
[ApiController]
[Route("api/albums")]
public sealed class AlbumsController : ControllerBase
{
    private static readonly HashSet<string> AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private readonly ISender _mediator;
    private readonly IFileStorageService _fileStorage;

    /// <summary>
    /// Khởi tạo controller album với MediatR sender.
    /// </summary>
    public AlbumsController(ISender mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
    }

    private string CurrentUserId => GetCurrentUserId()
        ?? throw new UnauthorizedAccessException("Bạn cần đăng nhập để thực hiện thao tác này.");

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub");

    /// <summary>
    /// Lấy danh sách album công khai cho trang khám phá.
    /// </summary>
    /// <param name="limit">Số lượng album tối đa, mặc định 10 và tối đa 50.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách album public còn hoạt động.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<AlbumPublicDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPublicAlbumsQuery(limit), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AlbumPublicDto>>.Ok(
            result,
            "Lấy danh sách album công khai thành công."));
    }

    /// <summary>
    /// Lấy danh sách album của nghệ sĩ hiện tại.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<AlbumDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyAlbumsQuery(CurrentUserId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AlbumDto>>.Ok(result, "Lấy danh sách album thành công."));
    }

    /// <summary>
    /// Lấy chi tiết album theo id.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AlbumDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AlbumPublicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new GetAlbumByIdQuery(id, currentUserId), cancellationToken);
        if (result is null)
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy album hoặc album đã bị xóa."));

        if (!string.IsNullOrWhiteSpace(currentUserId) &&
            string.Equals(result.ArtistId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            return Ok(ApiResponse<AlbumDto>.Ok(result, "Lấy thông tin album thành công."));
        }

        var publicDto = new AlbumPublicDto(
            result.Id,
            result.ArtistId,
            result.Title,
            result.Description,
            result.CoverImageUrl,
            result.IsPublic,
            result.ContentType,
            result.ReleaseDate,
            result.CreatedAt,
            result.Tracks);

        return Ok(ApiResponse<AlbumPublicDto>.Ok(publicDto, "Lấy thông tin album công khai thành công."));
    }

    /// <summary>
    /// Tạo album mới cho nghệ sĩ hiện tại.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Artist")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<AlbumDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromForm] CreateAlbumFormRequestDto request, CancellationToken cancellationToken)
    {
        string? savedCoverPath = null;

        try
        {
            ValidateImageFile(request.CoverImage, "Ảnh bìa album");
            var coverUrl = ResolveDefaultCoverUrl(request.CoverImageUrl);

            if (request.CoverImage is not null)
                coverUrl = await SaveCoverAsync(request.CoverImage, "album-covers", cancellationToken);

            savedCoverPath = ResolvePhysicalUploadPath(coverUrl);

            var commandRequest = new CreateAlbumRequestDto(
                request.Title,
                request.Description,
                coverUrl,
                request.IsPublic,
                request.ContentType,
                request.ReleaseDate);

            var result = await _mediator.Send(new CreateAlbumCommand(CurrentUserId, commandRequest), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AlbumDto>.Ok(result, "Tạo album thành công."));
        }
        catch
        {
            await DeleteIfExistsAsync(savedCoverPath, cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Cập nhật thông tin album của nghệ sĩ hiện tại.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Artist")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<AlbumDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string id, [FromForm] UpdateAlbumFormRequestDto request, CancellationToken cancellationToken)
    {
        string? savedCoverPath = null;
        string? previousCoverPath = null;

        try
        {
            ValidateImageFile(request.CoverImage, "Ảnh bìa album");

            var currentAlbum = await _mediator.Send(new GetAlbumByIdQuery(id, CurrentUserId), cancellationToken);
            var coverUrl = request.KeepCurrentCover ? currentAlbum?.CoverImageUrl : null;

            if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
            {
                coverUrl = ResolveDefaultCoverUrl(request.CoverImageUrl);
                previousCoverPath = ResolvePhysicalUploadPath(currentAlbum?.CoverImageUrl);
            }

            if (request.CoverImage is not null)
            {
                coverUrl = await SaveCoverAsync(request.CoverImage, "album-covers", cancellationToken);
                savedCoverPath = ResolvePhysicalUploadPath(coverUrl);
                previousCoverPath = ResolvePhysicalUploadPath(currentAlbum?.CoverImageUrl);
            }

            var commandRequest = new UpdateAlbumRequestDto(
                request.Title,
                request.Description,
                coverUrl,
                request.IsPublic,
                request.ContentType,
                request.ReleaseDate);

            var result = await _mediator.Send(new UpdateAlbumCommand(id, CurrentUserId, commandRequest), cancellationToken);
            await DeleteIfExistsAsync(previousCoverPath, cancellationToken);

            return Ok(ApiResponse<AlbumDto>.Ok(result, "Cập nhật album thành công."));
        }
        catch
        {
            await DeleteIfExistsAsync(savedCoverPath, cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Xóa mềm album của nghệ sĩ hiện tại.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAlbumCommand(id, CurrentUserId), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa album thành công."));
    }

    /// <summary>
    /// Thêm media vào album.
    /// </summary>
    [HttpPost("{id}/tracks")]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTrack(string id, [FromBody] AddTrackToAlbumRequestDto request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AddTrackToAlbumCommand(id, CurrentUserId, request), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Thêm media vào album thành công."));
    }

    /// <summary>
    /// Xóa media khỏi album.
    /// </summary>
    [HttpDelete("{id}/tracks/{mediaId}")]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveTrack(string id, string mediaId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveTrackFromAlbumCommand(id, CurrentUserId, mediaId), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Xóa media khỏi album thành công."));
    }

    /// <summary>
    /// Cập nhật thứ tự phát của media trong album.
    /// </summary>
    [HttpPatch("{albumId}/tracks/{mediaId}/order")]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTrackOrder(string albumId, string mediaId, [FromQuery] int newOrder, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateAlbumTrackOrderCommand(albumId, CurrentUserId, mediaId, newOrder), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thứ tự media trong album thành công."));
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
    /// Lưu ảnh bìa album và trả về URL public.
    /// </summary>
    private async Task<string?> SaveCoverAsync(IFormFile? file, string folderName, CancellationToken cancellationToken)
    {
        if (file is null)
            return null;

        var physicalPath = await _fileStorage.SaveAsync(file, folderName, cancellationToken);
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
    private async Task DeleteIfExistsAsync(string? physicalPath, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(physicalPath))
            await _fileStorage.DeleteAsync(physicalPath, cancellationToken);
    }
}
