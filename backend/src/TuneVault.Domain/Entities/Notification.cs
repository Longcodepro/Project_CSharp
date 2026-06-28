using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một thông báo (Notification) trong hệ thống TuneVault.
/// Người gửi (SenderId) là User phát sinh thông báo.
/// Người nhận (UserId) luôn là User.
/// Thực thể tự động đồng bộ Title theo NotificationType.
/// </summary>
public class Notification
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MaxMessageLength = 500;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của thông báo.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người dùng nhận thông báo.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người gửi thông báo (User hoặc thông báo hệ thống).
    /// Nullable — thông báo hệ thống tự động không cần người gửi cụ thể.
    /// </summary>
    public string? SenderId { get; private set; } = string.Empty;

    /// <summary>
    /// Phân loại thông báo (dùng Enum để kiểm soát nghiệp vụ hiển thị/điều hướng).
    /// </summary>
    public NotificationType Type { get; private set; }

    /// <summary>
    /// Tiêu đề của thông báo — được tự động đồng bộ theo <see cref="Type"/>.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Nội dung chi tiết của thông báo. Tối đa 500 ký tự.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Trạng thái đã đọc: <c>true</c> = đã đọc, <c>false</c> = chưa đọc.
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// Trạng thái hoạt động của thông báo (Soft Delete — false = đã xóa).
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Thời điểm thông báo được tạo (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // --- Constructors ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper khi map dữ liệu từ DB.
    /// </summary>
    private Notification() { }

    /// <summary>
    /// Khởi tạo một thông báo mới với đầy đủ thông tin người gửi và người nhận.
    /// Title sẽ được tự động đặt dựa trên <paramref name="type"/>.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất (4–5 ký tự).</param>
    /// <param name="userId">Mã định danh người nhận thông báo.</param>
    /// <param name="senderId">Mã định danh người gửi (User). Null nếu là thông báo hệ thống.</param>
    /// <param name="type">Loại thông báo — tự động xác định tiêu đề.</param>
    /// <param name="message">Nội dung thông báo (tối đa 500 ký tự).</param>
    /// <param name="isRead">Trạng thái đã đọc ban đầu (thường là false).</param>
    /// <exception cref="DomainException">Ném ra khi dữ liệu đầu vào vi phạm ràng buộc.</exception>
    public Notification(string id, string userId, string? senderId, NotificationType type, string message, bool isRead = false)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateType(type);
        ValidateMessage(message);

        DateTime now = DateTime.UtcNow;
        ValidateCreatedAt(now);

        Id = id.Trim();
        UserId = userId.Trim();
        SenderId = senderId?.Trim();
        Type = type;
        Message = message.Trim();
        IsRead = isRead;
        IsActive = true;
        CreatedAt = now;

        // Tự động đồng bộ tiêu đề theo loại thông báo
        Title = GetDefaultTitleForType(type);
    }

    // --- Business Methods ---

    /// <summary>
    /// Đánh dấu thông báo này là đã đọc.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu thông báo đã được đọc từ trước.</exception>
    public void MarkAsRead()
    {
        if (IsRead)
            throw new DomainException("Thông báo này đã được người dùng đọc từ trước đó.");

        IsRead = true;
    }

    /// <summary>
    /// Thực hiện Soft Delete — vô hiệu hóa thông báo.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu thông báo đã bị xóa trước đó.</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Thông báo này đã bị xóa trước đó.");

        IsActive = false;
    }

    // --- Helper Methods ---

    /// <summary>
    /// Ánh xạ <see cref="NotificationType"/> sang tiêu đề hiển thị chuẩn hóa.
    /// </summary>
    /// <param name="type">Loại thông báo.</param>
    /// <returns>Chuỗi tiêu đề tương ứng.</returns>
    private static string GetDefaultTitleForType(NotificationType type)
    {
        return type switch
        {
            NotificationType.FriendRequest  => "Lời mời kết bạn",
            NotificationType.FriendAccepted => "Lời mời kết bạn đã được chấp nhận",
            NotificationType.ShareSong      => "Bài hát được chia sẻ",
            NotificationType.ShareVideo     => "Video được chia sẻ",
            NotificationType.ShareAudio     => "Audio được chia sẻ",
            _                               => "Thông báo mới"
        };
    }

    // --- Validation Methods ---

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id thông báo không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id thông báo phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId người nhận thông báo không được để trống.");

        int length = userId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"UserId người nhận phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateType(NotificationType type)
    {
        if (!Enum.IsDefined(typeof(NotificationType), type))
            throw new DomainException("Loại thông báo (NotificationType) không hợp lệ.");
    }

    private static void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Nội dung thông báo không được để trống.");

        if (message.Trim().Length > MaxMessageLength)
            throw new DomainException($"Nội dung thông báo không được vượt quá {MaxMessageLength} ký tự.");
    }

    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian tạo thông báo không hợp lệ.");

        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian tạo thông báo không được vượt quá thời gian hiện tại.");
    }
}
