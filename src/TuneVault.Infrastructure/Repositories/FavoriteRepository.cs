using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class FavoriteRepository : IFavoriteRepository
{
    public Task ToggleAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> IsFavoriteAsync(Guid userId, Guid mediaItemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
