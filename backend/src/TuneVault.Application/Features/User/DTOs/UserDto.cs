namespace TuneVault.Application.Features.User.DTOs;

/// <summary>
/// Đối tượng chuyển đổi dữ liệu (DTO) đại diện cho thông tin cơ bản công khai của người dùng,
/// được trả về cho Client trong các trường hợp liệt kê danh sách (followers, following, artists, v.v.).
/// Không chứa thông tin nhạy cảm: không có Id hệ thống, không có Email, không có PasswordHash.
/// </summary>
public record UserDto
{
    /// <summary>
    /// Mã định danh hệ thống của người dùng (ví dụ: U001).
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Handle hiển thị công khai của người dùng giữa các thành viên trong hệ thống (ví dụ: john_doe).
    /// Được dùng thay thế cho Id hệ thống khi hiển thị ra ngoài.
    /// </summary>
    public string IdDisplay { get; init; } = string.Empty;

    /// <summary>
    /// Tên hiển thị cá nhân của người dùng trên giao diện.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Đường dẫn ảnh đại diện. Có thể là <c>null</c> nếu người dùng chưa cập nhật.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Vai trò của người dùng trong hệ thống.
    /// Giá trị: <c>"Artist"</c> hoặc <c>"User"</c>.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Trạng thái hoạt động của tài khoản.
    /// <c>true</c> nếu tài khoản đang được kích hoạt và có thể sử dụng.
    /// </summary>
    public bool IsActive { get; init; }
}
