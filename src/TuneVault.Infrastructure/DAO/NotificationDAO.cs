using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace TuneVault.Infrastructure.DAO
{
    public class NotificationDAO
    {
        private readonly DapperContext _context;

        public NotificationDAO(DapperContext context)
        {
            _context = context;
        }

        // ============================================================
        // CREATE NOTIFICATION
        // ============================================================

        public async Task<string> CreateNotificationAsync(
            string userId,
            string type,
            string? payloadJson = null)
        {
            using var connection = _context.CreateConnection();

            var notificationId = Guid.NewGuid().ToString();

            var sql = @"
                INSERT INTO [Notification]
                (
                    [Id],
                    [UserId],
                    [Type],
                    [PayloadJson],
                    [IsRead],
                    [CreatedAt]
                )
                VALUES
                (
                    @Id,
                    @UserId,
                    @Type,
                    @PayloadJson,
                    0,
                    GETDATE()
                );
            ";

            await connection.ExecuteAsync(sql, new
            {
                Id = notificationId,
                UserId = userId,
                Type = type,
                PayloadJson = payloadJson
            });

            return notificationId;
        }

        public async Task<bool> CreateNotificationsAsync(IEnumerable<NotificationInsertModel> notifications)
        {
            var rows = notifications.ToList();

            if (!rows.Any())
                return true;

            using var connection = _context.CreateConnection();

            var sql = @"
                INSERT INTO [Notification]
                (
                    [Id],
                    [UserId],
                    [Type],
                    [PayloadJson],
                    [IsRead],
                    [CreatedAt]
                )
                VALUES
                (
                    @Id,
                    @UserId,
                    @Type,
                    @PayloadJson,
                    0,
                    GETDATE()
                );
            ";

            await connection.ExecuteAsync(sql, rows);

            return true;
        }

        // ============================================================
        // GET NOTIFICATIONS
        // ============================================================

        public async Task<IEnumerable<dynamic>> GetNotificationsAsync(string userId, int limit = 50)
        {
            if (limit <= 0)
                limit = 50;

            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT TOP (@Limit)
                    [Id],
                    [UserId],
                    [Type],
                    [PayloadJson],
                    [IsRead],
                    [CreatedAt]
                FROM [Notification]
                WHERE [UserId] = @UserId
                ORDER BY [CreatedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                UserId = userId,
                Limit = limit
            });
        }

        public async Task<IEnumerable<dynamic>> GetUnreadNotificationsAsync(string userId, int limit = 50)
        {
            if (limit <= 0)
                limit = 50;

            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT TOP (@Limit)
                    [Id],
                    [UserId],
                    [Type],
                    [PayloadJson],
                    [IsRead],
                    [CreatedAt]
                FROM [Notification]
                WHERE [UserId] = @UserId
                  AND [IsRead] = 0
                ORDER BY [CreatedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                UserId = userId,
                Limit = limit
            });
        }

        public async Task<dynamic?> GetNotificationByIdAsync(string notificationId, string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    [Id],
                    [UserId],
                    [Type],
                    [PayloadJson],
                    [IsRead],
                    [CreatedAt]
                FROM [Notification]
                WHERE [Id] = @NotificationId
                  AND [UserId] = @UserId;
            ";

            return await connection.QueryFirstOrDefaultAsync(sql, new
            {
                NotificationId = notificationId,
                UserId = userId
            });
        }

        public async Task<int> CountUnreadNotificationsAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM [Notification]
                WHERE [UserId] = @UserId
                  AND [IsRead] = 0;
            ";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserId = userId
            });
        }

        // ============================================================
        // MARK AS READ
        // ============================================================

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE [Notification]
                SET [IsRead] = 1
                WHERE [Id] = @NotificationId
                  AND [UserId] = @UserId;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                NotificationId = notificationId,
                UserId = userId
            });

            return affectedRows > 0;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE [Notification]
                SET [IsRead] = 1
                WHERE [UserId] = @UserId
                  AND [IsRead] = 0;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                UserId = userId
            });

            return affectedRows > 0;
        }

        // ============================================================
        // DELETE
        // ============================================================

        public async Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                DELETE FROM [Notification]
                WHERE [Id] = @NotificationId
                  AND [UserId] = @UserId;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                NotificationId = notificationId,
                UserId = userId
            });

            return affectedRows > 0;
        }
    }

    public class NotificationInsertModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string? PayloadJson { get; set; }
    }
}