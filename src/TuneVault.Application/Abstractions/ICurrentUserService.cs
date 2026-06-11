using System;

namespace TuneVault.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Role { get; } // Đã chuyển thành string? để lưu chuỗi vai trò như "Admin"/"User"
    bool IsAuthenticated { get; }
}