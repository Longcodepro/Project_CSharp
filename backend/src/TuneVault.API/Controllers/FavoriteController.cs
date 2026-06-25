using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;
using TuneVault.Application.Features.Favorite.DTOs;
using TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;
using TuneVault.Application.Features.Favorite.Queries.CountFavoriteReactions;
using TuneVault.Application.Features.Favorite.Queries.GetFavorites;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller cung cấp các API tương tác yêu thích/cảm xúc của người dùng với media, album và playlist trong TuneVault.
/// </summary>
[Authorize]
[Route("api/favorites")]
public sealed class FavoriteController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public FavoriteController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Thiết lập hoặc cập nhật cảm xúc cho một bài hát hoặc video của người dùng hiện tại.
    /// </summary>
    /// <param name="mediaId">Mã media cần tương tác.</param>
    /// <param name="request">Cảm xúc muốn lưu. Nếu không gửi body thì mặc định là Like.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với trạng thái thành công.</returns>
    [HttpPut("{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> React(
        string mediaId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] FavoriteReactionRequestDto? request,
        CancellationToken ct)
    {
        return await ReactTargetAsync(mediaId, FavoriteTargetType.Media, request, ct);
    }

    /// <summary>
    /// Thiết lập hoặc cập nhật cảm xúc cho một album của người dùng hiện tại.
    /// </summary>
    [HttpPut("albums/{albumId}")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReactAlbum(
        string albumId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] FavoriteReactionRequestDto? request,
        CancellationToken ct)
    {
        return await ReactTargetAsync(albumId, FavoriteTargetType.Album, request, ct);
    }

    /// <summary>
    /// Thiết lập hoặc cập nhật cảm xúc cho một playlist của người dùng hiện tại.
    /// </summary>
    [HttpPut("playlists/{playlistId}")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReactPlaylist(
        string playlistId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] FavoriteReactionRequestDto? request,
        CancellationToken ct)
    {
        return await ReactTargetAsync(playlistId, FavoriteTargetType.Playlist, request, ct);
    }

    /// <summary>
    /// Lấy danh sách các loại cảm xúc hợp lệ cho Swagger hoặc frontend render lựa chọn.
    /// </summary>
    /// <returns>ApiResponse chứa danh sách tên và giá trị enum của cảm xúc.</returns>
    [HttpGet("reactions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<object>>), StatusCodes.Status200OK)]
    public IActionResult GetAvailableReactions()
    {
        var reactions = Enum.GetValues<FavoriteReaction>()
            .Where(reaction => reaction != FavoriteReaction.Remove)
            .Select(reaction => new
            {
                name = reaction.ToString(),
                value = (int)reaction
            })
            .ToList();

        return Ok(ApiResponse<IReadOnlyCollection<object>>.Ok(reactions, "Lấy danh sách cảm xúc thành công."));
    }

    /// <summary>
    /// Đếm tổng số lượt thể hiện cảm xúc của một media.
    /// </summary>
    [HttpGet("{mediaId}/reaction-count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReactionCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountMediaReactions(string mediaId, CancellationToken ct)
    {
        return await CountTargetReactionsAsync(mediaId, FavoriteTargetType.Media, ct);
    }

    /// <summary>
    /// Đếm tổng số lượt thể hiện cảm xúc của một album.
    /// </summary>
    [HttpGet("albums/{albumId}/reaction-count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReactionCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountAlbumReactions(string albumId, CancellationToken ct)
    {
        return await CountTargetReactionsAsync(albumId, FavoriteTargetType.Album, ct);
    }

    /// <summary>
    /// Đếm tổng số lượt thể hiện cảm xúc của một playlist.
    /// </summary>
    [HttpGet("playlists/{playlistId}/reaction-count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReactionCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountPlaylistReactions(string playlistId, CancellationToken ct)
    {
        return await CountTargetReactionsAsync(playlistId, FavoriteTargetType.Playlist, ct);
    }

    /// <summary>
    /// Unlike một bài hát hoặc video cho người dùng hiện tại.
    /// </summary>
    /// <param name="mediaId">Mã media cần unlike.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với trạng thái thành công.</returns>
    [HttpDelete("{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unlike(string mediaId, CancellationToken ct)
    {
        return await RemoveTargetReactionAsync(mediaId, FavoriteTargetType.Media, ct);
    }

    /// <summary>
    /// Xóa cảm xúc hiện tại của người dùng với một album.
    /// </summary>
    [HttpDelete("albums/{albumId}")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnlikeAlbum(string albumId, CancellationToken ct)
    {
        return await RemoveTargetReactionAsync(albumId, FavoriteTargetType.Album, ct);
    }

    /// <summary>
    /// Xóa cảm xúc hiện tại của người dùng với một playlist.
    /// </summary>
    [HttpDelete("playlists/{playlistId}")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnlikePlaylist(string playlistId, CancellationToken ct)
    {
        return await RemoveTargetReactionAsync(playlistId, FavoriteTargetType.Playlist, ct);
    }

    /// <summary>
    /// Lấy danh sách bài hát và cảm xúc mà người dùng đã thể hiện.
    /// </summary>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với danh sách tên bài hát và reaction đã chọn.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<List<FavoriteSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites(CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var query = new GetFavoritesQuery(userId);
        var result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<List<FavoriteSummaryDto>>.Ok(result, "Lấy danh sách cảm xúc đã thể hiện thành công."));
    }

    /// <summary>
    /// Lấy cảm xúc hiện tại của người dùng với một media.
    /// </summary>
    /// <param name="mediaId">Mã định danh của bài hát.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse chứa reaction hiện tại, hoặc null nếu chưa thể hiện cảm xúc.</returns>
    [HttpGet("status/{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckFavoriteStatus(string mediaId, CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var query = new CheckFavoriteStatusQuery(userId, mediaId, FavoriteTargetType.Media);
        var result = await _mediator.Send(query, ct);
        var message = result is null
            ? "Bạn chưa thể hiện cảm xúc với media này."
            : "Lấy cảm xúc hiện tại thành công.";

        return Ok(ApiResponse<FavoriteReaction?>.Ok(result, message));
    }

    /// <summary>
    /// Lấy cảm xúc hiện tại của người dùng với một album.
    /// </summary>
    [HttpGet("albums/{albumId}/status")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckAlbumFavoriteStatus(string albumId, CancellationToken ct)
    {
        return await CheckTargetFavoriteStatusAsync(albumId, FavoriteTargetType.Album, "album", ct);
    }

    /// <summary>
    /// Lấy cảm xúc hiện tại của người dùng với một playlist.
    /// </summary>
    [HttpGet("playlists/{playlistId}/status")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReaction?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckPlaylistFavoriteStatus(string playlistId, CancellationToken ct)
    {
        return await CheckTargetFavoriteStatusAsync(playlistId, FavoriteTargetType.Playlist, "playlist", ct);
    }

    private async Task<IActionResult> ReactTargetAsync(
        string targetId,
        FavoriteTargetType targetType,
        FavoriteReactionRequestDto? request,
        CancellationToken ct)
    {
        var reaction = request?.Reaction ?? FavoriteReaction.Like;
        var result = await _mediator.Send(new ToggleFavoriteCommand(targetId, targetType, reaction), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private async Task<IActionResult> RemoveTargetReactionAsync(
        string targetId,
        FavoriteTargetType targetType,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleFavoriteCommand(targetId, targetType, FavoriteReaction.Remove), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private async Task<IActionResult> CheckTargetFavoriteStatusAsync(
        string targetId,
        FavoriteTargetType targetType,
        string displayName,
        CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var query = new CheckFavoriteStatusQuery(userId, targetId, targetType);
        var result = await _mediator.Send(query, ct);
        var message = result is null
            ? $"Bạn chưa thể hiện cảm xúc với {displayName} này."
            : "Lấy cảm xúc hiện tại thành công.";

        return Ok(ApiResponse<FavoriteReaction?>.Ok(result, message));
    }

    private async Task<IActionResult> CountTargetReactionsAsync(
        string targetId,
        FavoriteTargetType targetType,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new CountFavoriteReactionsQuery(targetId, targetType), ct);
        return Ok(ApiResponse<FavoriteReactionCountDto>.Ok(result, "Lấy số lượt thể hiện cảm xúc thành công."));
    }
}
