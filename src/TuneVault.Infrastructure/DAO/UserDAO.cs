using Dapper;
using TuneVault.Domain.Entities;

namespace TuneVault.Infrastructure.DAO
{
    /// <summary>
    /// Xử lý các thao tác database liên quan đến User
    /// </summary>
    public class UserDAO
    {
        private readonly DapperContext _context;

        // .NET tự inject DapperContext vào đây
        public UserDAO(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả Users trong database
        /// </summary>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var sql = "SELECT * FROM Users";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<User>(sql);
        }

        /// <summary>
        /// Lấy User theo Id
        /// </summary>
        public async Task<User?> GetByIdAsync(string id)
        {
            var sql = "SELECT * FROM Users WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        /// <summary>
        /// Lấy User theo Email — dùng khi đăng nhập
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        /// <summary>
        /// Thêm User mới vào database
        /// </summary>
        public async Task CreateAsync(User user)
        {
            var sql = @"
                INSERT INTO Users 
                    (Id, UserName, Email, PasswordHash, Role, Rank, DisplayName, AvatarUrl, CreatedAt)
                VALUES 
                    (@Id, @UserName, @Email, @PasswordHash, @Role, @Rank, @DisplayName, @AvatarUrl, @CreatedAt)";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, user);
        }

        /// <summary>
        /// Cập nhật thông tin User
        /// </summary>
        public async Task UpdateAsync(User user)
        {
            var sql = @"
                UPDATE Users SET
                    UserName    = @UserName,
                    DisplayName = @DisplayName,
                    AvatarUrl   = @AvatarUrl,
                    Rank        = @Rank
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, user);
        }

        /// <summary>
        /// Xóa User theo Id
        /// </summary>
        public async Task DeleteAsync(string id)
        {
            var sql = "DELETE FROM Users WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}