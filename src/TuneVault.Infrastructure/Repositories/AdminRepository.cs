using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// AdminRepository - Lớp quản lý dữ liệu Admin trên database.
/// 
/// Chức năng chính:
/// - Thao tác CRUD với Admin entities
/// - Tìm kiếm Admin theo Username (email), Email, hoặc Id
/// - Lấy thông tin Admin đang active (IsActive = 1)
/// 
/// Sử dụng: Dapper ORM với raw SQL queries để tối ưu performance.
/// </summary>
public sealed class AdminRepository : IAdminRepository
{
    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// Khởi tạo AdminRepository với IDbConnectionFactory dependency.
    /// </summary>
    /// <param name="db">Factory để tạo kết nối database</param>
    public AdminRepository(IDbConnectionFactory db) => _db = db;

    /// <summary>
    /// Lấy Admin theo Username (email), chỉ lấy admin đang active.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuyển username thành lowercase để so sánh (case-insensitive)
    /// 3. SELECT Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive 
    ///    FROM Admins WHERE Email = @Username AND IsActive = 1
    /// 4. Trả về Admin object hoặc null nếu không tìm thấy
    /// </summary>
    /// <param name="username">Email/Username của admin (sẽ được chuyển thành lowercase)</param>
    /// <param name="cancellationToken">CancellationToken để hủy operation</param>
    /// <returns>Admin object hoặc null</returns>
    public async Task<Admin?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            SELECT Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive
            FROM Admins
            WHERE Email = @Username AND IsActive = 1;";

