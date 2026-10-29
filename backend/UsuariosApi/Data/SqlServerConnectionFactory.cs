using System.Data;
using Microsoft.Data.SqlClient;

namespace UsuariosApi.Data;

public sealed class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public SqlServerConnectionFactory(DatabaseOptions options)
    {
        _options = options;
    }

    public IDbConnection Create()
    {
        var connection = new SqlConnection(_options.ConnectionString);
        connection.Open();
        return connection;
    }
}
