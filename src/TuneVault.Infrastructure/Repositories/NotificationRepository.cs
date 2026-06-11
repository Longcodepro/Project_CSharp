using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository triển khai các thao tác lưu trữ và truy vấn thông báo của người dùng trong TuneVault.
/// Lớp này kết hợp NotificationDAO và DapperContext để đọc, tạo và cập nhật trạng thái thông báo.
/// </summary>
public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDAO _notificationDao;
    private readonly DapperContext _context;

    /// <summary>
    /// Khởi tạo một instance mới của NotificationRepository với DAO và context kết nối cơ sở dữ liệu.
    /// </summary>
    public NotificationRepository(NotificationDAO notificationDao, DapperContext context)
    {
        _notificationDao = notificationDao;
        _context = context;
    }

    /// <summary>
    /// Thêm một thông báo mới cho người dùng vào hệ thống.
    /// </summary>
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _notificationDao.CreateNotificationAsync(
            notification.UserId,
            notification.Type.ToString(),
            notification.Message);
    }

    /// <summary>
    /// Lấy toàn bộ thông báo của một người dùng và ánh xạ sang entity Notification.
    /// </summary>
    public async Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await _notificationDao.GetNotificationsAsync(RepositoryMappingHelper.ToDatabaseId(userId));
        return rows.Select(MapNotification).ToList();
    }

    /// <summary>
    /// Lấy danh sách thông báo chưa đọc của người dùng với số lượng giới hạn.
    /// </summary>
    public async Task<IReadOnlyCollection<Notification>> GetUnreadByUserIdAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default)
    {
        var rows = await _notificationDao.GetUnreadNotificationsAsync(RepositoryMappingHelper.ToDatabaseId(userId), take);
        return rows.Select(MapNotification).ToList();
    }

    /// <summary>
    /// Đếm số lượng thông báo chưa đọc của một người dùng.
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _notificationDao.CountUnreadNotificationsAsync(RepositoryMappingHelper.ToDatabaseId(userId));
    }

    /// <summary>
    /// Đánh dấu một thông báo cụ thể là đã đọc bằng truy vấn cập nhật trực tiếp.
    /// </summary>
    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();

        const string sql = @"
            UPDATE [Notification]
            SET [IsRead] = 1
            WHERE [Id] = @NotificationId;
        ";

        await connection.ExecuteAsync(sql, new
        {
            NotificationId = RepositoryMappingHelper.ToDatabaseId(notificationId)
        });
    }

    /// <summary>
    /// Đánh dấu toàn bộ thông báo của một người dùng là đã đọc.
    /// </summary>
    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _notificationDao.MarkAllNotificationsAsReadAsync(RepositoryMappingHelper.ToDatabaseId(userId));
    }

    /// <summary>
    /// Ánh xạ một dòng dữ liệu thông báo từ DAO thành entity Notification của tầng Domain.
    /// Nếu payload rỗng, phương thức sẽ dùng tiêu đề mặc định theo loại thông báo làm nội dung.
    /// </summary>
    private static Notification MapNotification(object row)
    {
        var type = RepositoryMappingHelper.ReadEnum(row, "Type", NotificationType.SystemAlert);
        var message = RepositoryMappingHelper.ReadString(row, "PayloadJson");
        if (string.IsNullOrWhiteSpace(message))
            message = type.ToTitle();

        return RepositoryMappingHelper.CreateEntity<Notification>(
            (nameof(Notification.Id), RepositoryMappingHelper.ReadString(row, "Id")),
            (nameof(Notification.UserId), RepositoryMappingHelper.ReadString(row, "UserId")),
            (nameof(Notification.Type), type),
            (nameof(Notification.Title), type.ToTitle()),
            (nameof(Notification.Message), message),
            (nameof(Notification.IsRead), RepositoryMappingHelper.ReadBool(row, "IsRead")),
            (nameof(Notification.CreatedAt), RepositoryMappingHelper.ReadDateTime(row, "CreatedAt")));
    }
}
