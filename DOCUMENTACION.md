# Documentación del sistema de gestión de usuarios

Sistema web para iniciar sesión y mantener un padrón de usuarios: creación, edición, listado y eliminación. Tiene tres partes que trabajan juntas: una vista en Angular, una API REST en ASP.NET Core y una base SQL Server.

## Alcance

Incluye:

- Login con token JWT.
- CRUD de usuarios protegido por sesión.
- Contraseñas guardadas con hash BCrypt.
- Base `UsuariosDb` y el script para crearla.
- Integración del frontend con la API.


## Tecnologías

| Parte | Tecnología |
|---|---|
| Vista | Angular 22, componentes standalone, formularios reactivos, SweetAlert2 |
| API | ASP.NET Core 8 (`net8.0`), Dapper, JWT HMAC-SHA256 |
| Base | SQL Server Express (`localhost\SQLEXPRESS`), base `UsuariosDb` |
| Contraseñas | BCrypt, factor de trabajo 11 |

Puertos de desarrollo:

- Vista: `http://localhost:4317`
- API: `http://localhost:5087`
- Swagger (solo en Development): `http://localhost:5087/swagger`

## Ejecución

Hace falta .NET 8 (runtime de ASP.NET y runtime base), Node.js, y SQL Server Express con la instancia `SQLEXPRESS`.

1. En SSMS, conectarse a `localhost\SQLEXPRESS` con autenticación de Windows. Cifrar la conexión y confiar en el certificado del servidor. Abrir `database/UsuariosDb.sql` y ejecutarlo (F5). El script crea la base, la tabla y el usuario inicial si no existen.
2. Abrir `backend/Usuarios.sln` en Visual Studio y pulsar F5. El perfil `http` levanta la API en el puerto 5087.
3. En `frontend`:

```bash
npm install
npm start
```

4. Abrir `http://localhost:4317`.

Cuenta inicial que crea el script: usuario `admin`, contraseña `Admin123!`.

La cadena de conexión está en `backend/UsuariosApi/appsettings.json` y en `appsettings.Development.json`:

```text
Server=localhost\SQLEXPRESS;Database=UsuariosDb;Trusted_Connection=True;TrustServerCertificate=True;
```

En Development, el archivo de Development reemplaza la cadena del archivo base. Al arrancar, la API comprueba que existan la tabla y el usuario `admin`. No crea la base: eso lo hace el script en SSMS.

## Estructura

```text
Evaluacion_tecnica
├── database/UsuariosDb.sql          Script de la base
├── backend/UsuariosApi
│   ├── Controllers                   Entrada HTTP
│   ├── Services                      Reglas de negocio
│   ├── Data                          SQL con Dapper
│   ├── Dtos                          Contratos de entrada y salida
│   ├── Security                      Hash y JWT
│   ├── Middleware                    Errores HTTP
│   └── Mapping                       Modelo interno → respuesta
└── frontend/src/app
    ├── pages/login                   Inicio de sesión
    ├── pages/users                   Listado y formulario
    ├── layout                        Barra superior
    ├── services                      Llamadas HTTP
    ├── guards                        Protección de rutas
    ├── interceptors                  Token en cada petición
    └── core                          Sesión, modelos y alertas
```

La API separa responsabilidades: el controlador recibe la petición, el servicio aplica las reglas y el repositorio ejecuta SQL parametrizado. El frontend no habla con SQL Server; solo llama a `/api`.

## Base de datos

Tabla `dbo.Usuarios`:

| Columna | Tipo | Regla |
|---|---|---|
| Id | INT IDENTITY | Clave primaria |
| Username | NVARCHAR(50) | Único, sin distinguir mayúsculas |
| Email | NVARCHAR(150) | Único, sin distinguir mayúsculas |
| Password | NVARCHAR(255) | Hash BCrypt, nunca la clave en texto |
| Estado | BIT | 1 activo, 0 inactivo. Por defecto activo |
| FechaCreacion | DATETIME2 | UTC. Por defecto la fecha del servidor |

## API

### Endpoints

| Método | Ruta | Sesión | Resultado |
|---|---|---|---|
| POST | `/api/auth/login` | No | 200 y token |
| GET | `/api/users` | Sí | Lista, sin hash |
| GET | `/api/users/{id}` | Sí | Un usuario, o 404 |
| POST | `/api/users` | Sí | 201 y el usuario creado |
| PUT | `/api/users/{id}` | Sí | Usuario actualizado |
| DELETE | `/api/users/{id}` | Sí | 204 sin cuerpo |

Las rutas de usuarios exigen `Authorization: Bearer {token}`.

### Login

Cuerpo:

```json
{ "username": "admin", "password": "Admin123!" }
```

Respuesta:

```json
{
  "token": "...",
  "id": 1,
  "username": "admin",
  "email": "admin@demo.local",
  "expiresAt": "2026-10-30T02:00:00Z"
}
```

