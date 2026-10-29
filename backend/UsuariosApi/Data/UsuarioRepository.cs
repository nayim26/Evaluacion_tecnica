using Dapper;
using UsuariosApi.Models;

namespace UsuariosApi.Data;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _factory;

    public UsuarioRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IReadOnlyList<Usuario> GetAll()
    {
        using var connection = _factory.Create();
        return connection.Query<Usuario>(
            """
            SELECT Id, Username, Email, '' AS Password, Estado, FechaCreacion
            FROM Usuarios
            ORDER BY Id DESC;
            """).ToList();
    }

    public Usuario? GetById(long id)
    {
        using var connection = _factory.Create();
        return connection.QuerySingleOrDefault<Usuario>(
            """
            SELECT Id, Username, Email, Password, Estado, FechaCreacion
            FROM Usuarios
            WHERE Id = @Id;
            """,
            new { Id = id });
    }

    public Usuario? GetByUsername(string username)
    {
        using var connection = _factory.Create();
        return connection.QuerySingleOrDefault<Usuario>(
            """
            SELECT Id, Username, Email, Password, Estado, FechaCreacion
            FROM Usuarios
            WHERE lower(Username) = lower(@Username)
            LIMIT 1;
            """,
            new { Username = username });
    }

    public bool UsernameExists(string username, long? excludeId = null)
    {
        using var connection = _factory.Create();
        var count = connection.ExecuteScalar<long>(
            """
            SELECT COUNT(1)
            FROM Usuarios
            WHERE lower(Username) = lower(@Username)
              AND (@ExcludeId IS NULL OR Id <> @ExcludeId);
            """,
            new { Username = username, ExcludeId = excludeId });
        return count > 0;
    }

    public bool EmailExists(string email, long? excludeId = null)
    {
        using var connection = _factory.Create();
        var count = connection.ExecuteScalar<long>(
            """
            SELECT COUNT(1)
            FROM Usuarios
            WHERE lower(Email) = lower(@Email)
              AND (@ExcludeId IS NULL OR Id <> @ExcludeId);
            """,
            new { Email = email, ExcludeId = excludeId });
        return count > 0;
    }

    public long Create(Usuario usuario)
    {
        using var connection = _factory.Create();
        connection.Execute(
            """
            INSERT INTO Usuarios (Username, Email, Password, Estado, FechaCreacion)
            VALUES (@Username, @Email, @Password, @Estado, @FechaCreacion);
            """,
            usuario);
        return connection.ExecuteScalar<long>("SELECT last_insert_rowid();");
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
            WHERE Id = @Id;
            """,
            usuario);
        return rows > 0;
    }

    public bool Delete(long id)
    {
        using var connection = _factory.Create();
        var rows = connection.Execute("DELETE FROM Usuarios WHERE Id = @Id;", new { Id = id });
        return rows > 0;
    }
}
