using MediatR;
using TuneVault.Application.Features.Notification.Queries;

namespace TuneVault.Application.Features.Notification.Commands.DeleteNotification;

public sealed record DeleteNotificationCommand(string NotificationId, string UserId) : IRequest<bool>;

public sealed class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
{
    private readonly INotificationQueryRepository _notificationRepository;

    public DeleteNotificationCommandHandler(INotificationQueryRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken ct)
    {
        return await _notificationRepository.DeleteAsync(request.NotificationId, request.UserId);
    }
}
