using Microsoft.AspNetCore.Mvc;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// SUMMARY PHẦN ALBUM - API CONTROLLER
/// File này tạo endpoint API cho chức năng album.
/// 
/// Nhiệm vụ được cover:
/// - POST   /api/album                         -> tạo album.
/// - GET    /api/album/{id}                    -> xem album theo Id.
/// - GET    /api/album/owner/{ownerId}         -> xem album của owner/user.
/// - PUT    /api/album/{id}                    -> sửa album.
/// - DELETE /api/album/{id}                    -> xóa album.
/// - PATCH  /api/album/{id}/visibility         -> đặt công khai / riêng tư.
/// - GET    /api/album/{albumId}/tracks        -> xem bài trong album.
/// - POST   /api/album/{albumId}/tracks        -> thêm bài vào album.
/// - DELETE /api/album/{albumId}/tracks/{id}   -> xóa bài khỏi album.
/// - PUT    /api/album/{albumId}/tracks/{id}/order -> sắp xếp bài.
/// 
/// Controller chỉ nhận request, validate đơn giản, rồi gọi Repository.
/// SQL nằm bên AlbumRepository, không viết SQL ở Controller.
/// </summary>
public sealed class AlbumController : BaseApiController
{
    private readonly IAlbumRepository _albumRepository;

    public AlbumController(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
    }

    /// <summary>
    /// Xem album theo Id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(id, cancellationToken);
        return album is null ? NotFound(new { message = "Album không tồn tại" }) : Ok(album);
    }

    /// <summary>
    /// Xem danh sách album của một owner/user.
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId, CancellationToken cancellationToken)
    {
        var albums = await _albumRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
        return Ok(albums);
    }

    /// <summary>
    /// Xem danh sách bài trong album.
    /// </summary>
    [HttpGet("{albumId}/tracks")]
    public async Task<IActionResult> GetTracks(string albumId, CancellationToken cancellationToken)
    {
        var tracks = await _albumRepository.GetTracksAsync(albumId, cancellationToken);
        return Ok(tracks);
    }

    /// <summary>
    /// Tạo album mới.
    /// Nếu không truyền Id thì tự sinh Id dạng Axxx.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerId))
            return BadRequest(new { message = "OwnerId không được để trống" });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title không được để trống" });

        var id = string.IsNullOrWhiteSpace(request.Id) ? GenerateAlbumId() : request.Id.Trim();

        await _albumRepository.CreateAsync(
            id,
            request.OwnerId.Trim(),
            request.Title.Trim(),
            request.CoverImgUrl?.Trim(),
            request.ReleaseDate,
            request.IsPublic,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Tạo album thành công" });
    }

    /// <summary>
    /// Sửa thông tin album.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title không được để trống" });

        await _albumRepository.UpdateAsync(
            id,
            request.Title.Trim(),
            request.CoverImgUrl?.Trim(),
            request.ReleaseDate,
            request.IsPublic,
            cancellationToken);

        return Ok(new { message = "Sửa album thành công" });
    }

    /// <summary>
    /// Xóa album.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _albumRepository.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Xóa album thành công" });
    }

    /// <summary>
    /// Đặt album công khai hoặc riêng tư.
    /// </summary>
    [HttpPatch("{id}/visibility")]
    public async Task<IActionResult> SetVisibility(string id, [FromBody] AlbumSetVisibilityRequest request, CancellationToken cancellationToken)
    {
        await _albumRepository.SetVisibilityAsync(id, request.IsPublic, cancellationToken);
        return Ok(new { message = request.IsPublic ? "Album đã được đặt công khai" : "Album đã được đặt riêng tư" });
    }

    /// <summary>
    /// Thêm bài vào album.
    /// </summary>
    [HttpPost("{albumId}/tracks")]
    public async Task<IActionResult> AddTrack(string albumId, [FromBody] AlbumAddTrackRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MediaItemId))
            return BadRequest(new { message = "MediaItemId không được để trống" });

        if (request.TrackOrder < 1)
            return BadRequest(new { message = "TrackOrder phải lớn hơn hoặc bằng 1" });

        await _albumRepository.AddTrackAsync(albumId, request.MediaItemId.Trim(), request.TrackOrder, cancellationToken);
        return Ok(new { message = "Thêm bài vào album thành công" });
    }

    /// <summary>
    /// Xóa bài khỏi album.
    /// </summary>
    [HttpDelete("{albumId}/tracks/{mediaItemId}")]
    public async Task<IActionResult> RemoveTrack(string albumId, string mediaItemId, CancellationToken cancellationToken)
    {
        await _albumRepository.RemoveTrackAsync(albumId, mediaItemId, cancellationToken);
        return Ok(new { message = "Xóa bài khỏi album thành công" });
    }

    /// <summary>
    /// Sắp xếp lại thứ tự bài trong album.
    /// </summary>
    [HttpPut("{albumId}/tracks/{mediaItemId}/order")]
    public async Task<IActionResult> ReorderTrack(string albumId, string mediaItemId, [FromBody] AlbumReorderTrackRequest request, CancellationToken cancellationToken)
    {
        if (request.TrackOrder < 1)
            return BadRequest(new { message = "TrackOrder phải lớn hơn hoặc bằng 1" });

        await _albumRepository.ReorderTrackAsync(albumId, mediaItemId, request.TrackOrder, cancellationToken);
        return Ok(new { message = "Sắp xếp bài trong album thành công" });
    }

    /// <summary>
    /// Sinh Id tạm cho album nếu người dùng không truyền Id.
    /// </summary>
    private static string GenerateAlbumId()
        => $"A{Random.Shared.Next(100, 1000)}";
}

/// <summary>
/// Body dùng khi tạo album.
/// </summary>
public sealed record CreateAlbumRequest(
    string? Id,
    string OwnerId,
    string Title,
    string? CoverImgUrl,
    DateTime? ReleaseDate,
    bool IsPublic);

/// <summary>
/// Body dùng khi sửa album.
/// </summary>
public sealed record UpdateAlbumRequest(
    string Title,
    string? CoverImgUrl,
    DateTime? ReleaseDate,
    bool IsPublic);

/// <summary>
/// Body dùng khi đổi trạng thái công khai / riêng tư của album.
/// </summary>
public sealed record AlbumSetVisibilityRequest(bool IsPublic);

/// <summary>
/// Body dùng khi thêm bài vào album.
/// </summary>
public sealed record AlbumAddTrackRequest(string MediaItemId, int TrackOrder);

/// <summary>
/// Body dùng khi sắp xếp thứ tự bài trong album.
/// </summary>
public sealed record AlbumReorderTrackRequest(int TrackOrder);
