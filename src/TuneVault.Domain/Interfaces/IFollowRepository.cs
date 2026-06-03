using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

public interface IFollowRepository
{
    Task FollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
    Task UnfollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Follow>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Follow>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default);
}
