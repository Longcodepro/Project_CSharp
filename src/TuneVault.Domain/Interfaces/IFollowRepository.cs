using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho quan hệ theo dõi giữa người dùng và đối tượng được theo dõi trong TuneVault.
/// Interface này chịu trách nhiệm thêm, xóa, kiểm tra và thống kê dữ liệu follow.
/// </summary>
public interface IFollowRepository
{
    /// <summary>
    /// Tạo quan hệ theo dõi từ một người dùng đến một người dùng hoặc nghệ sĩ khác.
    /// </summary>
    Task FollowAsync(string followerId, string followeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy quan hệ theo dõi giữa một người dùng và đối tượng đang được theo dõi.
    /// </summary>
    Task UnfollowAsync(string followerId, string followeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra một người dùng có đang theo dõi một đối tượng khác hay không.
    /// </summary>
    Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các bản ghi follow thể hiện những người đang theo dõi một người dùng hoặc nghệ sĩ.
    /// </summary>
    Task<IReadOnlyCollection<Follow>> GetFollowersAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các bản ghi follow thể hiện những đối tượng mà người dùng hiện đang theo dõi.
    /// </summary>
    Task<IReadOnlyCollection<Follow>> GetFollowingAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm tổng số người đang theo dõi một người dùng hoặc nghệ sĩ.
    /// </summary>
    Task<int> CountFollowersAsync(string userId, CancellationToken cancellationToken = default);
}
