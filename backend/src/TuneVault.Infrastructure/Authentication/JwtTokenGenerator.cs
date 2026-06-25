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
        return GenerateTokenInternal(userId, email, role, "access", GetAccessTokenLifetime());
    }

    public string GenerateRefreshToken(string userId, string email, string role)
    {
        return GenerateTokenInternal(userId, email, role, "refresh", GetRefreshTokenLifetime());
    }

    private string GenerateTokenInternal(string userId, string email, string role, string tokenUse, TimeSpan lifetime)
    {
        string secretKey = ReadRequiredConfiguration("JwtSettings:SecretKey", "Không tìm thấy JWT SecretKey trong cấu hình.");
        string issuer = ReadRequiredConfiguration("JwtSettings:Issuer", "Không tìm thấy JWT Issuer trong cấu hình.");
        string audience = ReadRequiredConfiguration("JwtSettings:Audience", "Không tìm thấy JWT Audience trong cấu hình.");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("token_use", tokenUse)
        };

        foreach (var roleValue in role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleValue));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenOptions = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenOptions);
        return tokenHandler.WriteToken(token);
    }

    private TimeSpan GetAccessTokenLifetime()
    {
        if (int.TryParse(_configuration["JwtSettings:ExpireDays"], out int expireDays) && expireDays > 0)
        {
            return TimeSpan.FromDays(expireDays);
        }

        return TimeSpan.FromDays(7);
    }

    private static TimeSpan GetRefreshTokenLifetime()
    {
        return TimeSpan.FromDays(30);
    }

    private string ReadRequiredConfiguration(string key, string errorMessage)
    {
        var value = _configuration[key];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(errorMessage);
    }
}