        return await conn.QuerySingleOrDefaultAsync<Admin>(
            new CommandDefinition(sql, new { Username = username.ToLowerInvariant() }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy Admin theo Email, chỉ lấy admin đang active.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuyển email thành lowercase để so sánh (case-insensitive)
    /// 3. SELECT Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive 
    ///    FROM Admins WHERE Email = @Email AND IsActive = 1
    /// 4. Trả về Admin object hoặc null nếu không tìm thấy
    /// </summary>
    /// <param name="email">Email của admin (sẽ được chuyển thành lowercase)</param>
    /// <param name="cancellationToken">CancellationToken để hủy operation</param>
    /// <returns>Admin object hoặc null</returns>
    public async Task<Admin?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            SELECT Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive
            FROM Admins
            WHERE Email = @Email AND IsActive = 1;";

        return await conn.QuerySingleOrDefaultAsync<Admin>(
            new CommandDefinition(sql, new { Email = email.ToLowerInvariant() }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy Admin theo Id (khóa chính string).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. SELECT Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive 
    ///    FROM Admins WHERE Id = @Id
    /// 3. Trả về Admin object hoặc null nếu không tìm thấy
    /// 4. Lưu ý: Không lọc theo IsActive để có thể lấy admin đã bị deactivate (nếu cần)
    /// </summary>
    /// <param name="id">Id (string) của admin</param>
    /// <param name="cancellationToken">CancellationToken để hủy operation</param>
    /// <returns>Admin object hoặc null</returns>
    public async Task<Admin?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            SELECT Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive
            FROM Admins
            WHERE Id = @Id;";

        return await conn.QuerySingleOrDefaultAsync<Admin>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Sinh Id mới cho Admin dạng A001, A002, ..., A999.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database.
    /// 2. Lấy số lớn nhất từ các Id hiện có trong bảng Admins có định dạng 'A' + 3 chữ số.
    ///    - Sử dụng `MAX(CAST(SUBSTRING(Id, 2, 3) AS INT))` để lấy phần số.
    ///    - `ISNULL(..., 0) + 1` để xử lý trường hợp bảng trống hoặc bắt đầu từ 1.
    /// 3. Format số đó thành 3 chữ số có số 0 ở đầu (ví dụ: 1 -> "001", 12 -> "012", 123 -> "123").
    /// 4. Tiền tố 'A' vào số đã định dạng.
    /// 5. Trả về Id mới (ví dụ: "A001").
    /// </summary>
    /// <param name="cancellationToken">CancellationToken để hủy operation.</param>
    /// <returns>Id mới sinh ra (ví dụ: "A001").</returns>
    public async Task<string> GenerateNextIdAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT ISNULL(MAX(CAST(SUBSTRING(Id, 2, 3) AS INT)), 0) + 1
            FROM Admins
            WHERE Id LIKE 'A[0-9][0-9][0-9]'"; // Ensure Id is A followed by exactly 3 digits
        var nextNumber = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return $"A{nextNumber:D3}"; // A001, A002, ..., A999
    }

    /// <summary>
    /// Thêm Admin mới vào database.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database.
    /// 2. Sinh Id mới cho Admin bằng cách gọi GenerateNextIdAsync.
    /// 3. Chuẩn bị SQL INSERT với các cột: Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive.
    /// 4. Trích xuất dữ liệu từ Admin entity sang parameters:
    ///    - Id: Id mới sinh ra.
    ///    - Name: Tên đầy đủ của admin.
    ///    - Email: Email của admin (đã được chuyển thành lowercase trong entity).
    ///    - PasswordHash: Hash của password (đã được hash từ trước bởi domain).
    ///    - PhoneNumber: Số điện thoại liên hệ.
    ///    - Role: Vai trò của admin (Admin, Moderator, v.v.).
    ///    - IsActive: Trạng thái hoạt động (thường khởi tạo là 1/true).
    /// 5. Thực hiện INSERT.
    /// </summary>
    /// <param name="admin">Admin entity chứa dữ liệu cần lưu (Id có thể null để tự sinh).</param>
    /// <param name="cancellationToken">CancellationToken để hủy operation.</param>
    public async Task AddAsync(Admin admin, CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            INSERT INTO Admins (Id, Name, Email, PasswordHash, PhoneNumber, Role, IsActive)
            VALUES (@Id, @Name, @Email, @PasswordHash, @PhoneNumber, @Role, @IsActive);";

        // Sinh Id mới nếu Id của admin chưa được gán
        var adminId = admin.Id;
        if (string.IsNullOrEmpty(adminId))
        {
            adminId = await GenerateNextIdAsync(cancellationToken);
        }

        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = adminId, // Use the generated or provided Id
            admin.Name,
            admin.Email,
            admin.PasswordHash,
            admin.PhoneNumber,
            admin.Role,
            admin.IsActive
        }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Update Admin (phương pháp chung - không được sử dụng).
    /// 
    /// Lý do không sử dụng:
    /// - Dapper không có change tracking như EF Core
    /// - Update Admin thường được gọi từ specific handlers
    /// - Mỗi update là một method riêng để rõ ràng (ví dụ: UpdatePasswordAsync, SetStatusAsync)
    /// 
    /// Chính sách hiện tại:
    /// - Throw NotImplementedException với lời nhắc sử dụng method cụ thể
    /// </summary>
    /// <param name="admin">Admin entity (không được sử dụng)</param>
    /// <exception cref="NotImplementedException">Luôn throw - yêu cầu sử dụng method cụ thể</exception>
    public void Update(Admin admin)
    {
        // Dapper không có change tracking — update được gọi từ Handler cụ thể
        // với method riêng (vd UpdatePasswordAsync). Để trống theo pattern hiện tại.
        throw new NotImplementedException("Dùng method cụ thể thay vì Update chung.");
    }

    /// <summary>
    /// Delete Admin (hard delete - không được sử dụng).
    /// 
    /// Lý do không sử dụng:
    /// - Hệ thống sử dụng soft delete (IsActive = 0) thay vì xóa cứng
    /// - Hard delete có thể gây mất dữ liệu quan trọng
    /// - Soft delete cho phép khôi phục dữ liệu nếu cần
    /// 
    /// Chính sách hiện tại:
    /// - Throw NotImplementedException với lời nhắc sử dụng soft-delete (SetStatus(false))
    /// </summary>
    /// <param name="admin">Admin entity (không được sử dụng)</param>
    /// <exception cref="NotImplementedException">Luôn throw - yêu cầu sử dụng soft-delete</exception>
    public void Delete(Admin admin)
    {
        throw new NotImplementedException("Dùng soft-delete qua SetStatus(false) thay vì xóa cứng.");
    }
}