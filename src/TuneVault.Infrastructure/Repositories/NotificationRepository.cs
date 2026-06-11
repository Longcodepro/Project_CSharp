/*
<<<<<<< HEAD
=======
using Dapper;
>>>>>>> 44e6411 (feat: add delete track and search pagination)
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
*/