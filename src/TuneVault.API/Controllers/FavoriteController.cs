using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;
using TuneVault.Application.Features.Favorite.DTOs;
using TuneVault.Application.Features.Favorite.Queries.CheckFavoriteStatus;
using TuneVault.Application.Features.Favorite.Queries.GetFavorites;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller cung cấp các API tương tác yêu thích/cảm xúc của người dùng với bài hát trong TuneVault.
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
    /// Thêm hoặc cập nhật cảm xúc cho một bài hát hoặc video của người dùng hiện tại.
    /// </summary>
    /// <param name="mediaId">Mã media cần tương tác.</param>
    /// <param name="request">Cảm xúc muốn lưu. Nếu không gửi body thì mặc định là Like.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với trạng thái thành công.</returns>
    [HttpPost("{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> React(
        string mediaId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] FavoriteReactionRequestDto? request,
        CancellationToken ct)
    {
        var reaction = request?.Reaction ?? FavoriteReaction.Like;
        var result = await _mediator.Send(new ToggleFavoriteCommand(mediaId, reaction), ct);
        return result.Success ? Ok(result) : BadRequest(result);
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
    /// Unlike một bài hát hoặc video cho người dùng hiện tại.
    /// </summary>
    /// <param name="mediaId">Mã media cần unlike.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với trạng thái thành công.</returns>
    [HttpDelete("{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unlike(string mediaId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleFavoriteCommand(mediaId, FavoriteReaction.Remove), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy danh sách các bài hát mà người dùng đã Like.
    /// </summary>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với danh sách các MediaItemDto.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<List<MediaItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<MediaItemDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites(CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<List<MediaItemDto>>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var query = new GetFavoritesQuery(userId);
        var result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<List<MediaItemDto>>.Ok(result, "Lấy danh sách yêu thích thành công."));
    }

    /// <summary>
    /// Kiểm tra trạng thái Like/Dislike hiện tại của một bài hát đối với người dùng.
    /// </summary>
    /// <param name="mediaId">Mã định danh của bài hát.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với trạng thái yêu thích (true nếu là yêu thích, false nếu không).</returns>
    [HttpGet("status/{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckFavoriteStatus(string mediaId, CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var query = new CheckFavoriteStatusQuery(userId, mediaId);
        var result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<bool>.Ok(result, "Kiểm tra trạng thái yêu thích thành công."));
    }
}
