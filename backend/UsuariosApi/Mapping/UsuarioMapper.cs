using System.Globalization;
using UsuariosApi.Dtos;
using UsuariosApi.Models;

namespace UsuariosApi.Mapping;

public static class UsuarioMapper
{
    public static UsuarioResponse ToResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = checked((int)usuario.Id),
            Username = usuario.Username,
            Email = usuario.Email,
            Estado = usuario.Estado == 1,
            FechaCreacion = ParseFecha(usuario.FechaCreacion)
        };
    }

    private static DateTime ParseFecha(string value)
    {
        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        throw new FormatException($"FechaCreacion inválida: {value}");
    }
}
