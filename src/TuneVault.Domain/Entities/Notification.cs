using System;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một thông báo (Notification) gửi tới người dùng trong hệ thống TuneVault.
/// Thực thể này tự động quản lý và đồng bộ tiêu đề (Title) dựa trên loại thông báo (NotificationType).
/// </summary>
public class Notification
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MaxMessageLength = 500;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của thông báo. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Mã định danh của người dùng sở hữu/nhận thông báo này. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Phân loại thông báo (Sử dụng Enum để kiểm soát nghiệp vụ hiển thị hoặc điều hướng).
    /// </summary>
    public NotificationType Type { get; private set; }

    /// <summary>
    /// Tiêu đề của thông báo. Thuộc tính này được hệ thống tự động thiết lập đồng bộ theo loại thông báo (Type).
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Nội dung chi tiết của thông báo. Giới hạn tối đa 500 ký tự.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Trạng thái đã đọc thông báo từ phía người dùng. True nghĩa là đã đọc, false là chưa đọc.
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// Mốc thời gian hệ thống khởi tạo và phát hành thông báo này.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // --- Constructors ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// Khi map từ DB lên, Dapper sẽ dùng hàm này và giữ nguyên cột Title đã lưu dưới cơ sở dữ liệu.
    /// </summary>
    private Notification() { }

    /// <summary>
    /// Khởi tạo một thông báo mới gửi tới người dùng. Tiêu đề (Title) sẽ được tự động đồng bộ dựa trên tham số phân loại (Type).
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của bản ghi thông báo.</param>
    /// <param name="userId">Mã định danh người dùng đích nhận thông báo.</param>
    /// <param name="type">Loại hình cấu trúc thông báo.</param>
    /// <param name="message">Nội dung văn bản chi tiết của thông báo.</param>
    /// <param name="isRead">Trạng thái thông báo.</param>
    /// <exception cref="DomainException">Ném ra khi thông tin đầu vào vi phạm các ràng buộc độ dài hoặc định dạng trống.</exception>
    public Notification(string id, string userId, NotificationType type, string message, bool isRead)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateType(type);
        ValidateMessage(message);

        DateTime now = DateTime.UtcNow;
        ValidateCreatedAt(now);

        Id = id.Trim();
        UserId = userId.Trim();
        Type = type;
        Message = message.Trim();
        IsRead = isRead;
        CreatedAt = now;

        // Tự động đồng bộ hóa Tiêu đề dựa trên Enum phân loại thông báo
        Title = GetDefaultTitleForType(type);
    }

    // --- Business Methods ---

    /// <summary>
    /// Đánh dấu thông báo này là đã đọc, cập nhật trạng thái IsRead về true.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu thông báo này vốn dĩ đã được đọc từ trước đó.</exception>
    public void MarkAsRead()
    {
        if (IsRead)
            throw new DomainException("Thông báo này đã được người dùng đọc từ trước đó.");

        IsRead = true;
    }

    // --- Helper Methods ---

    /// <summary>
    /// Bản đồ ánh xạ nội bộ để chuyển đổi một loại thông báo (Enum) sang tiêu đề hiển thị chuẩn hóa tương ứng.
    /// </summary>
    /// <param name="type">Loại thông báo cần lấy tiêu đề.</param>
    /// <returns>Chuỗi văn bản tiêu đề tương ứng đã được chuẩn hóa.</returns>
    private static string GetDefaultTitleForType(NotificationType type)
    {
        return type switch
        {
            NotificationType.NewFollower => "Người theo dõi mới",
            NotificationType.FriendRequest => "Lời mời kết bạn",
            NotificationType.MediaShared => "Nội dung được chia sẻ",
            NotificationType.SystemAlert => "Thông báo hệ thống",
            NotificationType.FriendAccepted => "Lời mời kết bạn đã được chấp nhận",
            _ => "Thông báo mới"
        };
    }

    // --- Validation Methods (Single Responsibility) ---

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của thông báo không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của thông báo phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId nhận thông báo không được để trống.");

        int length = userId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"UserId nhận thông báo phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateType(NotificationType type)
    {
        if (!Enum.IsDefined(typeof(NotificationType), type))
            throw new DomainException("Loại hình thông báo (NotificationType) không tồn tại trên hệ thống.");
    }

    private static void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Nội dung thông báo không được phép để trống.");

        if (message.Trim().Length > MaxMessageLength)
            throw new DomainException($"Nội dung thông báo không được phép vượt quá {MaxMessageLength} ký tự.");
    }

    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian tạo thông báo (CreatedAt) không được mang giá trị mặc định.");

        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian tạo thông báo không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}