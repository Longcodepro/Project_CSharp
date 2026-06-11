// Đường dẫn: src/TuneVault.Infrastructure/Repositories/UserRepository.cs
using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Lớp triển khai các phương thức truy vấn và xử lý dữ liệu cho đối tượng Người dùng sử dụng Dapper ORM.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    /// <summary>
    /// Khởi tạo một thực thể mới của lớp <see cref="UserRepository"/> bằng cách inject DapperContext.
    /// </summary>
    /// <param name="context">Đối tượng quản lý kết nối cơ sở dữ liệu.</param>
    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tìm kiếm thông tin người dùng trong cơ sở dữ liệu bằng tên đăng nhập thông qua truy vấn SQL thuần của Dapper.
    /// </summary>
    /// <param name="username">Tên đăng nhập hoặc chuỗi định danh hiển thị (IdDisplay).</param>
    /// <returns>Thực thể Người dùng nếu tồn tại; ngược lại trả về <c>null</c>.</returns>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = "SELECT * FROM Users WHERE IdDisplay = @Username";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }

    /// <summary>
    /// Tìm kiếm thông tin người dùng bằng mã định danh hệ thống (Id) sử dụng CommandDefinition của Dapper để hỗ trợ CancellationToken.
    /// </summary>
    /// <param name="id">Mã ID hệ thống của người dùng cần tìm.</param>
    /// <param name="cancellationToken">Mã token hỗ trợ hủy tiến trình bất đồng bộ.</param>
    /// <returns>Thực thể Người dùng nếu tìm thấy; ngược lại trả về <c>null</c>.</returns>
    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Users WHERE Id = @Id";
        var cmd = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(cmd);
    }

    /// <summary>
    /// Tìm kiếm người dùng theo IdDisplay (handle công khai) sử dụng CommandDefinition hỗ trợ CancellationToken.
    /// </summary>
    /// <param name="idDisplay">Chuỗi handle công khai của người dùng (ví dụ: john_doe).</param>
    /// <param name="cancellationToken">Mã token hỗ trợ hủy tiến trình bất đồng bộ.</param>
    /// <returns>Thực thể Người dùng nếu tìm thấy; ngược lại trả về <c>null</c>.</returns>
    public async Task<User?> GetByIdDisplayAsync(string idDisplay, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Users WHERE IdDisplay = @IdDisplay";
        var cmd = new CommandDefinition(sql, new { IdDisplay = idDisplay }, cancellationToken: cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(cmd);
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng có trạng thái nghệ sĩ (IsArtist = true) và đang hoạt động (IsActive = true).
    /// </summary>
    /// <param name="cancellationToken">Mã token hỗ trợ hủy tiến trình bất đồng bộ.</param>
    /// <returns>Danh sách các thực thể Người dùng là nghệ sĩ đang hoạt động.</returns>
    public async Task<IEnumerable<User>> GetAllArtistsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Users WHERE IsArtist = 1 AND IsActive = 1";
        var cmd = new CommandDefinition(sql, cancellationToken: cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<User>(cmd);
    }
}