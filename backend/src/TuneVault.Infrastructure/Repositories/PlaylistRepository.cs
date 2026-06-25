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
/// - [Playlists]: Id, UserId, Title, Description, CoverImageUrl, IsPublic, ContentType, ReleaseDate, CreatedAt, IsActive
/// - [PlaylistTracks]: Id, PlaylistId, MediaItemId, TrackOrder, AddedAt
/// </summary>
public sealed class PlaylistRepository : IPlaylistRepository
{
    private const string MediaItemSelectColumns = """
        m.Id,
        m.OwnerId,
        m.Title,
        m.Description,
        m.MediaType AS [Type],
        m.CoverImageUrl,
        m.CanvasUrl,
        m.Genre,
        m.AccessLevel,
        m.IsPublic,
        m.IsActive,
        m.IsValid,
        m.FavoriteCount,
        m.ViewCount,
        m.UploadedAt,
        m.ReleaseDate
        """;

    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// Khởi tạo PlaylistRepository với IDbConnectionFactory dependency.
    /// </summary>
    /// <param name="db">Factory để tạo kết nối database.</param>
    public PlaylistRepository(IDbConnectionFactory db) => _db = db;

    /// <summary>
    /// Lấy playlist theo Id.
    /// SQL: SELECT * FROM Playlists WHERE Id = @Id AND IsActive = 1
    /// </summary>
    public async Task<Playlist?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, Title, Description, CoverImageUrl, IsPublic,
                   ContentType, ReleaseDate, CreatedAt, IsActive
            FROM Playlists
            WHERE Id = @Id AND IsActive = 1
            """;
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Playlist>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy tất cả playlist của một user.
    /// SQL: SELECT * FROM Playlists WHERE UserId = @UserId AND IsActive = 1
    /// </summary>
    public async Task<IEnumerable<Playlist>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, Title, Description, CoverImageUrl, IsPublic,
                   ContentType, ReleaseDate, CreatedAt, IsActive
            FROM Playlists
            WHERE UserId = @UserId AND IsActive = 1
            ORDER BY CreatedAt DESC
            """;
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Playlist>(
            new CommandDefinition(sql, new { UserId = ownerId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy playlist công khai còn hoạt động để hiển thị ở trang khám phá.
    /// </summary>
    public async Task<IEnumerable<Playlist>> GetPublicAsync(int limit, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (@Limit)
                   Id, UserId, Title, Description, CoverImageUrl, IsPublic,
                   ContentType, ReleaseDate, CreatedAt, IsActive
            FROM Playlists
            WHERE IsPublic = 1 AND IsActive = 1
            ORDER BY CreatedAt DESC
            """;
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Playlist>(
            new CommandDefinition(sql, new { Limit = limit }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy các track thuộc một playlist theo thứ tự phát.
    /// </summary>
    public async Task<IEnumerable<PlaylistTrack>> GetPlaylistTracksAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, PlaylistId, MediaItemId, TrackOrder, AddedAt
            FROM PlaylistTracks
            WHERE PlaylistId = @PlaylistId
            ORDER BY TrackOrder ASC
            """;
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PlaylistTrack>(
            new CommandDefinition(sql, new { PlaylistId = playlistId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy tất cả playlist trong DB.
    /// Dùng để sinh ID tự động theo format PL001, PL002...
    /// SQL: SELECT * FROM Playlists
    /// </summary>
    public async Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, Title, Description, CoverImageUrl, IsPublic,
                   ContentType, ReleaseDate, CreatedAt, IsActive
            FROM Playlists
            """;
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
    /// SQL: INSERT INTO Playlists (Id, UserId, Title, Description, CoverImageUrl, IsPublic, CreatedAt, IsActive)
    /// </summary>
    public async Task AddAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Playlists
                (Id, UserId, Title, Description, CoverImageUrl, IsPublic, ContentType, ReleaseDate, CreatedAt, IsActive)
            VALUES
                (@Id, @UserId, @Title, @Description, @CoverImageUrl, @IsPublic, @ContentType, @ReleaseDate, @CreatedAt, 1)
            """;
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, playlist, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Cập nhật metadata của playlist.
    /// SQL: UPDATE Playlists SET Title, Description, CoverImageUrl, IsPublic WHERE Id = @Id AND IsActive = 1
    /// </summary>
    public async Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE Playlists SET
                Title         = @Title,
                Description   = @Description,
                CoverImageUrl = @CoverImageUrl,
                IsPublic      = @IsPublic,
                ContentType   = @ContentType,
                ReleaseDate   = @ReleaseDate
            WHERE Id = @Id AND IsActive = 1
            """;
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, playlist, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Xóa mềm playlist theo Id.
    /// SQL: UPDATE Playlists SET IsActive = 0 WHERE Id = @Id
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE Playlists SET IsActive = 0 WHERE Id = @Id AND IsActive = 1";
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
    /// SQL: SELECT các cột media cần map, tránh map trực tiếp cột Url vào value object.
    /// </summary>
    public async Task<IEnumerable<MediaItem>> GetTracksAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        const string sql = $"""
            SELECT {MediaItemSelectColumns}
            FROM MediaItems m
            INNER JOIN PlaylistTracks pt ON m.Id = pt.MediaItemId
            WHERE pt.PlaylistId = @PlaylistId
              AND m.IsActive = 1
              AND m.IsValid = 0
            ORDER BY pt.TrackOrder ASC
            """;
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<MediaItem>(
            new CommandDefinition(sql, new { PlaylistId = playlistId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Kiểm tra media item còn hoạt động trước khi thêm vào playlist.
    /// </summary>
    public async Task<bool> MediaItemExistsAsync(string mediaItemId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM MediaItems
            WHERE Id = @MediaItemId AND IsActive = 1 AND IsValid = 0
            """;
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { MediaItemId = mediaItemId }, cancellationToken: cancellationToken));
        return count > 0;
    }

    /// <summary>
    /// Kiểm tra một media item đã có trong playlist hay chưa.
    /// </summary>
    public async Task<bool> TrackExistsAsync(string playlistId, string mediaItemId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM PlaylistTracks
            WHERE PlaylistId = @PlaylistId AND MediaItemId = @MediaItemId
            """;
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { PlaylistId = playlistId, MediaItemId = mediaItemId }, cancellationToken: cancellationToken));
        return count > 0;
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

    /// <summary>
    /// Dịch chuyển hàng loạt TrackOrder để chèn track mới lên đầu hoặc khép khoảng trống sau khi xóa.
    /// </summary>
    public async Task ShiftTrackOrdersAsync(string playlistId, int startingFromOrder, int delta, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE PlaylistTracks
            SET TrackOrder = TrackOrder + @Delta
            WHERE PlaylistId = @PlaylistId
              AND TrackOrder >= @StartingFromOrder";

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    PlaylistId = playlistId,
                    StartingFromOrder = startingFromOrder,
                    Delta = delta
                },
                cancellationToken: cancellationToken));
    }
}
