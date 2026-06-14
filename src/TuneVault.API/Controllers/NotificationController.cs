using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Notification.Queries.GetNotifications;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller cung cấp các API thông báo của TuneVault.
/// Controller không gọi DAO/Repository trực tiếp nữa.
/// </summary>
public sealed class NotificationController : BaseApiController
{
    private readonly GetNotificationsQuery _getNotificationsQuery;
    private readonly MarkNotificationAsReadCommand _markNotificationAsReadCommand;

    public NotificationController(
        GetNotificationsQuery getNotificationsQuery,
        MarkNotificationAsReadCommand markNotificationAsReadCommand)
    {
        _getNotificationsQuery = getNotificationsQuery;
        _markNotificationAsReadCommand = markNotificationAsReadCommand;
    }

    /// <summary>
    /// Lấy danh sách thông báo còn hiển thị của một người dùng.
    /// Chỉ lấy Notification có IsActive = 1.
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAll(
        string userId,
        [FromQuery] int limit = 50)
    {
        var items = await _getNotificationsQuery.GetAllAsync(userId, limit);

        return Ok(items);
    }

    /// <summary>
    /// Lấy danh sách thông báo chưa đọc của một người dùng.
    /// Chỉ lấy IsActive = 1 và IsRead = 0.
    /// </summary>
    [HttpGet("unread/{userId}")]
    public async Task<IActionResult> GetUnread(
        string userId,
        [FromQuery] int limit = 50)
    {
        var items = await _getNotificationsQuery.GetUnreadAsync(userId, limit);

        return Ok(items);
    }

    /// <summary>
    /// Đếm số lượng thông báo chưa đọc để hiển thị badge trên frontend.
    /// </summary>
    [HttpGet("unread-count/{userId}")]
    public async Task<IActionResult> CountUnread(string userId)
    {
        var count = await _getNotificationsQuery.CountUnreadAsync(userId);

        return Ok(new
        {
            userId,
            unreadNotificationCount = count
        });
    }

    /// <summary>
    /// Đánh dấu một thông báo là đã đọc.
    /// </summary>
    [HttpPatch("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(
        string notificationId,
        [FromQuery] string userId)
    {
        var result = await _markNotificationAsReadCommand.MarkAsReadAsync(
            notificationId,
            userId);

        if (!result)
            return NotFound(new
            {
                message = "Không tìm thấy thông báo để đánh dấu đã đọc"
            });

        return Ok(new
        {
            success = true,
            message = "Đã đánh dấu thông báo là đã đọc",
            notificationId,
            userId
        });
    }

    /// <summary>
    /// Xóa mềm một thông báo.
    /// Không xóa dòng trong database, chỉ chuyển IsActive = 0.
    /// </summary>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> Delete(
        string notificationId,
        [FromQuery] string userId)
    {
        var result = await _markNotificationAsReadCommand.DeleteAsync(
            notificationId,
            userId);

        if (!result)
            return NotFound(new
            {
                message = "Không tìm thấy thông báo để xóa hoặc thông báo đã bị xóa trước đó"
            });

        return Ok(new
        {
            success = true,
            message = "Đã xóa thông báo",
            notificationId,
            userId,
            isActive = false
        });
    }

    /// <summary>
    /// Xóa mềm toàn bộ thông báo của một người dùng.
    /// Không xóa database, chỉ chuyển toàn bộ IsActive = 0.
    /// </summary>
    [HttpDelete("all/{userId}")]
    public async Task<IActionResult> DeleteAll(string userId)
    {
        var affectedRows = await _markNotificationAsReadCommand.DeleteAllAsync(userId);

        return Ok(new
        {
            success = true,
            message = "Đã xóa toàn bộ thông báo của người dùng",
            userId,
            deletedCount = affectedRows,
            isActive = false
        });
    }

    /// <summary>
    /// Tạo thông báo demo khi nghệ sĩ đăng bài mới.
    /// API này dùng để test frontend phần ArtistNewMedia notification.
    /// </summary>
    [HttpPost("artist-new-media")]
    public async Task<IActionResult> CreateArtistNewMedia(
        [FromBody] ArtistNewMediaNotificationRequest request)
    {
        var notificationId = await _markNotificationAsReadCommand.CreateArtistNewMediaNotificationAsync(
            request.UserId,
            request.ArtistId,
            request.MediaItemId,
            request.Title);

        return Ok(new
        {
            success = true,
            message = "Đã tạo thông báo nghệ sĩ đăng bài mới",
            notificationId,
            request.UserId
        });
    }

    /// <summary>
    /// Tạo thông báo hệ thống cho một người dùng.
    /// API này dùng để admin hoặc frontend test SystemAlert notification.
    /// </summary>
    [HttpPost("system")]
    public async Task<IActionResult> CreateSystemAlert(
        [FromBody] SystemNotificationRequest request)
    {
        var notificationId = await _markNotificationAsReadCommand.CreateSystemAlertAsync(
            request.UserId,
            request.Title,
            request.Message,
            request.SenderId);

        return Ok(new
        {
            success = true,
            message = "Đã tạo thông báo hệ thống",
            notificationId,
            request.UserId
        });
    }
}

/// <summary>
/// Request body dùng để tạo thông báo demo khi nghệ sĩ đăng bài mới.
/// </summary>
public sealed record ArtistNewMediaNotificationRequest(
    string UserId,
    string ArtistId,
    string MediaItemId,
    string Title);

/// <summary>
/// Request body dùng để tạo thông báo hệ thống.
/// </summary>
public sealed record SystemNotificationRequest(
    string UserId,
    string Title,
    string Message,
    string? SenderId = null);