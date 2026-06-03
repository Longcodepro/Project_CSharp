using System;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.ValueObjects;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho lượt đăng ký/nâng cấp hạng tài khoản (UserAccountTier) của người dùng.
/// Chỉ lưu trữ các khóa ngoại dạng chuỗi (string) để liên kết lịch sử giữa User và AccountTier.
/// Mọi định danh (Id, UserId, TierId) đều sử dụng kiểu chuỗi (string).
/// </summary>
public class UserAccountTier
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của lượt đăng ký hạng tài khoản này (Ví dụ: HIST1).
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Mã định danh của người dùng (Khóa ngoại liên kết với User).
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Mã định danh của hạng tài khoản được đăng ký (Khóa ngoại liên kết với AccountTier).
    /// </summary>
    public string TierId { get; private set; }

    /// <summary>
    /// Bản sao thông tin giá tiền thực tế tại thời điểm người dùng thực hiện bấm mua hạng này.
    /// </summary>
    public TierPrice PriceAtPurchase { get; private set; }

    /// <summary>
    /// Thời điểm người dùng thực hiện giao dịch thanh toán mua hạng tài khoản thành công.
    /// </summary>
    public DateTime PurchasedAt { get; private set; }

    /// <summary>
    /// Thời điểm hạng tài khoản này chính thức bắt đầu kích hoạt quyền lợi cho người dùng.
    /// </summary>
    public DateTime ActivatedAt { get; private set; }

    /// <summary>
    /// Thời điểm hạng tài khoản này hết hiệu lực đặc quyền.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Trạng thái hiệu lực logic của lượt đăng ký.
    /// </summary>
    public bool IsActive { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM.
    /// </summary>
    private UserAccountTier() { }

    /// <summary>
    /// Khởi tạo một lượt kích hoạt hạng tài khoản mới cho người dùng.
    /// </summary>
    public UserAccountTier(string id, string userId, string tierId, TierPrice priceAtPurchase, int durationInDays, DateTime? currentActiveExtensionDate = null)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateTierId(tierId);
        ValidatePriceAtPurchase(priceAtPurchase);
        ValidateDurationInDays(durationInDays);

        Id = id.Trim();
        UserId = userId.Trim();
        TierId = tierId.Trim();
        PriceAtPurchase = priceAtPurchase;
        
        PurchasedAt = DateTime.UtcNow;
        IsActive = true;

        // Xử lý logic nối đuôi hạng (Gia hạn sớm khi hạng tài khoản cũ chưa hết hạn hoàn toàn)
        if (currentActiveExtensionDate.HasValue && currentActiveExtensionDate.Value > PurchasedAt)
        {
            ActivatedAt = currentActiveExtensionDate.Value;
        }
        else
        {
            ActivatedAt = PurchasedAt;
        }

        ExpiresAt = ActivatedAt.AddDays(durationInDays);
    }

    // --- Business Methods ---

    /// <summary>
    /// Hạ cấp trạng thái hoạt động khi lượt đăng ký này đã quá hạn sử dụng.
    /// </summary>
    public void MarkAsExpired()
    {
        if (!IsActive)
            throw new DomainException("Lượt đăng ký hạng tài khoản này vốn dĩ đã kết thúc từ trước.");

        IsActive = false;
    }

    /// <summary>
    /// Chủ động hủy quyền lợi hạng tài khoản lập tức (Ví dụ do hoàn tiền hoặc vi phạm chính sách).
    /// </summary>
    public void CancelSubscription()
    {
        IsActive = false;
        ExpiresAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Kiểm tra xem lượt đăng ký này có đang mang lại quyền lợi hợp pháp cho User tại thời điểm chỉ định hay không.
    /// </summary>
    public bool IsCurrentlyValid(DateTime currentUtc)
    {
        if (!IsActive)
            return false;

        if (currentUtc < ActivatedAt)
            return false;

        if (currentUtc > ExpiresAt)
            return false;

        return true;
    }

    // --- Validation Methods ---

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Mã lượt đăng ký hạng tài khoản (Id) không được để trống.");

        string normalizedId = id.Trim();
        if (normalizedId.Length < MinIdLength || normalizedId.Length > MaxIdLength)
            throw new DomainException($"Mã lượt đăng ký hạng tài khoản (Id) phải từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("Mã người dùng (UserId) không được để trống.");
    }

    private static void ValidateTierId(string tierId)
    {
        if (string.IsNullOrWhiteSpace(tierId))
            throw new DomainException("Mã hạng tài khoản (TierId) không được để trống.");
    }

    private static void ValidatePriceAtPurchase(TierPrice priceAtPurchase)
    {
        if (priceAtPurchase == null)
            throw new DomainException("Thông tin giá tiền tại thời điểm mua không được để trống.");
    }

    private static void ValidateDurationInDays(int durationInDays)
    {
        if (durationInDays < 1)
            throw new DomainException("Số ngày sử dụng của hạng tài khoản phải từ 1 ngày trở lên.");
    }
}