using System;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.ValueObjects;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một Hạng tài khoản (Account Tier) trong hệ thống TuneVault (Ví dụ: Free, Premium, Student).
/// Quản lý thông tin cấu hình, biểu phí và thời gian áp dụng của hạng tài khoản đó.
/// Mọi định danh (Id) đều sử dụng kiểu chuỗi (string).
/// </summary>
public class AccountTier
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MaxCodeLength = 15;
    private const int MaxNameLength = 50;
    private const int MinDurationDays = 1;

    // --- Properties ---

    /// <summary>
    /// Mã định danh nghiệp vụ (Primary Key) của hạng tài khoản (Ví dụ: T0001, T002).
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Mã code hệ thống/nội bộ của hạng tài khoản (Ví dụ: PREMIUM_1M, VIP_YEAR).
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị thương mại của hạng tài khoản (Ví dụ: Gói Hội Viên Premium).
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Cấu hình giá tiền và đơn vị tiền tệ của hạng tài khoản.
    /// </summary>
    public TierPrice Price { get; private set; } = null!;

    /// <summary>
    /// Các đặc quyền, năng lực giới hạn đi kèm với hạng tài khoản này.
    /// </summary>
    public TierCapabilities Capabilities { get; private set; } = null!;

    /// <summary>
    /// Chu kỳ thời gian sử dụng tính bằng ngày (Ví dụ: 30 ngày, 365 ngày).
    /// </summary>
    public int DurationInDays { get; private set; }

    /// <summary>
    /// Thời điểm hạng tài khoản này được khởi tạo trên hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Mốc thời gian chính thức cho phép người dùng bắt đầu đăng ký hạng này.
    /// </summary>
    public DateTime ActiveFrom { get; private set; }

    /// <summary>
    /// Mốc thời gian đóng/ngừng cho phép đăng ký hạng này (có thể để trống).
    /// </summary>
    public DateTime? ActiveTo { get; private set; }

    /// <summary>
    /// Trạng thái hoạt động của hạng tài khoản.
    /// </summary>
    public bool IsActive { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM.
    /// </summary>
    private AccountTier() { }

    /// <summary>
    /// Khởi tạo một hạng tài khoản mới với các thông tin bắt buộc.
    /// </summary>
    public AccountTier(string id, string code, string name, TierPrice price, TierCapabilities capabilities, int durationInDays, DateTime activeFrom, DateTime? activeTo)
    {
        ValidateId(id);
        ValidateCode(code);
        ValidateName(name);
        ValidatePrice(price);
        ValidateCapabilities(capabilities);
        ValidateDurationInDays(durationInDays);
        ValidateScheduleDates(activeFrom, activeTo);

        Id = id.Trim();
        Code = code.Trim();
        Name = name.Trim();
        Price = price;
        Capabilities = capabilities;
        DurationInDays = durationInDays;
        
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        ActiveFrom = activeFrom;
        ActiveTo = activeTo;
    }

    // --- Business Methods ---

    /// <summary>
    /// Cập nhật định danh hiển thị và tên của hạng tài khoản.
    /// </summary>
    public void UpdateIdentity(string newCode, string newName)
    {
        ValidateCode(newCode);
        ValidateName(newName);

        Code = newCode.Trim();
        Name = newName.Trim();
    }

    /// <summary>
    /// Cập nhật biểu phí mới cho hạng tài khoản.
    /// </summary>
    public void UpdatePrice(TierPrice newPrice)
    {
        ValidatePrice(newPrice);
        Price = newPrice;
    }

    /// <summary>
    /// Cập nhật lại các giới hạn đặc quyền của hạng tài khoản.
    /// </summary>
    public void UpdateCapabilities(TierCapabilities newCapabilities)
    {
        ValidateCapabilities(newCapabilities);
        Capabilities = newCapabilities;
    }

    /// <summary>
    /// Thay đổi thời gian hiệu lực (số ngày) của hạng tài khoản.
    /// </summary>
    public void UpdateDuration(int newDurationInDays)
    {
        ValidateDurationInDays(newDurationInDays);
        DurationInDays = newDurationInDays;
    }

    /// <summary>
    /// Cập nhật lịch trình mở bán của hạng tài khoản.
    /// </summary>
    public void UpdateSchedule(DateTime newActiveFrom, DateTime? newActiveTo)
    {
        ValidateScheduleDates(newActiveFrom, newActiveTo);
        ActiveFrom = newActiveFrom;
        ActiveTo = newActiveTo;
    }

    /// <summary>
    /// Bật hoặc tắt trạng thái hoạt động của hạng tài khoản này.
    /// </summary>
    public void UpdateActiveStatus(bool newStatus)
    {
        if (IsActive == newStatus)
            throw new DomainException($"Hạng tài khoản vốn dĩ đã ở trạng thái {(newStatus ? "Hoạt động" : "Tạm dừng")} từ trước.");

        IsActive = newStatus;
    }

    /// <summary>
    /// Kiểm tra xem tại thời điểm hiện tại hạng tài khoản này có đang mở cho phép đăng ký hay không.
    /// </summary>
    public bool IsCurrentlyAvailable(DateTime currentUtc)
    {
        if (!IsActive)
            return false;

        if (currentUtc < ActiveFrom)
            return false;

        if (ActiveTo.HasValue && currentUtc > ActiveTo.Value)
            return false;

        return true;
    }

    // --- Validation Methods ---

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Mã hạng tài khoản (Id) không được để trống.");

        string normalizedId = id.Trim();
        if (normalizedId.Length < MinIdLength || normalizedId.Length > MaxIdLength)
            throw new DomainException($"Mã hạng tài khoản (Id) phải từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Mã code nội bộ của hạng tài khoản không được để trống.");

        if (code.Trim().Length > MaxCodeLength)
            throw new DomainException($"Mã code nội bộ của hạng tài khoản không được vượt quá {MaxCodeLength} ký tự.");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên hạng tài khoản không được để trống.");

        if (name.Trim().Length > MaxNameLength)
            throw new DomainException($"Tên hạng tài khoản không được vượt quá {MaxNameLength} ký tự.");
    }

    private static void ValidatePrice(TierPrice price)
    {
        if (price == null)
            throw new DomainException("Thông tin giá tiền không được để trống.");
    }

    private static void ValidateCapabilities(TierCapabilities capabilities)
    {
        if (capabilities == null)
            throw new DomainException("Thông tin giới hạn đặc quyền không được để trống.");
    }

    private static void ValidateDurationInDays(int durationInDays)
    {
        if (durationInDays < MinDurationDays)
            throw new DomainException($"Thời gian sử dụng của hạng tài khoản phải từ {MinDurationDays} ngày trở lên.");
    }

    private static void ValidateScheduleDates(DateTime activeFrom, DateTime? activeTo)
    {
        if (activeFrom == default)
            throw new DomainException("Mốc thời gian bắt đầu hoạt động không được để mặc định.");

        if (activeTo.HasValue && activeTo.Value <= activeFrom)
            throw new DomainException("Mốc thời gian kết thúc phải lớn hơn mốc thời gian bắt đầu.");
    }
}