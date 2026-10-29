using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace UsuariosApi.Data;

public static class DatabaseInitializer
{
    public static DatabaseOptions Resolve(IConfiguration configuration)
    {
        var connectionString = configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException(
                "Configure Database:ConnectionString apuntando a SQL Server (UsuariosDb).");

        return new DatabaseOptions
        {
            ConnectionString = connectionString,
            ScriptPath = FindScript()
        };
    }

    public static void Apply(DatabaseOptions options, ILogger logger)
    {
        var script = File.ReadAllText(options.ScriptPath);
        var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(batch => batch.Trim())
            .Where(batch => batch.Length > 0)
            .Where(batch => !batch.Contains("CREATE DATABASE", StringComparison.OrdinalIgnoreCase))
            .Where(batch => !batch.StartsWith("USE ", StringComparison.OrdinalIgnoreCase));

        using var connection = new SqlConnection(options.ConnectionString);
        try
        {
            connection.Open();
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                "No se pudo conectar a SQL Server. Ejecute database/UsuariosDb.sql en SSMS " +
                "y revise Database:ConnectionString en appsettings.",
                ex);
        }

        foreach (var batch in batches)
        {
            using var command = connection.CreateCommand();
            command.CommandText = batch;
            command.ExecuteNonQuery();
        }

        logger.LogInformation("SQL Server listo. Script: {Script}", options.ScriptPath);
    }

    private static string FindScript()
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "database", "UsuariosDb.sql")),
            Path.Combine(AppContext.BaseDirectory, "Sql", "UsuariosDb.sql"),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "database", "UsuariosDb.sql")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "database", "UsuariosDb.sql"))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(
            "No se encontró database/UsuariosDb.sql. Rutas: " + string.Join(" | ", candidates));
    }
}
