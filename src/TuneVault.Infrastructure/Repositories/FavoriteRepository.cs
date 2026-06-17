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
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                Id,
                UserId,
                MediaItemId,
                Reaction,
                LikedAt
            FROM Favorites
            WHERE UserId = @UserId
              AND MediaItemId = @MediaItemId;";

        var favorite = await connection.QueryFirstOrDefaultAsync<FavoriteRow>(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                MediaItemId = mediaItemId
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
                MediaItemId,
                Reaction,
                LikedAt
            FROM Favorites
            WHERE UserId = @UserId
            ORDER BY LikedAt DESC;";

        var result = await connection.QueryAsync<FavoriteRow>(
            new CommandDefinition(sql, new
            {
                UserId = userId
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
              AND IsValid = 0;
            """;

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { MediaItemId = mediaItemId }, cancellationToken: ct));

        return count > 0;
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
                (Id, UserId, MediaItemId, Reaction, LikedAt)
            VALUES
                (@Id, @UserId, @MediaItemId, @Reaction, GETDATE());

            UPDATE MediaItems
            SET FavoriteCount = FavoriteCount + 1
            WHERE Id = @MediaItemId;",
            new
            {
                favorite.Id,
                favorite.UserId,
                favorite.MediaItemId,
                Reaction = (byte)favorite.Reaction
            },
            transaction,
            cancellationToken: ct));

        transaction.Commit();
    }

    public async Task UpdateAsync(Favorite favorite, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Favorites
            SET Reaction = @Reaction,
                LikedAt = GETDATE()
            WHERE Id = @Id;",
            new
            {
                Reaction = (byte)favorite.Reaction,
                favorite.Id
            },
            cancellationToken: ct));
    }

    public async Task RemoveAsync(string id, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // First, get the MediaItemId associated with the Favorite to update FavoriteCount
        var mediaItemId = await connection.QueryFirstOrDefaultAsync<string?>(
            new CommandDefinition(
                "SELECT MediaItemId FROM Favorites WHERE Id = @Id",
                new { Id = id },
                transaction,
                cancellationToken: ct));

        await connection.ExecuteAsync(new CommandDefinition(@"
            DELETE FROM Favorites
            WHERE Id = @Id;",
            new
            {
                Id = id
            },
            transaction,
            cancellationToken: ct));

        if (!string.IsNullOrEmpty(mediaItemId))
        {
            await connection.ExecuteAsync(new CommandDefinition(@"
                UPDATE MediaItems
                SET FavoriteCount = CASE
                    WHEN FavoriteCount > 0 THEN FavoriteCount - 1
                    ELSE 0
                END
                WHERE Id = @MediaItemId;",
                new
                {
                    MediaItemId = mediaItemId
                },
                transaction,
                cancellationToken: ct));
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
    /// Dòng dữ liệu Favorite đọc trực tiếp từ SQL, giữ Reaction đúng kiểu tinyint trước khi map sang enum.
    /// </summary>
    private sealed record FavoriteRow(
        string Id,
        string UserId,
        string MediaItemId,
        byte Reaction,
        DateTime LikedAt);

    /// <summary>
    /// Map dữ liệu SQL sang entity Favorite để tầng Application không phụ thuộc Dapper row.
    /// </summary>
    private static Favorite ToEntity(FavoriteRow row)
    {
        var reaction = (FavoriteReaction)row.Reaction;
        return new Favorite(row.Id, row.UserId, row.MediaItemId, reaction);
    }
}
