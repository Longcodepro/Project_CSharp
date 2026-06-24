using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.CollectionLike.Commands.ToggleCollectionLike;
using TuneVault.Application.Features.CollectionLike.DTOs;
using TuneVault.Application.Features.CollectionLike.Queries.GetRecentCollectionLikes;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý lượt thích album và playlist của người dùng hiện tại.
/// </summary>
[ApiController]
[Authorize]
[Route("api/collection-likes")]
public sealed class CollectionLikesController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Khởi tạo controller với MediatR sender.
    /// </summary>
    public CollectionLikesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy các album/playlist người dùng đã thích gần nhất để render sidebar.
    /// </summary>
    /// <param name="limit">Số lượng tối đa, mặc định 3.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách album/playlist đã thích.</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CollectionLikeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 3, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRecentCollectionLikesQuery(limit), ct);
        return Ok(ApiResponse<IReadOnlyCollection<CollectionLikeDto>>.Ok(
            result,
            "Lấy danh sách album/playlist đã thích thành công."));
    }

    /// <summary>
    /// Bật/tắt lượt thích cho album hoặc playlist.
    /// </summary>
    /// <param name="request">Thông tin album/playlist cần thích hoặc bỏ thích.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>True nếu sau thao tác đang thích, false nếu đã bỏ thích.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Toggle([FromBody] CollectionLikeRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ToggleCollectionLikeCommand(request.TargetId, request.TargetType),
            ct);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
