using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

public interface IAlbumRepository
{
    Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Album>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken = default);
    Task AddAsync(Album album, CancellationToken cancellationToken = default);
    Task UpdateAsync(Album album, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
