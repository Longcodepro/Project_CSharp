using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace TuneVault.Infrastructure.DAO
{
    public class ShareDAO
    {
        private readonly DapperContext _context;

        public ShareDAO(DapperContext context)
        {
            _context = context;
        }

        // ============================================================
        // CREATE SHARE
        // ============================================================

        public async Task<string> CreateMediaShareAsync(
            string senderId,
            string receiverId,
            string shareType,
            string sharedItemId)
        {
            using var connection = _context.CreateConnection();

            var shareId = Guid.NewGuid().ToString();

            var sql = @"
                INSERT INTO [MediaShare]
                (
                    [Id],
                    [SenderId],
                    [ReceiverId],
                    [ShareType],
                    [SharedItemId],
                    [SharedAt],
                    [IsRead]
                )
                VALUES
                (
                    @Id,
                    @SenderId,
                    @ReceiverId,
                    @ShareType,
                    @SharedItemId,
                    GETDATE(),
                    0
                );
            ";

            await connection.ExecuteAsync(sql, new
            {
                Id = shareId,
                SenderId = senderId,
                ReceiverId = receiverId,
                ShareType = shareType,
                SharedItemId = sharedItemId
            });

            return shareId;
        }

        // ============================================================
        // CHECK SHARED ITEM EXISTS
        // Dùng cho BUS gọi trước khi tạo share
        // ============================================================

        public async Task<bool> TrackExistsAsync(string mediaItemId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM [MediaItem]
                WHERE [Id] = @MediaItemId;
            ";

            var count = await connection.ExecuteScalarAsync<int>(sql, new
            {
                MediaItemId = mediaItemId
            });

            return count > 0;
        }

        public async Task<bool> AlbumExistsAsync(string albumId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM [Album]
                WHERE [Id] = @AlbumId;
            ";

            var count = await connection.ExecuteScalarAsync<int>(sql, new
            {
                AlbumId = albumId
            });

            return count > 0;
        }

        public async Task<bool> PlaylistExistsAsync(string playlistId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM [Playlist]
                WHERE [Id] = @PlaylistId;
            ";

            var count = await connection.ExecuteScalarAsync<int>(sql, new
            {
                PlaylistId = playlistId
            });

            return count > 0;
        }

        // ============================================================
        // INBOX SHARE
        // ============================================================

        public async Task<IEnumerable<dynamic>> GetInboxSharesAsync(string receiverId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    ms.[Id],
                    ms.[SenderId],
                    sender.[UserName] AS SenderUserName,
                    sender.[DisplayName] AS SenderDisplayName,
                    sender.[AvatarUrl] AS SenderAvatarUrl,

                    ms.[ReceiverId],
                    ms.[ShareType],
                    ms.[SharedItemId],
                    ms.[SharedAt],
                    ms.[IsRead],

                    COALESCE(mi.[Title], a.[Title], p.[Title]) AS ItemTitle,

                    CASE
                        WHEN ms.[ShareType] = 'Track' THEN mi.[CoverImgUrl]
                        WHEN ms.[ShareType] = 'Album' THEN a.[CoverImgUrl]
                        WHEN ms.[ShareType] = 'Playlist' THEN p.[CoverImgUrl]
                        ELSE NULL
                    END AS ItemCoverImgUrl

                FROM [MediaShare] ms
                INNER JOIN [Users] sender
                    ON ms.[SenderId] = sender.[Id]

                LEFT JOIN [MediaItem] mi
                    ON ms.[ShareType] = 'Track'
                   AND ms.[SharedItemId] = mi.[Id]

                LEFT JOIN [Album] a
                    ON ms.[ShareType] = 'Album'
                   AND ms.[SharedItemId] = a.[Id]

                LEFT JOIN [Playlist] p
                    ON ms.[ShareType] = 'Playlist'
                   AND ms.[SharedItemId] = p.[Id]

                WHERE ms.[ReceiverId] = @ReceiverId
                ORDER BY ms.[SharedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                ReceiverId = receiverId
            });
        }

        public async Task<dynamic?> GetShareByIdAsync(string shareId, string receiverId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    ms.[Id],
                    ms.[SenderId],
                    sender.[UserName] AS SenderUserName,
                    sender.[DisplayName] AS SenderDisplayName,
                    sender.[AvatarUrl] AS SenderAvatarUrl,

                    ms.[ReceiverId],
                    ms.[ShareType],
                    ms.[SharedItemId],
                    ms.[SharedAt],
                    ms.[IsRead],

                    COALESCE(mi.[Title], a.[Title], p.[Title]) AS ItemTitle,

                    CASE
                        WHEN ms.[ShareType] = 'Track' THEN mi.[CoverImgUrl]
                        WHEN ms.[ShareType] = 'Album' THEN a.[CoverImgUrl]
                        WHEN ms.[ShareType] = 'Playlist' THEN p.[CoverImgUrl]
                        ELSE NULL
                    END AS ItemCoverImgUrl,

                    mi.[MediaUrl],
                    mi.[Duration],
                    mi.[Type],
                    mi.[Genre]

                FROM [MediaShare] ms
                INNER JOIN [Users] sender
                    ON ms.[SenderId] = sender.[Id]

                LEFT JOIN [MediaItem] mi
                    ON ms.[ShareType] = 'Track'
                   AND ms.[SharedItemId] = mi.[Id]

                LEFT JOIN [Album] a
                    ON ms.[ShareType] = 'Album'
                   AND ms.[SharedItemId] = a.[Id]

                LEFT JOIN [Playlist] p
                    ON ms.[ShareType] = 'Playlist'
                   AND ms.[SharedItemId] = p.[Id]

                WHERE ms.[Id] = @ShareId
                  AND ms.[ReceiverId] = @ReceiverId;
            ";

            return await connection.QueryFirstOrDefaultAsync(sql, new
            {
                ShareId = shareId,
                ReceiverId = receiverId
            });
        }

        public async Task<IEnumerable<dynamic>> GetSentSharesAsync(string senderId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    ms.[Id],
                    ms.[SenderId],
                    ms.[ReceiverId],
                    receiver.[UserName] AS ReceiverUserName,
                    receiver.[DisplayName] AS ReceiverDisplayName,
                    receiver.[AvatarUrl] AS ReceiverAvatarUrl,

                    ms.[ShareType],
                    ms.[SharedItemId],
                    ms.[SharedAt],
                    ms.[IsRead],

                    COALESCE(mi.[Title], a.[Title], p.[Title]) AS ItemTitle,

                    CASE
                        WHEN ms.[ShareType] = 'Track' THEN mi.[CoverImgUrl]
                        WHEN ms.[ShareType] = 'Album' THEN a.[CoverImgUrl]
                        WHEN ms.[ShareType] = 'Playlist' THEN p.[CoverImgUrl]
                        ELSE NULL
                    END AS ItemCoverImgUrl

                FROM [MediaShare] ms
                INNER JOIN [Users] receiver
                    ON ms.[ReceiverId] = receiver.[Id]

                LEFT JOIN [MediaItem] mi
                    ON ms.[ShareType] = 'Track'
                   AND ms.[SharedItemId] = mi.[Id]

                LEFT JOIN [Album] a
                    ON ms.[ShareType] = 'Album'
                   AND ms.[SharedItemId] = a.[Id]

                LEFT JOIN [Playlist] p
                    ON ms.[ShareType] = 'Playlist'
                   AND ms.[SharedItemId] = p.[Id]

                WHERE ms.[SenderId] = @SenderId
                ORDER BY ms.[SharedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                SenderId = senderId
            });
        }

        // ============================================================
        // READ STATUS
        // ============================================================

        public async Task<bool> MarkShareAsReadAsync(string shareId, string receiverId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE [MediaShare]
                SET [IsRead] = 1
                WHERE [Id] = @ShareId
                  AND [ReceiverId] = @ReceiverId;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                ShareId = shareId,
                ReceiverId = receiverId
            });

            return affectedRows > 0;
        }

        public async Task<bool> MarkAllSharesAsReadAsync(string receiverId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE [MediaShare]
                SET [IsRead] = 1
                WHERE [ReceiverId] = @ReceiverId
                  AND [IsRead] = 0;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                ReceiverId = receiverId
            });

            return affectedRows > 0;
        }

        public async Task<int> CountUnreadSharesAsync(string receiverId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM [MediaShare]
                WHERE [ReceiverId] = @ReceiverId
                  AND [IsRead] = 0;
            ";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                ReceiverId = receiverId
            });
        }

        // ============================================================
        // DELETE SHARE
        // ============================================================

        public async Task<bool> DeleteShareAsync(string shareId, string receiverId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                DELETE FROM [MediaShare]
                WHERE [Id] = @ShareId
                  AND [ReceiverId] = @ReceiverId;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                ShareId = shareId,
                ReceiverId = receiverId
            });

            return affectedRows > 0;
        }
    }
}