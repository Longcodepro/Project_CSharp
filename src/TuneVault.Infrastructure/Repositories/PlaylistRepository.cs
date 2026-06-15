using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// IMPLEMENTATION - PLAYLIST REPOSITORY (Infrastructure Layer)
/// ===========================================================
/// Mục đích: Cài đặt các CRUD operations cho Playlist sử dụng Dapper ORM.
/// 
/// SQL Tables:
/// - [Playlists]: Id, UserId, Title, Description, CoverImageUrl, IsPublic, CreatedAt
/// - [PlaylistTracks]: Id, PlaylistId, MediaItemId, TrackOrder, AddedAt
/// </summary>
public sealed class PlaylistRepository : IPlaylistRepository
{
    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// Khởi tạo PlaylistRepository với IDbConnectionFactory dependency.
    /// </summary>
    /// <param name="db">Factory để tạo kết nối database.</param>
    public PlaylistRepository(IDbConnectionFactory db) => _db = db;

    /// <summary>
    /// Lấy playlist theo Id.
    /// SQL: SELECT * FROM Playlists WHERE Id = @Id
    /// </summary>
    public async Task<Playlist?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Playlists WHERE Id = @Id";
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Playlist>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy tất cả playlist của một user.
    /// SQL: SELECT * FROM Playlists WHERE UserId = @UserId
    /// </summary>
    public async Task<IEnumerable<Playlist>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Playlists WHERE UserId = @UserId";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Playlist>(
            new CommandDefinition(sql, new { UserId = ownerId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy tất cả playlist trong DB.
    /// Dùng để sinh ID tự động theo format PL001, PL002...
    /// SQL: SELECT * FROM Playlists
    /// </summary>
    public async Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Playlists";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Playlist>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy tất cả PlaylistTrack trong DB.
    /// Dùng để sinh ID tự động theo format PT001, PT002...
    /// SQL: SELECT * FROM PlaylistTracks
    /// </summary>
    public async Task<IEnumerable<PlaylistTrack>> GetAllTracksAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM PlaylistTracks";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PlaylistTrack>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
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
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, playlist, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Cập nhật metadata của playlist.
    /// SQL: UPDATE Playlists SET Title, CoverImageUrl, IsPublic WHERE Id = @Id
    /// </summary>
    public async Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Playlists SET
                Title         = @Title,
                CoverImageUrl = @CoverImageUrl,
                IsPublic      = @IsPublic
            WHERE Id = @Id";
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, playlist, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Xóa playlist theo Id.
    /// SQL: DELETE FROM Playlists WHERE Id = @Id
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Playlists WHERE Id = @Id";
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
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
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, track, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Xóa track khỏi playlist.
    /// SQL: DELETE FROM PlaylistTracks WHERE PlaylistId = @PlaylistId AND MediaItemId = @MediaItemId
    /// </summary>
    public async Task RemoveTrackAsync(string playlistId, string mediaItemId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM PlaylistTracks WHERE PlaylistId = @PlaylistId AND MediaItemId = @MediaItemId";
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { PlaylistId = playlistId, MediaItemId = mediaItemId }, cancellationToken: cancellationToken));
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
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<MediaItem>(
            new CommandDefinition(sql, new { PlaylistId = playlistId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Cập nhật lại vị trí (TrackOrder) của một track trong playlist.
    /// Dùng sau khi xóa track để đẩy các track phía sau lên 1 vị trí.
    /// SQL: UPDATE PlaylistTracks SET TrackOrder = @NewTrackOrder WHERE PlaylistId = @PlaylistId AND Id = @TrackId
    /// </summary>
    public async Task UpdateTrackOrderAsync(string playlistId, string trackId, int newTrackOrder, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE PlaylistTracks SET TrackOrder = @NewTrackOrder
            WHERE PlaylistId = @PlaylistId AND Id = @TrackId";
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { PlaylistId = playlistId, TrackId = trackId, NewTrackOrder = newTrackOrder }, cancellationToken: cancellationToken));
    }
}