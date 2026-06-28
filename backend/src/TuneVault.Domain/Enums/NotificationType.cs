namespace TuneVault.Domain.Enums;

/// <summary>
/// Định nghĩa các loại thông báo dùng trong TuneVault.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Thông báo khi có lời mời kết bạn mới.
    /// </summary>
    FriendRequest = 1,

    /// <summary>
    /// Thông báo khi lời mời kết bạn đã được chấp nhận.
    /// </summary>
    FriendAccepted = 2,

    /// <summary>
    /// Thông báo khi bài hát được chia sẻ.
    /// </summary>
    ShareSong = 3,

    /// <summary>
    /// Thông báo khi video được chia sẻ.
    /// </summary>
    ShareVideo = 4,

    /// <summary>
    /// Thông báo khi audio được chia sẻ.
    /// </summary>
    ShareAudio = 5
}

/// <summary>
/// Cung cấp các phương thức mở rộng quản lý logic nghiệp vụ và đồng bộ dữ liệu cho NotificationType.
/// </summary>
public static class NotificationTypeExtensions
{
    /// <summary>
    /// Tự động chuyển đổi loại thông báo (Enum) thành văn bản Tiêu đề hiển thị chuẩn hóa tương ứng.
    /// Đây là nơi duy nhất (Single Source of Truth) định nghĩa tiêu đề thông báo cho toàn hệ thống.
    /// </summary>
    /// <param name="type">Loại thông báo hiện tại.</param>
    /// <returns>Chuỗi văn bản tiêu đề tiếng Việt đã được chuẩn hóa.</returns>
    public static string ToTitle(this NotificationType type)
    {
        return type switch
        {
            NotificationType.FriendRequest => "Lời mời kết bạn",
            NotificationType.FriendAccepted => "Lời mời kết bạn đã được chấp nhận",
            NotificationType.ShareSong => "Bài hát được chia sẻ",
            NotificationType.ShareVideo => "Video được chia sẻ",
            NotificationType.ShareAudio => "Audio được chia sẻ",
            _ => "Thông báo mới"
        };
    }
}
