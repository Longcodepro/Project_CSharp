using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Application.Features.History.Commands.RecordPlayHistory;
using TuneVault.Application.Features.History.Queries.GetRecentHistory;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller cung cấp các API lịch sử nghe nhạc của người dùng trong TuneVault.
/// </summary>
[Authorize]
[Route("api/history")]
public sealed class HistoryController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public HistoryController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Lấy danh sách bài hát nghe gần đây của một người dùng.
    /// </summary>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với danh sách các MediaItemDto.</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(ApiResponse<List<MediaItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<MediaItemDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentHistory(CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<List<MediaItemDto>>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var query = new GetRecentHistoryQuery(userId);
        var result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<List<MediaItemDto>>.Ok(result, "Lấy lịch sử phát gần đây thành công."));
    }

    /// <summary>
    /// Ghi nhận một lần nghe bài hát của người dùng vào PlayHistory.
    /// </summary>
    /// <param name="mediaId">Mã media được phát.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với trạng thái thành công.</returns>
    [HttpPost("{mediaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RecordPlayHistoryByMediaId(string mediaId, CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var result = await _mediator.Send(new RecordPlayHistoryCommand(userId, mediaId, DateTime.UtcNow), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

}
