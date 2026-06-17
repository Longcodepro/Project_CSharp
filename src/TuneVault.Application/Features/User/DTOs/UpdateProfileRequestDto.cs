namespace TuneVault.Application.Features.User.DTOs;

/// <summary>
/// Dữ liệu request dùng để cập nhật hồ sơ cá nhân của người dùng hiện tại.
/// Id người dùng không nằm trong body vì API luôn lấy từ JWT để tránh sửa nhầm hồ sơ người khác.
/// </summary>
/// <param name="DisplayName">Tên hiển thị mới của người dùng.</param>
/// <param name="Bio">Tiểu sử cá nhân mới, hoặc <c>null</c> nếu muốn xóa.</param>
/// <param name="AvatarUrl">Đường dẫn ảnh đại diện mới, hoặc <c>null</c> nếu muốn xóa.</param>
public sealed record UpdateProfileRequestDto(
    string DisplayName,
    string? Bio,
    string? AvatarUrl);
