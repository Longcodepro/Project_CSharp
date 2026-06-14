using Dapper;
using System.Data;
using TuneVault.Application.Features.History.Commands;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý SQL cho PlayHistory.
/// File này chỉ chứa database, không chứa logic controller.
/// Không dùng DaoSqlHelper.
/// </summary>
public sealed class PlayHistoryRepository : IPlayHistorySqlRepository
{
    private readonly DapperContext _context;

    public PlayHistoryRepository(DapperContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Ghi nhận một lần nghe bài hát.
    /// Database hiện tại dùng PlayHistory(Id, UserId, MediaItemId, HistoryOrder, StoppedAt).
    /// Theo DAO cũ, StoppedAt đang lưu GETDATE().
    /// </summary>
    public async Task<bool> RecordAsync(
        string userId,
        string mediaItemId,
        double? stoppedAt = null)
    {
        using var connection = _context.CreateConnection();

        var id = await GenerateNextPlayHistoryIdAsync(connection);

        await connection.ExecuteAsync(@"
            INSERT INTO PlayHistory
                (Id, UserId, MediaItemId, HistoryOrder, StoppedAt)
            VALUES
            (
                @Id,
                @UserId,
                @MediaItemId,
                ISNULL(
                    (
                        SELECT MAX(HistoryOrder)
                        FROM PlayHistory
                        WHERE UserId = @UserId
                    ),
                    0
                ) + 1,
                GETDATE()
            );",
            new
            {
                Id = id,
                UserId = userId,
                MediaItemId = mediaItemId
            });

        return true;
    }

    /// <summary>
    /// Lấy danh sách media item user nghe gần đây.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetRecentByUserIdAsync(
        string userId,
        int limit = 10)
    {
        using var connection = _context.CreateConnection();

        if (limit <= 0)
            limit = 10;

        var sql = @"
            SELECT TOP (@Limit)
                ph.Id AS PlayHistoryId,
                ph.HistoryOrder,
                ph.StoppedAt,
                ph.StoppedAt AS PlayedAt,

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
                m.ViewCount
            FROM PlayHistory ph
            INNER JOIN MediaItems m ON ph.MediaItemId = m.Id
            WHERE ph.UserId = @UserId
            ORDER BY ph.HistoryOrder DESC, ph.StoppedAt DESC;";

        return await connection.QueryAsync(sql, new
        {
            UserId = userId,
            Limit = limit
        });
    }

    /// <summary>
    /// Tạo Id lịch sử nghe dạng PH001, PH002, PH003...
    /// Gộp từ DaoSqlHelper.GenerateNextIdAsync, chỉ giữ phần liên quan đến PlayHistory.
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