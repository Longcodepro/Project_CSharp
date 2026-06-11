namespace TuneVault.Application.Interfaces;

public interface IJwtTokenGenerator
{
    // Đổi từ string role thành IEnumerable<string> roles để chứa được nhiều chức vụ/quyền hạn
    string GenerateToken(string userId, string username, IEnumerable<string> roles);
}