El token dura 480 minutos. Incluye el id (`sub`), el usuario (`unique_name`) y el correo (`email`). Emisor: `UsuariosApi`. Audiencia: `UsuariosWeb`. La clave de firma está en `Jwt:Key` y debe tener al menos 32 bytes.

Si el usuario no existe, la contraseña no coincide o la cuenta está inactiva, la API responde **401** con el mismo texto: `Usuario o contraseña incorrectos.` Así no se revela si la cuenta existe o si está inactiva. El intento inactivo sí queda en el log del servidor. Cuando el usuario no existe, se compara contra un hash fijo para que el tiempo de respuesta no delate la ausencia de la cuenta.

### Usuario

Creación:

```json
{
  "username": "ana.rios",
  "email": "ana.rios@empresa.com",
  "password": "Clave123",
  "estado": true
}
```

Edición: los mismos campos. `password` es opcional; si viene vacío, se conserva el hash anterior.

La respuesta nunca incluye la contraseña:

```json
{
  "id": 2,
  "username": "ana.rios",
  "email": "ana.rios@empresa.com",
  "estado": true,
  "fechaCreacion": "2026-10-29T22:00:00Z"
}
```

El listado ordena por `Id` descendente y sustituye el hash por una cadena vacía antes de mapear la respuesta.

### Validación

| Campo | Regla |
|---|---|
| Usuario | Obligatorio, 3 a 50 caracteres, solo letras, números, punto, guion y guion bajo |
| Correo | Obligatorio, máximo 150, con dominio real (`nombre@empresa.com`). Rechaza `ana@localhost` |
| Contraseña | Obligatoria en el alta, 6 a 100 caracteres. En la edición, solo si se envía una nueva |
| Estado | Booleano. Por defecto activo |

### Errores

El middleware traduce las excepciones de negocio a JSON `{ "message": "...", "errors": ["..."] }`. `errors` solo aparece cuando hay varios mensajes de validación.

| Código | Cuándo |
|---|---|
| 400 | Datos inválidos, o intento de borrar o desactivar la propia sesión |
| 401 | Login fallido, token ausente, token vencido o sesión inválida |
| 404 | El id no existe |
| 409 | Usuario o correo ya registrados. También si SQL devuelve los códigos 2627 o 2601 |
| 500 | Error no previsto |

## Vista

### Rutas

| Ruta | Quién entra | Pantalla |
|---|---|---|
| `/login` | Quien no tiene sesión (`guestGuard`) | Inicio de sesión |
| `/usuarios` | Sesión activa (`authGuard`) | Listado |
| `/usuarios/nuevo` | Sesión activa | Alta |
| `/usuarios/:id/editar` | Sesión activa | Edición |

Si la sesión falta o ya venció, cualquier ruta interna vuelve a `/login`. Si ya hay sesión, `/login` redirige al listado.

### Sesión

El login guarda la respuesta en `localStorage` con la clave `padron.session`. El interceptor añade el token a cada llamada que no sea el propio login. Un 401 distinto del login borra la sesión y vuelve a `/login`.

«Recordarme» guarda solo el nombre de usuario en `padron.rememberUsername`. No guarda la contraseña.

### Vistas

- **Login.** Título «Gestor de usuarios», tarjeta con usuario, contraseña y «Recordarme». Si faltan datos o las credenciales no sirven, el texto aparece arriba de los campos y un signo de exclamación queda dentro de cada input afectado.
- **Listado.** Tabla con id, usuario, correo, estado, fecha y acciones. Eliminar pide confirmación con SweetAlert. No se puede eliminar la cuenta con la que se inició sesión.
- **Formulario.** Alta y edición en la misma pantalla. Valida usuario, correo con dominio, longitud de contraseña y que ambas contraseñas coincidan. El éxito y el error se muestran con SweetAlert. Un fallo al cargar el listado se queda como aviso en la página, con botón «Reintentar».

### Integración

En desarrollo, `environment.apiBaseUrl` está vacío. El navegador llama a `/api` en el puerto 4317 y `frontend/proxy.conf.json` reenvía esas rutas a `http://127.0.0.1:5087`. La API, además, permite CORS desde `http://localhost:4317` y `http://127.0.0.1:4317`.

## Reglas de negocio

- Usuario y correo son únicos, sin distinguir mayúsculas.
- La contraseña solo se guarda como hash.
- Una cuenta inactiva no entra, y el mensaje es el mismo que el de una contraseña incorrecta.
- Nadie puede eliminarse ni desactivarse a sí mismo.
- El listado y el detalle no devuelven el hash.
- Las consultas usan parámetros de Dapper. El texto del usuario no se concatena al SQL.

## Recorrido de un login

1. El formulario envía usuario y contraseña a `POST /api/auth/login`.
2. La API busca el usuario, verifica el hash y el estado.
3. Si todo es correcto, firma un JWT y lo devuelve.
4. Angular guarda la sesión y abre `/usuarios`.
5. El listado llama a `GET /api/users` con el token.
6. Al salir, se borra `padron.session` y se vuelve a `/login`.
