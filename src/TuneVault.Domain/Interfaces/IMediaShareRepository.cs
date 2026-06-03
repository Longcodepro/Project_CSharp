using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

public interface IMediaShareRepository
{
    Task ShareAsync(MediaShare mediaShare, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MediaShare>> GetSharedByMeAsync(Guid senderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MediaShare>> GetSharedWithMeAsync(Guid receiverId, CancellationToken cancellationToken = default);
}
