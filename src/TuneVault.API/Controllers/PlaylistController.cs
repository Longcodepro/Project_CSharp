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
/// Luồng xử lý Request:
/// 1. Client gửi HTTP request (GET/POST)
/// 2. Controller nhận request
/// 3. IPlaylistRepository được DI inject
/// 4. Repository -> Database -> Entity -> DTO
/// 5. Controller -> HTTP Response (JSON)
/// 
/// Endpoints:
/// - GET /api/playlist?userId=xxx
///   → GetByUserId(): Lấy danh sách playlist của user
///   → Return: PlaylistDto[]
///
/// - POST /api/playlist
///   → Create(CreatePlaylistRequestDto, userId): Tạo playlist mới
///   → Input: Title, IsPublic, CoverImgUrl
///   → Return: PlaylistDto (201 Created)
///
/// - POST /api/playlist/{playlistId}/tracks
///   → AddTrack(playlistId, AddTrackToPlaylistRequestDto): Thêm track vào playlist
///   → Input: MediaItemId, TrackOrder
///   → Return: Success message
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
    /// <param name="userId">Mã user sở hữu playlist.</param>
    /// <returns>Danh sách PlaylistDto.</returns>
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
    /// <param name="request">Dữ liệu tạo playlist.</param>
    /// <param name="userId">Mã user tạo playlist.</param>
    /// <returns>PlaylistDto đã tạo.</returns>
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
    /// Thêm track mới vào playlist hiện có.
    /// </summary>
    /// <param name="playlistId">Mã playlist.</param>
    /// <param name="request">Dữ liệu track cần thêm.</param>
    /// <returns>Thông báo thành công hoặc NotFound nếu playlist không tồn tại.</returns>
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
}
