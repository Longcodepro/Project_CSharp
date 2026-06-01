using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

public sealed class MediaShareRepository : IMediaShareRepository
{
    public Task CreateAsync(MediaShare mediaShare, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<MediaShare>> GetSharedByMeAsync(string senderId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<MediaShare>> GetSharedWithMeAsync(string receiverId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
