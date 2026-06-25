namespace TuneVault.Domain.Enums;

/// <summary>
/// Định nghĩa các loại hình thông báo hệ thống phát sinh trong ứng dụng TuneVault.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Thông báo khi có người dùng khác bấm theo dõi (Follow).
    /// </summary>
    NewFollower = 1,

    /// <summary>
    /// Thông báo khi nhận được lời mời kết bạn mới (Chờ duyệt).
    /// </summary>
    FriendRequest = 2,

    /// <summary>
    /// Thông báo khi một người bạn chia sẻ nội dung âm nhạc (MediaShare) nội bộ.
    /// </summary>
    MediaShared = 3,

    /// <summary>
    /// Thông báo quảng bá hoặc nhắc nhở từ hệ thống quản trị TuneVault.
    /// </summary>
    SystemAlert = 4,

    /// <summary>
    /// Thông báo khi đối phương chính thức chấp nhận lời mời kết bạn của bạn.
    /// </summary>
    FriendAccepted = 5,

    /// <summary>
    /// Thông báo khi nghệ sĩ đang follow đăng bài mới.
    /// </summary>
    ArtistNewMedia = 6
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
            NotificationType.NewFollower => "Người theo dõi mới",
            NotificationType.FriendRequest => "Lời mời kết bạn",
            NotificationType.MediaShared => "Nội dung được chia sẻ",
            NotificationType.SystemAlert => "Thông báo hệ thống",
            NotificationType.FriendAccepted => "Lời mời kết bạn đã được chấp nhận",
            NotificationType.ArtistNewMedia => "Nghệ sĩ đăng bài mới",
            _ => "Thông báo mới"
        };
    }
}
