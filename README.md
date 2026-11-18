# Sistema de Notas Escolar

Sistema web para la gestión de estudiantes, materias, asignaciones de profesores y calificaciones trimestrales.

**Estado actual:** Etapa 3 finalizada. La API cuenta con autenticación JWT, hash de contraseñas con BCrypt, autorización por roles, gestión de usuarios, asignación de profesores a materias y CRUD de estudiantes y materias con activación/desactivación lógica. El frontend se desarrollará posteriormente en un proyecto independiente.

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | C# / ASP.NET Core Web API / .NET 10 |
| ORM | Entity Framework Core 10 |
| Base de datos | SQL Server Express |
| Seguridad | JWT Bearer + BCrypt.Net-Next |
| Frontend | Vue.js 3 + Vite + JavaScript (pendiente, en `SistemaNotasColegio.Web/`) |
| Control de versiones | Git y GitHub |

---

## Estructura del repositorio

```text
SistemaNotasColegio_Examen_Practc_PAC/
├── SistemaNotasColegio/                     # Backend ASP.NET Core Web API
│   ├── Configuration/                       # JWT, política de contraseñas, roles
│   ├── Controllers/                         # Auth, usuarios, estudiantes, materias, roles, asignaciones, salud
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/                  # Configuraciones Fluent API
│   │   └── Seed/                            # Creación segura de la Secretaría inicial
│   ├── DTOs/                                # Contratos de entrada y salida
│   ├── Migrations/                          # Migración InitialCreate
│   ├── Models/                              # Entidades del dominio
│   ├── Services/                            # Lógica de autenticación, gestión, estudiantes y materias
│   ├── Program.cs
│   ├── appsettings.json
│   └── SistemaNotasColegio.csproj
├── SistemaNotasColegio.Web/                 # Frontend Vue 3 + Vite (pendiente, Etapa 5)
├── SistemaNotasColegio_Examen_Practc_PAC.slnx
├── .gitignore
└── README.md
```

---

## Requisitos funcionales confirmados

### Roles

- **Secretaría**
  - Crea y modifica estudiantes.
  - Crea y modifica materias.
  - Crea, consulta, modifica y activa/desactiva usuarios.
  - Asigna profesores a materias.
  - No registra notas.
- **Profesor**
  - Registra y corrige notas únicamente de las materias que tiene asignadas.
  - No puede crear, modificar ni eliminar usuarios.

No existen roles de Administrador ni Estudiante. No existe registro público.

### Calificaciones

- Cada estudiante tiene **una sola nota por materia y por trimestre**.
- Los trimestres son: **primero, segundo y tercero**.
- Las notas van de **0 a 100**.
- El promedio anual es el promedio de los tres trimestres, **redondeado**.
- La nota mínima de aprobación es **51**.
- Si falta alguna de las tres notas, el estado debe ser **INCOMPLETO**.
- El sistema debe generar el **boletín individual** del estudiante.

> Las reglas de promedio, estado y boletín se implementarán en las etapas de calificaciones y reportes. El modelo de datos ya impone las restricciones de nota, trimestre y unicidad.

---

## Modelo de datos

| Tabla | Propósito |
|---|---|
| `Roles` | Catálogo con los roles `Secretaría` y `Profesor`. |
| `Usuarios` | Cuentas de acceso con hash BCrypt, rol y estado. |
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
- La única Secretaría activa no puede desactivarse ni cambiar de rol (protección en el servicio).

---

## Configuración local

### Requisitos

- .NET SDK 10.
- SQL Server Express accesible por la instancia configurada.
- Autenticación de Windows.
- `dotnet-ef` 10 como herramienta global.

### Base de datos

La cadena de conexión está en `SistemaNotasColegio/appsettings.json`:

