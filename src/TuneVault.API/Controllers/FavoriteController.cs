using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.Favorite.Commands;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller cung cấp các API tương tác yêu thích/cảm xúc của người dùng với bài hát trong TuneVault.
/// Controller không gọi DAO trực tiếp nữa.
/// </summary>
public sealed class FavoriteController : BaseApiController
{
    private readonly ToggleFavoriteCommand _toggleFavoriteCommand;

    public FavoriteController(ToggleFavoriteCommand toggleFavoriteCommand)
    {
        _toggleFavoriteCommand = toggleFavoriteCommand;
    }

    /// <summary>
    /// Đánh dấu một bài hát là Like cho người dùng.
    /// Nếu bài hát đã có trạng thái Favorite thì cập nhật lại thành Like.
    /// </summary>
    [HttpPost("like")]
    public async Task<IActionResult> Like(
        [FromBody] FavoriteStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _toggleFavoriteCommand.SetReactionAsync(
            request.UserId,
            request.MediaItemId,
            "Like",
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Đã Like bài hát",
            request.UserId,
            request.MediaItemId,
            likeStatus = "Like"
        });
    }

    /// <summary>
    /// Đánh dấu một bài hát là Dislike cho người dùng.
    /// DB hiện tại không lưu Dislike thật, nên Dislike được hiểu là xóa Favorite.
    /// </summary>
    [HttpPost("dislike")]
    public async Task<IActionResult> Dislike(
        [FromBody] FavoriteStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _toggleFavoriteCommand.SetReactionAsync(
            request.UserId,
            request.MediaItemId,
            "Dislike",
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Đã Dislike bài hát",
            request.UserId,
            request.MediaItemId,
            likeStatus = "Dislike"
        });
    }

    /// <summary>
    /// Xóa trạng thái Like hoặc Dislike của một bài hát khỏi danh sách Favorite của người dùng.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Remove(
        [FromQuery] string userId,
        [FromQuery] string mediaItemId,
        CancellationToken cancellationToken)
    {
        await _toggleFavoriteCommand.RemoveAsync(
            userId,
            mediaItemId,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Đã xóa trạng thái Like/Dislike",
            userId,
            mediaItemId
        });
    }

    /// <summary>
    /// Lấy danh sách các bài hát mà người dùng đã Like.
    /// </summary>
    [HttpGet("liked/{userId}")]
    public async Task<IActionResult> GetLiked(
        string userId,
        CancellationToken cancellationToken)
    {
        var items = await _toggleFavoriteCommand.GetByUserIdAsync(
            userId,
            cancellationToken);

        return Ok(items);
    }

    /// <summary>
    /// Kiểm tra trạng thái Like/Dislike hiện tại của một bài hát đối với người dùng.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(
        [FromQuery] string userId,
        [FromQuery] string mediaItemId,
        CancellationToken cancellationToken)
    {
        var isFavorite = await _toggleFavoriteCommand.IsFavoriteAsync(
            userId,
            mediaItemId,
            cancellationToken);

        return Ok(new
        {
            userId,
            mediaItemId,
            isFavorite
        });
    }
}

/// <summary>
/// Request body dùng để cập nhật trạng thái Like/Dislike của một media item.
/// </summary>
public sealed record FavoriteStatusRequest(
    string UserId,
    string MediaItemId);