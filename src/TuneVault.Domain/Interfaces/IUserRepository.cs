// Đường dẫn: src/TuneVault.Domain/Interfaces/IUserRepository.cs
using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Giao diện định nghĩa các phương thức giao tiếp với cơ sở dữ liệu của đối tượng Người dùng (User).
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Lấy thông tin người dùng dựa trên tên đăng nhập hoặc mã hiển thị.
    /// </summary>
    /// <param name="username">Tên đăng nhập hoặc Id hiển thị (IdDisplay) của người dùng.</param>
    /// <returns>Đối tượng <see cref="User"/> nếu tìm thấy; ngược lại trả về <c>null</c>.</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Lấy thông tin chi tiết của người dùng dựa trên mã định danh hệ thống (Id).
    /// </summary>
    /// <param name="id">Mã ID hệ thống độc nhất của người dùng.</param>
    /// <param name="cancellationToken">Mã token thông báo hủy tiến trình bất đồng bộ nếu cần.</param>
    /// <returns>Đối tượng <see cref="User"/> nếu tìm thấy; ngược lại trả về <c>null</c>.</returns>
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin người dùng dựa trên IdDisplay (handle công khai giữa các người dùng).
    /// </summary>
    /// <param name="idDisplay">Chuỗi định danh hiển thị công khai của người dùng (ví dụ: john_doe).</param>
    /// <param name="cancellationToken">Mã token thông báo hủy tiến trình bất đồng bộ nếu cần.</param>
    /// <returns>Đối tượng <see cref="User"/> nếu tìm thấy; ngược lại trả về <c>null</c>.</returns>
    Task<User?> GetByIdDisplayAsync(string idDisplay, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tất cả người dùng đã được xác thực là nghệ sĩ (IsArtist = true).
    /// </summary>
    /// <param name="cancellationToken">Mã token thông báo hủy tiến trình bất đồng bộ nếu cần.</param>
    /// <returns>Danh sách các đối tượng <see cref="User"/> có trạng thái nghệ sĩ.</returns>
    Task<IEnumerable<User>> GetAllArtistsAsync(CancellationToken cancellationToken = default);
}