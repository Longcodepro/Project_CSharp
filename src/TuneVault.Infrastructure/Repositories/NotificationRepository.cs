using Dapper;
using System.Data;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Notification.Queries.GetNotifications;
using TuneVault.Infrastructure.DAO;
using AppNotificationInsertModel = TuneVault.Application.Features.Notification.Commands.NotificationInsertModel;
namespace TuneVault.Infrastructure.Repositories
{
    /// <summary>
    /// Repository xử lý database cho bảng Notifications.
    /// File này chỉ viết SQL, không chứa logic nghiệp vụ.
    /// </summary>
    public sealed class NotificationRepository :
    INotificationCommandRepository,
    INotificationQueryRepository
    {
        private readonly DapperContext _context;

        public NotificationRepository(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Insert một notification mới.
        /// </summary>
        public async Task<string> InsertNotificationAsync(AppNotificationInsertModel notification)
        {
            using var connection = _context.CreateConnection();

            var notificationId = await GenerateNextNotificationIdAsync(connection);

            await connection.ExecuteAsync(@"
                INSERT INTO Notifications
                    (Id, UserId, SenderId, NotifyType, Title, Message, IsRead, CreatedAt, IsActive)
                VALUES
                    (@Id, @UserId, @SenderId, @NotifyType, @Title, @Message, 0, GETDATE(), 1);",
                new
                {
                    Id = notificationId,
                    notification.UserId,
                    notification.SenderId,
                    notification.NotifyType,
                    notification.Title,
                    notification.Message
                });

            return notificationId;
        }

        /// <summary>
        /// Lấy danh sách notification còn hiển thị của user.
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetNotificationsAsync(string userId, int limit = 50)
        {
            if (limit <= 0)
                limit = 50;

            using var connection = _context.CreateConnection();

            var sql = BaseNotificationSelectSql(@"
                WHERE n.UserId = @UserId
                  AND n.IsActive = 1
                ORDER BY n.CreatedAt DESC;");

            return await connection.QueryAsync(sql, new
            {
                UserId = userId,
                Limit = limit
            });
        }

        /// <summary>
        /// Lấy danh sách notification chưa đọc của user.
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetUnreadNotificationsAsync(string userId, int limit = 50)
        {
            if (limit <= 0)
                limit = 50;

            using var connection = _context.CreateConnection();

            var sql = BaseNotificationSelectSql(@"
                WHERE n.UserId = @UserId
                  AND n.IsActive = 1
                  AND n.IsRead = 0
                ORDER BY n.CreatedAt DESC;");

            return await connection.QueryAsync(sql, new
            {
                UserId = userId,
                Limit = limit
            });
        }

        /// <summary>
        /// Đếm số notification chưa đọc.
        /// </summary>
        public async Task<int> CountUnreadNotificationsAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM Notifications
                WHERE UserId = @UserId
                  AND IsActive = 1
                  AND IsRead = 0;",
                new { UserId = userId });
        }

        /// <summary>
        /// Đánh dấu notification là đã đọc.
        /// </summary>
        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            using var connection = _context.CreateConnection();

            var affectedRows = await connection.ExecuteAsync(@"
                UPDATE Notifications
                SET IsRead = 1
                WHERE Id = @NotificationId
                  AND UserId = @UserId
                  AND IsActive = 1;",
                new
                {
                    NotificationId = notificationId,
                    UserId = userId
                });

            return affectedRows > 0;
        }

        /// <summary>
        /// Xóa mềm một notification.
        /// </summary>
        public async Task<bool> DeleteAsync(string notificationId, string userId)
        {
            using var connection = _context.CreateConnection();

            var affectedRows = await connection.ExecuteAsync(@"
                UPDATE Notifications
                SET IsActive = 0
                WHERE Id = @NotificationId
                  AND UserId = @UserId
                  AND IsActive = 1;",
                new
                {
                    NotificationId = notificationId,
                    UserId = userId
                });

            return affectedRows > 0;
        }

        /// <summary>
        /// Xóa mềm toàn bộ notification của user.
        /// </summary>
        public async Task<int> DeleteAllAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            var affectedRows = await connection.ExecuteAsync(@"
                UPDATE Notifications
                SET IsActive = 0
                WHERE UserId = @UserId
                  AND IsActive = 1;",
                new { UserId = userId });

            return affectedRows;
        }

