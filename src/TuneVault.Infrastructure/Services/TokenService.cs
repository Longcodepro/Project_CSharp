using System;
using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

/// <summary>
/// Triển khai dịch vụ cấp phát mã cấu hình bảo mật JSON Web Token (JWT).
/// </summary>
public class TokenService : ITokenService
{
    /// <summary>
    /// Tạo Token bảo mật với tham số vai trò đã được chuyển sang dạng chuỗi (string).
    /// </summary>
    public string CreateToken(Guid userId, string email, string role)
    {
        // 💡 Giữ nguyên toàn bộ logic sinh Jwt Security Token bên dưới của bạn.
        // Chỉ cần đảm bảo chỗ gán Claim Role, bạn truyền trực tiếp biến 'role' (kiểu string) vào là xong.
        
        return "chuỗi_token_của_bạn"; 
    }
}