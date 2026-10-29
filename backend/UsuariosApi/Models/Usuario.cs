namespace UsuariosApi.Models;

public sealed class Usuario
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public long Estado { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
}
