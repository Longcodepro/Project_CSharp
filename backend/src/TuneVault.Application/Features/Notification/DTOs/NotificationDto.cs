namespace TuneVault.Application.Features.Notification.DTOs;

public sealed record NotificationDto(
    string Id,
    string UserId,
    string Type,
    int? TargetType,
    string? TargetId,
    string? PayloadJson,
    bool IsRead,
    DateTime CreatedAt);
