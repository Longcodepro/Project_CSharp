using MediatR;
using TuneVault.Application.Features.Notification.Queries;

namespace TuneVault.Application.Features.Notification.Queries;

public sealed record CountUnreadNotificationsQuery(string UserId) : IRequest<int>;

public sealed class CountUnreadNotificationsQueryHandler : IRequestHandler<CountUnreadNotificationsQuery, int>
{
    private readonly INotificationQueryRepository _notificationRepository;

    public CountUnreadNotificationsQueryHandler(INotificationQueryRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<int> Handle(CountUnreadNotificationsQuery request, CancellationToken ct)
    {
        return await _notificationRepository.CountUnreadNotificationsAsync(request.UserId);
    }
}
