using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class MediaRepository : IMediaRepository
{
    public Task CreateAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<MediaItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<MediaItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<MediaItem>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
