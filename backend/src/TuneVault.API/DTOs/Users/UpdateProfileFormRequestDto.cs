using Microsoft.AspNetCore.Http;

namespace TuneVault.API.DTOs.Users;

/// <summary>
/// Dữ liệu multipart/form-data dùng để cập nhật hồ sơ của chính người dùng hiện tại.
/// Avatar được gửi bằng file để backend lưu vào thư mục public trong wwwroot.
/// </summary>
public sealed class UpdateProfileFormRequestDto
{
    /// <summary>
    /// Handle hiển thị công khai mới của người dùng.
    /// </summary>
    public string IdDisplay { get; init; } = string.Empty;

    /// <summary>
    /// Tên hiển thị mới của người dùng.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Tiểu sử cá nhân mới. Có thể để trống nếu không muốn cập nhật nội dung bio.
    /// </summary>
    public string? Bio { get; init; }

    /// <summary>
    /// File ảnh đại diện mới.
    /// </summary>
    public IFormFile? AvatarFile { get; init; }

    /// <summary>
    /// Cho phép xóa avatar hiện tại nếu không tải ảnh mới lên.
    /// </summary>
    public bool RemoveAvatar { get; init; }
}
