using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.DTOs.Playlist;
using TuneVault.Application.Features.Playlist.Commands.AddTrackToPlaylist;
using TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;
using TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;
using TuneVault.Application.Features.Playlist.Commands.RemoveTrackFromPlaylist;
using TuneVault.Application.Features.Playlist.Commands.UpdateTrackOrder;
using TuneVault.Application.Features.Playlist.Queries.GetPlaylists;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// CONTROLLER - PLAYLIST FEATURE (Web API Layer)
/// =============================================
/// Mục đích: Nhận HTTP request, tạo Command/Query và chuyển cho Handler xử lý.
/// Controller không chứa bất kỳ logic nghiệp vụ nào.
/// 
/// Luồng xử lý:
/// Controller → Command/Query → Handler → Entity (validate) → Repository → Database
/// 
/// Endpoints:
/// - GET    /api/Playlist?userId=xxx                              → Lấy danh sách playlist của user
/// - POST   /api/Playlist?userId=xxx                              → Tạo playlist mới
/// - DELETE /api/Playlist/{playlistId}                            → Xóa playlist
/// - POST   /api/Playlist/{playlistId}/tracks                     → Thêm track vào playlist
/// - DELETE /api/Playlist/{playlistId}/tracks/{mediaItemId}       → Xóa track khỏi playlist
/// - PATCH  /api/Playlist/{playlistId}/tracks/{mediaItemId}/order → Cập nhật thứ tự track
/// </summary>
public sealed class PlaylistController : BaseApiController
{
    private readonly GetPlaylistsQueryHandler _getPlaylistsHandler;
    private readonly CreatePlaylistCommandHandler _createHandler;
    private readonly DeletePlaylistCommandHandler _deleteHandler;
    private readonly AddTrackToPlaylistCommandHandler _addTrackHandler;
    private readonly RemoveTrackFromPlaylistCommandHandler _removeTrackHandler;
    private readonly UpdateTrackOrderCommandHandler _updateTrackOrderHandler;

    /// <summary>
    /// Khởi tạo Controller với các Handlers được inject qua DI container.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public PlaylistController(IPlaylistRepository playlistRepository)
    {
        _getPlaylistsHandler = new GetPlaylistsQueryHandler(playlistRepository);
        _createHandler = new CreatePlaylistCommandHandler(playlistRepository);
        _deleteHandler = new DeletePlaylistCommandHandler(playlistRepository);
        _addTrackHandler = new AddTrackToPlaylistCommandHandler(playlistRepository);
        _removeTrackHandler = new RemoveTrackFromPlaylistCommandHandler(playlistRepository);
        _updateTrackOrderHandler = new UpdateTrackOrderCommandHandler(playlistRepository);
    }

    /// <summary>
    /// Lấy danh sách playlist của user theo userId.
    /// Tạo Query và chuyển cho GetPlaylistsQueryHandler xử lý.
    /// </summary>
    /// <param name="userId">Mã user sở hữu playlist.</param>
    /// <returns>Danh sách PlaylistDto.</returns>
    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] string userId)
    {
        var query = new GetPlaylistsQuery(userId);
        var result = await _getPlaylistsHandler.HandleAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Tạo playlist mới cho user.
    /// Tạo Command và chuyển cho CreatePlaylistCommandHandler xử lý.
    /// </summary>
    /// <param name="request">Dữ liệu tạo playlist.</param>
    /// <param name="userId">Mã user tạo playlist.</param>
    /// <returns>PlaylistDto đã tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequestDto request, [FromQuery] string userId)
    {
        var command = new CreatePlaylistCommand(userId, request);
        var result = await _createHandler.HandleAsync(command);
        return CreatedAtAction(nameof(GetByUserId), new { userId }, result);
    }

    /// <summary>
    /// Xóa toàn bộ playlist theo playlistId.
    /// Tạo Command và chuyển cho DeletePlaylistCommandHandler xử lý.
    /// </summary>
    /// <param name="playlistId">Mã playlist cần xóa.</param>
    /// <returns>Thông báo thành công hoặc NotFound.</returns>
    [HttpDelete("{playlistId}")]
    public async Task<IActionResult> Delete(string playlistId)
    {
        try
        {
            var command = new DeletePlaylistCommand(playlistId);
            await _deleteHandler.HandleAsync(command);
            return Ok(new { message = "Playlist deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Thêm track mới vào playlist hiện có.
    /// Tạo Command và chuyển cho AddTrackToPlaylistCommandHandler xử lý.
    /// </summary>
    /// <param name="playlistId">Mã playlist.</param>
    /// <param name="request">Dữ liệu track cần thêm.</param>
    /// <returns>Thông báo thành công hoặc NotFound.</returns>
    [HttpPost("{playlistId}/tracks")]
    public async Task<IActionResult> AddTrack(string playlistId, [FromBody] AddTrackToPlaylistRequestDto request)
    {
        try
        {
            var command = new AddTrackToPlaylistCommand(request);
            await _addTrackHandler.HandleAsync(command);
            return Ok(new { message = "Track added successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Xóa track khỏi playlist.
    /// Tạo Command và chuyển cho RemoveTrackFromPlaylistCommandHandler xử lý.
    /// </summary>
    /// <param name="playlistId">Mã playlist.</param>
    /// <param name="mediaItemId">Mã media item cần xóa.</param>
    /// <returns>Thông báo thành công hoặc NotFound.</returns>
    [HttpDelete("{playlistId}/tracks/{mediaItemId}")]
    public async Task<IActionResult> RemoveTrack(string playlistId, string mediaItemId)
    {
        try
        {
            var command = new RemoveTrackFromPlaylistCommand(playlistId, mediaItemId);
            await _removeTrackHandler.HandleAsync(command);
            return Ok(new { message = "Track removed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Cập nhật thứ tự (TrackOrder) của một track trong playlist.
    /// Tạo Command và chuyển cho UpdateTrackOrderCommandHandler xử lý.
    /// </summary>
    /// <param name="playlistId">Mã playlist.</param>
    /// <param name="mediaItemId">Mã media item cần cập nhật thứ tự.</param>
    /// <param name="newTrackOrder">Thứ tự mới của track (từ 1 đến 100).</param>
    /// <returns>Thông báo thành công hoặc NotFound.</returns>
    [HttpPatch("{playlistId}/tracks/{mediaItemId}/order")]
    public async Task<IActionResult> UpdateTrackOrder(string playlistId, string mediaItemId, [FromQuery] int newTrackOrder)
    {
        try
        {
            var command = new UpdateTrackOrderCommand(playlistId, mediaItemId, newTrackOrder);
            await _updateTrackOrderHandler.HandleAsync(command);
            return Ok(new { message = "Track order updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}