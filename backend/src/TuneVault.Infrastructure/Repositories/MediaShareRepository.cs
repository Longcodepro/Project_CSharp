using System.Data;
using Dapper;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Share.Commands.ShareMedia;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý lưu trữ chia sẻ media, playlist và album.
/// </summary>
public sealed class MediaShareRepository :
    IMediaShareCommandRepository,
    IMediaShareRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Khởi tạo repository chia sẻ với kết nối database.
    /// </summary>
    public MediaShareRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
    }

    /// <summary>
    /// Tìm bản ghi share đã tồn tại cho cùng sender, receiver, item và loại nội dung.
    /// </summary>
    public async Task<string?> FindExistingShareIdAsync(
        string senderId,
        string receiverId,
        ShareType shareType,
        string sharedItemId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var shareTypeValue = (int)shareType;

        return await connection.QueryFirstOrDefaultAsync<string>(new CommandDefinition(@"
            SELECT TOP 1 Id
            FROM MediaShares
            WHERE SenderId = @SenderId
              AND ReceiverId = @ReceiverId
              AND SharedItemId = @SharedItemId
              AND ShareType = @ShareType
            ORDER BY SharedAt DESC;",
            new
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                SharedItemId = sharedItemId,
                ShareType = shareTypeValue
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Tạo bản ghi chia sẻ và notification trong cùng một transaction để tránh lệch dữ liệu.
    /// </summary>
    public async Task<(string ShareId, string NotificationId)> CreateMediaShareWithNotificationAsync(
        string senderId,
        string receiverId,
        ShareType shareType,
        string sharedItemId,
        string? message,
        NotificationInsertModel notification,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            var shareId = await GenerateNextMediaShareIdAsync(connection, transaction);
            var notificationId = await GenerateNextNotificationIdAsync(connection, transaction);
            var shareTypeValue = (int)shareType;

            await connection.ExecuteAsync(new CommandDefinition(@"
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
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO Notifications
                    (Id, UserId, SenderId, NotifyType, Title, Message, TargetType, TargetId, IsRead, CreatedAt, IsActive)
                VALUES
                    (@Id, @UserId, @SenderId, @NotifyType, @Title, @Message, @TargetType, @TargetId, 0, GETDATE(), 1);",
                new
                {
                    Id = notificationId,
                    notification.UserId,
                    notification.SenderId,
                    NotifyType = (int)notification.NotifyType,
                    notification.Title,
                    notification.Message,
                    TargetType = notification.TargetType.HasValue ? (int?)notification.TargetType.Value : null,
                    notification.TargetId
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            transaction.Commit();
            return (shareId, notificationId);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra người dùng nhận chia sẻ còn hoạt động.
    /// </summary>
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

    /// <summary>
    /// Media public thì ai đăng nhập cũng share được, media private chỉ owner mới share được.
    /// </summary>
    public async Task<bool> TrackExistsAsync(string mediaItemId, string senderId)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM MediaItems
            WHERE Id = @MediaItemId
              AND IsActive = 1
              AND IsValid = 0
              AND (IsPublic = 1 OR OwnerId = @SenderId);",
            new
            {
                MediaItemId = mediaItemId,
                SenderId = senderId
            });

        return count > 0;
    }

    /// <summary>
    /// Album public thì ai đăng nhập cũng share được, album private chỉ owner mới share được.
    /// </summary>
    public async Task<bool> AlbumExistsAsync(string albumId, string senderId)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM Albums
            WHERE Id = @AlbumId
              AND IsActive = 1
              AND (IsPublic = 1 OR ArtistId = @SenderId);",
            new
            {
                AlbumId = albumId,
                SenderId = senderId
            });

        return count > 0;
    }

    /// <summary>
    /// Playlist public thì ai đăng nhập cũng share được, playlist private chỉ owner mới share được.
    /// </summary>
    public async Task<bool> PlaylistExistsAsync(string playlistId, string senderId)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM Playlists
            WHERE Id = @PlaylistId
              AND IsActive = 1
              AND (IsPublic = 1 OR UserId = @SenderId);",
            new
            {
                PlaylistId = playlistId,
                SenderId = senderId
            });

        return count > 0;
    }

    /// <summary>
    /// Lấy danh sách nội dung user hiện tại đã chia sẻ.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetSharedByMeAsync(string senderId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = BaseShareSelectSql(@"
                WHERE ms.SenderId = @SenderId
                ORDER BY ms.SharedAt DESC;");

        return await connection.QueryAsync(new CommandDefinition(
            sql,
            new { SenderId = senderId },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy danh sách nội dung được chia sẻ cho user hiện tại.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetSharedWithMeAsync(string receiverId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = BaseShareSelectSql(@"
                WHERE ms.ReceiverId = @ReceiverId
                ORDER BY ms.SharedAt DESC;");

        return await connection.QueryAsync(new CommandDefinition(
            sql,
            new { ReceiverId = receiverId },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Kiểm tra người nhận có quyền đánh dấu share đã đọc hay không.
    /// </summary>
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

    /// <summary>
    /// Chưa có trạng thái đọc riêng cho share nên tạm trả 0 để không sai dữ liệu.
    /// </summary>
    public Task<int> CountUnreadSharesAsync(string receiverId)
    {
        return Task.FromResult(0);
    }

    /// <summary>
    /// Sinh mã chia sẻ mới theo định dạng MSxxx.
    /// </summary>
    private static async Task<string> GenerateNextMediaShareIdAsync(IDbConnection connection, IDbTransaction? transaction = null)
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
            },
            transaction);

        return $"{prefix}{nextNumber:000}";
    }

    /// <summary>
    /// Sinh mã notification mới theo định dạng NTxxx.
    /// </summary>
    private static async Task<string> GenerateNextNotificationIdAsync(IDbConnection connection, IDbTransaction? transaction = null)
    {
        const string prefix = "NT";

        var nextNumber = await connection.ExecuteScalarAsync<int>(@"
            SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
            FROM Notifications
            WHERE Id LIKE @PrefixLike;",
            new
            {
                Prefix = prefix,
                PrefixLike = prefix + "%"
            },
            transaction);

        return $"{prefix}{nextNumber:000}";
    }

    /// <summary>
    /// SQL lấy lịch sử share, vẫn trả được khi item gốc đã bị ẩn hoặc xóa mềm.
    /// </summary>
    private static string BaseShareSelectSql(string whereAndOrder)
    {
        const int mediaShareType = (int)ShareType.MediaItem;
        const int albumShareType = (int)ShareType.Album;
        const int playlistShareType = (int)ShareType.Playlist;

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
                    WHEN {albumShareType} THEN 'Album'
                    WHEN {playlistShareType} THEN 'Playlist'
                    ELSE 'Track'
                END AS ShareTypeName,

                ms.SharedItemId,
                ms.Message,
                ms.SharedAt,
                CAST(0 AS bit) AS IsRead,

                COALESCE(mi.Title, a.Title, p.Title, N'Nội dung không còn khả dụng') AS ItemTitle,

                CASE
                    WHEN ms.ShareType = {mediaShareType} THEN mi.CoverImageUrl
                    WHEN ms.ShareType = {albumShareType} THEN a.CoverImageUrl
                    WHEN ms.ShareType = {playlistShareType} THEN p.CoverImageUrl
                    ELSE NULL
                END AS ItemCoverImgUrl,

                CASE
                    WHEN ms.ShareType = {mediaShareType} THEN mi.CoverImageUrl
                    WHEN ms.ShareType = {albumShareType} THEN a.CoverImageUrl
                    WHEN ms.ShareType = {playlistShareType} THEN p.CoverImageUrl
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
            LEFT JOIN MediaItems mi ON ms.ShareType = {mediaShareType} AND ms.SharedItemId = mi.Id
            LEFT JOIN Albums a ON ms.ShareType = {albumShareType} AND ms.SharedItemId = a.Id
            LEFT JOIN Playlists p ON ms.ShareType = {playlistShareType} AND ms.SharedItemId = p.Id
            {whereAndOrder}";
    }
}
