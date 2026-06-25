// Infrastructure/Repositories/UserRepository.cs
using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// UserRepository - Lớp quản lý dữ liệu User trên database.
/// 
/// Chức năng chính:
/// - Thao tác CRUD với User entities
/// - Quản lý follow/unfollow relationships
/// - Sinh ID tự động theo format U0000001
/// - Query dữ liệu User, Artist, Follow relationships
/// 
/// Sử dụng: Dapper ORM với raw SQL queries để tối ưu performance.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// Khởi tạo UserRepository với IDbConnectionFactory dependency.
    /// </summary>
    /// <param name="db">Factory để tạo kết nối database</param>
    public UserRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    // =========================================================================
    // QUERIES - Các method lấy dữ liệu từ database
    // =========================================================================

    /// <summary>
    /// Lấy User theo Id chính (khóa chính).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Thực hiện SELECT * WHERE Id = @Id
    /// 3. Trả về User object hoặc null nếu không tìm thấy
    /// </summary>
    /// <param name="id">Id của User (ví dụ: U0000001)</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>User object hoặc null</returns>
    public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
    {
        const string sql = "SELECT * FROM Users WHERE Id = @Id";
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User?>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy User theo IdDisplay (handle công khai, ví dụ: john_doe).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Thực hiện SELECT * WHERE IdDisplay = @IdDisplay
    /// 3. Trả về User object hoặc null nếu không tìm thấy
    /// </summary>
    /// <param name="idDisplay">Handle công khai của User</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>User object hoặc null</returns>
    public async Task<User?> GetByIdDisplayAsync(string idDisplay, CancellationToken ct)
    {
        const string sql = "SELECT * FROM Users WHERE IdDisplay = @IdDisplay AND IsActive = 1";
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User?>(
            new CommandDefinition(sql, new { IdDisplay = idDisplay }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy User theo Email (tìm kiếm độc nhất).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Thực hiện SELECT * WHERE Email = @Email (case-sensitive)
    /// 3. Trả về User object hoặc null nếu không tìm thấy
    /// </summary>
    /// <param name="email">Email của User</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>User object hoặc null</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email";
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User?>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy tất cả User có IsArtist = 1, sắp xếp theo DisplayName.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Thực hiện SELECT * WHERE IsArtist = 1 ORDER BY DisplayName ASC
    /// 3. Trả về danh sách User Artists
    /// </summary>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>IEnumerable&lt;User&gt; danh sách artists</returns>
    public async Task<IEnumerable<User>> GetAllArtistsAsync(CancellationToken ct)
    {
        const string sql = "SELECT * FROM Users WHERE IsArtist = 1 AND IsActive = 1 ORDER BY DisplayName ASC";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(
            new CommandDefinition(sql, cancellationToken: ct));
    }

    /// <summary>
    /// Kiểm tra User có tồn tại theo Id không (COUNT query).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Thực hiện SELECT COUNT(*) WHERE Id = @Id
    /// 3. Trả về true nếu count > 0, false nếu không
    /// </summary>
    /// <param name="id">Id của User</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>true nếu User tồn tại, false nếu không</returns>
    public async Task<bool> ExistsAsync(string id, CancellationToken ct)
    {
        const string sql = "SELECT COUNT(*) FROM Users WHERE Id = @Id";
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return count > 0;
    }

    /// <summary>
    /// Lấy danh sách User đang theo dõi followeeId (danh sách followers).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. JOIN Users với Follows trên FollowerId = Id
    /// 3. WHERE FolloweeId = @FolloweeId AND IsActive = 1
    /// 4. ORDER BY FollowedAt DESC (những người follow gần đây nhất)
    /// 5. Trả về danh sách User
    /// </summary>
    /// <param name="followeeId">Id của User mà ta muốn xem followers</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>IEnumerable&lt;User&gt; danh sách followers</returns>
    public async Task<IEnumerable<User>> GetFollowersAsync(string followeeId, CancellationToken ct)
    {
        const string sql = @"
            SELECT u.*
            FROM Users u
            JOIN Follows f ON u.Id = f.FollowerId
            WHERE f.FolloweeId = @FolloweeId AND f.IsActive = 1
            ORDER BY f.FollowedAt DESC";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(
            new CommandDefinition(sql, new { FolloweeId = followeeId }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy danh sách User mà followerId đang theo dõi (danh sách following).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. JOIN Users với Follows trên FolloweeId = Id
    /// 3. WHERE FollowerId = @FollowerId AND IsActive = 1
    /// 4. ORDER BY FollowedAt DESC (những người follow gần đây nhất)
    /// 5. Trả về danh sách User
    /// </summary>
    /// <param name="followerId">Id của User mà ta muốn xem following</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>IEnumerable&lt;User&gt; danh sách following</returns>
    public async Task<IEnumerable<User>> GetFollowingAsync(string followerId, CancellationToken ct)
    {
        const string sql = @"
            SELECT u.*
            FROM Users u
            JOIN Follows f ON u.Id = f.FolloweeId
            WHERE f.FollowerId = @FollowerId AND f.IsActive = 1
            ORDER BY f.FollowedAt DESC";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(
            new CommandDefinition(sql, new { FollowerId = followerId }, cancellationToken: ct));
    }

    /// <summary>
    /// Kiểm tra xem followerId có đang theo dõi followeeId không.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. SELECT COUNT(*) WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId AND IsActive = 1
    /// 3. Trả về true nếu đang follow, false nếu không
    /// </summary>
    /// <param name="followerId">Id của follower</param>
    /// <param name="followeeId">Id của followee (người được follow)</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>true nếu đang follow, false nếu không</returns>
    public async Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken ct)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Follows
            WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId AND IsActive = 1";
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { FollowerId = followerId, FolloweeId = followeeId }, cancellationToken: ct));
        return count > 0;
    }

    // =========================================================================
    // COMMANDS - Các method thay đổi dữ liệu trong database
    // =========================================================================

    /// <summary>
    /// Sinh Id mới cho User dạng U001, U002, ..., U999.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database.
    /// 2. Lấy số lớn nhất từ các Id hiện có trong bảng Users có định dạng 'U' + 3 chữ số.
    ///    - Sử dụng `MAX(CAST(SUBSTRING(Id, 2, 3) AS INT))` để lấy phần số.
    ///    - `ISNULL(..., 0) + 1` để xử lý trường hợp bảng trống hoặc bắt đầu từ 1.
    /// 3. Format số đó thành 3 chữ số có số 0 ở đầu (ví dụ: 1 -> "001", 12 -> "012", 123 -> "123").
    /// 4. Tiền tố 'U' vào số đã định dạng.
    /// 5. Trả về Id mới (ví dụ: "U001").
    /// </summary>
    /// <param name="ct">CancellationToken để hủy operation.</param>
    /// <returns>Id mới sinh ra (ví dụ: "U001").</returns>
    public async Task<string> GenerateNextIdAsync(CancellationToken ct)
    {
        const string sql = @"
            SELECT ISNULL(MAX(CAST(SUBSTRING(Id, 2, 3) AS INT)), 0) + 1
            FROM Users
            WHERE Id LIKE 'U[0-9][0-9][0-9]'"; // Ensure Id is U followed by exactly 3 digits
        using var conn = _db.CreateConnection();
        var nextNumber = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: ct));
        return $"U{nextNumber:D3}"; // U001, U002, ..., U999
    }

    /// <summary>
    /// Thêm User mới vào database.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuẩn bị SQL INSERT với các cột: Id, IdDisplay, Email, DisplayName, AvatarUrl, IsArtist, IsActive, CreatedAt, PasswordHash
    /// 3. Trích xuất dữ liệu từ User entity sang parameters
    /// 4. Thực hiện INSERT
    /// 5. Không trả về giá trị, chỉ lưu dữ liệu
    /// </summary>
    /// <param name="user">User entity chứa thông tin cần lưu</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    public async Task InsertAsync(User user, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO Users (Id, IdDisplay, Email, DisplayName, AvatarUrl, IsArtist, IsActive, CreatedAt, PasswordHash)
            VALUES (@Id, @IdDisplay, @Email, @DisplayName, @AvatarUrl, @IsArtist, @IsActive, @CreatedAt, @PasswordHash)";
        
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = user.Id,
                IdDisplay = user.IdDisplay,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                IsArtist = user.IsArtist,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                PasswordHash = user.PasswordHash
            }, cancellationToken: ct));
    }

    /// <summary>
    /// Cập nhật toàn bộ thông tin User.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuẩn bị SQL UPDATE với các trường: IdDisplay, Email, DisplayName, AvatarUrl, Bio, IsArtist, IsActive, PasswordHash
    /// 3. WHERE Id = @Id để xác định User cần update
    /// 4. Trích xuất dữ liệu từ User entity sang parameters
    /// 5. Thực hiện UPDATE và lấy số rows bị ảnh hưởng
    /// 6. Trả về true nếu cập nhật thành công (affected > 0), false nếu không
    /// </summary>
    /// <param name="user">User entity chứa dữ liệu cập nhật</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>true nếu cập nhật thành công, false nếu User không tồn tại</returns>
    public async Task<bool> UpdateAsync(User user, CancellationToken ct)
    {
        const string sql = @"
            UPDATE Users
            SET IdDisplay = @IdDisplay,
                Email = @Email,
                DisplayName = @DisplayName,
                AvatarUrl = @AvatarUrl,
                Bio = @Bio,
                IsArtist = @IsArtist,
                IsActive = @IsActive,
                PasswordHash = @PasswordHash
            WHERE Id = @Id";
        
        using var conn = _db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = user.Id,
                IdDisplay = user.IdDisplay,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                IsArtist = user.IsArtist,
                IsActive = user.IsActive,
                PasswordHash = user.PasswordHash
            }, cancellationToken: ct));
        return affected > 0;
    }

    /// <summary>
    /// Cập nhật mật khẩu (PasswordHash) của User.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuẩn bị SQL UPDATE chỉ cột PasswordHash
    /// 3. WHERE Id = @UserId để xác định User
    /// 4. Thực hiện UPDATE với PasswordHash mới
    /// 5. Không kiểm tra số rows bị ảnh hưởng (giả định User tồn tại)
    /// </summary>
    /// <param name="userId">Id của User cần cập nhật password</param>
    /// <param name="newPasswordHash">Hash của password mới</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    public async Task UpdatePasswordHashAsync(string userId, string newPasswordHash, CancellationToken ct)
    {
        const string sql = @"
            UPDATE Users
            SET PasswordHash = @NewPasswordHash
            WHERE Id = @UserId";
        
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, NewPasswordHash = newPasswordHash }, cancellationToken: ct));
    }

    /// <summary>
    /// Tạo quan hệ Follow giữa followerId và followeeId.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Kiểm tra xem bản ghi Follow cũ có tồn tại không (soft delete pattern)
    ///    - SELECT TOP 1 Id, IsActive WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId
    /// 3. Nếu bản ghi tồn tại và IsActive = 1: trả về false (đã follow rồi)
    /// 4. Nếu bản ghi tồn tại nhưng IsActive = 0: UPDATE IsActive = 1, FollowedAt = GETUTCDATE() để reactivate
    /// 5. Nếu không tồn tại: INSERT bản ghi mới với Id tạo từ Guid
        /// 6. Trả về true nếu thành công
        /// </summary>
        /// <param name="followerId">Id của follower</param>
        /// <param name="followeeId">Id của followee</param>
        /// <param name="ct">CancellationToken để hủy operation</param>
        /// <returns>true nếu follow thành công, false nếu đã follow rồi</returns>
        public async Task<bool> FollowUserAsync(string followerId, string followeeId, CancellationToken ct)
        {
            // Bước 1: Kiểm tra bản ghi cũ (Soft delete pattern)
            const string checkSql = @"
                SELECT TOP 1 Id, IsActive FROM Follows
                WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId";
            using var conn = _db.CreateConnection();
            var existing = await conn.QuerySingleOrDefaultAsync<(string Id, bool IsActive)?>(
                new CommandDefinition(checkSql, new { FollowerId = followerId, FolloweeId = followeeId }, cancellationToken: ct));

            if (existing.HasValue)
            {
                // Bước 3: Nếu đã active rồi, trả về false
                if (existing.Value.IsActive) return false; 
                
                // Bước 4: Nếu inactive, reactivate bản ghi cũ
                const string reactivateSql = @"
                    UPDATE Follows SET IsActive = 1, FollowedAt = GETUTCDATE()
                    WHERE Id = @Id";
                await conn.ExecuteAsync(new CommandDefinition(reactivateSql, new { existing.Value.Id }, cancellationToken: ct));
                return true;
            }

            // Bước 5: Tạo mới nếu không tồn tại
            var newId = await GenerateNextFollowIdAsync(ct); // Sử dụng phương thức mới để sinh ID
            const string insertSql = @"
                INSERT INTO Follows (Id, FollowerId, FolloweeId, FollowedAt, IsActive)
                VALUES (@Id, @FollowerId, @FolloweeId, GETUTCDATE(), 1)";
            var affected = await conn.ExecuteAsync(
                new CommandDefinition(insertSql, new { Id = newId, FollowerId = followerId, FolloweeId = followeeId }, cancellationToken: ct));
            return affected > 0;
        }

        /// <summary>
        /// Sinh Id mới cho Follow dạng F0001, F0002, ..., F9999.
        /// </summary>
        /// <param name="ct">CancellationToken để hủy operation</param>
        /// <returns>Id mới sinh ra (ví dụ: F0001)</returns>
        private async Task<string> GenerateNextFollowIdAsync(CancellationToken ct)
        {
            const string sql = @"
                SELECT ISNULL(MAX(CAST(SUBSTRING(Id, 2, 4) AS INT)), 0) + 1
                FROM Follows
                WHERE Id LIKE 'F[0-9][0-9][0-9][0-9]'";
            using var conn = _db.CreateConnection();
            var nextNumber = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, cancellationToken: ct));
            return $"F{nextNumber:D4}"; // F0001, F0002, ...
        }

        /// <summary>
    /// Hủy follow (Unfollow) — sử dụng Soft delete pattern (IsActive = 0).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. UPDATE Follows SET IsActive = 0
    /// 3. WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId AND IsActive = 1
    ///    - Chỉ unfollow nếu đang active
    /// 4. Lấy số rows bị ảnh hưởng
    /// 5. Trả về true nếu affected > 0 (unfollow thành công)
    /// </summary>
    /// <param name="followerId">Id của follower</param>
    /// <param name="followeeId">Id của followee</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>true nếu unfollow thành công, false nếu không đang follow</returns>
    public async Task<bool> UnfollowUserAsync(string followerId, string followeeId, CancellationToken ct)
    {
        const string sql = @"
            UPDATE Follows SET IsActive = 0
            WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId AND IsActive = 1";
        using var conn = _db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            new CommandDefinition(sql, new { FollowerId = followerId, FolloweeId = followeeId }, cancellationToken: ct));
        return affected > 0;
    }
}
