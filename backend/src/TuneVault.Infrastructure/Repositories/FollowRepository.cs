using Dapper;
using System.Data;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories
{
    /// <summary>
    /// Repository xử lý SQL cho Follow.
    /// File này chỉ chứa database, không chứa logic notification.
    /// </summary>
    public sealed class FollowRepository : IFollowRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public FollowRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        /// <summary>
        /// Follow một user/nghệ sĩ.
        /// Nếu đã có quan hệ follow nhưng IsActive = 0 thì bật lại IsActive = 1.
        /// Nếu chưa có thì insert mới.
        /// </summary>
        public async Task FollowAsync(string followerId, string followeeId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var existing = await connection.QueryFirstOrDefaultAsync<dynamic?>(@"
                SELECT Id, IsActive
                FROM Follows
                WHERE FollowerId = @FollowerId
                  AND FolloweeId = @FolloweeId;",
                new
                {
                    FollowerId = followerId,
                    FolloweeId = followeeId
                });

            if (existing != null)
            {
                bool isActive = existing.IsActive;

                if (isActive)
                    return;

                await connection.ExecuteAsync(@"
                    UPDATE Follows
                    SET IsActive = 1,
                        FollowedAt = GETDATE()
                    WHERE Id = @Id;

                    UPDATE Users
                    SET TotalFollowers = TotalFollowers + 1
                    WHERE Id = @FolloweeId;",
                    new
                    {
                        Id = (string)existing.Id,
                        FolloweeId = followeeId
                    });

                return;
            }

            var id = await GenerateNextFollowIdAsync(connection);

            await connection.ExecuteAsync(@"
                INSERT INTO Follows
                    (Id, FollowerId, FolloweeId, FollowedAt, IsActive)
                VALUES
                    (@Id, @FollowerId, @FolloweeId, GETDATE(), 1);

                UPDATE Users
                SET TotalFollowers = TotalFollowers + 1
                WHERE Id = @FolloweeId;",
                new
                {
                    Id = id,
                    FollowerId = followerId,
                    FolloweeId = followeeId
                });
        }

        /// <summary>
        /// Bỏ follow bằng cách chuyển IsActive = 0.
        /// Không xóa dòng khỏi database.
        /// </summary>
        public async Task UnfollowAsync(string followerId, string followeeId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(@"
                UPDATE Follows
                SET IsActive = 0
                WHERE FollowerId = @FollowerId
                  AND FolloweeId = @FolloweeId
                  AND IsActive = 1;

                IF @@ROWCOUNT > 0
                BEGIN
                    UPDATE Users
                    SET TotalFollowers = CASE WHEN TotalFollowers > 0 THEN TotalFollowers - 1 ELSE 0 END
                    WHERE Id = @FolloweeId;
                END",
                new
                {
                    FollowerId = followerId,
                    FolloweeId = followeeId
                });

        }

        /// <summary>
        /// Kiểm tra follower có đang follow followee không.
        /// </summary>
        public async Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM Follows
                WHERE FollowerId = @FollowerId
                  AND FolloweeId = @FolloweeId
                  AND IsActive = 1;",
                new
                {
                    FollowerId = followerId,
                    FolloweeId = followeeId
                });

            return count > 0;
        }

        /// <summary>
        /// Lấy danh sách nghệ sĩ/user mà user đang follow.
        /// </summary>
        public async Task<IReadOnlyCollection<Follow>> GetFollowingAsync(string userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
                SELECT
                    f.Id,
                    f.FollowerId,
                    f.FolloweeId,
                    f.FollowedAt,
                    f.IsActive
                FROM Follows f
                WHERE f.FollowerId = @FollowerId
                  AND f.IsActive = 1
                ORDER BY f.FollowedAt DESC;";

            var rows = await connection.QueryAsync(sql, new
            {
                FollowerId = userId
            });

            return rows.Select(MapFollow).ToList();
        }

        /// <summary>
        /// Lấy danh sách người đang follow một user/nghệ sĩ.
        /// </summary>
        public async Task<IReadOnlyCollection<Follow>> GetFollowersAsync(string userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
                SELECT
                    f.Id,
                    f.FollowerId,
                    f.FolloweeId,
                    f.FollowedAt,
                    f.IsActive
                FROM Follows f
                WHERE f.FolloweeId = @FolloweeId
                  AND f.IsActive = 1
                ORDER BY f.FollowedAt DESC;";

            var rows = await connection.QueryAsync(sql, new
            {
                FolloweeId = userId
            });

            return rows.Select(MapFollow).ToList();
        }

        /// <summary>
        /// Đếm số follower của một user/nghệ sĩ.
        /// </summary>
        public async Task<int> CountFollowersAsync(string userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM Follows
                WHERE FolloweeId = @FolloweeId
                  AND IsActive = 1;",
                new
                {
                    FolloweeId = userId
                });
        }

        private static Follow MapFollow(dynamic row)
        {
            var follow = new Follow(
                (string)row.Id,
                (string)row.FollowerId,
                (string)row.FolloweeId);

            return follow;
        }

        /// <summary>
        /// Tạo Id follow dạng FW001, FW002...
        /// Không dùng DaoSqlHelper nữa.
        /// </summary>
        private static async Task<string> GenerateNextFollowIdAsync(IDbConnection connection)
        {
            const string prefix = "FW";

            var lastId = await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT TOP 1 Id
                FROM Follows
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
