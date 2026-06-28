using System;

namespace TuneVault.Application.Abstractions;

/// <summary>
/// Cung cấp thông tin người dùng hiện tại từ phiên đăng nhập.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Mã người dùng trong JWT.</summary>
    string? UserId { get; }

    /// <summary>Tên đăng nhập hoặc tên hiển thị.</summary>
    string? UserName { get; }

    /// <summary>Vai trò của người dùng hiện tại.</summary>
    string? Role { get; }

    /// <summary>Cho biết request đã xác thực hay chưa.</summary>
    bool IsAuthenticated { get; }
}
