using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class FavoriteRepository : IFavoriteRepository
{
    public Task ToggleAsync(string userId, string mediaItemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> IsFavoriteAsync(string userId, string mediaItemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<Favorite>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
