using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho thông báo của người dùng trong TuneVault.
/// Interface này hỗ trợ lấy danh sách thông báo, thêm thông báo mới, đánh dấu đã đọc và thống kê thông báo chưa đọc.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Lấy toàn bộ thông báo của một người dùng.
    /// </summary>
    Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách thông báo chưa đọc của một người dùng với số lượng giới hạn.
    /// </summary>
    Task<IReadOnlyCollection<Notification>> GetUnreadByUserIdAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm số lượng thông báo chưa đọc của một người dùng.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm một thông báo mới vào hệ thống cho người dùng.
    /// </summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu một thông báo cụ thể là đã đọc.
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu toàn bộ thông báo của một người dùng là đã đọc.
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
