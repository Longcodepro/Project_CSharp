using Dapper;
using System.Data;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý SQL cho PlayHistory.
/// </summary>
public sealed class PlayHistoryRepository : IPlayHistoryRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public PlayHistoryRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
    }

    public async Task<PlayHistory?> GetByUserIdAndMediaItemIdAsync(string userId, string mediaItemId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
            SELECT
                Id,
                UserId,
                MediaItemId,
                HistoryOrder,
                StoppedAt
            FROM PlayHistory
            WHERE UserId = @UserId
              AND MediaItemId = @MediaItemId;";

        return await connection.QueryFirstOrDefaultAsync<PlayHistory>(sql, new
        {
            UserId = userId,
            MediaItemId = mediaItemId
        });
    }

    public async Task<IReadOnlyCollection<PlayHistory>> GetRecentByUserIdAsync(string userId, int take = 10, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
            SELECT TOP (@Take)
                Id,
                UserId,
                MediaItemId,
                HistoryOrder,
                StoppedAt
            FROM PlayHistory
            WHERE UserId = @UserId
            ORDER BY HistoryOrder ASC;";

        var result = await connection.QueryAsync<PlayHistory>(sql, new
        {
            UserId = userId,
            Take = take
        });

        return result.ToList();
    }

    public async Task<bool> MediaItemExistsAsync(string mediaItemId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(1)
            FROM MediaItems
            WHERE Id = @MediaItemId AND IsActive = 1;
            """;

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { MediaItemId = mediaItemId }, cancellationToken: ct));

        return count > 0;
    }

    public async Task AddAsync(PlayHistory playHistory, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var id = await GenerateNextPlayHistoryIdAsync(connection);
        playHistory.Id = id;

        await connection.ExecuteAsync(@"
            INSERT INTO PlayHistory
                (Id, UserId, MediaItemId, HistoryOrder, StoppedAt)
            VALUES
                (@Id, @UserId, @MediaItemId, @HistoryOrder, @StoppedAt);",
            playHistory);
    }

    public async Task UpdateAsync(PlayHistory playHistory, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(@"
            UPDATE PlayHistory
            SET HistoryOrder = @HistoryOrder,
                StoppedAt = @StoppedAt
            WHERE Id = @Id;",
            playHistory);
    }

    public async Task SaveRecentPlaybackAsync(PlayHistory playHistory, bool isNewRecord, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        if (isNewRecord)
        {
            playHistory.Id = await GenerateNextPlayHistoryIdAsync(connection, transaction);
        }

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE PlayHistory
                SET HistoryOrder = HistoryOrder + 1
                WHERE UserId = @UserId
                  AND (@CurrentId IS NULL OR Id <> @CurrentId);
                """,
                new
                {
                    playHistory.UserId,
                    CurrentId = isNewRecord ? null : playHistory.Id
                },
                transaction,
                cancellationToken: ct));

        if (isNewRecord)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO PlayHistory
                        (Id, UserId, MediaItemId, HistoryOrder, StoppedAt)
                    VALUES
                        (@Id, @UserId, @MediaItemId, @HistoryOrder, @StoppedAt);
                    """,
                    playHistory,
                    transaction,
                    cancellationToken: ct));
        }
        else
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    UPDATE PlayHistory
                    SET HistoryOrder = @HistoryOrder,
                        StoppedAt = @StoppedAt
                    WHERE Id = @Id;
                    """,
                    playHistory,
                    transaction,
                    cancellationToken: ct));
        }

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                WITH Ranked AS
                (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            ORDER BY HistoryOrder ASC,
                                     CASE WHEN StoppedAt IS NULL THEN 1 ELSE 0 END ASC,
                                     StoppedAt DESC,
                                     Id ASC
                        ) AS NewOrder
                    FROM PlayHistory
                    WHERE UserId = @UserId
                )
                UPDATE ph
                SET HistoryOrder = ranked.NewOrder
                FROM PlayHistory ph
                INNER JOIN Ranked ranked ON ranked.Id = ph.Id;
                """,
                new { playHistory.UserId },
                transaction,
                cancellationToken: ct));

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE FROM PlayHistory
                WHERE UserId = @UserId
                  AND HistoryOrder > 10;
                """,
                new { playHistory.UserId },
                transaction,
                cancellationToken: ct));

        transaction.Commit();
    }

    /// <summary>
    /// Tạo Id lịch sử nghe dạng PH001, PH002, PH003...
    /// </summary>
    private static async Task<string> GenerateNextPlayHistoryIdAsync(IDbConnection connection, IDbTransaction? transaction = null)
    {
        const string prefix = "PH";

        var nextNumber = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
                FROM PlayHistory
                WHERE Id LIKE @PrefixLike;
                """,
                new
                {
                    Prefix = prefix,
                    PrefixLike = prefix + "%"
                },
                transaction));

        return $"{prefix}{nextNumber:000}";
    }
}
