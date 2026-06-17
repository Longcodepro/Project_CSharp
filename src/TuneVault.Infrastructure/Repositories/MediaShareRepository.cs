using Dapper;
using System.Data;
using TuneVault.Application.Features.Share.Commands.ShareMedia;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

public sealed class MediaShareRepository :
    IMediaShareCommandRepository,
    IMediaShareRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MediaShareRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
    }

    public async Task<string> CreateMediaShareAsync(
        string senderId,
        string receiverId,
        string shareType,
        string sharedItemId,
        string? message)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var shareId = await GenerateNextMediaShareIdAsync(connection);
        var shareTypeValue = ToShareType(shareType);

        await connection.ExecuteAsync(@"
            INSERT INTO MediaShares
                (Id, SenderId, ReceiverId, SharedItemId, ShareType, Message, SharedAt)
            VALUES
                (@Id, @SenderId, @ReceiverId, @SharedItemId, @ShareType, @Message, GETDATE());",
            new
            {
                Id = shareId,
                SenderId = senderId,
                ReceiverId = receiverId,
                SharedItemId = sharedItemId,
                ShareType = shareTypeValue,
                Message = message
            });

        return shareId;
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM Users
            WHERE Id = @UserId
              AND IsActive = 1;",
            new { UserId = userId });

        return count > 0;
    }

    public async Task<bool> TrackExistsAsync(string mediaItemId, string senderId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM MediaItems
                WHERE Id = @MediaItemId
                  AND OwnerId = @SenderId
                  AND IsActive = 1
                  AND IsPublic = 1
                  AND IsValid = 0;",
                new
                {
                    MediaItemId = mediaItemId,
                    SenderId = senderId
                });

            return count > 0;
    }

        public async Task<bool> AlbumExistsAsync(string albumId, string senderId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM Albums
                WHERE Id = @AlbumId
                  AND ArtistId = @SenderId
                  AND IsActive = 1
                  AND IsPublic = 1;",
                new
                {
                    AlbumId = albumId,
                    SenderId = senderId
                });

            return count > 0;
    }

        public async Task<bool> PlaylistExistsAsync(string playlistId, string senderId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM Playlists
                WHERE Id = @PlaylistId
                  AND UserId = @SenderId
                  AND IsActive = 1;",
                new
                {
                    PlaylistId = playlistId,
                    SenderId = senderId
                });

            return count > 0;
    }

    public async Task<IEnumerable<dynamic>> GetSharedByMeAsync(string senderId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = BaseShareSelectSql(@"
                WHERE ms.SenderId = @SenderId
                ORDER BY ms.SharedAt DESC;");

        return await connection.QueryAsync(sql, new
        {
            SenderId = senderId
        });
    }

    public async Task<IEnumerable<dynamic>> GetSharedWithMeAsync(string receiverId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = BaseShareSelectSql(@"
                WHERE ms.ReceiverId = @ReceiverId
                ORDER BY ms.SharedAt DESC;");

        return await connection.QueryAsync(sql, new
        {
            ReceiverId = receiverId
        });
    }

    public async Task<bool> MarkShareAsReadAsync(string shareId, string receiverId)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM MediaShares
                WHERE Id = @ShareId
                  AND ReceiverId = @ReceiverId;",
                new
                {
                    ShareId = shareId,
                    ReceiverId = receiverId
                });

            return count > 0;
    }

    public Task<int> CountUnreadSharesAsync(string receiverId)
    {
        return Task.FromResult(0);
    }

    private static async Task<string> GenerateNextMediaShareIdAsync(IDbConnection connection)
    {
        const string prefix = "MS";

        var nextNumber = await connection.ExecuteScalarAsync<int>(@"
            SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
            FROM MediaShares
            WHERE Id LIKE @PrefixLike;",
            new
            {
                Prefix = prefix,
                PrefixLike = prefix + "%"
            });

        return $"{prefix}{nextNumber:000}";
    }

    private static int ToShareType(string? shareType)
    {
        return Normalize(shareType) switch
        {
            "album" => 2,
            "playlist" => 3,
            "mediaitem" => 1,
            "media" => 1,
            "song" => 1,
            "track" => 1,
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

    private static string BaseShareSelectSql(string whereAndOrder)
    {
        return $@"
            SELECT
                ms.Id,
                ms.SenderId,
                sender.IdDisplay AS SenderUserName,
                sender.IdDisplay AS SenderIdDisplay,
                sender.DisplayName AS SenderDisplayName,
                sender.AvatarUrl AS SenderAvatarUrl,

                ms.ReceiverId,
                receiver.IdDisplay AS ReceiverUserName,
                receiver.IdDisplay AS ReceiverIdDisplay,
                receiver.DisplayName AS ReceiverDisplayName,
                receiver.AvatarUrl AS ReceiverAvatarUrl,

                ms.ShareType,
                CASE ms.ShareType
                    WHEN 2 THEN 'Album'
                    WHEN 3 THEN 'Playlist'
                    ELSE 'Track'
                END AS ShareTypeName,

                ms.SharedItemId,
                ms.Message,
                ms.SharedAt,

                COALESCE(mi.Title, a.Title, p.Title) AS ItemTitle,

                CASE
                    WHEN ms.ShareType = 1 THEN mi.CoverImageUrl
                    WHEN ms.ShareType = 2 THEN a.CoverImageUrl
                    WHEN ms.ShareType = 3 THEN p.CoverImageUrl
                    ELSE NULL
                END AS ItemCoverImgUrl,

                CASE
                    WHEN ms.ShareType = 1 THEN mi.CoverImageUrl
                    WHEN ms.ShareType = 2 THEN a.CoverImageUrl
                    WHEN ms.ShareType = 3 THEN p.CoverImageUrl
                    ELSE NULL
                END AS ItemCoverImageUrl,

                COALESCE(mi.AudioUrl, mi.VideoUrl) AS MediaUrl,
                mi.AudioUrl,
                mi.VideoUrl,
                mi.DurationSeconds AS Duration,
                mi.DurationSeconds,
                mi.MediaType AS Type,
                mi.MediaType,
                mi.Genre
            FROM MediaShares ms
            INNER JOIN Users sender ON ms.SenderId = sender.Id
            INNER JOIN Users receiver ON ms.ReceiverId = receiver.Id
            LEFT JOIN MediaItems mi ON ms.ShareType = 1 AND ms.SharedItemId = mi.Id
            LEFT JOIN Albums a ON ms.ShareType = 2 AND ms.SharedItemId = a.Id
            LEFT JOIN Playlists p ON ms.ShareType = 3 AND ms.SharedItemId = p.Id
            {whereAndOrder}";
    }
}
