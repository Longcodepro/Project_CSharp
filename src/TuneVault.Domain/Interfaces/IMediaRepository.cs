using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

public interface IMediaRepository
{
    Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MediaItem>> SearchAsync(string keyword, CancellationToken cancellationToken = default);
    Task AddAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
