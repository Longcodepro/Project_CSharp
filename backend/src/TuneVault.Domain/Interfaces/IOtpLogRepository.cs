namespace TuneVault.Domain.Interfaces;

/// <summary>Kho dữ liệu cho bảng OtpLogs (mã OTP gửi qua email).</summary>
public interface IOtpLogRepository
{
    /// <summary>Sinh Id mới dạng chuỗi cho một OtpLog (vd OTP0000001).</summary>
    Task<string> GenerateNextIdAsync(CancellationToken ct);

    /// <summary>
    /// Thêm một bản ghi OTP mới.
    /// Trước khi thêm, mọi OTP active cũ cùng email+purpose phải được hủy để chỉ mã mới nhất còn hiệu lực.
    /// </summary>
    Task InsertAsync(string id, string email, string otpCode, string purpose, DateTime expiresAt, CancellationToken ct);

    /// <summary>
    /// Xác minh OTP còn hiệu lực (IsActive=1 và chưa hết hạn) cho đúng email+purpose.
    /// Nếu hợp lệ → set IsActive=0 (consume) và trả về true.
    /// Nếu mã sai → trả về false nhưng không hủy OTP để người dùng có thể nhập lại.
    /// </summary>
    Task<bool> VerifyAndConsumeAsync(string email, string otpCode, string purpose, CancellationToken ct);
}
