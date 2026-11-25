# Gimnasio API – Semana 14 (Autenticación JWT y Roles de Negocio)

Web API RESTful en capas desarrollada en **.NET 10 (C#)** con autenticación **JWT (JSON Web Token)** y autorización basada en roles específicos del negocio (`Gerente` y `Recepcionista`).

---

## 1. Matriz de Permisos (Endpoint × Rol)

| Endpoint | Método | Sin token (Público) | Rol `Recepcionista` | Rol `Gerente` | Protección / Tipo |
| :--- | :---: | :---: | :---: | :---: | :--- |
| `/api/auth/registro` | POST | Sí (201) | Sí (201) | Sí (201) | `[AllowAnonymous]` (Público) |
| `/api/auth/login` | POST | Sí (200) | Sí (200) | Sí (200) | `[AllowAnonymous]` (Público) |
| `/api/auth/perfil` | GET | No (401) | Sí (200) | Sí (200) | `[Authorize]` (Autenticado) |
| `/api/planes` | GET | Sí (200) | Sí (200) | Sí (200) | `[AllowAnonymous]` (Público) |
| `/api/planes/{id}` | GET | Sí (200) | Sí (200) | Sí (200) | `[AllowAnonymous]` (Público) |
| `/api/planes/{id}/socios` | GET | No (401) | Sí (200) | Sí (200) | `[Authorize]` (Autenticado) |
| `/api/planes` | POST | No (401) | No (403) | Sí (201) | `[Authorize(Roles = "Gerente")]` |
| `/api/planes/{id}` | PUT | No (401) | No (403) | Sí (204) | `[Authorize(Roles = "Gerente")]` |
| `/api/planes/{id}` | DELETE | No (401) | No (403) | Sí (204) | `[Authorize(Roles = "Gerente")]` |
| `/api/socios` | GET | No (401) | Sí (200) | Sí (200) | `[Authorize]` (Autenticado) |
| `/api/socios/{id}` | GET | No (401) | Sí (200) | Sí (200) | `[Authorize]` (Autenticado) |
| `/api/socios` | POST | No (401) | Sí (201) | Sí (201) | `[Authorize]` (Autenticado) |
| `/api/socios/{id}` | PUT | No (401) | Sí (204) | Sí (204) | `[Authorize]` (Autenticado) |
| `/api/socios/{id}` | DELETE | No (401) | No (403) | Sí (204) | `[Authorize(Roles = "Gerente")]` |

---

## 2. Códigos de Estado HTTP

| Resultado | Código | Dónde aparece |
| :--- | :---: | :--- |
| `Ok(datos)` | **200** | GET exitosos y login con credenciales válidas. |
| `StatusCode(201)` / `CreatedAtAction(...)` | **201** | POST exitosos (incluye registro de usuario y creación de planes/socios). |
| `NoContent()` | **204** | PUT y DELETE exitosos. |
| `ValidationProblem()` | **400** | Validaciones de formato y campos requeridos (`DataAnnotations` en DTOs). |
| `Unauthorized()` | **401** | Login con credenciales inválidas o solicitudes sin token / token expirado en endpoints protegidos. |
| `Forbid()` (automático de ASP.NET) | **403** | Token válido, pero el usuario no posee el rol requerido para la acción (ej. `Recepcionista` intentando crear un plan o eliminar un socio). |
| `NotFound()` | **404** | Recursos no encontrados por ID. |
| `Conflict(...)` | **409** | Reglas de negocio incumplidas (correo repetido, DNI duplicado, desactivar o eliminar plan con socios). |

---

## 3. Credenciales Iniciales de Prueba (Script SQL)

- **Gerente**: `gerente@gimnasio.com` / `Clave123!`
- **Recepcionista**: `recepcion@gimnasio.com` / `Clave123!`

*Al registrarse vía `/api/auth/registro`, el servidor asigna por defecto el rol `Recepcionista`. Para promover a `Gerente`, se ejecuta:*
```sql
UPDATE dbo.Usuarios SET Rol = 'Gerente' WHERE Correo = 'correo_registrado@ejemplo.com';
```
