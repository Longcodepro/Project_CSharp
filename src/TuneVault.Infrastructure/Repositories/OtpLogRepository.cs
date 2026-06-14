using Dapper;
using Microsoft.Extensions.Configuration;
using TuneVault.Infrastructure.Persistence;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// OtpLogRepository - Lớp quản lý dữ liệu OTP Log trên database.
/// 
/// Chức năng chính:
/// - Sinh Id tự động cho OTP Log theo format OTP0000001, OTP0000002, ...
/// - Lưu lịch sử gửi OTP (email, mã OTP, mục đích, thời hạn)
/// - Xác minh OTP và đánh dấu là đã sử dụng (consume)
/// 
/// OTP Pattern:
/// - OTP được gửi tới email để xác thực
/// - Mỗi OTP chỉ được sử dụng một lần (IsActive = 0 sau khi verify)
/// - OTP có thời hạn (ExpiresAt)
/// 
/// Sử dụng: Dapper ORM với raw SQL queries để tối ưu performance.
/// </summary>
public sealed class OtpLogRepository : IOtpLogRepository
{
    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// Khởi tạo OtpLogRepository với IDbConnectionFactory dependency.
    /// </summary>
    /// <param name="db">Factory để tạo kết nối database</param>
    public OtpLogRepository(IDbConnectionFactory db) => _db = db;

