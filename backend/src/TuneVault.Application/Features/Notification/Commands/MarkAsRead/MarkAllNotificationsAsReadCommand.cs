using MediatR;
using TuneVault.Application.Features.Notification.Queries;

namespace TuneVault.Application.Features.Notification.Commands.MarkAsRead;

/// <summary>
/// Command đánh dấu toàn bộ notification của user là đã đọc.
/// </summary>
/// <param name="UserId">Mã user sở hữu notification.</param>
public sealed record MarkAllNotificationsAsReadCommand(string UserId) : IRequest<int>;

/// <summary>
/// Handler đánh dấu toàn bộ notification của user là đã đọc.
/// </summary>
public sealed class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
{
    private readonly INotificationQueryRepository _notificationRepository;

    /// <summary>
    /// Khởi tạo handler mark all notifications as read.
    /// </summary>
    /// <param name="notificationRepository">Repository truy vấn/cập nhật notification.</param>
    public MarkAllNotificationsAsReadCommandHandler(INotificationQueryRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Đánh dấu toàn bộ notification còn hoạt động của user là đã đọc.
    /// </summary>
    /// <param name="request">Command chứa user id hiện tại.</param>
    /// <param name="ct">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Số notification được cập nhật.</returns>
    public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken ct)
    {
        return await _notificationRepository.MarkAllAsReadAsync(request.UserId);
    }
}
