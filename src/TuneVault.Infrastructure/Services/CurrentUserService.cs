using System;
using System.Security.Claims;
using System.Linq; // Added for Select, Any, Distinct
using Microsoft.AspNetCore.Http;
// Removed: using Microsoft.IdentityModel.Tokens; // Not needed for explicit claim access
using TuneVault.Application.Abstractions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Services;

/// <summary>
/// Triển khai lấy thông tin định danh của người dùng đang thực hiện Request từ HttpContext.
/// Implement cả ICurrentUserService (Application level) và ICurrentUserContext (Domain level).
/// </summary>
public class CurrentUserService : ICurrentUserService, ICurrentUserContext
{
    private const string JwtRoleClaimType = "role";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Khởi tạo service với HttpContextAccessor để truy cập JWT claims từ HttpContext.
    /// </summary>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ====== ICurrentUserService (Application) ======

    /// <summary>
    /// Lấy UserId dưới dạng string từ claim "sub" (JWT subject).
    /// </summary>
    public string? UserId => 
        _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value 
        ?? _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    /// <summary>
    /// Lấy tất cả các vai trò dưới dạng chuỗi phân cách bằng dấu phẩy.
    /// </summary>
    public string? Role
    {
        get
        {
            var roles = GetRoleClaims();
            return roles?.Any() == true ? string.Join(", ", roles.Select(c => c.Value)) : null;
        }
    }

    /// <summary>
    /// Kiểm tra xem người dùng đã xác thực hay chưa (từ ICurrentUserService).
    /// </summary>
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    // ====== ICurrentUserContext (Domain) ======

    /// <summary>
    /// Lấy UserId dưới dạng string (format: U001, A001, v.v.).
    /// Ưu tiên claim "sub", fallback sang NameIdentifier.
    /// </summary>
    public string? GetCurrentUserId()
    {
        if (_httpContextAccessor.HttpContext?.User == null)
            return null;

        // Use explicit claim access instead of FindFirstValue extension method
        return _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value 
            ?? _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Lấy tất cả các vai trò của người dùng từ JWT claims (có thể có nhiều).
    /// </summary>
    public IEnumerable<string> GetCurrentUserRoles()
    {
        if (_httpContextAccessor.HttpContext?.User == null)
            return Enumerable.Empty<string>();

        return GetRoleClaims()
            .Select(c => c.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kiểm tra xem người dùng có một vai trò cụ thể không (case-insensitive).
    /// </summary>
    public bool HasRole(string role)
    {
        return GetCurrentUserRoles()
            .Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Kiểm tra xem người dùng đã xác thực hay chưa (dựa trên GetCurrentUserId).
    /// Implement từ ICurrentUserContext interface.
    /// </summary>
    bool ICurrentUserContext.IsAuthenticated() => !string.IsNullOrWhiteSpace(GetCurrentUserId());

    /// <summary>
    /// Lấy role từ cả claim chuẩn JWT "role" và claim URI của .NET để không lệch giữa token và middleware.
    /// </summary>
    private IEnumerable<Claim> GetRoleClaims()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
            return Enumerable.Empty<Claim>();

        return principal.Claims.Where(c => c.Type == JwtRoleClaimType || c.Type == ClaimTypes.Role);
    }
}
