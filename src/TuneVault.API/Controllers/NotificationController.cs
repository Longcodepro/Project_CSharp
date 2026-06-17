using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Notification.DTOs;
using TuneVault.Application.Features.Notification.Queries;
using TuneVault.Application.Features.Notification.Commands.MarkAsRead;
using TuneVault.Application.Features.Notification.Commands.DeleteNotification;


namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý notification của user hiện tại.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Khởi tạo controller notification với MediatR sender.
    /// </summary>
    /// <param name="mediator">Sender dùng để gửi command/query sang Application layer.</param>
    public NotificationController(ISender mediator) => _mediator = mediator;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? User.FindFirstValue("sub")
                                 ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Lấy danh sách notification của user hiện tại.
    /// </summary>
    /// <param name="limit">Số notification tối đa.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách notification.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(CurrentUserId, limit), ct);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(result, "Lấy danh sách thông báo thành công."));
    }

    /// <summary>
    /// Lấy danh sách notification chưa đọc của user hiện tại.
    /// </summary>
    /// <param name="limit">Số notification tối đa.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách notification chưa đọc.</returns>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUnreadNotificationsQuery(CurrentUserId, limit), ct);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(result, "Lấy danh sách thông báo chưa đọc thành công."));
    }

    /// <summary>
    /// Đếm số notification chưa đọc của user hiện tại.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Số notification chưa đọc.</returns>
    [HttpGet("unread-count")]
    public async Task<IActionResult> CountUnread(CancellationToken ct)
    {
        var count = await _mediator.Send(new CountUnreadNotificationsQuery(CurrentUserId), ct);
        var result = new UnreadNotificationCountDto(CurrentUserId, count);
        return Ok(ApiResponse<UnreadNotificationCountDto>.Ok(result, "Lấy số lượng thông báo chưa đọc thành công."));
    }

    /// <summary>
    /// Đánh dấu một notification là đã đọc.
    /// </summary>
    /// <param name="notificationId">Mã notification cần đánh dấu đã đọc.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả cập nhật.</returns>
    [HttpPatch("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(string notificationId, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkNotificationAsReadCommand(notificationId, CurrentUserId), ct);
        return result
            ? Ok(ApiResponse<bool>.Ok(true, "Đánh dấu thông báo đã đọc thành công."))
            : NotFound(ApiResponse<bool>.Fail($"Không tìm thấy thông báo '{notificationId}' thuộc tài khoản hiện tại hoặc thông báo đã bị xóa."));
    }

    /// <summary>
    /// Đánh dấu toàn bộ notification của user hiện tại là đã đọc.
    /// </summary>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Số notification đã được cập nhật.</returns>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var count = await _mediator.Send(new MarkAllNotificationsAsReadCommand(CurrentUserId), ct);
        return Ok(ApiResponse<int>.Ok(count, "Đánh dấu tất cả thông báo đã đọc thành công."));
    }

    /// <summary>
    /// Xóa mềm một notification.
    /// </summary>
    /// <param name="notificationId">Mã notification cần xóa.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả xóa notification.</returns>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> Delete(string notificationId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand(notificationId, CurrentUserId), ct);
        return result
            ? Ok(ApiResponse<bool>.Ok(true, "Xóa thông báo thành công."))
            : NotFound(ApiResponse<bool>.Fail($"Không tìm thấy thông báo '{notificationId}' thuộc tài khoản hiện tại hoặc thông báo đã bị xóa."));
    }
}
