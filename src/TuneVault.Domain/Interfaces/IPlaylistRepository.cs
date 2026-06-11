using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// INTERFACE - PLAYLIST REPOSITORY (Domain Layer)
/// ============================================
/// Mục đích: Định nghĩa hợp đồng cho tất cả các operation CRUD và track management của Playlist.
/// 
/// Luồng xử lý:
/// - PlaylistController gọi IPlaylistRepository (dependency injection)
/// - DI container trỏ tới PlaylistRepository (implementation)
/// - PlaylistRepository thực hiện SQL queries trên database
/// 
/// Các method chính:
/// - GetByIdAsync(id): Lấy 1 playlist theo ID
/// - GetByOwnerIdAsync(ownerId): Lấy tất cả playlist của 1 user
/// - AddAsync/UpdateAsync/DeleteAsync: CRUD operations
/// - AddTrackAsync/RemoveTrackAsync: Quản lý tracks trong playlist
/// - GetTracksAsync(playlistId): Lấy danh sách tracks của playlist
/// </summary>

public interface IPlaylistRepository
{
    Task<Playlist?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Playlist>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Playlist playlist, CancellationToken cancellationToken = default);
    Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task AddTrackAsync(PlaylistTrack track, CancellationToken cancellationToken = default);
    Task RemoveTrackAsync(string playlistId, string mediaItemId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MediaItem>> GetTracksAsync(string playlistId, CancellationToken cancellationToken = default);
}
