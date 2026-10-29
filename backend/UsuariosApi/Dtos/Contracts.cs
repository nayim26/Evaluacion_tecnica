using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UsuariosApi.Dtos;

public sealed class ErrorResponse
{
    public string Message { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Errors { get; set; }
}

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El usuario no puede superar 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public sealed class UsuarioResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public sealed class UsuarioCreateRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El usuario solo puede contener letras, números, punto, guion y guion bajo.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}$", ErrorMessage = "El correo debe incluir un dominio válido, por ejemplo nombre@empresa.com.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;

    public bool Estado { get; set; } = true;
}

public sealed class UsuarioUpdateRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El usuario solo puede contener letras, números, punto, guion y guion bajo.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}$", ErrorMessage = "El correo debe incluir un dominio válido, por ejemplo nombre@empresa.com.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string? Password { get; set; }

    public bool Estado { get; set; } = true;
}
