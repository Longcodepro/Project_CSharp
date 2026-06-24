namespace TuneVault.Application.Features.Notification.Queries;

public interface INotificationQueryRepository
{
    Task<IEnumerable<dynamic>> GetNotificationsAsync(string userId, int limit = 50);
    Task<IEnumerable<dynamic>> GetUnreadNotificationsAsync(string userId, int limit = 50);
    Task<int> CountUnreadNotificationsAsync(string userId);
    Task<bool> MarkAsReadAsync(string notificationId, string userId);
    /// <summary>
    /// Đánh dấu toàn bộ notification còn hoạt động của user là đã đọc.
    /// </summary>
    /// <param name="userId">Mã người dùng sở hữu notification.</param>
    /// <returns>Số notification đã được cập nhật.</returns>
    Task<int> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteAsync(string notificationId, string userId);
}
