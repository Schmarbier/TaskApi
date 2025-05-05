# TaskApi (.NET 8 + EF Core + JWT + Serilog)

Este proyecto es una API con autenticación JWT, control de errores y trazabilidad completa mediante Serilog y almacenamiento en archivo y base de datos SQL Server.

---

## ⚙️ Requisitos

- .NET 8 SDK
- SQL Server (local o remoto)
- Visual Studio 2022 o VS Code (opcional)
- Docker (opcional para levantar servicios como Seq si lo desean implementar)

---

## 🧪 Setup local

### 1. Cloná el repositorio

```bash
git clone https://turepo.com/task-api.git
cd task-api
````

### 2. Restaurar paquetes

```bash
dotnet restore
```

---

## ⚠️ Configuración requerida

El proyecto **usa variables de entorno** o `user-secrets` para evitar subir configuraciones sensibles.

### 🛠️ Variables a configurar

| Clave de configuración                        | Descripción                                | Ejemplo                                                              |
| --------------------------------------------- | ------------------------------------------ | -------------------------------------------------------------------- |
| `ConnectionStrings__DefaultConnection`        | Cadena de conexión a la base               | `Server=localhost;Database=ApiTestIdentity;Trusted_Connection=True;` |
| `Jwt__Key`                                    | Clave secreta del token JWT                | `una_clave_secreta_1234567890`                                       |
| `Jwt__Issuer`                                 | Emisor del token                           | `TaskApi`                                                            |
| `Jwt__Audience`                               | Audiencia esperada                         | `TaskApiUsers`                                                       |
| `Serilog__WriteTo__0__Args__path`             | Ruta del archivo de log                    | `Logs/log-.txt`                                                      |
| `Serilog__WriteTo__1__Args__connectionString` | Cadena de conexión para logs en SQL Server | `Server=localhost;Database=ApiTestIdentity;Trusted_Connection=True;`                                                     |

---

## 🔐 Configuración con `dotnet user-secrets` (si se desea)

```bash
dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ApiTestIdentity;Trusted_Connection=True;"
dotnet user-secrets set "Jwt:Key" "una_clave_supersecreta"
dotnet user-secrets set "Jwt:Issuer" "TaskApi"
dotnet user-secrets set "Jwt:Audience" "TaskApiUsers"
dotnet user-secrets set "Serilog:WriteTo:0:Args:path" "Logs/log-.txt"
dotnet user-secrets set "Serilog:WriteTo:1:Args:connectionString" "Server=localhost;Database=ApiTestIdentity;Trusted_Connection=True;"
```

> 📌 Ojo, `user-secrets` solo funciona en ambiente `Development`.

---

## 🧩 Migraciones de base de datos

Para crear la base de datos y las tablas, ejecutá:

```bash
dotnet ef database update
```

---

## 🪵 Logging

Este proyecto usa [Serilog](https://serilog.net/) con:

* Archivos en `/Logs`
* Logs estructurados en SQL Server
* Enriquecimiento con `UserId` automáticamente si el usuario está autenticado
* `ErrorId` único por error para trazabilidad

---

## 🔍 Errores

Cuando ocurre un error interno:

```json
{
  "message": "Ocurrió un error interno. Si el problema persiste, contactá al soporte.",
  "errorId": "e7c344fa-3ed5-490e-8242-ffb9d70a53e0"
}
```

Podés buscar ese `errorId` en la base de logs.

---

## 📂 Estructura de carpetas relevante

```
/WebAPI
  /Controllers
  /Middleware
  /Responses
/Domain
  /Entities
/Application
  /Interfaces
  /Services
/Infraestructure
  /Data
  /Security
```
