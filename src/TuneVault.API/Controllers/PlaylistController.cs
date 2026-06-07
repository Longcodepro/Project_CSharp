using Microsoft.AspNetCore.Mvc;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// SUMMARY PHẦN PLAYLIST - API CONTROLLER
/// File này tạo endpoint API cho chức năng playlist.
/// 
/// Nhiệm vụ được cover:
/// - POST   /api/playlist                         -> tạo playlist.
/// - GET    /api/playlist/{id}                    -> xem playlist theo Id.
/// - GET    /api/playlist/owner/{ownerId}         -> xem playlist của owner/user.
/// - PUT    /api/playlist/{id}                    -> sửa playlist.
/// - DELETE /api/playlist/{id}                    -> xóa playlist.
/// - PATCH  /api/playlist/{id}/visibility         -> đặt công khai / riêng tư.
/// - GET    /api/playlist/{playlistId}/tracks     -> xem bài trong playlist.
/// - POST   /api/playlist/{playlistId}/tracks     -> thêm bài vào playlist.
/// - DELETE /api/playlist/{playlistId}/tracks/{id} -> xóa bài khỏi playlist.
/// - PUT    /api/playlist/{playlistId}/tracks/{id}/order -> sắp xếp bài.
/// 
/// Controller chỉ nhận request, validate đơn giản, rồi gọi Repository.
/// SQL nằm bên PlaylistRepository, không viết SQL ở Controller.
/// </summary>
public sealed class PlaylistController : BaseApiController
{
    private readonly IPlaylistRepository _playlistRepository;

    public PlaylistController(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Xem playlist theo Id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id, cancellationToken);
        return playlist is null ? NotFound(new { message = "Playlist không tồn tại" }) : Ok(playlist);
    }

    /// <summary>
    /// Xem danh sách playlist của một owner/user.
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId, CancellationToken cancellationToken)
    {
        var playlists = await _playlistRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
        return Ok(playlists);
    }

    /// <summary>
    /// Xem danh sách bài trong playlist.
    /// </summary>
    [HttpGet("{playlistId}/tracks")]
    public async Task<IActionResult> GetTracks(string playlistId, CancellationToken cancellationToken)
    {
        var tracks = await _playlistRepository.GetTracksAsync(playlistId, cancellationToken);
        return Ok(tracks);
    }

    /// <summary>
    /// Tạo playlist mới.
    /// Nếu không truyền Id thì tự sinh Id dạng Pxxxx.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerId))
            return BadRequest(new { message = "OwnerId không được để trống" });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title không được để trống" });

        var id = string.IsNullOrWhiteSpace(request.Id) ? GeneratePlaylistId() : request.Id.Trim();

        await _playlistRepository.CreateAsync(
            id,
            request.OwnerId.Trim(),
            request.Title.Trim(),
            request.CoverImgUrl?.Trim(),
            request.IsPublic,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Tạo playlist thành công" });
    }

    /// <summary>
    /// Sửa thông tin playlist.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePlaylistRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title không được để trống" });

        await _playlistRepository.UpdateAsync(
            id,
            request.Title.Trim(),
            request.CoverImgUrl?.Trim(),
            request.IsPublic,
            cancellationToken);

        return Ok(new { message = "Sửa playlist thành công" });
    }

    /// <summary>
    /// Xóa playlist.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _playlistRepository.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Xóa playlist thành công" });
    }

    /// <summary>
    /// Đặt playlist công khai hoặc riêng tư.
    /// </summary>
    [HttpPatch("{id}/visibility")]
    public async Task<IActionResult> SetVisibility(string id, [FromBody] PlaylistSetVisibilityRequest request, CancellationToken cancellationToken)
    {
        await _playlistRepository.SetVisibilityAsync(id, request.IsPublic, cancellationToken);
        return Ok(new { message = request.IsPublic ? "Playlist đã được đặt công khai" : "Playlist đã được đặt riêng tư" });
    }

    /// <summary>
    /// Thêm bài vào playlist.
    /// </summary>
    [HttpPost("{playlistId}/tracks")]
    public async Task<IActionResult> AddTrack(string playlistId, [FromBody] PlaylistAddTrackRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MediaItemId))
            return BadRequest(new { message = "MediaItemId không được để trống" });

        if (request.TrackOrder < 1)
            return BadRequest(new { message = "TrackOrder phải lớn hơn hoặc bằng 1" });

        await _playlistRepository.AddTrackAsync(playlistId, request.MediaItemId.Trim(), request.TrackOrder, cancellationToken);
        return Ok(new { message = "Thêm bài vào playlist thành công" });
    }

    /// <summary>
    /// Xóa bài khỏi playlist.
    /// </summary>
    [HttpDelete("{playlistId}/tracks/{mediaItemId}")]
    public async Task<IActionResult> RemoveTrack(string playlistId, string mediaItemId, CancellationToken cancellationToken)
    {
        await _playlistRepository.RemoveTrackAsync(playlistId, mediaItemId, cancellationToken);
        return Ok(new { message = "Xóa bài khỏi playlist thành công" });
    }

    /// <summary>
    /// Sắp xếp lại thứ tự bài trong playlist.
    /// </summary>
    [HttpPut("{playlistId}/tracks/{mediaItemId}/order")]
    public async Task<IActionResult> ReorderTrack(string playlistId, string mediaItemId, [FromBody] PlaylistReorderTrackRequest request, CancellationToken cancellationToken)
    {
        if (request.TrackOrder < 1)
            return BadRequest(new { message = "TrackOrder phải lớn hơn hoặc bằng 1" });

        await _playlistRepository.ReorderTrackAsync(playlistId, mediaItemId, request.TrackOrder, cancellationToken);
        return Ok(new { message = "Sắp xếp bài trong playlist thành công" });
    }

    /// <summary>
    /// Sinh Id tạm cho playlist nếu người dùng không truyền Id.
    /// </summary>
    private static string GeneratePlaylistId()
        => $"P{Random.Shared.Next(1000, 10000)}";
}

/// <summary>
/// Body dùng khi tạo playlist.
/// </summary>
public sealed record CreatePlaylistRequest(
    string? Id,
    string OwnerId,
    string Title,
    string? CoverImgUrl,
    bool IsPublic);

/// <summary>
/// Body dùng khi sửa playlist.
/// </summary>
public sealed record UpdatePlaylistRequest(
    string Title,
    string? CoverImgUrl,
    bool IsPublic);

/// <summary>
/// Body dùng khi đổi trạng thái công khai / riêng tư của playlist.
/// </summary>
public sealed record PlaylistSetVisibilityRequest(bool IsPublic);

/// <summary>
/// Body dùng khi thêm bài vào playlist.
/// </summary>
public sealed record PlaylistAddTrackRequest(string MediaItemId, int TrackOrder);

/// <summary>
/// Body dùng khi sắp xếp thứ tự bài trong playlist.
/// </summary>
public sealed record PlaylistReorderTrackRequest(int TrackOrder);
