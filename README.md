# Padrón de usuarios

Aplicación full stack para la prueba técnica de desarrollador: login con contraseña cifrada y mantenimiento (CRUD) de usuarios.

- **Backend:** API REST en C# / .NET 8
- **Frontend:** Angular 22, formularios reactivos y guards
- **Base de datos:** SQLite (script SQL incluido). Hay un script equivalente para SQL Server

La API crea la base y aplica el script al iniciar. No hace falta instalar un servidor SQL para ejecutar el proyecto.

## Credenciales de demostración

| Usuario | Contraseña |
|---|---|
| `admin` | `Admin123!` |

## 1. Instalar los programas

Hace falta **.NET SDK 8** y **Node.js 20 o superior** (con npm).

### Ubuntu

```bash
# .NET SDK 8
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> ~/.bashrc
echo 'export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
dotnet --version

# Node.js: https://nodejs.org  (en esta máquina ya está Node 22)
node --version
npm --version
```

Opcional, solo si quiere aplicar el script a mano:

```bash
sudo apt-get install -y sqlite3
```

### Windows

1. Instale [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Instale [Node.js 22](https://nodejs.org).
3. Abra una terminal nueva y compruebe `dotnet --version` y `node --version`.

## 2. Ejecutar

Use dos terminales, desde la raíz del proyecto.

**API** (http://localhost:5087):

```bash
cd backend
dotnet run --project UsuariosApi --launch-profile http
```

Swagger: http://localhost:5087/swagger

**Angular** (http://localhost:4317):

```bash
cd frontend
npm install
npm start
```

El servidor de Angular reenvía `/api` hacia la API, así que el navegador habla con un solo origen.

## 3. Base de datos

Tabla `Usuarios`:

| Columna | Uso |
|---|---|
| Id | Entero, clave primaria |
| Username | Único |
| Email | Único |
| Password | Hash BCrypt, nunca texto plano |
| Estado | `1` activo, `0` inactivo |
| FechaCreacion | UTC |

| Archivo | Motor |
|---|---|
| `database/usuarios.sql` | SQLite. Lo ejecuta la API. |
| `database/usuarios.sqlserver.sql` | SQL Server, mismo modelo y el mismo usuario inicial. |

La base queda en `backend/UsuariosApi/data/usuarios.db`. Para recrearla, detenga la API, borre ese archivo y vuelva a arrancar.

Aplicarla a mano:

```bash
mkdir -p backend/UsuariosApi/data
sqlite3 backend/UsuariosApi/data/usuarios.db < database/usuarios.sql
```

## 4. Endpoints

| Método | Ruta | Auth | Respuesta |
|---|---|---|---|
| POST | `/api/auth/login` | No | 200 token, 400 datos inválidos, 401 credenciales, 403 inactivo |
| GET | `/api/users` | Bearer | 200 listado |
| GET | `/api/users/{id}` | Bearer | 200 o 404 |
| POST | `/api/users` | Bearer | 201, 400 o 409 si el usuario o el correo ya existen |
| PUT | `/api/users/{id}` | Bearer | 200. La contraseña es opcional. |
| DELETE | `/api/users/{id}` | Bearer | 204. No permite borrar la sesión actual. |

Ejemplo:

```bash
curl -s -X POST http://127.0.0.1:5087/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"username":"admin","password":"Admin123!"}'
```

El JSON usa camelCase (`username`, `fechaCreacion`). Las peticiones también aceptan PascalCase.

Hay una colección en `backend/UsuariosApi/UsuariosApi.http`.

## 5. Frontend

| Ruta | Acceso |
|---|---|
| `/login` | Pública. Si ya hay sesión, redirige al listado. |
| `/usuarios` | Protegida por `authGuard`. |
| `/usuarios/nuevo` | Alta con formulario reactivo. |
| `/usuarios/:id/editar` | Edición. La contraseña se deja en blanco para no cambiarla. |

El guard está en `frontend/src/app/guards/auth.guard.ts`. El token viaja en `Authorization: Bearer` gracias al interceptor. Si la API responde 401, la aplicación vuelve al login.

## 6. Seguridad incluida

- Contraseñas con BCrypt (factor 11).
- JWT con emisor, audiencia y vencimiento (8 horas).
- Consultas parametrizadas (Dapper), sin concatenar SQL.
- El listado y el detalle no devuelven el hash.
- No se puede eliminar ni desactivar al usuario de la sesión actual.
- Un usuario inactivo no puede entrar, aunque la contraseña sea correcta.
- Errores HTTP en JSON: 400, 401, 403, 404, 409 y 500, sin filtrar la excepción interna.

La clave JWT de desarrollo está en `backend/UsuariosApi/appsettings.json` (`Jwt:Key`). Cámbiela fuera de esta prueba. El token se guarda en `localStorage`; en un despliegue real convendría una cookie HttpOnly.

## 7. Estructura

```text
backend/UsuariosApi     API, controladores, servicios y repositorio
database                Scripts SQL
frontend/src/app        Login, listado, formulario, guard e interceptor
```


## Problemas frecuentes

- **No se pudo conectar con la API.** La API no está en el puerto 5087, o `npm start` no se lanzó desde `frontend`.
- **Usuario o contraseña incorrectos.** Use `admin` / `Admin123!`, o borre `usuarios.db` para regenerar la cuenta.
- **El puerto está ocupado.** Cierre el proceso anterior o cambie `applicationUrl` en `launchSettings.json` y el `target` de `frontend/proxy.conf.json` para que coincidan.
