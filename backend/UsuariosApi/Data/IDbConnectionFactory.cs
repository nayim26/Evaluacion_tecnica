using System.Data;

namespace UsuariosApi.Data;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
