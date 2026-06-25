namespace TuneVault.Domain.Enums;

/// <summary>
/// Định nghĩa các trạng thái của mối quan hệ bạn bè giữa hai người dùng.
/// </summary>
public enum FriendStatus
{
    /// <summary>
    /// Lời mời kết bạn đang chờ xử lý từ phía người nhận.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Hai người dùng đã chính thức trở thành bạn bè của nhau.
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// Một trong hai người dùng đã chặn người còn lại.
    /// </summary>
    Blocked = 3
}
