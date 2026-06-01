using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace TuneVault.Infrastructure.DAO
{
    public class InteractionDAO
    {
        private readonly DapperContext _context;

        public InteractionDAO(DapperContext context)
        {
            _context = context;
        }

        // ============================================================
        // FAVORITE: LIKE / DISLIKE
        // ============================================================

        public async Task<bool> SetFavoriteStatusAsync(string userId, string mediaItemId, string likeStatus)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE [Favorite]
                SET [LikeStatus] = @LikeStatus,
                    [LikedAt] = GETDATE()
                WHERE [UserId] = @UserId
                  AND [MediaItemId] = @MediaItemId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO [Favorite]
                    (
                        [UserId],
                        [MediaItemId],
                        [LikeStatus],
                        [LikedAt]
                    )
                    VALUES
                    (
                        @UserId,
                        @MediaItemId,
                        @LikeStatus,
                        GETDATE()
                    );
                END
            ";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                MediaItemId = mediaItemId,
                LikeStatus = likeStatus
            });

            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(string userId, string mediaItemId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                DELETE FROM [Favorite]
                WHERE [UserId] = @UserId
                  AND [MediaItemId] = @MediaItemId;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                MediaItemId = mediaItemId
            });

            return affectedRows > 0;
        }

        public async Task<IEnumerable<dynamic>> GetFavoriteMediaByStatusAsync(string userId, string likeStatus)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    m.[Id],
                    m.[OwnerId],
                    m.[Title],
                    m.[Description],
                    m.[MediaUrl],
                    m.[CoverImgUrl],
                    m.[CanvasUrl],
                    m.[Duration],
                    m.[Type],
                    m.[Genre],
                    m.[IsPublic],
                    m.[UploadedAt],
                    m.[ReleaseDate],
                    m.[ViewCount],
                    f.[LikeStatus],
                    f.[LikedAt]
                FROM [Favorite] f
                INNER JOIN [MediaItem] m
                    ON f.[MediaItemId] = m.[Id]
                WHERE f.[UserId] = @UserId
                  AND f.[LikeStatus] = @LikeStatus
                ORDER BY f.[LikedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                UserId = userId,
                LikeStatus = likeStatus
            });
        }

        public async Task<IEnumerable<dynamic>> GetLikedMediaAsync(string userId)
        {
            return await GetFavoriteMediaByStatusAsync(userId, "Like");
        }

        public async Task<IEnumerable<dynamic>> GetDislikedMediaAsync(string userId)
        {
            return await GetFavoriteMediaByStatusAsync(userId, "Dislike");
        }

        public async Task<string?> GetFavoriteStatusAsync(string userId, string mediaItemId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT [LikeStatus]
                FROM [Favorite]
                WHERE [UserId] = @UserId
                  AND [MediaItemId] = @MediaItemId;
            ";

            return await connection.QueryFirstOrDefaultAsync<string?>(sql, new
            {
                UserId = userId,
                MediaItemId = mediaItemId
            });
        }

        // ============================================================
        // PLAY HISTORY
        // ============================================================

        public async Task<bool> AddPlayHistoryAsync(string userId, string mediaItemId, double? stoppedAt = null)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                INSERT INTO [PlayHistory]
                (
                    [Id],
                    [UserId],
                    [MediaItemId],
                    [PlayedAt],
                    [StoppedAt]
                )
                VALUES
                (
                    @Id,
                    @UserId,
                    @MediaItemId,
                    GETDATE(),
                    @StoppedAt
                );
            ";

            await connection.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                MediaItemId = mediaItemId,
                StoppedAt = stoppedAt
            });

            return true;
        }

        public async Task<IEnumerable<dynamic>> GetRecentPlayedMediaAsync(string userId, int limit = 10)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT TOP (@Limit)
                    ph.[Id] AS PlayHistoryId,
                    ph.[PlayedAt],
                    ph.[StoppedAt],

                    m.[Id],
                    m.[OwnerId],
                    m.[Title],
                    m.[Description],
                    m.[MediaUrl],
                    m.[CoverImgUrl],
                    m.[CanvasUrl],
                    m.[Duration],
                    m.[Type],
                    m.[Genre],
                    m.[IsPublic],
                    m.[UploadedAt],
                    m.[ReleaseDate],
                    m.[ViewCount]
                FROM [PlayHistory] ph
                INNER JOIN [MediaItem] m
                    ON ph.[MediaItemId] = m.[Id]
                WHERE ph.[UserId] = @UserId
                ORDER BY ph.[PlayedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                UserId = userId,
                Limit = limit
            });
        }

        // ============================================================
        // FOLLOW / UNFOLLOW
        // ============================================================

        public async Task<bool> FollowArtistAsync(string followerId, string followeeId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [Follow]
                    WHERE [FollowerId] = @FollowerId
                      AND [FolloweeId] = @FolloweeId
                )
                BEGIN
                    INSERT INTO [Follow]
                    (
                        [FollowerId],
                        [FolloweeId],
                        [FollowedAt]
                    )
                    VALUES
                    (
                        @FollowerId,
                        @FolloweeId,
                        GETDATE()
                    );

                    SELECT 1;
                END
                ELSE
                BEGIN
                    SELECT 0;
                END
            ";

            var result = await connection.ExecuteScalarAsync<int>(sql, new
            {
                FollowerId = followerId,
                FolloweeId = followeeId
            });

            return result == 1;
        }

        public async Task<bool> UnfollowArtistAsync(string followerId, string followeeId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                DELETE FROM [Follow]
                WHERE [FollowerId] = @FollowerId
                  AND [FolloweeId] = @FolloweeId;
            ";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                FollowerId = followerId,
                FolloweeId = followeeId
            });

            return affectedRows > 0;
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followeeId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM [Follow]
                WHERE [FollowerId] = @FollowerId
                  AND [FolloweeId] = @FolloweeId;
            ";

            var count = await connection.ExecuteScalarAsync<int>(sql, new
            {
                FollowerId = followerId,
                FolloweeId = followeeId
            });

            return count > 0;
        }

        public async Task<IEnumerable<dynamic>> GetFollowingArtistsAsync(string followerId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    u.[Id],
                    u.[UserName],
                    u.[Email],
                    u.[Role],
                    u.[Rank],
                    u.[DisplayName],
                    u.[AvatarUrl],
                    u.[CreatedAt],
                    f.[FollowedAt]
                FROM [Follow] f
                INNER JOIN [Users] u
                    ON f.[FolloweeId] = u.[Id]
                WHERE f.[FollowerId] = @FollowerId
                  AND u.[Role] = 'Artist'
                ORDER BY f.[FollowedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                FollowerId = followerId
            });
        }

        public async Task<IEnumerable<dynamic>> GetFollowersAsync(string followeeId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    u.[Id],
                    u.[UserName],
                    u.[Email],
                    u.[Role],
                    u.[Rank],
                    u.[DisplayName],
                    u.[AvatarUrl],
                    u.[CreatedAt],
                    f.[FollowedAt]
                FROM [Follow] f
                INNER JOIN [Users] u
                    ON f.[FollowerId] = u.[Id]
                WHERE f.[FolloweeId] = @FolloweeId
                ORDER BY f.[FollowedAt] DESC;
            ";

            return await connection.QueryAsync(sql, new
            {
                FolloweeId = followeeId
            });
        }

        public async Task<int> CountFollowersAsync(string followeeId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM [Follow]
                WHERE [FolloweeId] = @FolloweeId;
            ";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                FolloweeId = followeeId
            });
        }
    }
}