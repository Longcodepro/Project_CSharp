using System;

namespace TuneVault.Application.Abstractions;

/// <summary>
/// Định nghĩa các phương thức trừu tượng để xử lý nghiệp vụ liên quan đến cấp phát và xác thực Token bảo mật.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Tạo JSON Web Token (JWT) cho người dùng dựa trên thông tin định danh và vai trò được phân định.
    /// </summary>
    /// <param name="userId">Định danh duy nhất của người dùng (Guid).</param>
    /// <param name="email">Địa chỉ email đăng ký của tài khoản.</param>
    /// <param name="role">Chuỗi đại diện cho vai trò/quyền hạn của tài khoản (ví dụ: "Admin", "Artist", "User").</param>
    /// <returns>Chuỗi mã hóa JWT hoàn chỉnh dùng để đính kèm vào HTTP Header.</returns>
    string CreateToken(Guid userId, string email, string role);
}