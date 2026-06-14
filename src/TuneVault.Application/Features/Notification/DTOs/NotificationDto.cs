namespace TuneVault.Application.Features.Notification.DTOs;

public sealed record NotificationDto(
    string Id,
    string UserId,
    string Type,
    string? PayloadJson,
    bool IsRead,
    DateTime CreatedAt);