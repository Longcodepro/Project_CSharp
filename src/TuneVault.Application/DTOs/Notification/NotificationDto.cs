namespace TuneVault.Application.DTOs.Notification;

public sealed record NotificationDto(
    string Id,
    string UserId,
    string Type,
    string? PayloadJson,
    bool IsRead,
    DateTime CreatedAt);
