namespace TuneVault.Application.Features.User.DTOs;

/// <summary>
/// Thông tin hồ sơ công khai của một người dùng dùng cho trang chi tiết profile.
/// DTO này chỉ trả về dữ liệu cần thiết cho màn hình public profile.
/// </summary>
public sealed record UserPublicDetailDto
{
    /// <summary>
    /// Mã định danh hệ thống của người dùng (ví dụ: U001).
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Handle công khai của người dùng.
    /// </summary>
    public string IdDisplay { get; init; } = string.Empty;

    /// <summary>
    /// Tên hiển thị của người dùng.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Đường dẫn public của ảnh đại diện.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Phần giới thiệu ngắn của người dùng.
    /// </summary>
    public string? Bio { get; init; }

    /// <summary>
    /// Email của tài khoản. Chỉ hiển thị trên trang profile công khai nếu người dùng cho phép.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Vai trò hiển thị ra client.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Tổng số người đang theo dõi tài khoản này.
    /// </summary>
    public int TotalFollowers { get; init; }

    /// <summary>
    /// Tổng số tài khoản mà người dùng đang theo dõi.
    /// </summary>
    public int FollowingCount { get; init; }

    /// <summary>
    /// Thời điểm tài khoản được tạo trong hệ thống (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Trạng thái hoạt động của tài khoản.
    /// </summary>
    public bool IsActive { get; init; }
}
