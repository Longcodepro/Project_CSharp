using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Friend.Commands.AcceptFriendRequest;
using TuneVault.Application.Features.Friend.Commands.CancelFriendRequest;
using TuneVault.Application.Features.Friend.Commands.RejectFriendRequest;
using TuneVault.Application.Features.Friend.Commands.RemoveFriend;
using TuneVault.Application.Features.Friend.Commands.SendFriendRequest;
using TuneVault.Application.Features.Friend.DTOs;
using TuneVault.Application.Features.Friend.Queries.GetIncomingFriendRequests;
using TuneVault.Application.Features.Friend.Queries.GetMyFriends;
using TuneVault.Application.Features.Friend.Queries.GetSentFriendRequests;
using TuneVault.Domain.Interfaces;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý lời mời kết bạn và danh sách bạn bè của người dùng hiện tại.
/// </summary>
[ApiController]
[Route("api/friends")]
[Authorize]
public sealed class FriendsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo controller bạn bè.
    /// </summary>
    public FriendsController(ISender mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
    }

    /// <summary>
    /// Gửi lời mời kết bạn tới một user khác.
    /// </summary>
    [HttpPost("requests/{receiverId}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendRequest(string receiverId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        var requestId = await _mediator.Send(new SendFriendRequestCommand(currentUserId, receiverId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(new { requestId }, "Gửi lời mời kết bạn thành công."));
    }

    /// <summary>
    /// Chấp nhận một lời mời kết bạn đang chờ xử lý.
    /// </summary>
    [HttpPost("requests/{requestId}/accept")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptRequest(string requestId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        await _mediator.Send(new AcceptFriendRequestCommand(currentUserId, requestId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Chấp nhận lời mời kết bạn thành công."));
    }

    /// <summary>
    /// Từ chối một lời mời kết bạn đang chờ xử lý.
    /// </summary>
    [HttpPost("requests/{requestId}/reject")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RejectRequest(string requestId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        await _mediator.Send(new RejectFriendRequestCommand(currentUserId, requestId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Từ chối lời mời kết bạn thành công."));
    }

    /// <summary>
    /// Hủy một lời mời kết bạn do chính người dùng hiện tại đã gửi.
    /// </summary>
    [HttpDelete("requests/{requestId}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelRequest(string requestId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        await _mediator.Send(new CancelFriendRequestCommand(currentUserId, requestId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Hủy lời mời kết bạn thành công."));
    }

    /// <summary>
    /// Xóa một người khỏi danh sách bạn bè hiện tại.
    /// </summary>
    [HttpDelete("{friendUserId}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFriend(string friendUserId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        await _mediator.Send(new RemoveFriendCommand(currentUserId, friendUserId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Xóa bạn bè thành công."));
    }

    /// <summary>
    /// Lấy danh sách bạn bè của người dùng hiện tại.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<FriendDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyFriends(CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        var result = await _mediator.Send(new GetMyFriendsQuery(currentUserId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<FriendDto>>.Ok(result, result.Count == 0
            ? "Bạn chưa có bạn bè nào."
            : "Lấy danh sách bạn bè thành công."));
    }

    /// <summary>
    /// Lấy danh sách lời mời kết bạn người dùng hiện tại nhận được.
    /// </summary>
    [HttpGet("requests/inbox")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<FriendRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIncomingRequests(CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        var result = await _mediator.Send(new GetIncomingFriendRequestsQuery(currentUserId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<FriendRequestDto>>.Ok(result, result.Count == 0
            ? "Bạn chưa có lời mời kết bạn nào."
            : "Lấy danh sách lời mời kết bạn đã nhận thành công."));
    }

    /// <summary>
    /// Lấy danh sách lời mời kết bạn người dùng hiện tại đã gửi.
    /// </summary>
    [HttpGet("requests/sent")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<FriendRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSentRequests(CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        var result = await _mediator.Send(new GetSentFriendRequestsQuery(currentUserId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<FriendRequestDto>>.Ok(result, result.Count == 0
            ? "Bạn chưa gửi lời mời kết bạn nào."
            : "Lấy danh sách lời mời kết bạn đã gửi thành công."));
    }

    /// <summary>
    /// Lấy user id hiện tại từ JWT context và chặn request chưa đăng nhập.
    /// </summary>
    private string GetCurrentUserIdOrThrow()
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để thực hiện thao tác này.");

        return currentUserId;
    }
}
