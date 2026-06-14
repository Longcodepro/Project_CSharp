using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.Share.Commands.ShareMedia;
using TuneVault.Application.Features.Share.Queries.GetSharedWithMe;

namespace TuneVault.API.Controllers;

public sealed class ShareController : BaseApiController
{
    private readonly ShareMediaCommand _shareMediaCommand;
    private readonly GetSharedWithMeQuery _getSharedWithMeQuery;

    public ShareController(
        ShareMediaCommand shareMediaCommand,
        GetSharedWithMeQuery getSharedWithMeQuery)
    {
        _shareMediaCommand = shareMediaCommand;
        _getSharedWithMeQuery = getSharedWithMeQuery;
    }

    [HttpPost("track")]
    public async Task<IActionResult> ShareTrack([FromBody] ShareItemRequest request)
    {
        try
        {
            var shareId = await _shareMediaCommand.ShareTrackAsync(
                request.SenderId,
                request.ReceiverId,
                request.SharedItemId);

            return Ok(new
            {
                success = true,
                message = "Chia sẻ Track thành công",
                shareId,
                request.SenderId,
                request.ReceiverId,
                shareType = "Track",
                request.SharedItemId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("album")]
    public async Task<IActionResult> ShareAlbum([FromBody] ShareItemRequest request)
    {
        try
        {
            var shareId = await _shareMediaCommand.ShareAlbumAsync(
                request.SenderId,
                request.ReceiverId,
                request.SharedItemId);

            return Ok(new
            {
                success = true,
                message = "Chia sẻ Album thành công",
                shareId,
                request.SenderId,
                request.ReceiverId,
                shareType = "Album",
                request.SharedItemId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("playlist")]
    public async Task<IActionResult> SharePlaylist([FromBody] ShareItemRequest request)
    {
        try
        {
            var shareId = await _shareMediaCommand.SharePlaylistAsync(
                request.SenderId,
                request.ReceiverId,
                request.SharedItemId);

            return Ok(new
            {
                success = true,
                message = "Chia sẻ Playlist thành công",
                shareId,
                request.SenderId,
                request.ReceiverId,
                shareType = "Playlist",
                request.SharedItemId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("inbox/{receiverId}")]
    public async Task<IActionResult> GetInbox(string receiverId)
    {
        var items = await _getSharedWithMeQuery.GetInboxAsync(receiverId);

        return Ok(items);
    }

    [HttpPatch("{shareId}/read")]
    public async Task<IActionResult> MarkAsRead(
        string shareId,
        [FromQuery] string receiverId)
    {
        var result = await _shareMediaCommand.MarkAsReadAsync(
            shareId,
            receiverId);

        if (!result)
            return NotFound(new
            {
                message = "Không tìm thấy chia sẻ để đánh dấu đã đọc"
            });

        return Ok(new
        {
            success = true,
            message = "Đã đánh dấu chia sẻ là đã đọc",
            shareId,
            receiverId
        });
    }

    [HttpGet("unread-count/{receiverId}")]
    public async Task<IActionResult> CountUnread(string receiverId)
    {
        var count = await _getSharedWithMeQuery.CountUnreadAsync(receiverId);

        return Ok(new
        {
            receiverId,
            unreadShareCount = count
        });
    }
}

public sealed record ShareItemRequest(
    string SenderId,
    string ReceiverId,
    string SharedItemId);