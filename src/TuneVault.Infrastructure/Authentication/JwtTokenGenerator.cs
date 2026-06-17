using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TuneVault.Application.Interfaces;

namespace TuneVault.Infrastructure.Authentication;

/// <summary>
/// Lớp thực thi dịch vụ cấp phát mã thông báo JWT (JSON Web Token).
/// Chịu trách nhiệm đọc các tham số bảo mật từ file cấu hình hệ thống (appsettings.json),
/// mã hóa thông tin tài khoản thành chuỗi mã bảo mật phục vụ quá trình xác thực và phân quyền.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Hàm khởi tạo (Constructor) để nạp dịch vụ quản lý cấu hình hệ thống.
    /// </summary>
    /// <param name="configuration">Giao diện cung cấp quyền truy cập vào các cặp khóa-giá trị trong file appsettings.json.</param>
    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Hàm thực thi việc sinh chuỗi mã thông báo bảo mật JWT từ các thông tin tài khoản đã được xác thực.
    /// </summary>
    /// <param name="userId">Mã định danh duy nhất (Id) của tài khoản dưới cơ sở dữ liệu.</param>
    /// <param name="username">Tên tài khoản hoặc Email định danh dùng để hiển thị trên hệ thống.</param>
    /// <param name="roles">Danh sách các vai trò/quyền hạn được cấp phép của tài khoản này.</param>
    /// <returns>Một chuỗi ký tự (string) đại diện cho mã thông báo JWT hợp lệ.</returns>
    public string GenerateToken(string userId, string email, string role)
    {
        // 1. Đọc các thông số cấu hình an toàn từ phân đoạn "JwtSettings" trong file appsettings.json
        string secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("Không tìm thấy JWT SecretKey trong cấu hình.");
        string issuer = _configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("Không tìm thấy JWT Issuer trong cấu hình.");
        string audience = _configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("Không tìm thấy JWT Audience trong cấu hình.");
        
        if (!int.TryParse(_configuration["JwtSettings:ExpireDays"], out int expireDays))
        {
            expireDays = 7; // Thời gian hết hạn mặc định nếu file cấu hình thiếu thông tin hoặc sai định dạng
        }

        // 2. Khởi tạo danh sách các mảnh thông tin xác thực (Claims) đưa vào trong Payload của Token
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),         // Lưu trữ Id người dùng
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, email), // Lưu trữ tên tài khoản/email đăng nhập
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Mã định danh duy nhất cho chính cái Token này
        };

        // 3. Duyệt qua danh sách quyền hạn truyền vào và đóng gói chúng thành các Claim thuộc loại Role
        foreach (var roleValue in role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleValue));
        }

        // 4. Tạo chìa khóa bảo mật mã hóa từ chuỗi SecretKey đã lấy ra từ cấu hình
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        
        // 5. Định nghĩa thuật toán ký số bảo mật HmacSha256
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 6. Thiết lập các thông số cấu hình thời hạn và thông tin phát hành Token
        var tokenOptions = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(expireDays),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        // 7. Tiến hành biên dịch và đúc kết cấu trúc trên thành chuỗi mã thông báo Token hoàn chỉnh
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenOptions);

        return tokenHandler.WriteToken(token);
    }
}
