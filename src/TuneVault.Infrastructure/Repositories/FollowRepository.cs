using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class FollowRepository : IFollowRepository
{
    public Task FollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Follow>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Follow>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UnfollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
