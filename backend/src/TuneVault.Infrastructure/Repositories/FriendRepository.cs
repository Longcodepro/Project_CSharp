using Dapper;
using System.Data;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Domain.Enums;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository Dapper xử lý lời mời kết bạn và quan hệ bạn bè.
/// </summary>
public sealed class FriendRepository : IFriendRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Khởi tạo repository với factory tạo kết nối database.
    /// </summary>
    public FriendRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
    }

    /// <summary>
    /// Kiểm tra người dùng có còn hoạt động hay không.
    /// </summary>
    public async Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(1)
            FROM Users
            WHERE Id = @UserId
              AND IsActive = 1;",
            new { UserId = userId },
            cancellationToken: cancellationToken));

        return count > 0;
    }

    /// <summary>
    /// Lấy bản ghi bạn bè hoặc lời mời theo mã.
    /// </summary>
    public async Task<FriendRelationSnapshot?> GetByIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<FriendRelationSnapshot>(new CommandDefinition(@"
            SELECT
                Id,
                RequestedById,
                RequestedToId,
                Status,
                CreatedAt
            FROM Friends
            WHERE Id = @RequestId
              AND IsActive = 1;",
            new { RequestId = requestId },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy quan hệ hiện có giữa hai user mà không phân biệt chiều gửi nhận.
    /// </summary>
    public async Task<FriendRelationSnapshot?> GetRelationshipAsync(string firstUserId, string secondUserId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<FriendRelationSnapshot>(new CommandDefinition(@"
            SELECT TOP (1)
                Id,
                RequestedById,
                RequestedToId,
                Status,
                CreatedAt
            FROM Friends
            WHERE (
                    (RequestedById = @FirstUserId AND RequestedToId = @SecondUserId)
                 OR (RequestedById = @SecondUserId AND RequestedToId = @FirstUserId)
                  )
              AND IsActive = 1
            ORDER BY CreatedAt DESC;",
            new
            {
                FirstUserId = firstUserId,
                SecondUserId = secondUserId
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Tạo một lời mời kết bạn mới ở trạng thái pending.
    /// </summary>
    public async Task<string> CreateRequestAsync(string requestedById, string requestedToId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var requestId = await GenerateNextFriendIdAsync(connection, cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO Friends
                (Id, RequestedById, RequestedToId, Status, CreatedAt, IsActive)
            VALUES
                (@Id, @RequestedById, @RequestedToId, @Status, GETDATE(), 1);",
                new
                {
                    Id = requestId,
                    RequestedById = requestedById,
                    RequestedToId = requestedToId,
                    Status = FriendStatus.Pending
                },
            cancellationToken: cancellationToken));

        return requestId;
    }

    /// <summary>
    /// Chuyển lời mời từ pending sang accepted.
    /// </summary>
    public async Task AcceptRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Friends
            SET Status = @AcceptedStatus
            WHERE Id = @RequestId
              AND Status = @PendingStatus
              AND IsActive = 1;",
            new
            {
                RequestId = requestId,
                AcceptedStatus = FriendStatus.Accepted,
                PendingStatus = FriendStatus.Pending
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Hủy lời mời đang pending bằng soft delete khi người nhận từ chối hoặc người gửi hủy.
    /// </summary>
    public async Task DeletePendingRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Friends
            SET IsActive = 0
            WHERE Id = @RequestId
              AND Status = @PendingStatus
              AND IsActive = 1;",
            new
            {
                RequestId = requestId,
                PendingStatus = FriendStatus.Pending
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Hủy quan hệ bạn bè đã chấp nhận giữa hai user bằng soft delete.
    /// </summary>
    public async Task DeleteAcceptedFriendshipAsync(string currentUserId, string friendUserId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE Friends
            SET IsActive = 0
            WHERE Status = @AcceptedStatus
              AND IsActive = 1
              AND (
                    (RequestedById = @CurrentUserId AND RequestedToId = @FriendUserId)
                 OR (RequestedById = @FriendUserId AND RequestedToId = @CurrentUserId)
              );",
            new
            {
                CurrentUserId = currentUserId,
                FriendUserId = friendUserId,
                AcceptedStatus = FriendStatus.Accepted
            },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy danh sách bạn bè của một user.
    /// </summary>
    public async Task<IReadOnlyCollection<FriendListItem>> GetFriendsAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var items = await connection.QueryAsync<FriendListItem>(new CommandDefinition(@"
            SELECT
                CASE WHEN f.RequestedById = @UserId THEN u2.Id ELSE u1.Id END AS UserId,
                CASE WHEN f.RequestedById = @UserId THEN u2.IdDisplay ELSE u1.IdDisplay END AS IdDisplay,
                CASE WHEN f.RequestedById = @UserId THEN u2.DisplayName ELSE u1.DisplayName END AS DisplayName,
                CASE WHEN f.RequestedById = @UserId THEN u2.AvatarUrl ELSE u1.AvatarUrl END AS AvatarUrl,
                f.CreatedAt AS FriendsSince
            FROM Friends f
            INNER JOIN Users u1 ON f.RequestedById = u1.Id
            INNER JOIN Users u2 ON f.RequestedToId = u2.Id
            WHERE f.Status = @AcceptedStatus
              AND f.IsActive = 1
              AND (f.RequestedById = @UserId OR f.RequestedToId = @UserId)
              AND u1.IsActive = 1
              AND u2.IsActive = 1
            ORDER BY f.CreatedAt DESC;",
            new
            {
                UserId = userId,
                AcceptedStatus = FriendStatus.Accepted
            },
            cancellationToken: cancellationToken));

        return items.ToList();
    }

    /// <summary>
    /// Lấy danh sách lời mời người dùng hiện tại nhận được.
    /// </summary>
    public async Task<IReadOnlyCollection<FriendRequestItem>> GetIncomingRequestsAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var items = await connection.QueryAsync<FriendRequestItem>(new CommandDefinition(@"
            SELECT
                f.Id AS RequestId,
                u.Id AS UserId,
                u.IdDisplay,
                u.DisplayName,
                u.AvatarUrl,
                f.CreatedAt AS RequestedAt,
                'incoming' AS Direction
            FROM Friends f
            INNER JOIN Users u ON f.RequestedById = u.Id
            WHERE f.RequestedToId = @UserId
              AND f.Status = @PendingStatus
              AND f.IsActive = 1
              AND u.IsActive = 1
            ORDER BY f.CreatedAt DESC;",
            new
            {
                UserId = userId,
                PendingStatus = FriendStatus.Pending
            },
            cancellationToken: cancellationToken));

        return items.ToList();
    }

    /// <summary>
    /// Lấy danh sách lời mời người dùng hiện tại đã gửi.
    /// </summary>
    public async Task<IReadOnlyCollection<FriendRequestItem>> GetSentRequestsAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var items = await connection.QueryAsync<FriendRequestItem>(new CommandDefinition(@"
            SELECT
                f.Id AS RequestId,
                u.Id AS UserId,
                u.IdDisplay,
                u.DisplayName,
                u.AvatarUrl,
                f.CreatedAt AS RequestedAt,
                'sent' AS Direction
            FROM Friends f
            INNER JOIN Users u ON f.RequestedToId = u.Id
            WHERE f.RequestedById = @UserId
              AND f.Status = @PendingStatus
              AND f.IsActive = 1
              AND u.IsActive = 1
            ORDER BY f.CreatedAt DESC;",
            new
            {
                UserId = userId,
                PendingStatus = FriendStatus.Pending
            },
            cancellationToken: cancellationToken));

        return items.ToList();
    }

    /// <summary>
    /// Sinh mã friend request mới.
    /// </summary>
    private static async Task<string> GenerateNextFriendIdAsync(IDbConnection connection, CancellationToken cancellationToken)
    {
        const string prefix = "FR";

        var nextNumber = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(Id, LEN(@Prefix) + 1, 20))), 0) + 1
            FROM Friends
            WHERE Id LIKE @PrefixLike;",
            new
            {
                Prefix = prefix,
                PrefixLike = prefix + "%"
            },
            cancellationToken: cancellationToken));

        return $"{prefix}{nextNumber:000}";
    }
}
