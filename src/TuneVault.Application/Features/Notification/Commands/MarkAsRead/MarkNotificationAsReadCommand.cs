using MediatR;
using TuneVault.Application.Features.Notification.Queries;

namespace TuneVault.Application.Features.Notification.Commands.MarkAsRead;

public sealed record MarkNotificationAsReadCommand(string NotificationId, string UserId) : IRequest<bool>;

public sealed class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationQueryRepository _notificationRepository;

    public MarkNotificationAsReadCommandHandler(INotificationQueryRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken ct)
    {
        return await _notificationRepository.MarkAsReadAsync(request.NotificationId, request.UserId);
    }
}
