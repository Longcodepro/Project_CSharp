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
            ORDER BY StoppedAt DESC, HistoryOrder DESC;";

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

    /// <summary>
    /// Tạo Id lịch sử nghe dạng PH001, PH002, PH003...
    /// </summary>
    private static async Task<string> GenerateNextPlayHistoryIdAsync(IDbConnection connection)
    {
        const string prefix = "PH";

        var nextNumber = await connection.ExecuteScalarAsync<int>(@"
            SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
            FROM PlayHistory
            WHERE Id LIKE @PrefixLike;",
            new
            {
                Prefix = prefix,
                PrefixLike = prefix + "%"
            });

        return $"{prefix}{nextNumber:000}";
    }
}
