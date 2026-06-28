using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;
using TuneVault.Application.Features.Favorite.DTOs;
using TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;
using TuneVault.Application.Features.Favorite.Queries.CountFavoriteReactions;
using TuneVault.Application.Features.Favorite.Queries.GetFavorites;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

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

    [HttpPut("{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> React(string mediaId, CancellationToken ct)
    {
        return await SetTargetFavoriteAsync(mediaId, FavoriteTargetType.Media, true, ct);
    }

    [HttpGet("{mediaId}/reaction-count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReactionCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountMediaReactions(string mediaId, CancellationToken ct)
    {
        return await CountTargetReactionsAsync(mediaId, FavoriteTargetType.Media, ct);
    }

    [HttpGet("albums/{albumId}/reaction-count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReactionCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountAlbumReactions(string albumId, CancellationToken ct)
    {
        return await CountTargetReactionsAsync(albumId, FavoriteTargetType.Album, ct);
    }

    [HttpGet("playlists/{playlistId}/reaction-count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FavoriteReactionCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountPlaylistReactions(string playlistId, CancellationToken ct)
    {
        return await CountTargetReactionsAsync(playlistId, FavoriteTargetType.Playlist, ct);
    }

    [HttpDelete("{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unlike(string mediaId, CancellationToken ct)
    {
        return await SetTargetFavoriteAsync(mediaId, FavoriteTargetType.Media, false, ct);
    }

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
        return Ok(ApiResponse<List<FavoriteSummaryDto>>.Ok(result, "Lấy danh sách yêu thích thành công."));
    }

    [HttpGet("status/{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
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
        var message = result
            ? "Media này đang được yêu thích."
            : "Bạn chưa yêu thích media này.";

        return Ok(ApiResponse<bool>.Ok(result, message));
    }

    private async Task<IActionResult> SetTargetFavoriteAsync(
        string targetId,
        FavoriteTargetType targetType,
        bool isActive,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleFavoriteCommand(targetId, targetType, isActive), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private async Task<IActionResult> CountTargetReactionsAsync(
        string targetId,
        FavoriteTargetType targetType,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return BadRequest(ApiResponse<object?>.Fail("Mã nội dung cần đếm cảm xúc là bắt buộc."));
        }

        var result = await _mediator.Send(new CountFavoriteReactionsQuery(targetId, targetType), ct);
        return Ok(ApiResponse<FavoriteReactionCountDto>.Ok(result, "Lấy số lượt yêu thích thành công."));
    }
}
