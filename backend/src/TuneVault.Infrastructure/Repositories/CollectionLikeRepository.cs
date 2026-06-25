using Dapper;
using System.Data;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository Dapper cho lượt thích album và playlist.
/// </summary>
public sealed class CollectionLikeRepository : ICollectionLikeRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Khởi tạo repository với factory tạo kết nối SQL Server.
    /// </summary>
    public CollectionLikeRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
    }

    /// <summary>
    /// Lấy lượt thích hiện có theo user, target id và target type.
    /// </summary>
    public async Task<CollectionLike?> GetByUserAndTargetAsync(
        string userId,
        string targetId,
        CollectionLikeTargetType targetType,
        CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, UserId, TargetId, TargetType, LikedAt
            FROM CollectionLikes
            WHERE UserId = @UserId
              AND TargetId = @TargetId
              AND TargetType = @TargetType;
            """;

        var row = await connection.QueryFirstOrDefaultAsync<CollectionLikeRow>(
            new CommandDefinition(
                sql,
                new
                {
                    UserId = userId,
                    TargetId = targetId,
                    TargetType = (byte)targetType
                },
                cancellationToken: ct));

        return row is null ? null : ToEntity(row);
    }

    /// <summary>
    /// Lấy album/playlist người dùng đã thích gần nhất, kèm metadata để render sidebar.
    /// </summary>
    public async Task<IReadOnlyCollection<CollectionLikeSummary>> GetRecentByUserAsync(
        string userId,
        int limit,
        CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT TOP (@Limit)
                cl.Id,
                cl.TargetId,
                cl.TargetType,
                COALESCE(a.Title, p.Title) AS Title,
                COALESCE(a.Description, p.Description) AS Description,
                COALESCE(a.CoverImageUrl, p.CoverImageUrl) AS CoverImageUrl,
                cl.LikedAt
            FROM CollectionLikes cl
            LEFT JOIN Albums a
                ON cl.TargetType = @AlbumType
               AND cl.TargetId = a.Id
               AND a.IsActive = 1
            LEFT JOIN Playlists p
                ON cl.TargetType = @PlaylistType
               AND cl.TargetId = p.Id
            WHERE cl.UserId = @UserId
              AND COALESCE(a.Id, p.Id) IS NOT NULL
            ORDER BY cl.LikedAt DESC;
            """;

        var rows = await connection.QueryAsync<CollectionLikeSummaryRow>(
            new CommandDefinition(
                sql,
                new
                {
                    UserId = userId,
                    Limit = Math.Clamp(limit, 1, 10),
                    AlbumType = (byte)CollectionLikeTargetType.Album,
                    PlaylistType = (byte)CollectionLikeTargetType.Playlist
                },
                cancellationToken: ct));

        return rows.Select(row => new CollectionLikeSummary(
            row.Id,
            row.TargetId,
            (CollectionLikeTargetType)row.TargetType,
            row.Title,
            row.Description,
            row.CoverImageUrl,
            row.LikedAt)).ToList();
    }

    /// <summary>
    /// Kiểm tra target tồn tại và còn xem được với user hiện tại.
    /// </summary>
    public async Task<bool> TargetExistsAsync(
        string targetId,
        CollectionLikeTargetType targetType,
        string userId,
        CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = targetType == CollectionLikeTargetType.Album
            ? """
              SELECT COUNT(1)
              FROM Albums
              WHERE Id = @TargetId
                AND IsActive = 1
                AND (IsPublic = 1 OR ArtistId = @UserId);
              """
            : """
              SELECT COUNT(1)
              FROM Playlists
              WHERE Id = @TargetId
                AND (IsPublic = 1 OR UserId = @UserId);
              """;

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new
                {
                    TargetId = targetId,
                    UserId = userId
                },
                cancellationToken: ct));

        return count > 0;
    }

    /// <summary>
    /// Thêm lượt thích mới và sinh mã dạng CL001.
    /// </summary>
    public async Task AddAsync(CollectionLike like, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        like.Id = await GenerateNextCollectionLikeIdAsync(connection, transaction, ct);

        const string sql = """
            INSERT INTO CollectionLikes (Id, UserId, TargetId, TargetType, LikedAt)
            VALUES (@Id, @UserId, @TargetId, @TargetType, SYSUTCDATETIME());
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    like.Id,
                    like.UserId,
                    like.TargetId,
                    TargetType = (byte)like.TargetType
                },
                transaction,
                cancellationToken: ct));

        transaction.Commit();
    }

    /// <summary>
    /// Xóa lượt thích theo id.
    /// </summary>
    public async Task RemoveAsync(string id, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            DELETE FROM CollectionLikes
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    private static async Task<string> GenerateNextCollectionLikeIdAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken ct)
    {
        const string prefix = "CL";

        var nextNumber = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
                FROM CollectionLikes
                WHERE Id LIKE @PrefixLike;
                """,
                new
                {
                    Prefix = prefix,
                    PrefixLike = prefix + "%"
                },
                transaction,
                cancellationToken: ct));

        return $"{prefix}{nextNumber:000}";
    }

    private static CollectionLike ToEntity(CollectionLikeRow row)
    {
        return new CollectionLike(
            row.Id,
            row.UserId,
            row.TargetId,
            (CollectionLikeTargetType)row.TargetType);
    }

    private sealed record CollectionLikeRow(
        string Id,
        string UserId,
        string TargetId,
        byte TargetType,
        DateTime LikedAt);

    private sealed record CollectionLikeSummaryRow(
        string Id,
        string TargetId,
        byte TargetType,
        string Title,
        string? Description,
        string? CoverImageUrl,
        DateTime LikedAt);
}
