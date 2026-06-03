using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

public interface IFavoriteRepository
{
    Task<bool> IsFavoriteAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ToggleAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default);
}
