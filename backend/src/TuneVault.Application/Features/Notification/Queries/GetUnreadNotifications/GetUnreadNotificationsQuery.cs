using MediatR;
using TuneVault.Application.Features.Notification.DTOs;
using TuneVault.Application.Features.Notification.Queries;

namespace TuneVault.Application.Features.Notification.Queries;

public sealed record GetUnreadNotificationsQuery(string UserId, int Limit) : IRequest<IEnumerable<NotificationDto>>;

public sealed class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly INotificationQueryRepository _notificationRepository;

    public GetUnreadNotificationsQueryHandler(INotificationQueryRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(GetUnreadNotificationsQuery request, CancellationToken ct)
    {
        var notifications = await _notificationRepository.GetUnreadNotificationsAsync(request.UserId, request.Limit);
        return notifications.Select(GetNotificationsQueryHandler.MapNotification);
    }
}
