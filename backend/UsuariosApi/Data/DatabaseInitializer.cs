using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace UsuariosApi.Data;

public static class DatabaseInitializer
{
    public static DatabaseOptions Resolve(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var providerName = configuration["Database:Provider"] ?? "Sqlite";
        var provider = providerName.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
            ? DatabaseProvider.SqlServer
            : DatabaseProvider.Sqlite;

        var connectionString = configuration["Database:ConnectionString"]
            ?? (provider == DatabaseProvider.Sqlite
                ? "Data Source=data/usuarios.db"
                : throw new InvalidOperationException("Configure Database:ConnectionString para SQL Server."));

        if (provider == DatabaseProvider.Sqlite)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
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
                Provider = provider,
                ConnectionString = builder.ToString(),
                DatabasePath = builder.DataSource,
                ScriptPath = FindScript(environment, "usuarios.sql")
            };
        }

        return new DatabaseOptions
        {
            Provider = provider,
            ConnectionString = connectionString,
            ScriptPath = FindScript(environment, "usuarios.sqlserver.sql")
        };
    }

    public static void Apply(DatabaseOptions options, ILogger logger)
    {
        if (options.Provider == DatabaseProvider.Sqlite)
        {
            ApplySqlite(options, logger);
            return;
        }

        ApplySqlServer(options, logger);
    }

    private static void ApplySqlite(DatabaseOptions options, ILogger logger)
    {
        SQLitePCL.Batteries_V2.Init();
        var script = File.ReadAllText(options.ScriptPath);

        using var connection = new SqliteConnection(options.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = script;
        command.ExecuteNonQuery();

        logger.LogInformation("SQLite lista en {Path}", options.DatabasePath);
    }

    private static void ApplySqlServer(DatabaseOptions options, ILogger logger)
    {
        var script = File.ReadAllText(options.ScriptPath);
        var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(batch => batch.Trim())
            .Where(batch => batch.Length > 0);

        using var connection = new SqlConnection(options.ConnectionString);
        connection.Open();

        foreach (var batch in batches)
        {
            using var command = connection.CreateCommand();
            command.CommandText = batch;
            command.ExecuteNonQuery();
        }

        logger.LogInformation("SQL Server inicializado con {Script}", options.ScriptPath);
    }

    private static string FindScript(IWebHostEnvironment environment, string fileName)
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "database", fileName)),
            Path.Combine(AppContext.BaseDirectory, "Sql", fileName),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "Sql", fileName))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"No se encontró database/{fileName}. Revisado: {string.Join(" | ", candidates)}");
    }
}
