using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TuneVault.Infrastructure.Persistence;

/// <summary>
/// Quản lý chuỗi kết nối và khởi tạo IDbConnection cho Dapper.
/// </summary>
public class DapperContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Đọc chuỗi kết nối từ file appsettings.json với Key là "DefaultConnection"
        _connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối 'DefaultConnection' trong appsettings.json.");
    }

    /// <summary>
    /// Tạo và trả về một kết nối SQL Server mới.
    /// </summary>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}