using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho chức năng chia sẻ media giữa người dùng trong TuneVault.
/// Interface này quản lý việc gửi, nhận, đánh dấu đã đọc và thống kê số lượt chia sẻ chưa đọc.
/// </summary>
public interface IMediaShareRepository
{
    /// <summary>
    /// Lưu một bản ghi chia sẻ media mới từ người gửi đến người nhận.
    /// </summary>
    Task ShareAsync(MediaShare mediaShare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các nội dung mà người dùng đã chia sẻ cho người khác.
    /// </summary>
    Task<IReadOnlyCollection<MediaShare>> GetSharedByMeAsync(Guid senderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các nội dung mà người dùng đã nhận được từ người khác.
    /// </summary>
    Task<IReadOnlyCollection<MediaShare>> GetSharedWithMeAsync(Guid receiverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu một bản ghi chia sẻ cụ thể là đã đọc đối với người nhận.
    /// </summary>
    Task MarkAsReadAsync(Guid shareId, Guid receiverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu toàn bộ nội dung đã chia sẻ đến một người nhận là đã đọc.
    /// </summary>
    Task MarkAllAsReadAsync(Guid receiverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm số lượng nội dung chia sẻ chưa đọc của một người nhận.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid receiverId, CancellationToken cancellationToken = default);
}
