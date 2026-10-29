using UsuariosApi.Models;

namespace UsuariosApi.Data;

public interface IUsuarioRepository
{
    IReadOnlyList<Usuario> GetAll();
    Usuario? GetById(long id);
    Usuario? GetByUsername(string username);
    bool UsernameExists(string username, long? excludeId = null);
    bool EmailExists(string email, long? excludeId = null);
    long Create(Usuario usuario);
    bool Update(Usuario usuario);
    bool Delete(long id);
}
