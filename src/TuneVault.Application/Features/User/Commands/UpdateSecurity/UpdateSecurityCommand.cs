using MediatR;

namespace TuneVault.Application.Features.User.Commands.UpdateSecurity;

/// <summary>
/// Command (yêu cầu ghi) đại diện cho nghiệp vụ cập nhật thông tin bảo mật của người dùng,
/// cụ thể là thay đổi mật khẩu (PasswordHash đã được băm từ tầng API/Application trước khi đưa vào Command).
/// Kết quả trả về là <c>bool</c> xác nhận thao tác thành công hay thất bại.
/// </summary>
/// <param name="Id">Mã định danh hệ thống của người dùng cần cập nhật bảo mật (ví dụ: U001).</param>
/// <param name="NewPasswordHash">
/// Chuỗi hash mật khẩu mới đã được mã hóa (ví dụ: BCrypt hash).
/// Tối thiểu 60 ký tự theo quy tắc của <see cref="TuneVault.Domain.Entities.User"/> Entity.
/// </param>
public record UpdateSecurityCommand(
    string Id,
    string NewPasswordHash) : IRequest<bool>;
