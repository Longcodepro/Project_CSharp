using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.ValueObjects;

/// <summary>
/// Đối tượng giá trị (Value Object) đại diện cho thông tin biểu phí của Hạng tài khoản.
/// Đảm bảo tính toàn vẹn giữa số tiền và đơn vị tiền tệ pháp định.
/// </summary>
public class TierPrice
{
    // --- Constants ---
    private const decimal MinAmount = 0;
    private const int MaxCurrencyLength = 5;

    // --- Properties ---

    /// <summary>
    /// Số tiền cần thanh toán cho hạng tài khoản (Ví dụ: 59000).
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Đơn vị tiền tệ pháp định (Ví dụ: VND, USD).
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM.
    /// </summary>
    private TierPrice() { }

    /// <summary>
    /// Khởi tạo một đối tượng giá tiền hợp lệ.
    /// </summary>
    public TierPrice(decimal amount, string currency)
    {
        ValidateAmount(amount);
        ValidateCurrency(currency);

        Amount = amount;
        Currency = currency.Trim().ToUpper(); // Chuẩn hóa đơn vị tiền tệ viết hoa (VND, USD)
    }

    // --- Business Methods ---

    /// <summary>
    /// Kiểm tra xem hạng tài khoản này có phải là hạng miễn phí (Free Tier) hay không.
    /// </summary>
    public bool IsFree()
    {
        if (Amount == 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Định dạng chuỗi hiển thị giá tiền cơ bản.
    /// </summary>
    public string ToDisplayString()
    {
        return $"{Amount:N0} {Currency}";
    }

    // --- Validation Methods ---

    private static void ValidateAmount(decimal amount)
    {
        if (amount < MinAmount)
            throw new DomainException("Số tiền của hạng tài khoản không được nhỏ hơn 0.");
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Đơn vị tiền tệ không được để trống.");

        string normalizedCurrency = currency.Trim();
        if (normalizedCurrency.Length > MaxCurrencyLength)
            throw new DomainException($"Đơn vị tiền tệ không được vượt quá {MaxCurrencyLength} ký tự.");
    }
}