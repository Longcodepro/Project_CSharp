namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Giao diện truy cập thông tin người dùng hiện tại (từ JWT claims trong HttpContext).
/// Được sử dụng trong các handler để lấy UserId, Role của người gọi API để kiểm tra quyền.
/// Tầng Infrastructure sẽ cung cấp triển khai cụ thể từ HttpContext.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Lấy mã định danh (Id) của người dùng hiện tại từ JWT claim (sub hoặc NameIdentifier).
    /// </summary>
    /// <returns>
    /// Mã Id của người dùng nếu đã xác thực; <c>null</c> hoặc chuỗi rỗng nếu chưa xác thực.
    /// </returns>
    string? GetCurrentUserId();

    /// <summary>
    /// Lấy danh sách các vai trò (Roles) của người dùng hiện tại từ JWT claim.
    /// </summary>
    /// <returns>
    /// Danh sách tên vai trò (ví dụ: "Admin", "Artist", "Listener").
    /// Trả về empty list nếu không có role.
    /// </returns>
    IEnumerable<string> GetCurrentUserRoles();

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại có một role cụ thể không.
    /// </summary>
    /// <param name="role">Tên vai trò cần kiểm tra (ví dụ: "Admin", "Artist").</param>
    /// <returns>True nếu người dùng có vai trò; False nếu không.</returns>
    bool HasRole(string role);

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại đã xác thực hay chưa.
    /// </summary>
    /// <returns>True nếu userId tồn tại và không null/empty; False nếu chưa xác thực.</returns>
    bool IsAuthenticated();
}