namespace UsuariosApi.Data;

public sealed class DatabaseOptions
{
    public required string ConnectionString { get; init; }
    public required string ScriptPath { get; init; }
}
