using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TuneVault.Application.Common;
using TuneVault.Application.Features.History.DTOs;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Application.Features.History.Commands.RecordPlayHistory;
using TuneVault.Application.Features.History.Queries.GetHistoryResume;
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

        var result = await _mediator.Send(new RecordPlayHistoryCommand(userId, mediaId, null), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lưu thời điểm người dùng dừng phát một media để lần sau có thể phát tiếp.
    /// </summary>
    /// <param name="mediaId">Mã media cần lưu trạng thái dừng.</param>
    /// <param name="request">Body chứa StoppedAt là vị trí dừng theo giây.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse với trạng thái ghi nhận thành công.</returns>
    [HttpPatch("{mediaId}/stop")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RecordPlaybackStop(
        string mediaId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RecordPlaybackStopRequestDto? request,
        CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        if (request?.StoppedAt is null)
        {
            return BadRequest(ApiResponse<object?>.Fail("Vị trí dừng phát StoppedAt không được để trống."));
        }

        var result = await _mediator.Send(
            new RecordPlayHistoryCommand(userId, mediaId, request.StoppedAt),
            ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy thông tin dừng phát đã lưu của một media trong lịch sử người dùng.
    /// </summary>
    /// <param name="mediaId">Mã media cần lấy trạng thái phát tiếp.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>ApiResponse chứa tên media và thời điểm dừng đã lưu.</returns>
    [HttpGet("{mediaId}/resume")]
    [ProducesResponseType(typeof(ApiResponse<HistoryResumeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResumeInfo(string mediaId, CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));
        }

        var result = await _mediator.Send(new GetHistoryResumeQuery(userId, mediaId), ct);
        return result is null
            ? NotFound(ApiResponse<object?>.Fail("Media này chưa có trong lịch sử phát của bạn."))
            : Ok(ApiResponse<HistoryResumeDto>.Ok(result, "Lấy thông tin phát tiếp thành công."));
    }

}
