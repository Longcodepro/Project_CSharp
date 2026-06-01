using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class FollowRepository : IFollowRepository
{
    public Task FollowAsync(string followerId, string followeeId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<Follow>> GetFollowersAsync(string followeeId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<Follow>> GetFollowingAsync(string followerId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UnfollowAsync(string followerId, string followeeId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
