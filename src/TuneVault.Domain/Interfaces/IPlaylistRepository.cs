using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// INTERFACE - PLAYLIST REPOSITORY (Domain Layer)
/// ============================================
/// Mục đích: Định nghĩa hợp đồng cho tất cả các operation CRUD và track management của Playlist.
/// 
/// Các method chính:
/// - GetByIdAsync(id): Lấy 1 playlist theo ID
/// - GetByOwnerIdAsync(ownerId): Lấy tất cả playlist của 1 user
/// - GetAllAsync(): Lấy tất cả playlist trong DB (dùng để sinh ID PL001, PL002...)
/// - GetAllTracksAsync(): Lấy tất cả track trong DB (dùng để sinh ID PT001, PT002...)
/// - AddAsync/UpdateAsync/DeleteAsync: CRUD operations
/// - AddTrackAsync/RemoveTrackAsync: Quản lý tracks trong playlist
/// - GetTracksAsync(playlistId): Lấy danh sách tracks của playlist
/// - UpdateTrackOrderAsync: Cập nhật lại vị trí track sau khi xóa
/// </summary>
public interface IPlaylistRepository
{
    Task<Playlist?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Playlist>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả playlist trong DB.
    /// Dùng để sinh ID tự động theo format PL001, PL002...
    /// </summary>
    Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả PlaylistTrack trong DB.
    /// Dùng để sinh ID tự động theo format PT001, PT002...
    /// </summary>
    Task<IEnumerable<PlaylistTrack>> GetAllTracksAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Playlist playlist, CancellationToken cancellationToken = default);
    Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task AddTrackAsync(PlaylistTrack track, CancellationToken cancellationToken = default);
    Task RemoveTrackAsync(string playlistId, string mediaItemId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MediaItem>> GetTracksAsync(string playlistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật lại vị trí (TrackOrder) của một track trong playlist.
    /// Dùng sau khi xóa track để đẩy các track phía sau lên 1 vị trí.
    /// </summary>
    Task UpdateTrackOrderAsync(string playlistId, string trackId, int newTrackOrder, CancellationToken cancellationToken = default);
}