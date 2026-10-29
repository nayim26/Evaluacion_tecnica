namespace UsuariosApi.Data;

public sealed class DatabaseOptions
{
    public required DatabaseProvider Provider { get; init; }
    public required string ConnectionString { get; init; }
    public required string ScriptPath { get; init; }
    public string? DatabasePath { get; init; }
}
