namespace TuneVault.Application.Features.Notification.DTOs;

/// <summary>
/// DTO cho số lượng thông báo chưa đọc của một người dùng.
/// </summary>
public sealed record UnreadNotificationCountDto(
    string UserId,
    int UnreadCount);