```text
Server=PAOLA\SQLEXPRESS;Database=SistemaNotasColegioDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

> `PAOLA\SQLEXPRESS` es el alias configurado en este equipo y resuelve a la misma instancia local de SQL Server Express. Puede cambiarse por `localhost\SQLEXPRESS` sin modificar el código.

Aplicar la migración:

```powershell
dotnet ef database update `
  --project SistemaNotasColegio/SistemaNotasColegio.csproj `
  --startup-project SistemaNotasColegio/SistemaNotasColegio.csproj
```

### Secretos obligatorios

La clave JWT y las credenciales iniciales **no se guardan en el repositorio**. Se configuran con User Secrets en desarrollo:

```powershell
dotnet user-secrets set "Jwt:Key" "<clave-aleatoria-de-al-menos-32-bytes>" --project SistemaNotasColegio/SistemaNotasColegio.csproj

dotnet user-secrets set "Seed:Secretaria:Cedula" "<cedula>" --project SistemaNotasColegio/SistemaNotasColegio.csproj
dotnet user-secrets set "Seed:Secretaria:Nombres" "<nombres>" --project SistemaNotasColegio/SistemaNotasColegio.csproj
dotnet user-secrets set "Seed:Secretaria:Apellidos" "<apellidos>" --project SistemaNotasColegio/SistemaNotasColegio.csproj
dotnet user-secrets set "Seed:Secretaria:Correo" "<correo>" --project SistemaNotasColegio/SistemaNotasColegio.csproj
dotnet user-secrets set "Seed:Secretaria:NombreUsuario" "<usuario>" --project SistemaNotasColegio/SistemaNotasColegio.csproj
dotnet user-secrets set "Seed:Secretaria:Password" "<contraseña-fuerte>" --project SistemaNotasColegio/SistemaNotasColegio.csproj
```

En producción se usan variables de entorno con doble guion bajo, por ejemplo:

```text
Jwt__Key
Jwt__Issuer
Jwt__Audience
Jwt__ExpirationMinutes
Seed__Secretaria__Cedula
Seed__Secretaria__Nombres
Seed__Secretaria__Apellidos
Seed__Secretaria__Correo
Seed__Secretaria__NombreUsuario
Seed__Secretaria__Password
```

La API no inicia si `Jwt:Key` falta o tiene menos de 32 bytes.

### Usuario inicial de Secretaría

Al iniciar la API, `DatabaseSeeder`:

1. Verifica si ya existe algún usuario con rol Secretaría.
2. Si existe, no crea nada (operación idempotente entre reinicios).
3. Si no existe y `Seed:Secretaria` está configurado, valida que la contraseña tenga al menos 12 caracteres con mayúscula, minúscula, número y símbolo.
4. Crea el usuario con hash BCrypt.
5. Si la configuración está incompleta o es débil, falla con un error claro y no crea credenciales por defecto.

Consultar únicamente la contraseña inicial configurada, sin exponer la clave JWT:

```powershell
dotnet user-secrets list --project SistemaNotasColegio/SistemaNotasColegio.csproj |
  Select-String '^Seed:Secretaria:Password'
