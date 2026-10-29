namespace UsuariosApi.Models;

public sealed class Usuario
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
