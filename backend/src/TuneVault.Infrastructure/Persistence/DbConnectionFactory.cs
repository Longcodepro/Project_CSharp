using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace TuneVault.Infrastructure.Persistence;

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public DbConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_options.ConnectionString);
    }
}