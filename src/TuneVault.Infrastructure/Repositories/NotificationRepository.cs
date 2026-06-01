using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    public Task CreateAsync(Notification notification, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task MarkAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
