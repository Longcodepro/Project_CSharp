// Đường dẫn: src/TuneVault.Application/Features/User/DTOs/UserProfileDto.cs
namespace TuneVault.Application.Features.User.DTOs;

/// <summary>
/// Đối tượng chuyển đổi dữ liệu (DTO) đại diện cho thông tin profile đầy đủ của người dùng
/// được trả về Client. Chỉ chứa các thông tin công khai, không tiết lộ Id nội bộ hay Email hệ thống.
/// </summary>
public record UserProfileDto
{
    /// <summary>
    /// Lấy hoặc đặt handle hiển thị công khai giữa các người dùng (ví dụ: john_doe).
    /// </summary>
    public string IdDisplay { get; init; } = string.Empty;

    /// <summary>
    /// Lấy hoặc đặt tên hiển thị cá nhân của người dùng.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Lấy hoặc đặt đường dẫn ảnh đại diện của người dùng (có thể null nếu chưa cập nhật).
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Lấy hoặc đặt tiểu sử cá nhân của người dùng (có thể null nếu chưa cập nhật).
    /// </summary>
    public string? Bio { get; init; }

    /// <summary>
    /// Lấy hoặc đặt vai trò của người dùng trong hệ thống ("Artist" hoặc "User").
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Lấy hoặc đặt tổng số người theo dõi tài khoản này.
    /// </summary>
    public int TotalFollowers { get; init; }

    /// <summary>
    /// Lấy hoặc đặt thời điểm tài khoản được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Lấy hoặc đặt trạng thái hoạt động của tài khoản (true nếu đang kích hoạt).
    /// </summary>
    public bool IsActive { get; init; }
}