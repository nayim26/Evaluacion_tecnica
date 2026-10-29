# Padrón de usuarios

Aplicación full stack: login con contraseña cifrada (BCrypt) y CRUD de usuarios.

| Capa | Stack |
|---|---|
| Backend | C# / .NET 8 (ASP.NET Core, JWT, Dapper) |
| Frontend | Angular 22 (formularios reactivos, guards) |
| Base de datos | **SQL Server** en Windows · SQLite en desarrollo local |

Demo: `admin` / `Admin123!`

---

## Windows (Visual Studio + SQL Server + VS Code)

### 1. Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (Express o superior) + SSMS opcional
- [Node.js 20+](https://nodejs.org)
- Visual Studio 2022 (backend) y VS Code (Angular)

### 2. Base de datos

1. Cree la base `UsuariosDb` en SQL Server.
2. Abra `backend/UsuariosApi/appsettings.Development.json` (o use el perfil Production) y configure:

```json
"Database": {
  "Provider": "SqlServer",
  "ConnectionString": "Server=localhost;Database=UsuariosDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Si usa autenticación SQL:

```text
Server=localhost;Database=UsuariosDb;User Id=sa;Password=SU_CLAVE;TrustServerCertificate=True;
```

Al arrancar, la API aplica `database/usuarios.sqlserver.sql` (crea tabla + usuario admin si no existe). También puede ejecutarlo a mano en SSMS.

### 3. API (Visual Studio)

1. Abra `backend/Usuarios.sln`.
2. Perfil de arranque: `http` → `http://localhost:5087`.
3. F5 / ejecutar.

Swagger (solo Development): http://localhost:5087/swagger

Antes de producción, cambie `Jwt:Key` en `appsettings.Production.json`.

### 4. Angular (VS Code)

```bash
cd frontend
npm install
npm start
```

UI: http://localhost:4317  
El proxy reenvía `/api` a `http://127.0.0.1:5087`.

Build de producción:

```bash
npm run build
```

Salida en `frontend/dist/frontend/browser` (archivos estáticos listos para IIS o cualquier servidor web).

Si Angular y la API no comparten origen, edite `src/environments/environment.production.ts` (`apiBaseUrl`).

---

## Desarrollo en Linux / sin SQL Server

Por defecto usa SQLite (`Database:Provider = Sqlite`). No hace falta instalar motor SQL.

```bash
cd backend
dotnet run --project UsuariosApi --launch-profile http

cd frontend
npm install && npm start
```

---

## Endpoints

| Método | Ruta | Auth |
|---|---|---|
| POST | `/api/auth/login` | No |
| GET | `/api/users` | Bearer |
| GET | `/api/users/{id}` | Bearer |
| POST | `/api/users` | Bearer |
| PUT | `/api/users/{id}` | Bearer |
| DELETE | `/api/users/{id}` | Bearer |

---

## Estructura

```text
backend/UsuariosApi   API REST
database              Scripts SQLite y SQL Server
frontend              Angular
```

## Notas

- Contraseñas: BCrypt. El listado no devuelve el hash.
- No se puede eliminar ni desactivar la sesión actual.
- Un usuario inactivo no puede iniciar sesión.
