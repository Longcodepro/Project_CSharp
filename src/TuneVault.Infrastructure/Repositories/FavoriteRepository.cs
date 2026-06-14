using Dapper;
using System.Data;
using TuneVault.Application.Features.Favorite.Commands;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý SQL cho Favorite.
/// Chỉ chứa SQL và helper liên quan trực tiếp đến Favorite.
/// </summary>
public sealed class FavoriteRepository : IFavoriteSqlRepository
{
    private readonly DapperContext _context;

    public FavoriteRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFavoriteAsync(string userId, string mediaItemId)
    {
        using var connection = _context.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM Favorites
            WHERE UserId = @UserId
              AND MediaItemId = @MediaItemId;",
            new
            {
                UserId = userId,
                MediaItemId = mediaItemId
            });

        return count > 0;
    }

    public async Task<IEnumerable<dynamic>> GetByUserIdAsync(string userId)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            SELECT
                m.Id,
                m.OwnerId,
                m.Title,
                m.Description,
                COALESCE(m.AudioUrl, m.VideoUrl) AS MediaUrl,
                m.AudioUrl,
                m.VideoUrl,
                m.CoverImageUrl,
                m.CoverImageUrl AS CoverImgUrl,
                m.CanvasUrl,
                m.DurationSeconds,
                m.DurationSeconds AS Duration,
                m.MediaType,
                m.MediaType AS Type,
                m.Genre,
                m.IsPublic,
                m.UploadedAt,
                m.ReleaseDate,
                m.ViewCount,
                f.Id AS FavoriteId,
                f.Reaction,
                CASE f.Reaction
                    WHEN 2 THEN 'Love'
                    WHEN 3 THEN 'Chill'
                    WHEN 4 THEN 'Energetic'
                    ELSE 'Like'
                END AS LikeStatus,
                f.LikedAt
            FROM Favorites f
            INNER JOIN MediaItems m ON f.MediaItemId = m.Id
            WHERE f.UserId = @UserId
            ORDER BY f.LikedAt DESC;";

        return await connection.QueryAsync(sql, new
        {
            UserId = userId
        });
    }

    public async Task ToggleAsync(string userId, string mediaItemId)
    {
        var isFavorite = await IsFavoriteAsync(userId, mediaItemId);

        if (isFavorite)
        {
            await RemoveAsync(userId, mediaItemId);
            return;
        }

        await SetReactionAsync(userId, mediaItemId, "Like");
    }

    public async Task SetReactionAsync(string userId, string mediaItemId, string reaction)
    {
        if (string.Equals(reaction, "Dislike", StringComparison.OrdinalIgnoreCase))
        {
            await RemoveAsync(userId, mediaItemId);
            return;
        }

        using var connection = _context.CreateConnection();

        var reactionValue = ToFavoriteReaction(reaction);

        var existingId = await connection.QueryFirstOrDefaultAsync<string?>(@"
            SELECT Id
            FROM Favorites
            WHERE UserId = @UserId
              AND MediaItemId = @MediaItemId;",
            new
            {
                UserId = userId,
                MediaItemId = mediaItemId
            });

        if (!string.IsNullOrWhiteSpace(existingId))
        {
            await connection.ExecuteAsync(@"
                UPDATE Favorites
                SET Reaction = @Reaction,
                    LikedAt = GETDATE()
                WHERE Id = @Id;",
                new
                {
                    Id = existingId,
                    Reaction = reactionValue
                });

            return;
        }

        var id = await GenerateNextFavoriteIdAsync(connection);

        await connection.ExecuteAsync(@"
            INSERT INTO Favorites
                (Id, UserId, MediaItemId, Reaction, LikedAt)
            VALUES
                (@Id, @UserId, @MediaItemId, @Reaction, GETDATE());

            UPDATE MediaItems
            SET FavoriteCount = FavoriteCount + 1
            WHERE Id = @MediaItemId;",
            new
            {
                Id = id,
                UserId = userId,
                MediaItemId = mediaItemId,
                Reaction = reactionValue
            });
    }

    public async Task RemoveAsync(string userId, string mediaItemId)
    {
        using var connection = _context.CreateConnection();

        await connection.ExecuteAsync(@"
            DELETE FROM Favorites
            WHERE UserId = @UserId
              AND MediaItemId = @MediaItemId;

            IF @@ROWCOUNT > 0
            BEGIN
                UPDATE MediaItems
                SET FavoriteCount = CASE
                    WHEN FavoriteCount > 0 THEN FavoriteCount - 1
                    ELSE 0
                END
                WHERE Id = @MediaItemId;
            END",
            new
            {
                UserId = userId,
                MediaItemId = mediaItemId
            });
    }

    /// <summary>
    /// Tạo Id Favorite dạng FV001, FV002, FV003...
    /// </summary>
    private static async Task<string> GenerateNextFavoriteIdAsync(IDbConnection connection)
    {
        const string prefix = "FV";

        var nextNumber = await connection.ExecuteScalarAsync<int>(@"
            SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
            FROM Favorites
            WHERE Id LIKE @PrefixLike;",
            new
            {
                Prefix = prefix,
                PrefixLike = prefix + "%"
            });

        return $"{prefix}{nextNumber:000}";
    }

    /// <summary>
    /// Chuyển reaction dạng string sang số lưu trong database.
    /// 1 = Like, 2 = Love, 3 = Chill, 4 = Energetic.
    /// </summary>
    private static int ToFavoriteReaction(string? reaction)
    {
        return Normalize(reaction) switch
        {
            "love" => 2,
            "chill" => 3,
            "energetic" => 4,
            _ => 1
        };
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim()
                   .Replace(" ", string.Empty)
                   .Replace("_", string.Empty)
                   .Replace("-", string.Empty)
                   .ToLowerInvariant();
    }
}