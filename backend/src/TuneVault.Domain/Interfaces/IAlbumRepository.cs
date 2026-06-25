using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Repository thao tác với album và các track thuộc album.
/// </summary>
public interface IAlbumRepository
{
    Task<Album?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Album>> GetByArtistIdAsync(string artistId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Lấy danh sách album công khai còn hoạt động để hiển thị ở trang khám phá.
    /// </summary>
    Task<IReadOnlyCollection<Album>> GetPublicAsync(int limit, CancellationToken cancellationToken = default);
    Task<IEnumerable<Album>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AlbumTrack>> GetAllTracksAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AlbumTrack>> GetAlbumTracksAsync(string albumId, CancellationToken cancellationToken = default);
    Task AddAsync(Album album, CancellationToken cancellationToken = default);
    Task UpdateAsync(Album album, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task AddTrackAsync(AlbumTrack track, CancellationToken cancellationToken = default);
    Task RemoveTrackAsync(string albumId, string mediaItemId, CancellationToken cancellationToken = default);
    Task<bool> TrackExistsAsync(string albumId, string mediaItemId, CancellationToken cancellationToken = default);
    Task UpdateTrackOrderAsync(string albumId, string trackId, int newTrackOrder, CancellationToken cancellationToken = default);
    Task ShiftTrackOrdersAsync(string albumId, int startingFromOrder, int delta, CancellationToken cancellationToken = default);
}
