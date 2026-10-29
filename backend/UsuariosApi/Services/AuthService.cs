using UsuariosApi.Data;
using UsuariosApi.Dtos;
using UsuariosApi.Exceptions;
using UsuariosApi.Security;

namespace UsuariosApi.Services;

public interface IAuthService
{
    LoginResponse Login(LoginRequest request);
}

public sealed class AuthService : IAuthService
{
    // Evita filtrar si el usuario existe midiendo tiempos de respuesta.
    private const string DummyHash = "$2b$11$t.yQEcD0njME56zriYN56eYXSrGEe4p6CaLSwXtBEjG.DkJoJp6tm";

    private readonly IUsuarioRepository _repository;
    private readonly IJwtTokenService _tokens;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUsuarioRepository repository, IJwtTokenService tokens, ILogger<AuthService> logger)
    {
        _repository = repository;
        _tokens = tokens;
        _logger = logger;
    }

    public LoginResponse Login(LoginRequest request)
    {
        var username = request.Username.Trim();
        var usuario = _repository.GetByUsername(username);
        var passwordOk = usuario is not null && PasswordHasher.Verify(request.Password, usuario.Password);

        if (usuario is null)
        {
            _ = PasswordHasher.Verify(request.Password, DummyHash);
        }

        if (usuario is null || !passwordOk)
        {
            _logger.LogWarning("Inicio de sesión rechazado para {Username}", username);
            throw new UnauthorizedException("Usuario o contraseña incorrectos.");
        }

        if (!usuario.Estado)
        {
            _logger.LogWarning("Inicio de sesión de una cuenta inactiva: {Username}", username);
            throw new ForbiddenException("El usuario está inactivo.");
        }

        var (token, expiresAt) = _tokens.Create(usuario);
        _logger.LogInformation("Inicio de sesión correcto para {Username}", usuario.Username);

        return new LoginResponse
        {
            Token = token,
            Id = checked((int)usuario.Id),
            Username = usuario.Username,
            Email = usuario.Email,
            ExpiresAt = expiresAt
        };
    }
}
