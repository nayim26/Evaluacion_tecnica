using Microsoft.Data.Sqlite;

namespace UsuariosApi.Data;

public static class DatabaseInitializer
{
    public static DatabaseOptions Resolve(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration["Database:ConnectionString"] ?? "Data Source=data/usuarios.db";
        var builder = new SqliteConnectionStringBuilder(configured);

        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(environment.ContentRootPath, builder.DataSource));
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new DatabaseOptions
        {
            ConnectionString = builder.ToString(),
            DatabasePath = builder.DataSource,
            ScriptPath = FindScript(environment)
        };
    }

    public static void Apply(DatabaseOptions options, ILogger logger)
    {
        SQLitePCL.Batteries_V2.Init();
        var script = File.ReadAllText(options.ScriptPath);

        using var connection = new SqliteConnection(options.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = script;
        command.ExecuteNonQuery();

        logger.LogInformation("Base SQLite lista en {DatabasePath}", options.DatabasePath);
        logger.LogInformation("Script SQL aplicado desde {ScriptPath}", options.ScriptPath);
    }

    private static string FindScript(IWebHostEnvironment environment)
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "database", "usuarios.sql")),
            Path.Combine(AppContext.BaseDirectory, "Sql", "usuarios.sql"),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "Sql", "usuarios.sql"))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(
            "No se encontró el script database/usuarios.sql. Rutas revisadas: " + string.Join(" | ", candidates));
    }
}
