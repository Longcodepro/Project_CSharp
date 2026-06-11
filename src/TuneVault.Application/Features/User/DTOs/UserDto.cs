// Đường dẫn: src/TuneVault.Application/Features/User/DTOs/UserDto.cs
namespace TuneVault.Application.Features.User.DTOs;

/// <summary>
/// Đối tượng chuyển đổi dữ liệu (DTO) đại diện cho thông tin cơ bản của người dùng
/// được trả về Client. Chỉ chứa các thông tin công khai, không tiết lộ Id nội bộ hệ thống.
/// </summary>
public record UserDto
{
    /// <summary>
    /// Lấy hoặc đặt handle hiển thị công khai của người dùng (ví dụ: john_doe).
    /// </summary>
    public string IdDisplay { get; init; } = string.Empty;

    /// <summary>
    /// Lấy hoặc đặt tên hiển thị của người dùng trên hệ thống.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Lấy hoặc đặt vai trò của người dùng ("Artist" hoặc "User").
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Lấy hoặc đặt trạng thái hoạt động của tài khoản người dùng (true nếu đang kích hoạt).
    /// </summary>
    public bool IsActive { get; init; }
}