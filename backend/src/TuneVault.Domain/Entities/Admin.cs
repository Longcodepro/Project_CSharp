using System;
using System.Text.RegularExpressions;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho đối tượng quản trị viên trong hệ thống TuneVault.
/// Mọi định danh (Id) đều sử dụng kiểu chuỗi (string) để đồng nhất với mã nghiệp vụ.
/// </summary>
public class Admin
{
    private const int MinPasswordHashLength = 60; // Độ dài tiêu chuẩn của Bcrypt hash

    /// <summary>
    /// Mã định danh nghiệp vụ của quản trị viên (ví dụ: AD001).
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Tên quản trị viên.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Địa chỉ email đăng nhập.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Mật khẩu đã băm (Password Hash).
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên lạc.
    /// </summary>
    public string PhoneNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Vai trò của quản trị viên.
    /// </summary>
    public string Role { get; private set; } = "Admin";

    /// <summary>
    /// Trạng thái kích hoạt của tài khoản.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM khi map dữ liệu từ DB.
    /// </summary>
    private Admin() { }

    /// <summary>
    /// Khởi tạo một đối tượng Admin mới với các thông tin bắt buộc.
    /// </summary>
    /// <param name="id">Mã định danh nghiệp vụ (ví dụ: AD001).</param>
    /// <param name="name">Tên quản trị viên.</param>
    /// <param name="email">Email đăng nhập.</param>
    /// <param name="passwordHash">Mật khẩu đã được băm.</param>
    /// <param name="phoneNumber">Số điện thoại.</param>
    /// <param name="role">Vai trò của admin.</param>
    public Admin(string id, string name, string email, string passwordHash, string phoneNumber, string role)
    {
        ValidateAdminId(id);
        ValidateName(name);
        ValidateEmail(email);
        ValidatePhoneNumber(phoneNumber);
        
        Id = id.Trim();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber.Trim();
        Role = role;
    }

    // --- Phương thức nghiệp vụ ---

    /// <summary>
    /// Thay đổi vai trò (Role) của Admin.
    /// </summary>
    /// <param name="newRole">Tên vai trò mới.</param>
    public void ChangeRole(string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
            throw new DomainException("Role không được để trống.");
        
        Role = newRole;
    }

    /// <summary>
    /// Vô hiệu hóa hoặc kích hoạt tài khoản Admin.
    /// </summary>
    /// <param name="isActive">Trạng thái mới.</param>
    public void SetStatus(bool isActive) => IsActive = isActive;

    /// <summary>
    /// Cập nhật mật khẩu mới (cần được băm từ Service trước khi truyền vào).
    /// </summary>
    /// <param name="newPasswordHash">Chuỗi băm mật khẩu mới.</param>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash) || newPasswordHash.Length < MinPasswordHashLength)
            throw new DomainException("PasswordHash không hợp lệ.");
        
        PasswordHash = newPasswordHash;
    }

    // --- Các phương thức kiểm tra hợp lệ (Validation) ---

    /// <summary>
    /// Kiểm tra tính hợp lệ của mã định danh Admin (Id).
    /// </summary>
    private static void ValidateAdminId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Mã Id Admin không được để trống.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của tên quản trị viên.
    /// </summary>
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên Admin không được để trống.");
    }

    /// <summary>
    /// Kiểm tra định dạng email của quản trị viên.
    /// </summary>
    private static void ValidateEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email.Trim(), emailPattern))
            throw new DomainException("Email không đúng định dạng.");
    }

    /// <summary>
    /// Kiểm tra định dạng số điện thoại (chấp nhận 10 đến 11 chữ số).
    /// </summary>
    private static void ValidatePhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone.Trim(), @"^\d{10,11}$"))
            throw new DomainException("Số điện thoại không hợp lệ (phải từ 10-11 chữ số).");
    }
}