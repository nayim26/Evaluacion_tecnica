using System.Data;
using Microsoft.Data.Sqlite;

namespace UsuariosApi.Data;

public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public SqliteConnectionFactory(DatabaseOptions options)
    {
        _options = options;
    }

    public IDbConnection Create()
    {
        var connection = new SqliteConnection(_options.ConnectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
