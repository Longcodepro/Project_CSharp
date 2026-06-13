using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Triển khai <see cref="IUserRepository"/> bằng Dapper + SQL Server.
/// Chịu trách nhiệm toàn bộ truy vấn và thao tác dữ liệu liên quan đến <see cref="User"/>.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    /// <summary>
    /// Khởi tạo repository với <see cref="DapperContext"/> được inject qua DI container.
    /// </summary>
    /// <param name="context">Wrapper context chứa factory tạo kết nối SQL Server.</param>
    public UserRepository(DapperContext context) => _context = context;

    // =========================================================================
    // QUERIES
    // =========================================================================

    /// <summary>
    /// Lấy User theo Id nội bộ (Primary Key).
    /// </summary>
    /// <param name="id">Mã định danh nội bộ (VD: U001).</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Entity <see cref="User"/> hoặc <c>null</c> nếu không tồn tại.</returns>
    public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
    {
        // Step 1: Định nghĩa câu truy vấn SELECT theo PK
        const string sql = "SELECT * FROM [Users] WHERE Id = @Id";

        // Step 2: Mở connection và thực thi truy vấn trả về tối đa 1 kết quả
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy User theo handle công khai (IdDisplay), không phân biệt hoa thường.
    /// </summary>
    /// <param name="idDisplay">Handle công khai (VD: john_doe).</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Entity <see cref="User"/> hoặc <c>null</c> nếu không tồn tại.</returns>
    public async Task<User?> GetByIdDisplayAsync(string idDisplay, CancellationToken ct)
    {
        // Step 1: Chuẩn hóa handle về lowercase trước khi truy vấn
        const string sql = "SELECT * FROM [Users] WHERE IdDisplay = @IdDisplay";

        // Step 2: Mở connection và thực thi truy vấn
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { IdDisplay = idDisplay.ToLowerInvariant() }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy danh sách tất cả User là nghệ sĩ và đang hoạt động.
    /// </summary>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách các <see cref="User"/> có IsArtist = true và IsActive = true.</returns>
    public async Task<IEnumerable<User>> GetAllArtistsAsync(CancellationToken ct)
    {
        // Step 1: Lọc theo cả IsArtist và IsActive để tránh trả về tài khoản đã vô hiệu hóa
        const string sql = "SELECT * FROM [Users] WHERE IsArtist = 1 AND IsActive = 1";

        // Step 2: Trả về danh sách (có thể rỗng nếu chưa có nghệ sĩ nào)
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<User>(
            new CommandDefinition(sql, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy danh sách người đang theo dõi một User (followers).
    /// </summary>
    /// <param name="followeeId">Id nội bộ của User được theo dõi.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách các <see cref="User"/> đang follow followeeId.</returns>
    public async Task<IEnumerable<User>> GetFollowersAsync(string followeeId, CancellationToken ct)
    {
        // Step 1: JOIN bảng Users với Follows để lấy thông tin người theo dõi
        // Step 2: Chỉ lấy bản ghi IsActive = 1 (tránh lấy quan hệ đã bị soft-delete)
        const string sql = @"
            SELECT U.*
            FROM [Users] U
            INNER JOIN [Follows] F ON U.Id = F.FollowerId
            WHERE F.FolloweeId = @FolloweeId AND F.IsActive = 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<User>(
            new CommandDefinition(sql, new { FolloweeId = followeeId }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy danh sách User mà một User đang theo dõi (following).
    /// </summary>
    /// <param name="followerId">Id nội bộ của User đang theo dõi.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách các <see cref="User"/> đang được followerId theo dõi.</returns>
    public async Task<IEnumerable<User>> GetFollowingAsync(string followerId, CancellationToken ct)
    {
        // Step 1: JOIN ngược lại — lấy FolloweeId để biết User đang follow ai
        // Step 2: Chỉ lấy bản ghi IsActive = 1
        const string sql = @"
            SELECT U.*
            FROM [Users] U
            INNER JOIN [Follows] F ON U.Id = F.FolloweeId
            WHERE F.FollowerId = @FollowerId AND F.IsActive = 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<User>(
            new CommandDefinition(sql, new { FollowerId = followerId }, cancellationToken: ct));
    }

    /// <summary>
    /// Kiểm tra xem một User có đang theo dõi User khác hay không.
    /// </summary>
    /// <param name="followerId">Id nội bộ của người theo dõi.</param>
    /// <param name="followeeId">Id nội bộ của người được theo dõi.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu quan hệ follow đang active, <c>false</c> nếu chưa follow.</returns>
    public async Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken ct)
    {
        // Step 1: Đếm bản ghi thỏa mãn FollowerId + FolloweeId + IsActive = 1
        const string sql = @"
            SELECT COUNT(1)
            FROM [Follows]
            WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId AND IsActive = 1";

        // Step 2: Chuyển count > 0 thành bool
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { FollowerId = followerId, FolloweeId = followeeId }, cancellationToken: ct));
        return count > 0;
    }

    /// <summary>
    /// Kiểm tra User có tồn tại trong hệ thống hay không.
    /// </summary>
    /// <param name="id">Id nội bộ của User cần kiểm tra.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu User tồn tại.</returns>
    public async Task<bool> ExistsAsync(string id, CancellationToken ct)
    {
        // Step 1: Dùng COUNT(1) thay vì SELECT * để tối ưu hiệu năng
        const string sql = "SELECT COUNT(1) FROM [Users] WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return count > 0;
    }

    // =========================================================================
    // COMMANDS
    // =========================================================================

    /// <summary>
    /// Cập nhật thông tin của một User trong database.
    /// </summary>
    /// <param name="user">Entity User với các thông tin đã được thay đổi qua method nghiệp vụ.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu có ít nhất 1 row bị ảnh hưởng.</returns>
    public async Task<bool> UpdateAsync(User user, CancellationToken ct)
    {
        // Step 1: Cập nhật toàn bộ các cột có thể thay đổi (không cập nhật Id hay CreatedAt)
        const string sql = @"
            UPDATE [Users]
            SET DisplayName    = @DisplayName,
                Bio            = @Bio,
                AvatarUrl      = @AvatarUrl,
                Email          = @Email,
                PasswordHash   = @PasswordHash,
                IsArtist       = @IsArtist,
                TotalFollowers = @TotalFollowers,
                IsActive       = @IsActive
            WHERE Id = @Id";

        // Step 2: Thực thi và kiểm tra rowsAffected để xác nhận thành công
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                user.Id,
                user.DisplayName,
                user.Bio,
                user.AvatarUrl,
                user.Email,
                user.PasswordHash,
                user.IsArtist,
                user.TotalFollowers,
                user.IsActive
            }, cancellationToken: ct));
        return rowsAffected > 0;
    }

    /// <summary>
    /// Tạo hoặc kích hoạt lại bản ghi follow trong bảng Follows.
    /// Nếu đã tồn tại bản ghi (soft-deleted) → UPDATE IsActive = 1.
    /// Nếu chưa tồn tại → INSERT bản ghi mới.
    /// </summary>
    /// <param name="followerId">Id nội bộ của người thực hiện follow.</param>
    /// <param name="followeeId">Id nội bộ của người được follow.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu thao tác thành công.</returns>
    public async Task<bool> FollowUserAsync(string followerId, string followeeId, CancellationToken ct)
    {
        // Step 1: Dùng IF EXISTS để phân biệt UPDATE (kích hoạt lại) vs INSERT (tạo mới)
        // Step 2: Bảng Follows có cột Id là PK tự sinh — cần tự tạo Id khi INSERT
        const string followSql = @"
            IF EXISTS (SELECT 1 FROM Follows WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId)
            BEGIN
                -- Nếu đã từng follow, kích hoạt lại và làm mới ngày giờ thao tác
                UPDATE Follows 
                SET IsActive = 1, 
                    FollowedAt = GETUTCDATE() 
                WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId;
            END
            ELSE
            BEGIN
                -- Tạo mới bản ghi với Id tự sinh (NEWID dạng string ngắn)
                INSERT INTO Follows (Id, FollowerId, FolloweeId, FollowedAt, IsActive)
                VALUES (LEFT(REPLACE(NEWID(), '-', ''), 10), @FollowerId, @FolloweeId, GETUTCDATE(), 1);
            END";

        // Step 3: Thực thi và kiểm tra kết quả
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(followSql, new { FollowerId = followerId, FolloweeId = followeeId }, cancellationToken: ct));
        return rowsAffected > 0;
    }

    /// <summary>
    /// Soft-delete quan hệ follow bằng cách chuyển IsActive về 0.
    /// Không xóa bản ghi vật lý khỏi database.
    /// </summary>
    /// <param name="followerId">Id nội bộ của người hủy follow.</param>
    /// <param name="followeeId">Id nội bộ của người bị hủy follow.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu có ít nhất 1 row bị ảnh hưởng.</returns>
    public async Task<bool> UnfollowUserAsync(string followerId, string followeeId, CancellationToken ct)
    {
        // Step 1: Chỉ UPDATE IsActive = 0 (Soft Delete) — không DELETE khỏi DB
        // Step 2: Thêm điều kiện IsActive = 1 để tránh UPDATE bản ghi đã soft-deleted
        const string unfollowSql = @"
            UPDATE Follows 
            SET IsActive = 0 
            WHERE FollowerId = @FollowerId AND FolloweeId = @FolloweeId AND IsActive = 1;";

        // Step 3: Thực thi và trả về kết quả
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(unfollowSql, new { FollowerId = followerId, FolloweeId = followeeId }, cancellationToken: ct));
        return rowsAffected > 0;
    }
}
