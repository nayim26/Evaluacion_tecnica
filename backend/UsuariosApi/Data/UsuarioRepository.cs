using Dapper;
using UsuariosApi.Models;

namespace UsuariosApi.Data;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _factory;
    private readonly DatabaseOptions _options;

    public UsuarioRepository(IDbConnectionFactory factory, DatabaseOptions options)
    {
        _factory = factory;
        _options = options;
    }

    public IReadOnlyList<Usuario> GetAll()
    {
        using var connection = _factory.Create();
        return connection.Query<Usuario>(
            """
            SELECT Id, Username, Email, '' AS Password, Estado, FechaCreacion
            FROM Usuarios
            ORDER BY Id DESC
            """).ToList();
    }

    public Usuario? GetById(long id)
    {
        using var connection = _factory.Create();
        return connection.QuerySingleOrDefault<Usuario>(
            """
            SELECT Id, Username, Email, Password, Estado, FechaCreacion
            FROM Usuarios
            WHERE Id = @Id
            """,
            new { Id = id });
    }

    public Usuario? GetByUsername(string username)
    {
        using var connection = _factory.Create();
        var sql = _options.Provider == DatabaseProvider.SqlServer
            ? """
              SELECT TOP 1 Id, Username, Email, Password, Estado, FechaCreacion
              FROM Usuarios
              WHERE LOWER(Username) = LOWER(@Username)
              """
            : """
              SELECT Id, Username, Email, Password, Estado, FechaCreacion
              FROM Usuarios
              WHERE lower(Username) = lower(@Username)
              LIMIT 1
              """;

        return connection.QuerySingleOrDefault<Usuario>(sql, new { Username = username });
    }

    public bool UsernameExists(string username, long? excludeId = null)
    {
        using var connection = _factory.Create();
        return connection.ExecuteScalar<long>(
            """
            SELECT COUNT(1)
            FROM Usuarios
            WHERE LOWER(Username) = LOWER(@Username)
              AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            """,
            new { Username = username, ExcludeId = excludeId }) > 0;
    }

    public bool EmailExists(string email, long? excludeId = null)
    {
        using var connection = _factory.Create();
        return connection.ExecuteScalar<long>(
            """
            SELECT COUNT(1)
            FROM Usuarios
            WHERE LOWER(Email) = LOWER(@Email)
              AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            """,
            new { Email = email, ExcludeId = excludeId }) > 0;
    }

    public long Create(Usuario usuario)
    {
        using var connection = _factory.Create();

        if (_options.Provider == DatabaseProvider.SqlServer)
        {
            return connection.ExecuteScalar<long>(
                """
                INSERT INTO Usuarios (Username, Email, Password, Estado, FechaCreacion)
                VALUES (@Username, @Email, @Password, @Estado, @FechaCreacion);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                """,
                ToParams(usuario));
        }

        connection.Execute(
            """
            INSERT INTO Usuarios (Username, Email, Password, Estado, FechaCreacion)
            VALUES (@Username, @Email, @Password, @Estado, @FechaCreacion)
            """,
            ToParams(usuario));

        return connection.ExecuteScalar<long>("SELECT last_insert_rowid()");
    }

    public bool Update(Usuario usuario)
    {
        using var connection = _factory.Create();
        var rows = connection.Execute(
            """
            UPDATE Usuarios
            SET Username = @Username,
                Email = @Email,
                Password = @Password,
                Estado = @Estado
            WHERE Id = @Id
            """,
            ToParams(usuario));
        return rows > 0;
    }

    public bool Delete(long id)
    {
        using var connection = _factory.Create();
        return connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id }) > 0;
    }

    private static object ToParams(Usuario usuario) => new
    {
        usuario.Id,
        usuario.Username,
        usuario.Email,
        usuario.Password,
        Estado = usuario.Estado,
        FechaCreacion = usuario.FechaCreacion
    };
}
