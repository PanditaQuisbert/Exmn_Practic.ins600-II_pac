# Sistema de Notas Escolar

Sistema web para la gestión de estudiantes, materias, asignaciones de profesores y calificaciones trimestrales.

**Estado actual:** Etapa 1 finalizada. La API base, el modelo de dominio, EF Core, SQL Server, Swagger, CORS y la migración inicial están implementados y verificados. La autenticación JWT/BCrypt y el frontend se desarrollarán en las siguientes etapas.

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | C# / ASP.NET Core Web API / .NET 10 |
| ORM | Entity Framework Core 10 |
| Base de datos | SQL Server 2022 Express |
| Seguridad | JWT y BCrypt (paquetes instalados; login pendiente para la Etapa 2) |
| Frontend | Vue.js 3 + Vite + JavaScript (pendiente, en `SistemaNotasColegio.Web/`) |
| Control de versiones | Git y GitHub |

---

## Estructura del repositorio

```text
SistemaNotasColegio_Examen_Practc_PAC/
├── SistemaNotasColegio/                 # Backend ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── HealthController.cs           # Verificación de API y base de datos
│   │   └── WeatherForecastController.cs # Plantilla conservada, oculta de Swagger
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/              # Configuraciones Fluent API
│   ├── Migrations/                      # Migración InitialCreate
│   ├── Models/                          # Entidades del dominio
│   │   └── Common/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── SistemaNotasColegio.csproj
├── SistemaNotasColegio.Web/             # Frontend Vue 3 + Vite (pendiente, Etapa 5)
├── SistemaNotasColegio_Examen_Practc_PAC.slnx
├── .gitignore
└── README.md
```

> El frontend será un proyecto independiente dentro de esta misma carpeta raíz. No se colocará dentro de `SistemaNotasColegio/`.

---

## Requisitos funcionales confirmados

### Roles

- **Secretaría**
  - Crea y modifica estudiantes.
  - Crea y modifica materias.
  - Crea y modifica usuarios.
  - No registra notas.
- **Profesor**
  - Registra y corrige notas únicamente de las materias que tiene asignadas.

No existen roles de Administrador ni Estudiante.

### Calificaciones

- Cada estudiante tiene **una sola nota por materia y por trimestre**.
- Los trimestres son: **primero, segundo y tercero**.
- Las notas van de **0 a 100**.
- El promedio anual es el promedio de los tres trimestres, **redondeado**.
- La nota mínima de aprobación es **51**.
- Si falta alguna de las tres notas, el estado debe ser **INCOMPLETO**.
- El sistema debe generar el **boletín individual** del estudiante.

> Las reglas de promedio, estado y boletín se implementarán en las etapas de servicios y reportes. En la Etapa 1 solo se dejó preparado el modelo de datos y sus restricciones.

---

## Modelo de datos

| Tabla | Propósito |
|---|---|
| `Roles` | Catálogo con los roles `Secretaría` y `Profesor`. |
| `Usuarios` | Cuentas de acceso. Un usuario profesor se relaciona con sus materias mediante asignaciones. |
| `Estudiantes` | Datos de los estudiantes. |
| `Materias` | Catálogo de materias. |
| `AsignacionesMaterias` | Materias asignadas a cada profesor. |
| `Calificaciones` | Nota de un estudiante en una materia y un trimestre. |
| `__EFMigrationsHistory` | Control interno de migraciones de EF Core. |

### Restricciones principales

- Cédula única en `Usuarios` y `Estudiantes`.
- Correo y nombre de usuario únicos en `Usuarios`.
- Código único en `Materias`.
- Una asignación única por par profesor/materia.
- Una calificación única por estudiante/materia/trimestre.
- `Nota` entre 0 y 100 (`CK_Calificaciones_Nota`).
- `Trimestre` entre 1 y 3 (`CK_Calificaciones_Trimestre`).
- Borrados restringidos (`NO ACTION`) para proteger la integridad histórica.

---

## Configuración local

### Requisitos

- .NET SDK 10.
- SQL Server Express en `localhost\SQLEXPRESS`.
- Autenticación de Windows.
- `dotnet-ef` instalado como herramienta global.

### Base de datos

La cadena de conexión está en `SistemaNotasColegio/appsettings.json`:

```text
Server=localhost\SQLEXPRESS;Database=SistemaNotasColegioDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

La base `SistemaNotasColegioDB` se crea al aplicar la migración:

```powershell
dotnet ef database update `
  --project SistemaNotasColegio/SistemaNotasColegio.csproj `
  --startup-project SistemaNotasColegio/SistemaNotasColegio.csproj
```

### Ejecutar la API

```powershell
dotnet run --project SistemaNotasColegio/SistemaNotasColegio.csproj --launch-profile http
```

- API: `http://localhost:5012`
- Swagger: `http://localhost:5012/swagger`
- Salud: `http://localhost:5012/api/health`

### Compilar

```powershell
dotnet build SistemaNotasColegio_Examen_Practc_PAC.slnx --nologo
```

---

## Control de versiones

- Repositorio Git local inicializado en la rama `main`.
- Se versionan el backend, la migración, la documentación y los archivos de configuración sin secretos.
- No se versionan `bin/`, `obj/`, `.vs/`, archivos `*.user`, `node_modules/` ni secretos locales.
- La publicación en GitHub queda pendiente: no hay un remoto configurado ni GitHub CLI instalado.

---

## Endpoints actuales

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/health` | Verifica la API y la conexión a SQL Server. |
| GET | `/weatherforecast` | Endpoint de plantilla conservado, oculto de Swagger. |

Los endpoints de autenticación, estudiantes, materias, usuarios, asignaciones, calificaciones y boletín se agregarán en las siguientes etapas.

---

## Seguridad

Los paquetes `Microsoft.AspNetCore.Authentication.JwtBearer` y `BCrypt.Net-Next` ya están instalados.

La implementación de login, generación/validación de JWT, hash de contraseñas, roles y políticas de autorización corresponde a la **Etapa 2**. La clave JWT se configurará mediante **User Secrets** o variables de entorno, nunca dentro del repositorio.

---

## Etapas

| Etapa | Estado |
|---|---|
| Etapa 1: infraestructura, EF Core, SQL Server, entidades, migración, Swagger, CORS y Git | **Finalizada** |
| Etapa 2: login, JWT, BCrypt, autorización por roles y gestión de usuarios | Pendiente |
| Etapa 3: CRUD de estudiantes, materias y asignaciones | Pendiente |
| Etapa 4: registro/corrección de calificaciones y reglas de promedio/estado | Pendiente |
| Etapa 5: frontend Vue 3 + Vite | Pendiente |
| Etapa 6: boletín, reportes, pruebas, documentación final y publicación en GitHub | Pendiente |

---

## Notas de implementación

- Se conservó el controlador de plantilla `WeatherForecastController` por indicación expresa de no eliminar archivos existentes; solo se ocultó de Swagger.
- La Etapa 1 no incluye funcionalidades simuladas ni endpoints de negocio. Solo infraestructura verificable.
- La base `INS600_Notas_AlvarezDB` no fue modificada ni utilizada.
