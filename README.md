# Padrón de usuarios

Aplicación full stack para Windows: login + CRUD de usuarios.

| Capa | Tecnología |
|---|---|
| Backend | C# / .NET 8 — abrir `backend/Usuarios.sln` en Visual Studio |
| Frontend | Angular 22 — VS Code |
| Base de datos | SQL Server — un solo script en SSMS |

Demo: `admin` / `Admin123!`

---

## ¿Qué es `Usuarios.sln`?

Es el **archivo de solución de Visual Studio**. Agrupa el proyecto de la API (`UsuariosApi`).

En Windows: doble clic en `backend/Usuarios.sln` → Visual Studio → F5. Eso levanta la API en http://localhost:5087.

No es la base de datos. La base se crea con el script SQL (abajo).

---

## Contraseñas (importante)

En la pantalla **Nuevo usuario** escriba una contraseña normal, por ejemplo `Clave123`.

La API la **hashea sola** con BCrypt antes de guardarla.

El texto raro `$2b$11$...` que aparece en el script SQL es solo el hash ya preparado del usuario demo `admin` / `Admin123!`. **No lo copie al crear usuarios** desde la aplicación.

---

## Arranque en Windows

### 1. Base de datos (SSMS)

1. Abra SQL Server Management Studio.
2. Conéctese a su instancia (`localhost` o `.\SQLEXPRESS`).
3. Abra `database/UsuariosDb.sql` y pulse **Ejecutar (F5)**.

Ese archivo único crea:

- la base `UsuariosDb`
- la tabla `Usuarios`
- el usuario `admin` (contraseña `Admin123!`)

### 2. API (Visual Studio)

1. Abra `backend/Usuarios.sln`.
2. Revise la cadena en `UsuariosApi/appsettings.Development.json` si su servidor no es `localhost`:

```json
"Database": {
  "ConnectionString": "Server=localhost;Database=UsuariosDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

SQL Express suele ser `Server=.\\SQLEXPRESS;...`.  
Autenticación SQL: `User Id=sa;Password=SU_CLAVE;TrustServerCertificate=True;`.

3. F5. Swagger: http://localhost:5087/swagger

Al iniciar, la API vuelve a comprobar tabla y admin (por si la base ya existía vacía). La creación de la base la hace el script de SSMS.

### 3. Angular (VS Code)

```bash
cd frontend
npm install
npm start
```

UI: http://localhost:4317

---

## Endpoints

| Método | Ruta | Auth |
|---|---|---|
| POST | `/api/auth/login` | No |
| GET/POST | `/api/users` | Bearer |
| GET/PUT/DELETE | `/api/users/{id}` | Bearer |

## Notas

- El listado nunca muestra el hash de la contraseña.
- No puede eliminar ni desactivar la sesión con la que entró.
- Un usuario inactivo no puede iniciar sesión.
