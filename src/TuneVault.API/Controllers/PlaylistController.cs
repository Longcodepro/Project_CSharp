using Microsoft.AspNetCore.Mvc;
using TuneVault.API.Controllers;
using TuneVault.Application.DTOs.Playlist;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// CONTROLLER - PLAYLIST FEATURE (Web API Layer)
/// =============================================
/// Mục đích: Xử lý HTTP requests/responses cho tất cả Playlist endpoints.
/// 
/// Endpoints:
/// - GET    /api/Playlist?userId=xxx               → Lấy danh sách playlist của user
/// - POST   /api/Playlist                          → Tạo playlist mới
/// - DELETE /api/Playlist/{playlistId}             → Xóa playlist
/// - POST   /api/Playlist/{playlistId}/tracks      → Thêm track vào playlist
/// - DELETE /api/Playlist/{playlistId}/tracks/{mediaItemId} → Xóa track khỏi playlist
/// </summary>

public sealed class PlaylistController : BaseApiController
{
    private readonly IPlaylistRepository _playlistRepository;

    public PlaylistController(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Lấy danh sách playlist của user theo userId.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] string userId)
    {
        var playlists = await _playlistRepository.GetByOwnerIdAsync(userId);
        var result = playlists.Select(p => new PlaylistDto(
            p.Id,
            p.UserId,
            p.Title,
            p.CoverImageUrl,
            p.IsPublic,
            p.CreatedAt
        )).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Tạo playlist mới cho user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequestDto request, [FromQuery] string userId)
    {
        var playlistId = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();

        var playlist = new Playlist(
            playlistId,
            userId,
            request.Title,
            null,
            request.CoverImgUrl,
            request.IsPublic
        );

        await _playlistRepository.AddAsync(playlist);

        var result = new PlaylistDto(
            playlist.Id,
            playlist.UserId,
            playlist.Title,
            playlist.CoverImageUrl,
            playlist.IsPublic,
            playlist.CreatedAt
        );

        return CreatedAtAction(nameof(GetByUserId), new { userId }, result);
    }

    /// <summary>
    /// Xóa toàn bộ playlist theo playlistId.
    /// </summary>
    [HttpDelete("{playlistId}")]
    public async Task<IActionResult> Delete(string playlistId)
    {
        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
            return NotFound($"Playlist '{playlistId}' không tồn tại");

        await _playlistRepository.DeleteAsync(playlistId);

        return Ok(new { message = "Playlist deleted successfully" });
    }

    /// <summary>
    /// Thêm track mới vào playlist hiện có.
    /// </summary>
    [HttpPost("{playlistId}/tracks")]
    public async Task<IActionResult> AddTrack(string playlistId, [FromBody] AddTrackToPlaylistRequestDto request)
    {
        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
            return NotFound($"Playlist '{playlistId}' không tồn tại");

        var trackId = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();

        var track = new PlaylistTrack(
            trackId,
            playlistId,
            request.MediaItemId,
            request.TrackOrder
        );

        await _playlistRepository.AddTrackAsync(track);

        return Ok(new { message = "Track added successfully" });
    }

    /// <summary>
    /// Xóa track khỏi playlist.
    /// </summary>
    [HttpDelete("{playlistId}/tracks/{mediaItemId}")]
    public async Task<IActionResult> RemoveTrack(string playlistId, string mediaItemId)
    {
        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
            return NotFound($"Playlist '{playlistId}' không tồn tại");

        await _playlistRepository.RemoveTrackAsync(playlistId, mediaItemId);

        return Ok(new { message = "Track removed successfully" });
    }
}