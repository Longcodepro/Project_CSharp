using System;
using System.Linq;
using System.Text.RegularExpressions;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho người dùng (User) trong hệ thống TuneVault.
/// Quản lý thông tin hồ sơ, trạng thái và các quy tắc nghiệp vụ.
/// Mọi định danh (Id, IdDisplay) đều sử dụng kiểu chuỗi (string).
/// </summary>
public class User
{
    // --- Constants ---
    private const int MaxDisplayNameLength = 24;
    private const int MaxBioLength = 500;
    private const int MinIdDisplayLength = 3;
    private const int MaxIdDisplayLength = 15;
    private const int MinPasswordHashLength = 60;

    // --- Properties ---

    /// <summary>
    /// Mã định danh nghiệp vụ (Primary Key) của người dùng (ví dụ: U001).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Mã định danh hiển thị công khai (handle) giữa các user.
    /// </summary>
    public string IdDisplay { get; private set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị cá nhân của người dùng.
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Địa chỉ email đăng nhập.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Mật khẩu đã băm.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Đường dẫn ảnh đại diện (có thể để trống).
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Tiểu sử người dùng.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Trạng thái nghệ sĩ.
    /// </summary>
    public bool IsArtist { get; private set; }

    /// <summary>
    /// Tổng số người theo dõi.
    /// </summary>
    public int TotalFollowers { get; private set; }

    /// <summary>
    /// Thời điểm tài khoản được tạo.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Trạng thái hoạt động của tài khoản.
    /// </summary>
    public bool IsActive { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM.
    /// </summary>
    private User() { }

    /// <summary>
    /// Khởi tạo người dùng mới với các định danh dạng chuỗi và thông tin bắt buộc.
    /// </summary>
        public User(string id, string idDisplay, string displayName, string email, string passwordHash)
        {
            ValidateUserId(id);
            ValidateIdDisplay(idDisplay);
            ValidateDisplayName(displayName);
            ValidateEmail(email);
            ValidatePasswordHash(passwordHash);

        Id = id.Trim();
        IdDisplay = idDisplay.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        IsArtist = false;
        TotalFollowers = 0;
    }

    // --- Business Methods ---

    /// <summary>
    /// Cập nhật thông tin profile của người dùng.
    /// </summary>
    public void UpdateProfile(string idDisplay, string displayName, string? bio, string? avatarUrl)
    {
        ValidateIdDisplay(idDisplay);
        ValidateDisplayName(displayName);
        ValidateBio(bio);

        IdDisplay = idDisplay.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        Bio = bio?.Trim();
        AvatarUrl = avatarUrl?.Trim();
    }

    /// <summary>
    /// Xác thực người dùng là nghệ sĩ.
    /// </summary>
    public void VerifyAsArtist()
    {
        if (IsArtist)
            throw new DomainException("Tài khoản này đã được xác thực là nghệ sĩ.");
        
        IsArtist = true;
    }

    /// <summary>
    /// Tăng lượt theo dõi.
    /// </summary>
    public void IncrementFollowers() => TotalFollowers++;

    /// <summary>
    /// Giảm lượt theo dõi (không dưới 0).
    /// </summary>
    public void DecrementFollowers()
    {
        if (TotalFollowers > 0) TotalFollowers--;
    }

    /// <summary>
    /// Thay đổi mật khẩu người dùng.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        ValidatePasswordHash(newPasswordHash);
        PasswordHash = newPasswordHash;
    }

    // --- Validation Methods ---

    private static void ValidateUserId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Mã User (Id) không được để trống.");
    }

    private static void ValidateIdDisplay(string idDisplay)
    {
        if (string.IsNullOrWhiteSpace(idDisplay))
            throw new DomainException("IdDisplay không được để trống.");

        string normalizedId = idDisplay.Trim();
        if (normalizedId.Length < MinIdDisplayLength || normalizedId.Length > MaxIdDisplayLength)
            throw new DomainException($"IdDisplay phải từ {MinIdDisplayLength} đến {MaxIdDisplayLength} ký tự.");

        if (normalizedId.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            throw new DomainException("IdDisplay chỉ được chứa chữ cái, số và dấu gạch dưới.");
        
        if (!char.IsLetter(normalizedId[0]))
             throw new DomainException("IdDisplay phải bắt đầu bằng một chữ cái.");
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("DisplayName không được để trống.");

        if (displayName.Trim().Length > MaxDisplayNameLength)
            throw new DomainException($"DisplayName không được vượt quá {MaxDisplayNameLength} ký tự.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException("Địa chỉ email không hợp lệ.");
    }

    private static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length < MinPasswordHashLength)
            throw new DomainException("PasswordHash không hợp lệ.");
    }

    private static void ValidateBio(string? bio)
    {
        if (!string.IsNullOrWhiteSpace(bio) && bio.Trim().Length > MaxBioLength)
            throw new DomainException($"Bio không được vượt quá {MaxBioLength} ký tự.");
    }
}
