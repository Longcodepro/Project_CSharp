using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Commands.UpdateProfile;

/// <summary>
/// Command (yêu cầu ghi) đại diện cho nghiệp vụ cập nhật thông tin cá nhân của người dùng.
/// Tầng API tạo và gửi Command này qua MediatR đến <see cref="UpdateProfileCommandHandler"/>.
/// Kết quả trả về là <see cref="UserProfileDto"/> phản ánh trạng thái sau khi cập nhật.
/// </summary>
/// <param name="Id">Mã định danh hệ thống của người dùng cần cập nhật (ví dụ: U001).</param>
/// <param name="IdDisplay">Handle công khai mới. Tối đa 15 ký tự, chỉ gồm chữ, số và dấu gạch dưới.</param>
/// <param name="DisplayName">Tên hiển thị mới. Tối đa 24 ký tự, không được để trống.</param>
/// <param name="Bio">Tiểu sử mới. Tối đa 300 ký tự. Truyền <c>null</c> để xóa tiểu sử.</param>
/// <param name="AvatarUrl">Đường dẫn public của ảnh đại diện mới hoặc <c>null</c> nếu muốn xóa ảnh.</param>
/// <param name="ShouldUpdateAvatar">Cho biết lần cập nhật này có thực sự thay đổi avatar hay không.</param>
public record UpdateProfileCommand(
    string Id,
    string IdDisplay,
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    bool ShouldUpdateAvatar) : IRequest<UserProfileDto>;
