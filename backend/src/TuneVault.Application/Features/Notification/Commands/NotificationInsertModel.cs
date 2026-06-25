using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Notification.Commands;

public sealed class NotificationInsertModel
{
    public string UserId { get; set; } = string.Empty;
    public string? SenderId { get; set; }
    public NotificationType NotifyType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationTargetType? TargetType { get; set; }
    public string? TargetId { get; set; }
}
