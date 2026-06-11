using System;
using System.Threading;
using System.Threading.Tasks;
using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa hợp đồng lưu trữ dữ liệu (Repository Pattern) cho thực thể Quản trị viên (Admin).
/// </summary>
public interface IAdminRepository
{
    /// <summary>
    /// Tìm kiếm và lấy thông tin Quản trị viên từ cơ sở dữ liệu dựa trên Tên tài khoản (Username).
    /// </summary>
    /// <param name="username">Tên tài khoản Admin cần tìm kiếm.</param>
    /// <param name="cancellationToken">Mã token hủy bỏ tiến trình bất đồng bộ nếu có yêu cầu dừng.</param>
    /// <returns>Thực thể <see cref="Admin"/> nếu tìm thấy; ngược lại trả về giá trị null.</returns>
    Task<Admin?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm và lấy thông tin Quản trị viên dựa trên địa chỉ Email đã đăng ký.
    /// </summary>
    /// <param name="email">Địa chỉ email cần truy vấn.</param>
    /// <param name="cancellationToken">Mã token hủy bỏ tiến trình bất đồng bộ.</param>
    /// <returns>Thực thể <see cref="Admin"/> nếu tìm thấy; ngược lại trả về giá trị null.</returns>
    Task<Admin?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Truy xuất thông tin Quản trị viên theo mã định danh khóa chính (Id).
    /// </summary>
    /// <param name="id">Mã Guid định danh của Admin.</param>
    /// <param name="cancellationToken">Mã token hủy bỏ tiến trình bất đồng bộ.</param>
    /// <returns>Thực thể <see cref="Admin"/> tương ứng hoặc null nếu không tồn tại.</returns>
    Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đăng ký thêm một thực thể Quản trị viên mới vào ngữ cảnh lưu trữ dữ liệu.
    /// </summary>
    /// <param name="admin">Thực thể dữ liệu Admin cần khởi tạo.</param>
    /// <param name="cancellationToken">Mã token hủy bỏ tiến trình bất đồng bộ.</param>
    Task AddAsync(Admin admin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu cập nhật thay đổi thông tin của một thực thể Quản trị viên hiện có.
    /// </summary>
    /// <param name="admin">Thực thể Admin chứa thông tin cần cập nhật.</param>
    void Update(Admin admin);

    /// <summary>
    /// Đánh dấu xóa bỏ hoàn toàn một tài khoản Quản trị viên ra khỏi hệ thống dữ liệu.
    /// </summary>
    /// <param name="admin">Thực thể Admin cần loại bỏ.</param>
    void Delete(Admin admin);
}