    /// <summary>
    /// Sinh Id mới cho OTP Log dạng O001, O002, ..., O999.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database.
    /// 2. Lấy số lớn nhất từ các Id hiện có trong bảng OtpLogs có định dạng 'O' + 3 chữ số.
    ///    - Sử dụng `MAX(CAST(SUBSTRING(Id, 2, 3) AS INT))` để lấy phần số.
    ///    - `ISNULL(..., 0) + 1` để xử lý trường hợp bảng trống hoặc bắt đầu từ 1.
    /// 3. Format số đó thành 3 chữ số có số 0 ở đầu (ví dụ: 1 -> "001", 12 -> "012", 123 -> "123").
    /// 4. Tiền tố 'O' vào số đã định dạng.
    /// 5. Trả về Id mới (ví dụ: "O001").
    /// </summary>
    /// <param name="ct">CancellationToken để hủy operation.</param>
    /// <returns>Id mới sinh ra (ví dụ: "O001").</returns>
    public async Task<string> GenerateNextIdAsync(CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        // Lấy ID lớn nhất hiện tại
        var maxId = await conn.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(@"
                SELECT MAX(Id)
                FROM OtpLogs
                WHERE Id LIKE 'O[0-9][0-9][0-9]';", cancellationToken: ct)); // Ensure Id is O followed by exactly 3 digits

        // Nếu bảng trống, bắt đầu từ O001
        if (string.IsNullOrEmpty(maxId))
        {
            return "O001";
        }

        // Extract phần số từ "O001" → "001" → 1
        var numericPart = int.Parse(maxId.Substring(1)); 
        // Format lại thành "O002"
        var nextId = $"O{numericPart + 1:D3}"; 
        return nextId;
    }

    /// <summary>
    /// Thêm OTP Log mới vào database.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuẩn bị SQL INSERT với các cột: Id, Email, OtpCode, Purpose, CreatedAt, ExpiresAt, IsActive
    /// 3. Trích xuất dữ liệu sang parameters:
    ///    - Id: Id đã được sinh trước (từ GenerateNextIdAsync)
    ///    - Email: Email nhận OTP (chuyển thành lowercase để đảm bảo consistency)
    ///    - OtpCode: Mã OTP gửi tới email (ví dụ: 123456)
    ///    - Purpose: Mục đích OTP (ví dụ: "ResetPassword", "SignUp", "ChangeEmail")
    ///    - CreatedAt: Thời gian tạo = GETUTCDATE() (current server time)
    ///    - ExpiresAt: Thời gian hết hạn (được tính trước bởi application, ví dụ: 5 phút từ bây giờ)
    ///    - IsActive: 1 (OTP còn hiệu lực, chưa bị sử dụng)
    /// 4. Thực hiện INSERT
    /// </summary>
    /// <param name="id">Id của OTP Log (từ GenerateNextIdAsync)</param>
    /// <param name="email">Email nhận OTP</param>
    /// <param name="otpCode">Mã OTP (số, ví dụ: 123456)</param>
    /// <param name="purpose">Mục đích gửi OTP (ResetPassword, SignUp, ChangeEmail, v.v.)</param>
    /// <param name="expiresAt">Thời gian hết hạn của OTP</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    public async Task InsertAsync(string id, string email, string otpCode, string purpose, DateTime expiresAt, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            INSERT INTO OtpLogs (Id, Email, OtpCode, Purpose, CreatedAt, ExpiresAt, IsActive)
            VALUES (@Id, @Email, @OtpCode, @Purpose, GETUTCDATE(), @ExpiresAt, 1);";

        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = id,
            Email = email.ToLowerInvariant(), // Lưu email dạng lowercase
            OtpCode = otpCode,
            Purpose = purpose,
            ExpiresAt = expiresAt
        }, cancellationToken: ct));
    }

    /// <summary>
    /// Xác minh OTP và đánh dấu là đã sử dụng (consume).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Tìm kiếm OTP matching:
    ///    - Email = @Email (chuyển thành lowercase để so sánh)
    ///    - OtpCode = @OtpCode (mã OTP được cung cấp)
    ///    - Purpose = @Purpose (mục đích phải match, ví dụ: "ResetPassword")
    ///    - IsActive = 1 (OTP chưa bị sử dụng)
    ///    - ExpiresAt > GETUTCDATE() (OTP chưa hết hạn)
    /// 3. SELECT TOP 1 ... ORDER BY CreatedAt DESC (lấy OTP mới nhất nếu có nhiều)
    /// 4. Nếu không tìm thấy (null):
    ///    - Trả về false (OTP không hợp lệ/không tồn tại/đã hết hạn/đã sử dụng)
    /// 5. Nếu tìm thấy:
    ///    - UPDATE OtpLogs SET IsActive = 0 WHERE Id = @Id (đánh dấu đã sử dụng)
    ///    - Trả về true (verify thành công)
    /// </summary>
    /// <param name="email">Email của user (sẽ được chuyển thành lowercase)</param>
    /// <param name="otpCode">Mã OTP do user cung cấp</param>
    /// <param name="purpose">Mục đích OTP (phải match khi lưu)</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>true nếu verify thành công, false nếu OTP không hợp lệ/hết hạn/đã sử dụng</returns>
    public async Task<bool> VerifyAndConsumeAsync(string email, string otpCode, string purpose, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        // Bước 1-3: Tìm OTP hợp lệ
        var sql = @"
            SELECT TOP 1 Id
            FROM OtpLogs
            WHERE Email = @Email
              AND OtpCode = @OtpCode
              AND Purpose = @Purpose
              AND IsActive = 1
              AND ExpiresAt > GETUTCDATE()
            ORDER BY CreatedAt DESC;";

        var existingOtpId = await conn.QuerySingleOrDefaultAsync<string>(new CommandDefinition(sql, new
        {
            Email = email.ToLowerInvariant(),
            OtpCode = otpCode,
            Purpose = purpose
        }, cancellationToken: ct));

        // Bước 4: Nếu không tìm thấy
        if (string.IsNullOrEmpty(existingOtpId))
        {
            return false; // OTP không tìm thấy, không hợp lệ, đã hết hạn, hoặc đã sử dụng
        }

        // Bước 5: Consume OTP (đánh dấu đã sử dụng)
        var updateSql = @"
            UPDATE OtpLogs
            SET IsActive = 0
            WHERE Id = @Id;";

        await conn.ExecuteAsync(new CommandDefinition(updateSql, new { Id = existingOtpId }, cancellationToken: ct));

        return true;
    }
}