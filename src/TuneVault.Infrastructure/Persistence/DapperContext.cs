using System.Data;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Persistence;

/// <summary>
/// Wrapper context cho Dapper, delegate việc tạo connection sang <see cref="IDbConnectionFactory"/>.
/// Giữ lại class này để tương thích với <see cref="UserRepository"/> hiện tại
/// mà không cần sửa đổi các file đã tạo.
/// </summary>
public class DapperContext
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Khởi tạo context với <see cref="IDbConnectionFactory"/> được inject qua DI.
    /// </summary>
    /// <param name="connectionFactory">Factory chịu trách nhiệm tạo kết nối đến SQL Server.</param>
    public DapperContext(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Tạo và trả về một <see cref="IDbConnection"/> mới.
    /// Delegate xuống <see cref="IDbConnectionFactory.CreateConnection()"/>.
    /// </summary>
    /// <returns>Một <see cref="IDbConnection"/> chưa được mở.</returns>
    public IDbConnection CreateConnection() => _connectionFactory.CreateConnection();
}