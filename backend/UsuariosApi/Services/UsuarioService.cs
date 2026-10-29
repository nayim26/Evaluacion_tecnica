using UsuariosApi.Data;
using UsuariosApi.Dtos;
using UsuariosApi.Exceptions;
using UsuariosApi.Mapping;
using UsuariosApi.Models;
using UsuariosApi.Security;

namespace UsuariosApi.Services;

public interface IUsuarioService
{
    IReadOnlyList<UsuarioResponse> GetAll();
    UsuarioResponse GetById(long id);
    UsuarioResponse Create(UsuarioCreateRequest request);
    UsuarioResponse Update(long id, UsuarioUpdateRequest request, long currentUserId);
    void Delete(long id, long currentUserId);
}

public sealed class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<UsuarioResponse> GetAll()
    {
        return _repository.GetAll().Select(UsuarioMapper.ToResponse).ToList();
    }

    public UsuarioResponse GetById(long id)
    {
        var usuario = _repository.GetById(id) ?? throw new NotFoundException("Usuario no encontrado.");
        return UsuarioMapper.ToResponse(usuario);
    }

    public UsuarioResponse Create(UsuarioCreateRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        EnsurePassword(request.Password);

        if (_repository.UsernameExists(username))
        {
            throw new ConflictException("El nombre de usuario ya está registrado.");
        }

        if (_repository.EmailExists(email))
        {
            throw new ConflictException("El correo ya está registrado.");
        }

        var id = _repository.Create(new Usuario
        {
            Username = username,
            Email = email,
            Password = PasswordHasher.Hash(request.Password),
            Estado = request.Estado,
            FechaCreacion = DateTime.UtcNow
        });

        return GetById(id);
    }

    public UsuarioResponse Update(long id, UsuarioUpdateRequest request, long currentUserId)
    {
        var existing = _repository.GetById(id) ?? throw new NotFoundException("Usuario no encontrado.");
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (existing.Id == currentUserId && !request.Estado)
        {
            throw new BadRequestException("No puede desactivar el usuario con el que inició sesión.");
        }

        if (_repository.UsernameExists(username, existing.Id))
        {
            throw new ConflictException("El nombre de usuario ya está registrado.");
        }

        if (_repository.EmailExists(email, existing.Id))
        {
            throw new ConflictException("El correo ya está registrado.");
        }

        var password = existing.Password;
        if (!string.IsNullOrEmpty(request.Password))
        {
            EnsurePassword(request.Password);
            password = PasswordHasher.Hash(request.Password);
        }

        existing.Username = username;
        existing.Email = email;
        existing.Password = password;
        existing.Estado = request.Estado;

        if (!_repository.Update(existing))
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        return GetById(id);
    }

    public void Delete(long id, long currentUserId)
    {
        var existing = _repository.GetById(id) ?? throw new NotFoundException("Usuario no encontrado.");
        if (existing.Id == currentUserId)
        {
            throw new BadRequestException("No puede eliminar el usuario con el que inició sesión.");
        }

        if (!_repository.Delete(id))
        {
            throw new NotFoundException("Usuario no encontrado.");
        }
    }

    private static void EnsurePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || password.Length > 100)
        {
            throw new BadRequestException("La contraseña debe tener entre 6 y 100 caracteres.");
        }
    }
}
