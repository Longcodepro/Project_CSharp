using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Playlist.Commands.AddTrackToPlaylist;
using TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;
using TuneVault.Application.Features.Playlist.Commands.DeletePlaylist;
using TuneVault.Application.Features.Playlist.Commands.RemoveTrackFromPlaylist;
using TuneVault.Application.Features.Playlist.Commands.UpdatePlaylist;
using TuneVault.Application.Features.Playlist.Commands.UpdateTrackOrder;
using TuneVault.Application.Features.Playlist.DTOs;
using TuneVault.Application.Features.Playlist.Queries.GetPlaylistById;
using TuneVault.Application.Features.Playlist.Queries.GetPlaylists;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý playlist của người dùng: CRUD playlist, public/private và track trong playlist.
/// </summary>
[ApiController]
[Route("api/playlists")]
[Authorize]
public sealed class PlaylistController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Khởi tạo controller playlist với MediatR sender.
    /// </summary>
    /// <param name="mediator">Sender dùng để gửi command/query sang Application layer.</param>
    public PlaylistController(ISender mediator) => _mediator = mediator;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? User.FindFirstValue("sub")
                                 ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Lấy danh sách playlist của người dùng hiện tại.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách playlist của user đang đăng nhập.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlaylistDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPlaylists(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPlaylistsQuery(CurrentUserId), ct);
        return Ok(ApiResponse<IEnumerable<PlaylistDto>>.Ok(result, "Lấy danh sách playlist thành công."));
    }

    /// <summary>
    /// Lấy danh sách playlist của người dùng hiện tại theo route rõ nghĩa.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách playlist của user đang đăng nhập.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlaylistDto>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMine(CancellationToken ct) => GetMyPlaylists(ct);

    /// <summary>
    /// Lấy chi tiết playlist theo id.
    /// </summary>
    /// <param name="id">Mã playlist cần lấy.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thông tin playlist kèm danh sách track.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PlaylistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPlaylistByIdQuery(id, CurrentUserId), ct);
        return result is not null
            ? Ok(ApiResponse<PlaylistDto>.Ok(result, "Lấy thông tin playlist thành công."))
            : NotFound(ApiResponse<object?>.Fail($"Không tìm thấy playlist '{id}' hoặc playlist đã bị xóa."));
    }

    /// <summary>
    /// Tạo playlist mới cho người dùng hiện tại.
    /// </summary>
    /// <param name="request">Thông tin playlist cần tạo.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Playlist vừa được tạo.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PlaylistDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreatePlaylistCommand(CurrentUserId, request), ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<PlaylistDto>.Ok(result, "Tạo playlist thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin và trạng thái public/private của playlist.
    /// </summary>
    /// <param name="id">Mã playlist cần cập nhật.</param>
    /// <param name="request">Payload cập nhật playlist.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Playlist sau khi cập nhật.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PlaylistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePlaylistRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdatePlaylistCommand(id, CurrentUserId, request), ct);
        return Ok(ApiResponse<PlaylistDto>.Ok(result, "Cập nhật playlist thành công."));
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
        await _mediator.Send(new DeletePlaylistCommand(id, CurrentUserId), ct);
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
        await _mediator.Send(new AddTrackToPlaylistCommand(id, CurrentUserId, request), ct);
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
        await _mediator.Send(new RemoveTrackFromPlaylistCommand(id, CurrentUserId, mediaId), ct);
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
        await _mediator.Send(new UpdateTrackOrderCommand(playlistId, CurrentUserId, mediaItemId, newOrder), ct);
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật thứ tự bài hát thành công."));
    }
}
