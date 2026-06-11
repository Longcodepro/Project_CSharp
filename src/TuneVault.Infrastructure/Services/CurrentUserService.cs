using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

/// <summary>
/// Triển khai lấy thông tin định danh của người dùng đang thực hiện Request từ HttpContext.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Đã sửa: Thay FindFirstValue bằng FindFirst(..)?Value chuẩn .NET gốc
    /// </summary>
    public Guid? UserId => 
        Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    /// <summary>
    /// Đã sửa: Thay FindFirstValue bằng FindFirst(..)?Value để lấy chuỗi Role từ Claim
    /// </summary>
    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}