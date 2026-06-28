using MediatR;
using System.Collections.Generic;
using TuneVault.Application.Features.Notification.DTOs;
using TuneVault.Application.Features.Notification.Queries;

namespace TuneVault.Application.Features.Notification.Queries;

public sealed record GetNotificationsQuery(string UserId, int Limit) : IRequest<IEnumerable<NotificationDto>>;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly INotificationQueryRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationQueryRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var notifications = await _notificationRepository.GetNotificationsAsync(request.UserId, request.Limit);
        return notifications.Select(MapNotification);
    }

    internal static NotificationDto MapNotification(dynamic n)
    {
        var row = (IDictionary<string, object?>)n;

        return new NotificationDto(
            Id: row["Id"]?.ToString() ?? string.Empty,
            UserId: row["UserId"]?.ToString() ?? string.Empty,
            SenderId: row.TryGetValue("SenderId", out var senderId) ? senderId?.ToString() : null,
            SenderIdDisplay: row.TryGetValue("SenderIdDisplay", out var senderIdDisplay) ? senderIdDisplay?.ToString() : null,
            SenderDisplayName: row.TryGetValue("SenderDisplayName", out var senderDisplayName) ? senderDisplayName?.ToString() : null,
            SenderAvatarUrl: row.TryGetValue("SenderAvatarUrl", out var senderAvatarUrl) ? senderAvatarUrl?.ToString() : null,
            Type: row.TryGetValue("Type", out var type) ? type?.ToString() ?? string.Empty : string.Empty,
            Title: row.TryGetValue("Title", out var title) ? title?.ToString() : null,
            Message: row.TryGetValue("Message", out var message) ? message?.ToString() : null,
            TargetType: row.TryGetValue("TargetType", out var targetType) && targetType is not null ? Convert.ToInt32(targetType) : null,
            TargetId: row.TryGetValue("TargetId", out var targetId) ? targetId?.ToString() : null,
            PayloadJson: row.TryGetValue("PayloadJson", out var payloadJson) ? payloadJson?.ToString() : null,
            IsRead: row.TryGetValue("IsRead", out var isRead) && Convert.ToBoolean(isRead),
            CreatedAt: row.TryGetValue("CreatedAt", out var createdAt) && createdAt is not null ? Convert.ToDateTime(createdAt) : DateTime.UtcNow);
    }
}
