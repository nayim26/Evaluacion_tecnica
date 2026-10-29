using UsuariosApi.Dtos;
using UsuariosApi.Models;

namespace UsuariosApi.Mapping;

public static class UsuarioMapper
{
    public static UsuarioResponse ToResponse(Usuario usuario) => new()
    {
        Id = checked((int)usuario.Id),
        Username = usuario.Username,
        Email = usuario.Email,
        Estado = usuario.Estado,
        FechaCreacion = DateTime.SpecifyKind(usuario.FechaCreacion, DateTimeKind.Utc)
    };
}
