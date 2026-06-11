using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository triển khai các thao tác quản lý quan hệ follow trong TuneVault.
/// Lớp này sử dụng InteractionDAO để lưu, xóa, kiểm tra và ánh xạ dữ liệu follow sang entity Domain.
/// </summary>
public sealed class FollowRepository : IFollowRepository
{
    private readonly InteractionDAO _interactionDao;

    /// <summary>
    /// Khởi tạo một instance mới của FollowRepository với DAO xử lý dữ liệu tương tác.
    /// </summary>
    public FollowRepository(InteractionDAO interactionDao)
    {
        _interactionDao = interactionDao;
    }

    /// <summary>
    /// Tạo quan hệ theo dõi từ người dùng đến người dùng hoặc nghệ sĩ khác.
    /// </summary>
    public async Task FollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
    {
        await _interactionDao.FollowArtistAsync(
            RepositoryMappingHelper.ToDatabaseId(followerId),
            RepositoryMappingHelper.ToDatabaseId(followeeId));
    }

    /// <summary>
    /// Hủy quan hệ theo dõi giữa người dùng và đối tượng đang được theo dõi.
    /// </summary>
    public async Task UnfollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
    {
        await _interactionDao.UnfollowArtistAsync(
            RepositoryMappingHelper.ToDatabaseId(followerId),
            RepositoryMappingHelper.ToDatabaseId(followeeId));
    }

    /// <summary>
    /// Kiểm tra người dùng có đang theo dõi một người dùng hoặc nghệ sĩ khác hay không.
    /// </summary>
    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
    {
        return await _interactionDao.IsFollowingAsync(
            RepositoryMappingHelper.ToDatabaseId(followerId),
            RepositoryMappingHelper.ToDatabaseId(followeeId));
    }

    /// <summary>
    /// Lấy danh sách những người đang theo dõi một người dùng hoặc nghệ sĩ.
    /// </summary>
    public async Task<IReadOnlyCollection<Follow>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var followeeId = RepositoryMappingHelper.ToDatabaseId(userId);
        var rows = await _interactionDao.GetFollowersAsync(followeeId);

        return rows.Select(row => MapFollow(
            followerId: RepositoryMappingHelper.ReadString(row, "Id"),
            followeeId: followeeId,
            followedAt: RepositoryMappingHelper.ReadDateTime(row, "FollowedAt"))).ToList();
    }

    /// <summary>
    /// Lấy danh sách người dùng hoặc nghệ sĩ mà một người dùng đang theo dõi.
    /// </summary>
    public async Task<IReadOnlyCollection<Follow>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var followerId = RepositoryMappingHelper.ToDatabaseId(userId);
        var rows = await _interactionDao.GetFollowingArtistsAsync(followerId);

        return rows.Select(row => MapFollow(
            followerId: followerId,
            followeeId: RepositoryMappingHelper.ReadString(row, "Id"),
            followedAt: RepositoryMappingHelper.ReadDateTime(row, "FollowedAt"))).ToList();
    }

    /// <summary>
    /// Đếm số lượng người theo dõi của một người dùng hoặc nghệ sĩ.
    /// </summary>
    public async Task<int> CountFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _interactionDao.CountFollowersAsync(RepositoryMappingHelper.ToDatabaseId(userId));
    }

    /// <summary>
    /// Tạo entity Follow từ các giá trị đã được đọc từ dữ liệu nguồn.
    /// </summary>
    private static Follow MapFollow(string followerId, string followeeId, DateTime followedAt)
    {
        return RepositoryMappingHelper.CreateEntity<Follow>(
            (nameof(Follow.Id), string.Empty),
            (nameof(Follow.FollowerId), followerId),
            (nameof(Follow.FolloweeId), followeeId),
            (nameof(Follow.FollowedAt), followedAt),
            (nameof(Follow.IsActive), true));
    }
}
