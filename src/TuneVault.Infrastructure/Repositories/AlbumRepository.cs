using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository Dapper xử lý CRUD album và track thuộc album.
/// </summary>
public sealed class AlbumRepository : IAlbumRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Khởi tạo repository album.
    /// </summary>
    public AlbumRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
    }

    /// <summary>
    /// Lấy album còn hoạt động theo id.
    /// </summary>
    public async Task<Album?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Album>(new CommandDefinition(@"
            SELECT
                Id,
                ArtistId,
                Title,
                Description,
                CoverImageUrl,
                CreatedAt,
                IsActive,
                IsPublic,
                ReleaseDate,
                ContentType
            FROM Albums
            WHERE Id = @Id
              AND IsActive = 1;",
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy danh sách album còn hoạt động của một nghệ sĩ.
    /// </summary>
    public async Task<IReadOnlyCollection<Album>> GetByArtistIdAsync(string artistId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var items = await connection.QueryAsync<Album>(new CommandDefinition(@"
            SELECT
                Id,
                ArtistId,
                Title,
                Description,
                CoverImageUrl,
                CreatedAt,
                IsActive,
                IsPublic,
                ReleaseDate,
                ContentType
            FROM Albums
            WHERE ArtistId = @ArtistId
              AND IsActive = 1
            ORDER BY CreatedAt DESC;",
            new { ArtistId = artistId },
            cancellationToken: cancellationToken));

        return items.ToList();
    }

    /// <summary>
    /// Lấy album công khai còn hoạt động để hiển thị ở trang khám phá.
    /// </summary>
    public async Task<IReadOnlyCollection<Album>> GetPublicAsync(int limit, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var items = await connection.QueryAsync<Album>(new CommandDefinition(@"
            SELECT TOP (@Limit)
                Id,
                ArtistId,
                Title,
                Description,
                CoverImageUrl,
                CreatedAt,
                IsActive,
                IsPublic,
                ReleaseDate,
                ContentType
            FROM Albums
            WHERE IsPublic = 1
              AND IsActive = 1
            ORDER BY CreatedAt DESC;",
            new { Limit = limit },
            cancellationToken: cancellationToken));

        return items.ToList();
    }

    /// <summary>
    /// Lấy tất cả album để sinh id tuần tự.
    /// </summary>
    public async Task<IEnumerable<Album>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        return await connection.QueryAsync<Album>(new CommandDefinition(@"
            SELECT
                Id,
                ArtistId,
                Title,
                Description,
                CoverImageUrl,
                CreatedAt,
                IsActive,
                IsPublic,
                ReleaseDate,
                ContentType
            FROM Albums;",
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy tất cả album track để sinh mã tuần tự.
    /// </summary>
    public async Task<IEnumerable<AlbumTrack>> GetAllTracksAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        return await connection.QueryAsync<AlbumTrack>(new CommandDefinition(@"
            SELECT
                Id,
                AlbumId,
                MediaItemId,
                TrackOrder,
                AddedAt
            FROM AlbumTracks;",
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy track của album theo thứ tự phát.
    /// </summary>
    public async Task<IEnumerable<AlbumTrack>> GetAlbumTracksAsync(string albumId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        return await connection.QueryAsync<AlbumTrack>(new CommandDefinition(@"
            SELECT
                Id,
                AlbumId,
                MediaItemId,
                TrackOrder,
                AddedAt
            FROM AlbumTracks
            WHERE AlbumId = @AlbumId
            ORDER BY TrackOrder ASC, AddedAt DESC;",
            new { AlbumId = albumId },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Thêm album mới.
    /// </summary>
    public async Task AddAsync(Album album, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO Albums
                (Id, ArtistId, Title, Description, CoverImageUrl, CreatedAt, IsActive, IsPublic, ReleaseDate, ContentType)
            VALUES
                (@Id, @ArtistId, @Title, @Description, @CoverImageUrl, @CreatedAt, @IsActive, @IsPublic, @ReleaseDate, @ContentType);",
            new
            {
                album.Id,
                album.ArtistId,
                album.Title,
                album.Description,
                album.CoverImageUrl,
                album.CreatedAt,
                album.IsActive,
                album.IsPublic,
                album.ReleaseDate,
                ContentType = album.ContentType is null ? (int?)null : (int)album.ContentType.Value
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Cập nhật thông tin album.
    /// </summary>
    public async Task UpdateAsync(Album album, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Albums
            SET Title = @Title,
                Description = @Description,
                CoverImageUrl = @CoverImageUrl,
                IsPublic = @IsPublic,
                ReleaseDate = @ReleaseDate,
                ContentType = @ContentType
            WHERE Id = @Id
              AND ArtistId = @ArtistId
              AND IsActive = 1;",
            new
            {
                album.Id,
                album.ArtistId,
                album.Title,
                album.Description,
                album.CoverImageUrl,
                album.IsPublic,
                album.ReleaseDate,
                ContentType = album.ContentType is null ? (int?)null : (int)album.ContentType.Value
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Xóa mềm album.
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Albums
            SET IsActive = 0
            WHERE Id = @Id
              AND IsActive = 1;",
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Thêm track vào album.
    /// </summary>
    public async Task AddTrackAsync(AlbumTrack track, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO AlbumTracks
                (Id, AlbumId, MediaItemId, TrackOrder, AddedAt)
            VALUES
                (@Id, @AlbumId, @MediaItemId, @TrackOrder, @AddedAt);",
            new
            {
                track.Id,
                track.AlbumId,
                track.MediaItemId,
                track.TrackOrder,
                track.AddedAt
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Xóa media khỏi album theo cặp album-media.
    /// </summary>
    public async Task RemoveTrackAsync(string albumId, string mediaItemId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            DELETE FROM AlbumTracks
            WHERE AlbumId = @AlbumId
              AND MediaItemId = @MediaItemId;",
            new
            {
                AlbumId = albumId,
                MediaItemId = mediaItemId
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Kiểm tra media đã có trong album hay chưa.
    /// </summary>
    public async Task<bool> TrackExistsAsync(string albumId, string mediaItemId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(1)
            FROM AlbumTracks
            WHERE AlbumId = @AlbumId
              AND MediaItemId = @MediaItemId;",
            new
            {
                AlbumId = albumId,
                MediaItemId = mediaItemId
            },
            cancellationToken: cancellationToken));

        return count > 0;
    }

    /// <summary>
    /// Cập nhật thứ tự phát của một track.
    /// </summary>
    public async Task UpdateTrackOrderAsync(string albumId, string trackId, int newTrackOrder, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE AlbumTracks
            SET TrackOrder = @NewTrackOrder
            WHERE AlbumId = @AlbumId
              AND Id = @TrackId;",
            new
            {
                AlbumId = albumId,
                TrackId = trackId,
                NewTrackOrder = newTrackOrder
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Dịch chuyển hàng loạt track order khi chèn hoặc xóa.
    /// </summary>
    public async Task ShiftTrackOrdersAsync(string albumId, int startingFromOrder, int delta, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE AlbumTracks
            SET TrackOrder = TrackOrder + @Delta
            WHERE AlbumId = @AlbumId
              AND TrackOrder >= @StartingFromOrder;",
            new
            {
                AlbumId = albumId,
                StartingFromOrder = startingFromOrder,
                Delta = delta
            },
            cancellationToken: cancellationToken));
    }
}