```

Para cambiarla después, use `PUT /api/usuarios/{id}` con el campo `password` o actualice el secreto y vuelva a sembrar en una base sin usuarios de Secretaría.

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

## Autenticación y autorización

### Configuración JWT

| Parámetro | Valor |
|---|---|
| Algoritmo | HMAC SHA-256 |
| Emisor | `SistemaNotasColegio.Api` |
| Audiencia | `SistemaNotasColegio.Web` |
| Duración | 60 minutos |
| Validaciones | Firma, emisor, audiencia, expiración y lifetime |
| Clave | User Secrets o variable de entorno, mínimo 32 bytes |

Claims emitidos:

- `sub` y `nameid`: identificador del usuario.
- `name`: nombre de usuario.
- `role`: `Secretaría` o `Profesor`.
- `jti` y `nombreCompleto`.

La autorización usa políticas:

- `SoloSecretaria`
- `SoloProfesor`

El rol se toma **exclusivamente del token firmado**. La API nunca confía en un rol enviado por el frontend.

### Endpoints

| Método | Ruta | Autorización | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | Anónimo | Inicia sesión y devuelve JWT + datos del usuario. |
| GET | `/api/health` | Anónimo | Verifica la API y la conexión a SQL Server. |
| GET | `/api/usuarios` | Secretaría | Lista usuarios; admite `rolId` e `incluirInactivos`. |
| GET | `/api/usuarios/{id}` | Secretaría | Consulta un usuario. |
| POST | `/api/usuarios` | Secretaría | Registra un usuario con rol 1 o 2. |
| PUT | `/api/usuarios/{id}` | Secretaría | Modifica datos, rol y opcionalmente contraseña. |
| PATCH | `/api/usuarios/{id}/estado` | Secretaría | Activa o desactiva un usuario. |
| GET | `/api/estudiantes` | Secretaría | Lista estudiantes activos; admite `incluirInactivos`. |
| GET | `/api/estudiantes/{id}` | Secretaría | Consulta un estudiante, esté activo o inactivo. |
| POST | `/api/estudiantes` | Secretaría | Registra un estudiante con cédula única. |
| PUT | `/api/estudiantes/{id}` | Secretaría | Modifica cédula, nombres y apellidos. |
| PATCH | `/api/estudiantes/{id}/estado` | Secretaría | Activa o desactiva lógicamente un estudiante. |
| GET | `/api/materias` | Secretaría | Lista materias activas; admite `incluirInactivas`. |
| GET | `/api/materias/{id}` | Secretaría | Consulta una materia, esté activa o inactiva. |
| POST | `/api/materias` | Secretaría | Registra una materia con código único. |
| PUT | `/api/materias/{id}` | Secretaría | Modifica código y nombre. |
| PATCH | `/api/materias/{id}/estado` | Secretaría | Activa o desactiva lógicamente una materia. |
| GET | `/api/roles` | Secretaría | Lista los roles permitidos. |
| GET | `/api/asignaciones` | Secretaría | Lista asignaciones; filtra por `profesorId` o `materiaId`. |
| POST | `/api/asignaciones` | Secretaría | Asigna una materia activa a un profesor activo. |
| DELETE | `/api/asignaciones/{id}` | Secretaría | Elimina una asignación sin calificaciones registradas. |
| GET | `/weatherforecast` | Anónimo | Endpoint de plantilla conservado, oculto de Swagger. |

### Respuesta de login

```json
{
  "token": "<jwt>",
  "tipoToken": "Bearer",
  "expiraEn": "2026-11-18T15:00:00Z",
  "usuario": {
    "id": 1,
    "cedula": "0000000000",
    "nombres": "Nombre",
    "apellidos": "Apellido",
    "correo": "usuario@colegio.local",
    "nombreUsuario": "usuario",
    "rolId": 1,
    "rol": "Secretaría"
  }
}
```

### Errores de autenticación

Usuario inexistente, contraseña incorrecta y usuario inactivo devuelven el mismo mensaje:

```json
{ "mensaje": "Usuario o contraseña incorrectos." }
```

con HTTP `401`, para no revelar si el usuario existe.

---

## Pruebas ejecutadas en la Etapa 2

Batería real contra la API en ejecución y la base `SistemaNotasColegioDB`:

- Login correcto de Secretaría (`200`) y claims JWT verificados.
- Login correcto de Profesor (`200`) y claim de rol verificado.
- Contraseña incorrecta (`401`, mensaje genérico).
- Usuario inexistente (`401`, mismo mensaje).
- Usuario desactivado (`401`) y posterior reactivación con login correcto.
- Acceso sin token (`401`).
- Token inválido y token con firma alterada (`401`).
- Profesor intentando listar, crear usuarios o consultar roles (`403`).
- Profesor intentando gestionar asignaciones (`403`).
- Secretaría listando y creando usuarios (`200` / `201`).
- Secretaría modificando y activando/desactivando usuarios (`200`).
- Validación de datos inválidos y roles no permitidos (`400`).
- Cédula duplicada y asignación duplicada (`409`).
- Creación, listado y eliminación de asignaciones (`201` / `200` / `204`).
- Protección de la última Secretaría activa (`409`).
- Verificación de hash BCrypt en SQL Server.
- Swagger con esquema `http` / `bearer` / `JWT`.
- Datos de prueba eliminados; estado final: 1 usuario (Secretaría inicial), 0 materias y 0 asignaciones.

No se modificó la migración inicial: `has-pending-model-changes` reporta que el modelo no cambió.

---

## Pruebas ejecutadas en la Etapa 3

Batería real contra la API en ejecución y la base `SistemaNotasColegioDB`: **73 comprobaciones, 0 fallos**.

- Secretaría listó estudiantes y materias (`200`).
- Secretaría registró un estudiante y una materia (`201`).
- Secretaría consultó los registros creados (`200`).
- Secretaría modificó nombres, apellidos, cédula, código y nombre de materia (`200`).
- Secretaría desactivó y reactivó estudiante y materia (`200`).
- Los registros inactivos dejan de aparecer en el listado por defecto y sí aparecen con `incluirInactivos=true` / `incluirInactivas=true`.
- Cédula duplicada al crear y al actualizar (`409`).
- Código de materia duplicado al crear y al actualizar (`409`).
- Campos vacíos, longitudes y formatos inválidos (`400`).
- Consulta, modificación y cambio de estado de identificadores inexistentes (`404`).
- Profesor intentando listar, crear, modificar o cambiar estado de estudiantes y materias (`403`).
- Solicitudes sin token y con token inválido (`401`).
- Restricciones `UNIQUE` de cédula y código verificadas directamente en SQL Server con transacciones revertidas.
- Se insertó una calificación temporal, se desactivaron el estudiante y la materia, y se verificó que la calificación y el historial permanecen en la base.
- Datos temporales eliminados; estado final: 1 usuario (Secretaría inicial), 0 estudiantes, 0 materias, 0 asignaciones y 0 calificaciones.

No se modificó el esquema: no hay migraciones pendientes.

---

## Control de versiones

- Repositorio Git local en la rama `main`.
- Se versionan el backend, la migración, la documentación y la configuración sin secretos.
- No se versionan `bin/`, `obj/`, `.vs/`, archivos `*.user`, `node_modules/` ni secretos locales.
- La publicación en GitHub queda pendiente: no hay un remoto configurado ni GitHub CLI instalado.

---

## Etapas

| Etapa | Estado |
|---|---|
| Etapa 1: infraestructura, EF Core, SQL Server, entidades, migración, Swagger, CORS y Git | **Finalizada** |
| Etapa 2: login, JWT, BCrypt, autorización por roles, usuarios y asignaciones | **Finalizada** |
| Etapa 3: CRUD de estudiantes y materias | **Finalizada** |
| Etapa 4: registro/corrección de calificaciones y reglas de promedio/estado | Pendiente |
| Etapa 5: frontend Vue 3 + Vite | Pendiente |
| Etapa 6: boletín, reportes, pruebas finales, documentación y publicación en GitHub | Pendiente |

---

## Notas de implementación

- Se conservó el controlador de plantilla `WeatherForecastController` por indicación expresa de no eliminar archivos existentes; solo se ocultó de Swagger.
- No se agregó ningún rol adicional ni registro público.
- La Etapa 2 no modifica el esquema de base de datos.
- La Etapa 3 tampoco modifica el esquema: usa los campos existentes `Cedula`, `Nombres`, `Apellidos` y `Activo` en estudiantes, y `Codigo`, `Nombre` y `Activo` en materias. El modelo no define código estudiantil ni descripción de materia, por lo que no se inventaron campos.
- Estudiantes y materias no se eliminan físicamente: solo se activan o desactivan para preservar calificaciones e historial académico.
- La base `INS600_Notas_AlvarezDB` no fue modificada ni utilizada.
