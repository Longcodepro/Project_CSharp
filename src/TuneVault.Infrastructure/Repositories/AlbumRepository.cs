using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class AlbumRepository : IAlbumRepository
{
    public Task AddTrackAsync(string albumId, string mediaItemId, int trackOrder, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task AddAsync(Album album, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyCollection<Album>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<Album>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task RemoveTrackAsync(string albumId, string mediaItemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(Album album, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
