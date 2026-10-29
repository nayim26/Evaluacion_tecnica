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
        return connection.QuerySingleOrDefault<Usuario>(
            """
            SELECT TOP 1 Id, Username, Email, Password, Estado, FechaCreacion
            FROM Usuarios
            WHERE LOWER(Username) = LOWER(@Username)
            """,
            new { Username = username });
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
        return connection.ExecuteScalar<long>(
            """
            INSERT INTO Usuarios (Username, Email, Password, Estado, FechaCreacion)
            VALUES (@Username, @Email, @Password, @Estado, @FechaCreacion);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """,
            ToParams(usuario));
    }

    public bool Update(Usuario usuario)
    {
        using var connection = _factory.Create();
        return connection.Execute(
            """
            UPDATE Usuarios
            SET Username = @Username,
                Email = @Email,
                Password = @Password,
                Estado = @Estado
            WHERE Id = @Id
            """,
            ToParams(usuario)) > 0;
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
        usuario.Estado,
        usuario.FechaCreacion
    };
}
