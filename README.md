# Padrón de usuarios

Login y mantenimiento (CRUD) de usuarios para Windows.

| Capa | Cómo lo abro |
|---|---|
| API | `backend/Usuarios.sln` en Visual Studio → F5 → http://localhost:5087 |
| Pantalla | `frontend` en VS Code → `npm install` / `npm start` → http://localhost:4317 |
| Base | `database/UsuariosDb.sql` en SSMS → F5 |

Demo: `admin` / `Admin123!`

## Qué es cada cosa

- **`Usuarios.sln`**: solución de Visual Studio. Solo la API. No es la base.
- **`UsuariosDb.sql`**: un solo script. Crea la base, la tabla y el admin.
- **Contraseña en la UI**: escribo texto normal (`Clave123`). La API hashea con BCrypt. El `$2b$11$...` del script es solo el hash del admin de demo; no lo pego al crear usuarios.
- **`UsuariosApi.http`**: pruebas de la API sin Angular. Primero Login; el token se reutiliza solo.

## Arranque

1. SSMS → abrir `database/UsuariosDb.sql` → Ejecutar.
2. Visual Studio → `backend/Usuarios.sln` → F5.  
   Cadena por defecto: `Server=localhost;Database=UsuariosDb;Trusted_Connection=True;TrustServerCertificate=True;`  
   (Express: `Server=.\\SQLEXPRESS;...`)
3. VS Code:

```bash
cd frontend
npm install
npm start
```

Al arrancar, la API revisa tabla y admin. La base la creo yo con el script.

## Endpoints

| Método | Ruta | Token |
|---|---|---|
| POST | `/api/auth/login` | No |
| GET / POST | `/api/users` | Sí |
| GET / PUT / DELETE | `/api/users/{id}` | Sí |

## Reglas que dejé

- Usuario y correo únicos.
- Inactivo no entra.
- No me puedo borrar ni desactivar a mí mismo.
- El listado no devuelve el hash.
- SQL con parámetros (Dapper): sin concatenar input en la consulta.
