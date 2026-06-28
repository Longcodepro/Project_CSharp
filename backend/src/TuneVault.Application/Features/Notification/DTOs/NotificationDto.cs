namespace TuneVault.Application.Features.Notification.DTOs;

public sealed record NotificationDto(
    string Id,
    string UserId,
    string? SenderId,
    string? SenderIdDisplay,
    string? SenderDisplayName,
    string? SenderAvatarUrl,
    string Type,
    string? Title,
    string? Message,
    int? TargetType,
    string? TargetId,
    string? PayloadJson,
    bool IsRead,
    DateTime CreatedAt);
