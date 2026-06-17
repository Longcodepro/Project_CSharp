using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Share.Commands.ShareMedia;
using TuneVault.Application.Features.Share.DTOs;
using TuneVault.Application.Features.Share.Queries.GetSharedByMe;
using TuneVault.Application.Features.Share.Queries.GetSharedWithMe;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý chia sẻ media/video/playlist giữa người dùng.
/// </summary>
[ApiController]
[Route("api/shares")]
[Authorize]
public sealed class ShareController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Khởi tạo controller chia sẻ với MediatR sender.
    /// </summary>
    /// <param name="mediator">Sender dùng để gửi command/query sang Application layer.</param>
    public ShareController(ISender mediator) => _mediator = mediator;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? User.FindFirstValue("sub")
                                 ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Chia sẻ media, video hoặc playlist cho user khác.
    /// </summary>
    /// <param name="request">Payload chia sẻ gồm người nhận, item và loại item.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thông tin bản ghi chia sẻ vừa tạo.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShareResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Share([FromBody] ShareMediaRequestDto request, CancellationToken ct)
    {
        var shareId = await _mediator.Send(
            new ShareMediaCommand(CurrentUserId, request.ReceiverId, request.ShareType, request.SharedItemId, request.Message), ct);
        var result = new ShareResultDto(shareId, CurrentUserId, request.ReceiverId, request.ShareType, request.SharedItemId);
        return Ok(ApiResponse<ShareResultDto>.Ok(result, "Chia sẻ thành công."));
    }

    /// <summary>
    /// Lấy danh sách item người khác đã chia sẻ cho user hiện tại.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách chia sẻ nhận được.</returns>
    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSharedWithMeQuery(CurrentUserId), ct);
        return Ok(ApiResponse<List<SharedItemDto>>.Ok(result, "Lấy danh sách chia sẻ đã nhận thành công."));
    }

    /// <summary>
    /// Lấy danh sách item user hiện tại đã chia sẻ cho người khác.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách chia sẻ đã gửi.</returns>
    [HttpGet("sent")]
    public async Task<IActionResult> GetSent(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSharedByMeQuery(CurrentUserId), ct);
        return Ok(ApiResponse<List<SharedItemDto>>.Ok(result, "Lấy danh sách chia sẻ đã gửi thành công."));
    }
}
