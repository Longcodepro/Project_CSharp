namespace TuneVault.Application.Features.Notification.DTOs;

/// <summary>
/// Request body dùng để tạo thông báo hệ thống.
/// </summary>
public sealed record SystemNotificationRequest(
    string UserId,
    string Title,
    string Message,
    string? SenderId = null);