using System.Data;

namespace TuneVault.Infrastructure.Persistence;

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        throw new NotImplementedException("Configure the real SQL connection here.");
    }
}
