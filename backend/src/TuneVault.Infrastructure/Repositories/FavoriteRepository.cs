using Dapper;
using System.Data;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý SQL cho Favorite.
/// Chỉ chứa SQL và helper liên quan trực tiếp đến Favorite.
/// </summary>
public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public FavoriteRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
    }

    public async Task<Favorite?> GetByUserIdAndMediaItemIdAsync(string userId, string mediaItemId, CancellationToken ct = default)
        => await GetByUserIdAndTargetAsync(userId, mediaItemId, FavoriteTargetType.Media, false, ct);

    public async Task<Favorite?> GetByUserIdAndTargetAsync(
        string userId,
        string targetId,
        FavoriteTargetType targetType,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                Id,
                UserId,
                TargetId,
                TargetType,
                IsActive,
                LikedAt
            FROM Favorites
            WHERE UserId = @UserId
              AND TargetId = @TargetId
              AND TargetType = @TargetType
              AND (@IncludeInactive = 1 OR IsActive = 1);";

        var favorite = await connection.QueryFirstOrDefaultAsync<FavoriteRow>(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                TargetId = targetId,
                TargetType = (byte)targetType,
                IncludeInactive = includeInactive
            }, cancellationToken: ct));

        return favorite is null ? null : ToEntity(favorite);
    }

    public async Task<IReadOnlyCollection<Favorite>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                Id,
                UserId,
                TargetId,
                TargetType,
                IsActive,
                LikedAt
            FROM Favorites
            WHERE UserId = @UserId
              AND TargetType = @TargetType
              AND IsActive = 1
            ORDER BY LikedAt DESC;";

        var result = await connection.QueryAsync<FavoriteRow>(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                TargetType = (byte)FavoriteTargetType.Media
            }, cancellationToken: ct));

        return result.Select(ToEntity).ToList();
    }

    public async Task<bool> MediaItemExistsAsync(string mediaItemId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(1)
            FROM MediaItems
            WHERE Id = @MediaItemId
              AND IsActive = 1
              AND IsPublic = 1
              ;
            """;

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { MediaItemId = mediaItemId }, cancellationToken: ct));

        return count > 0;
    }

    public async Task<bool> TargetExistsAsync(
        string targetId,
        FavoriteTargetType targetType,
        string userId,
        CancellationToken ct = default)
    {
        if (targetType == FavoriteTargetType.Media)
            return await MediaItemExistsAsync(targetId, ct);

        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = targetType switch
        {
            FavoriteTargetType.Album => """
                SELECT COUNT(1)
                FROM Albums
                WHERE Id = @TargetId
                  AND IsActive = 1
                  AND (IsPublic = 1 OR ArtistId = @UserId);
                """,
            FavoriteTargetType.Playlist => """
                SELECT COUNT(1)
                FROM Playlists
                WHERE Id = @TargetId
                  AND IsActive = 1
                  AND (IsPublic = 1 OR UserId = @UserId);
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, "Loại target không hợp lệ.")
        };

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { TargetId = targetId, UserId = userId }, cancellationToken: ct));

        return count > 0;
    }

    public async Task<int> CountReactionsAsync(
        string targetId,
        FavoriteTargetType targetType,
        CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(1)
            FROM Favorites
            WHERE TargetId = @TargetId
              AND TargetType = @TargetType
              AND IsActive = 1;
            """;

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new
            {
                TargetId = targetId,
                TargetType = (byte)targetType
            }, cancellationToken: ct));
    }

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var id = await GenerateNextFavoriteIdAsync(connection, transaction, ct);
        favorite.Id = id;

        await connection.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO Favorites
                (Id, UserId, TargetId, TargetType, IsActive, LikedAt)
            VALUES
                (@Id, @UserId, @TargetId, @TargetType, @IsActive, SYSUTCDATETIME());",
            new
            {
                favorite.Id,
                favorite.UserId,
                favorite.TargetId,
                TargetType = (byte)favorite.TargetType,
                favorite.IsActive
            },
            transaction,
            cancellationToken: ct));

        if (favorite.TargetType == FavoriteTargetType.Media)
        {
            await SyncFavoriteCountAsync(connection, transaction, favorite.TargetId, ct);
        }

        transaction.Commit();
    }

    public async Task UpdateAsync(Favorite favorite, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Favorites
            SET IsActive = @IsActive,
                LikedAt = SYSUTCDATETIME()
            WHERE Id = @Id;",
            new
            {
                favorite.IsActive,
                favorite.Id
            },
            transaction,
            cancellationToken: ct));

        if (favorite.TargetType == FavoriteTargetType.Media)
        {
            await SyncFavoriteCountAsync(connection, transaction, favorite.TargetId, ct);
        }

        transaction.Commit();
    }

    public async Task RemoveAsync(string id, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var favoriteTarget = await connection.QueryFirstOrDefaultAsync<FavoriteTargetRow>(
            new CommandDefinition(
                "SELECT TargetId, TargetType FROM Favorites WHERE Id = @Id",
                new { Id = id },
                transaction,
                cancellationToken: ct));

        var deletedRows = await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Favorites
            SET IsActive = 0
            WHERE Id = @Id;",
            new
            {
                Id = id
            },
            transaction,
            cancellationToken: ct));

        if (deletedRows > 0 &&
            favoriteTarget is not null &&
            (FavoriteTargetType)favoriteTarget.TargetType == FavoriteTargetType.Media)
        {
            await SyncFavoriteCountAsync(connection, transaction, favoriteTarget.TargetId, ct);
        }

        transaction.Commit();
    }

    /// <summary>
    /// Tạo Id Favorite dạng FV001, FV002, FV003...
    /// </summary>
    private static async Task<string> GenerateNextFavoriteIdAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken ct)
    {
        const string prefix = "FV";

        var nextNumber = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
            FROM Favorites
            WHERE Id LIKE @PrefixLike;",
            new
            {
                Prefix = prefix,
                PrefixLike = prefix + "%"
            },
            transaction,
            cancellationToken: ct));

        return $"{prefix}{nextNumber:000}";
    }

    /// <summary>
    /// Đồng bộ FavoriteCount theo số dòng reaction thật để tránh lệch dữ liệu khi insert/delete lỗi giữa chừng.
    /// </summary>
    private static async Task SyncFavoriteCountAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        string mediaItemId,
        CancellationToken ct)
    {
        const string sql = """
            UPDATE MediaItems
            SET FavoriteCount = (
                SELECT COUNT(1)
                FROM Favorites
                WHERE TargetId = @MediaItemId
                  AND TargetType = @MediaTargetType
                  AND IsActive = 1
            )
            WHERE Id = @MediaItemId;
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                MediaItemId = mediaItemId,
                MediaTargetType = (byte)FavoriteTargetType.Media
            },
            transaction,
            cancellationToken: ct));
    }

    /// <summary>
    /// Dòng dữ liệu Favorite đọc trực tiếp từ SQL.
    /// </summary>
    private sealed record FavoriteRow(
        string Id,
        string UserId,
        string TargetId,
        byte TargetType,
        bool IsActive,
        DateTime LikedAt);

    private sealed record FavoriteTargetRow(string TargetId, byte TargetType);

    /// <summary>
    /// Map dữ liệu SQL sang entity Favorite để tầng Application không phụ thuộc Dapper row.
    /// </summary>
    private static Favorite ToEntity(FavoriteRow row)
    {
        return new Favorite(
            row.Id,
            row.UserId,
            row.TargetId,
            (FavoriteTargetType)row.TargetType,
            row.LikedAt,
            row.IsActive);
    }
}