        /// <summary>
        /// Lấy thông tin ngắn gọn của user.
        /// </summary>
        public async Task<UserBrief?> GetUserBriefAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<UserBrief>(@"
                SELECT DisplayName, IdDisplay, AvatarUrl
                FROM Users
                WHERE Id = @UserId;",
                new { UserId = userId });
        }

        /// <summary>
        /// Lấy tên bài hát/media.
        /// </summary>
        public async Task<string?> GetMediaTitleAsync(string mediaItemId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT Title
                FROM MediaItems
                WHERE Id = @MediaItemId;",
                new { MediaItemId = mediaItemId });
        }

        /// <summary>
        /// Lấy tên album.
        /// </summary>
        public async Task<string?> GetAlbumTitleAsync(string albumId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT Title
                FROM Albums
                WHERE Id = @AlbumId;",
                new { AlbumId = albumId });
        }

        /// <summary>
        /// Lấy tên playlist.
        /// </summary>
        public async Task<string?> GetPlaylistTitleAsync(string playlistId)
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT Title
                FROM Playlists
                WHERE Id = @PlaylistId;",
                new { PlaylistId = playlistId });
        }

        /// <summary>
        /// SQL SELECT chung cho notification.
        /// </summary>
        private static string BaseNotificationSelectSql(string whereAndOrder)
        {
            return $@"
                SELECT TOP (@Limit)
                    n.Id,
                    n.UserId,
                    n.SenderId,
                    sender.IdDisplay AS SenderIdDisplay,
                    sender.DisplayName AS SenderDisplayName,
                    sender.AvatarUrl AS SenderAvatarUrl,
                    n.NotifyType,
                    n.NotifyType AS TypeId,
                    CASE n.NotifyType
                        WHEN 1 THEN 'NewFollower'
                        WHEN 2 THEN 'FriendRequest'
                        WHEN 3 THEN 'MediaShared'
                        WHEN 4 THEN 'SystemAlert'
                        WHEN 5 THEN 'FriendAccepted'
                        WHEN 6 THEN 'ArtistNewMedia'
                        ELSE 'SystemAlert'
                    END AS Type,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    CASE WHEN n.IsRead = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS HasRead,
                    CASE WHEN n.IsRead = 1 THEN N'Đã đọc' ELSE N'Chưa đọc' END AS ReadStatus,
                    n.IsActive,
                    n.CreatedAt
                FROM Notifications n
                LEFT JOIN Users sender ON n.SenderId = sender.Id
                {whereAndOrder}";
        }

        /// <summary>
        /// Tạo Id notification dạng NT001, NT002, NT003...
        /// Hàm này thay cho DaoSqlHelper.GenerateNextIdAsync.
        /// </summary>
        private static async Task<string> GenerateNextNotificationIdAsync(IDbConnection connection)
        {
            const string prefix = "NT";

            var lastId = await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT TOP 1 Id
                FROM Notifications
                WHERE Id LIKE @PrefixLike
                ORDER BY TRY_CONVERT(INT, SUBSTRING(Id, LEN(@Prefix) + 1, 20)) DESC;",
                new
                {
                    Prefix = prefix,
                    PrefixLike = $"{prefix}%"
                });

            if (string.IsNullOrWhiteSpace(lastId))
                return $"{prefix}001";

            var numberPart = lastId[prefix.Length..];

            if (!int.TryParse(numberPart, out var currentNumber))
                currentNumber = 0;

            return $"{prefix}{currentNumber + 1:000}";
        }
    }
}