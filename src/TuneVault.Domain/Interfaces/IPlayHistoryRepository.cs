using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

public interface IPlayHistoryRepository
{
    Task RecordAsync(PlayHistory playHistory, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PlayHistory>> GetRecentByUserIdAsync(Guid userId, int take = 20, CancellationToken cancellationToken = default);
}
