using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// IMPLEMENTATION - PLAYLIST REPOSITORY (Infrastructure Layer)
/// ===========================================================
/// Mục đích: Cài đặt các CRUD operations cho Playlist sử dụng Dapper ORM.
/// 
/// Luồng xử lý:
/// 1. PlaylistController -> IPlaylistRepository (injected)
/// 2. DI container -> PlaylistRepository.ctor (DapperContext injected)
/// 3. PlaylistRepository -> DapperContext.CreateConnection()
/// 4. Dapper -> SQL queries trên database
/// 
/// SQL Tables:
/// - [Playlists]: Id, UserId, Title, Description, CoverImageUrl, IsPublic, CreatedAt
/// - [PlaylistTracks]: Id, PlaylistId, MediaItemId, TrackOrder, AddedAt
/// 
/// Ý nghĩa các method:
/// - GetByIdAsync: SELECT * FROM Playlists WHERE Id = @Id
/// - GetByOwnerIdAsync: SELECT * FROM Playlists WHERE UserId = @UserId
/// - AddAsync: INSERT INTO Playlists (...)
/// - UpdateAsync: UPDATE Playlists SET Title, CoverImageUrl, IsPublic
/// - DeleteAsync: DELETE FROM Playlists
/// - AddTrackAsync: INSERT INTO PlaylistTracks (...)
/// - RemoveTrackAsync: DELETE FROM PlaylistTracks
/// - GetTracksAsync: SELECT m.* FROM MediaItems INNER JOIN PlaylistTracks
/// </summary>

public sealed class PlaylistRepository : IPlaylistRepository
{
    private readonly DapperContext _context;

    public PlaylistRepository(DapperContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy playlist theo Id.
    /// SQL: SELECT * FROM Playlists WHERE Id = @Id
    /// </summary>
    public async Task<Playlist?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Playlists WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Playlist>(sql, new { Id = id });
    }

    /// <summary>
    /// Lấy tất cả playlist của một user.
    /// SQL: SELECT * FROM Playlists WHERE UserId = @UserId
    /// </summary>
    public async Task<IEnumerable<Playlist>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Playlists WHERE UserId = @UserId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Playlist>(sql, new { UserId = ownerId });
    }

    /// <summary>
    /// Thêm playlist mới vào database.
    /// SQL: INSERT INTO Playlists (Id, UserId, Title, CoverImageUrl, IsPublic, CreatedAt)
    /// </summary>
    public async Task AddAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Playlists (Id, UserId, Title, CoverImageUrl, IsPublic, CreatedAt)
            VALUES (@Id, @UserId, @Title, @CoverImageUrl, @IsPublic, @CreatedAt)";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, playlist);
    }

    /// <summary>
    /// Cập nhật metadata của playlist.
    /// SQL: UPDATE Playlists SET Title, CoverImageUrl, IsPublic WHERE Id = @Id
    /// </summary>
    public async Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Playlists SET
                Title        = @Title,
                CoverImageUrl = @CoverImageUrl,
                IsPublic     = @IsPublic
            WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, playlist);
    }

    /// <summary>
    /// Xóa playlist theo Id.
    /// SQL: DELETE FROM Playlists WHERE Id = @Id
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Playlists WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    /// <summary>
    /// Thêm track vào playlist.
    /// SQL: INSERT INTO PlaylistTracks (Id, PlaylistId, MediaItemId, TrackOrder, AddedAt)
    /// </summary>
    public async Task AddTrackAsync(PlaylistTrack track, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO PlaylistTracks (Id, PlaylistId, MediaItemId, TrackOrder, AddedAt)
            VALUES (@Id, @PlaylistId, @MediaItemId, @TrackOrder, @AddedAt)";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, track);
    }

    /// <summary>
    /// Xóa track khỏi playlist.
    /// SQL: DELETE FROM PlaylistTracks WHERE PlaylistId = @PlaylistId AND MediaItemId = @MediaItemId
    /// </summary>
    public async Task RemoveTrackAsync(string playlistId, string mediaItemId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM PlaylistTracks WHERE PlaylistId = @PlaylistId AND MediaItemId = @MediaItemId";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { PlaylistId = playlistId, MediaItemId = mediaItemId });
    }

    /// <summary>
    /// Lấy danh sách tracks trong playlist theo thứ tự.
    /// SQL: SELECT m.* FROM MediaItems INNER JOIN PlaylistTracks pt ON m.Id = pt.MediaItemId WHERE pt.PlaylistId = @PlaylistId ORDER BY pt.TrackOrder ASC
    /// </summary>
    public async Task<IEnumerable<MediaItem>> GetTracksAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT m.* FROM MediaItems m
            INNER JOIN PlaylistTracks pt ON m.Id = pt.MediaItemId
            WHERE pt.PlaylistId = @PlaylistId
            ORDER BY pt.TrackOrder ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<MediaItem>(sql, new { PlaylistId = playlistId });
    }
}
