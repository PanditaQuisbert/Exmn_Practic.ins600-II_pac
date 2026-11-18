# Sistema de Notas Escolar

Sistema web para la gestión de estudiantes, materias, asignaciones de profesores y calificaciones trimestrales.

**Estado actual:** Etapa 5 finalizada. La API cuenta con autenticación JWT, hash de contraseñas con BCrypt, autorización por roles, gestión de usuarios, asignación de profesores a materias, CRUD de estudiantes y materias con activación/desactivación lógica, registro y corrección de calificaciones por parte del Profesor, y consulta del boletín académico por parte de la Secretaría. El frontend Vue 3 + Vite está implementado en el proyecto hermano `SistemaNotasColegio.Web/` y consume la API real.

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | C# / ASP.NET Core Web API / .NET 10 |
| ORM | Entity Framework Core 10 |
| Base de datos | SQL Server Express |
| Seguridad | JWT Bearer + BCrypt.Net-Next |
| Frontend | Vue.js 3 + Vite + JavaScript |
| Control de versiones | Git y GitHub |

---

## Estructura del repositorio

```text
SistemaNotasColegio_Examen_Practc_PAC/
├── SistemaNotasColegio/                     # Backend ASP.NET Core Web API
│   ├── Configuration/                       # JWT, política de contraseñas, roles
│   ├── Controllers/                         # Auth, usuarios, estudiantes, materias, roles, asignaciones, calificaciones, boletines, profesores, salud
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/                  # Configuraciones Fluent API
│   │   └── Seed/                            # Creación segura de la Secretaría inicial
│   ├── DTOs/                                # Contratos de entrada y salida
│   ├── Migrations/                          # Migración InitialCreate
│   ├── Models/                              # Entidades del dominio
│   ├── Services/                            # Lógica de autenticación, gestión, estudiantes, materias, calificaciones y boletines
│   ├── Program.cs
│   ├── appsettings.json
│   └── SistemaNotasColegio.csproj
├── SistemaNotasColegio.Web/                 # Frontend Vue 3 + Vite
│   ├── public/                              # favicon
│   ├── src/
│   │   ├── components/                      # Tablas, modales, badges, boletín
│   │   ├── composables/                     # Paginación local
│   │   ├── layouts/                         # Layout con sidebar y topbar
│   │   ├── router/                          # Rutas y guard de roles
│   │   ├── services/                        # Axios central y servicios por recurso
│   │   ├── stores/                          # Sesión, carga, notificaciones, confirmaciones
│   │   ├── styles/                          # Estilos institucionales e impresión
│   │   ├── utils/                           # Formatos, validaciones, errores
│   │   └── views/                           # Vistas de Secretaría y Profesor
│   ├── .env.example
│   ├── .gitignore
│   ├── index.html
│   ├── package.json
│   ├── vite.config.js
│   └── README.md
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
  - Consulta boletines académicos.
  - No registra, modifica ni consulta calificaciones.
- **Profesor**
  - Consulta únicamente las materias que tiene asignadas.
  - Registra y corrige notas únicamente de las materias que tiene asignadas.
  - No puede crear, modificar ni eliminar usuarios.
  - No puede consultar boletines.

No existen roles de Administrador ni Estudiante. No existe registro público.

### Calificaciones

- Cada estudiante tiene **una sola nota por materia y por trimestre** (índice único en SQL Server).
- Los trimestres son: **primero, segundo y tercero**.
- Las notas van de **0 a 100**.
- El promedio anual es el promedio de los tres trimestres, **redondeado**.
- La nota mínima de aprobación es **51**.
- Si falta alguna de las tres notas, el estado debe ser **INCOMPLETO**.
- El sistema genera el **boletín individual** del estudiante.

Reglas implementadas en la Etapa 4:

- El promedio anual se calcula únicamente cuando existen las **tres** notas trimestrales: `(T1 + T2 + T3) / 3`.
- Redondeo explícito: `Math.Round(promedio, 0, MidpointRounding.AwayFromZero)`. Un valor terminado en `.5` se aleja de cero; por ejemplo, `50.5` se redondea a `51`.
- Estado académico: promedio `>= 51` → `APROBADO`; promedio `< 51` → `REPROBADO`; promedio nulo o nota faltante → `INCOMPLETO` con `promedioAnual: null`.
- El promedio **no se almacena**: se calcula al consultar el boletín, de modo que nunca queda desactualizado.
- `POST /api/calificaciones` crea una nota nueva. Si ya existe una nota para el mismo estudiante, materia y trimestre responde `409 Conflict` e indica usar `PUT` para corregirla.
- `PUT /api/calificaciones/{id}` corrige la nota existente sin cambiar estudiante, materia ni trimestre, y registra el usuario que actualizó (`ActualizadoPorUsuarioId`).
- El Profesor nunca envía su propio ID: la API lo obtiene del claim `nameid`/`sub` del JWT.
- Un Profesor puede leer las calificaciones de las materias que tiene asignadas, pero solo puede modificar las que él mismo registró.
- Registrar o modificar una nota en una materia no asignada devuelve `403 Forbidden`.
- Los filtros de listado (`estudianteId`, `materiaId`, `trimestre`) siempre se restringen a las materias asignadas al Profesor autenticado; filtrar por una materia no asignada devuelve una lista vacía.
- El Profesor no puede acceder a boletines y la Secretaría no puede registrar, modificar ni consultar calificaciones (`403`).

### Boletín académico

`GET /api/boletines/estudiantes/{estudianteId}` (solo Secretaría) devuelve:

- Datos del estudiante: id, cédula, nombres, apellidos y estado `activo`.
- Una fila por materia con las notas de los tres trimestres, el promedio anual y el estado académico.
- El boletín incluye las materias activas y también las materias inactivas que ya tengan notas registradas, para preservar el historial académico.

Regla para estudiantes inexistentes o inactivos:

- Estudiante **inexistente** → `404 Not Found`.
- Estudiante **inactivo** → `200 OK`. Se permite consultar su boletín porque el historial académico debe permanecer disponible; la respuesta incluye `"activo": false` para que el consumidor distinga su estado.

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

### Ejecutar el frontend

Requisitos: Node.js 20 o superior y npm 10 o superior.

```powershell
cd SistemaNotasColegio.Web
npm install
copy .env.example .env
npm run dev
```

- Frontend en desarrollo: `http://localhost:5173`
- El archivo `.env` solo debe contener la URL de la API: `VITE_API_BASE_URL=http://localhost:5012`
- `.env` es local y está excluido por Git; `.env.example` no contiene credenciales.
- Compilación de producción: `npm run build` (salida en `dist/`).
- Vista previa de la compilación: `npm run preview` (`http://localhost:4173`).

El backend ya permite CORS para los puertos `5173` y `4173`. Los detalles de instalación,
rutas, funcionalidades y limitaciones del frontend están en `SistemaNotasColegio.Web/README.md`.

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
| GET | `/api/profesores/mis-asignaciones` | Profesor | Lista las materias asignadas al Profesor autenticado; el ID se toma del JWT. |
| GET | `/api/calificaciones` | Profesor | Lista calificaciones de sus materias; filtra por `estudianteId`, `materiaId` y `trimestre`. |
| GET | `/api/calificaciones/{id}` | Profesor | Consulta una calificación de una materia asignada. |
| POST | `/api/calificaciones` | Profesor | Registra una nota; si ya existe responde `409 Conflict`. |
| PUT | `/api/calificaciones/{id}` | Profesor | Corrige una nota registrada por el propio Profesor. |
| GET | `/api/boletines/estudiantes/{estudianteId}` | Secretaría | Consulta el boletín con notas trimestrales, promedio anual y estado. |
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

## Pruebas ejecutadas en la Etapa 4

Batería real contra la API en ejecución y la base `SistemaNotasColegioDB`: **88 comprobaciones, 0 fallos**.

- Profesor consultó sus asignaciones (`200`); cada Profesor vio únicamente sus propias materias.
- Un Profesor no pudo consultar las asignaciones de otro: el parámetro `profesorId` enviado por el cliente se ignora y la identidad se toma del JWT.
- Profesor registró una nota válida (`201`) y corrigió la nota existente (`200`) conservando el mismo registro.
- Duplicado de estudiante/materia/trimestre rechazado con `409`; verificado en SQL Server que solo existe una fila.
- Nota menor que 0 y mayor que 100 (`400`).
- Trimestre `0` y `4` (`400`).
- Estudiante inexistente y materia inexistente (`404`).
- Profesor registrando en materia no asignada (`403`).
- Profesor intentando modificar una nota registrada por otro Profesor (`403`, mensaje específico).
- Profesor consultó sus calificaciones y una nota por id (`200`).
- Consulta de una nota de materia no asignada (`403`); listado filtrado por materia no asignada devuelve lista vacía sin fuga de información.
- Filtros por estudiante, materia y trimestre respetando las asignaciones (`200`).
- Secretaría no pudo listar, registrar, modificar ni consultar calificaciones (`403`).
- Profesor no pudo consultar boletines (`403`).
- Secretaría consultó el boletín (`200`) con datos del estudiante y las tres notas.
- Boletín con una nota faltante devolvió `promedioAnual: null` y estado `INCOMPLETO`.
- Cálculos verificados: notas `85, 70, 60` → promedio `72`, `APROBADO`; promedio `50.5` → `51`, `APROBADO`; promedio `50` → `REPROBADO`; promedio `51` → `APROBADO`.
- Boletín de estudiante inexistente (`404`); boletín de estudiante inactivo (`200`, con `activo: false`).
- Solicitudes sin token y con token inválido (`401`) en calificaciones, asignaciones del Profesor y boletines.
- El índice único de SQL Server rechazó una inserción duplicada directa (error 2601/2627).
- La corrección mantuvo una sola fila y guardó `ActualizadoPorUsuarioId`.
- Datos temporales eliminados; estado final: 1 usuario (Secretaría inicial), 0 estudiantes, 0 materias, 0 asignaciones y 0 calificaciones.

No se modificó el esquema: `has-pending-model-changes` reporta que el modelo no cambió.

---

## Pruebas ejecutadas en la Etapa 5 (frontend)

### Compilación

```text
npm run build → 131 módulos transformados, 0 errores, 0 advertencias
```

Salida generada en `SistemaNotasColegio.Web/dist/`.

### Integración real con la API (27/27 comprobaciones)

Script contra `http://localhost:5012` con datos temporales `TEST-T5-*`:

- Salud de la API y conexión a SQL Server; login de Secretaría (`200`, rol 1) y contraseña inválida (`401`).
- Alta de estudiante, materia y dos profesores (`201`).
- Asignación de la misma materia a dos profesores (`201`).
- `GET /api/profesores/mis-asignaciones` devuelve la materia asignada con `materiaCodigo` y `materiaActiva`.
- Profesor consultando `GET /api/estudiantes` y `GET /api/usuarios` (`403`, limitación documentada).
- Registro de T1, T2 y T3 (`201`); duplicado (`409`).
- Listado con nombre del estudiante y autor; corrección con `PUT` (`200`, `fechaActualizacion`).
- Otro profesor intentando corregir una nota ajena (`403`).
- Boletín: T1=85, T2=70, T3=90, `promedioAnual` 82 (`Math.Round` entero) y estado `APROBADO`.
- Profesor consultando boletín (`403`); eliminación de asignación con notas (`409`) y sin notas (`204`).
- Nota sobre estudiante inactivo rechazada (`400`) y reactivación (`200`).
- Limpieza verificada: 0 residuos `TEST-T5-*`.

### Verificación funcional de la interfaz en navegador

Ejecutada contra la API real y la base `SistemaNotasColegioDB`, con usuarios y datos temporales:

- Login de Secretaría con redirección a `/secretaria` y datos reales en el panel.
- Estudiantes: alta mediante el formulario, búsqueda, edición, desactivación (desaparece del listado activo) y reactivación con `incluirInactivos`.
- Materias: alta, edición y validaciones del formulario.
- Usuarios: alta de Profesor con rol 2 y edición de datos sin cambiar contraseña.
- Asignaciones: creación desde el modal, eliminación correcta y mensaje del backend al intentar eliminar una asignación con notas (`409`).
- Logout con confirmación y limpieza de la sesión.
- Profesor: panel con conteos reales, `mis asignaciones`, registro de T1/T2/T3 (80/70/90), validación previa de duplicados sin llamar a la API y corrección de T1 a 85 mediante `PUT`.
- Propiedad de las notas: un segundo Profesor ve sus propias notas como editables y las del primero como «Solo lectura» con el botón deshabilitado.
- Boletín: búsqueda del estudiante, tabla con 85/70/90/82, estado «Aprobado», botón de impresión invocando `window.print()` (diálogo interceptado) y diseño formal.
- Rutas protegidas: Profesor en `/secretaria/...` redirigido a `/403`.
- Sesión expirada: token alterado → `401`, sesión eliminada y redirección a `/login?sesionExpirada=1` con mensaje.
- Credenciales incorrectas: mensaje «Usuario o contraseña incorrectos.» y estado «Servidor conectado».
- API fuera de línea: con el backend detenido, el login muestra «Sin conexión con la API» y, al intentar ingresar, «No se pudo conectar con la API en http://localhost:5012. Verifique que el backend esté en ejecución.», sin crear sesión. Tras reiniciar la API el estado vuelve a «Servidor conectado».
- Responsive: a 526 px de ancho no hay desbordamiento horizontal en panel, estudiantes, materias, usuarios, asignaciones ni boletines (la tabla del boletín se desplaza dentro de su contenedor); el menú lateral pasa a modo off-canvas (264 px) con botón de apertura y fondo para cerrar. Se corrigió un desbordamiento de 11 px del boletín añadiendo `min-width: 0` a los hijos de `.boletin-layout`.
- Base final: 1 usuario (Secretaría inicial), 0 estudiantes, 0 materias, 0 asignaciones y 0 calificaciones.

### Pruebas no ejecutadas

- Captura de pantalla del navegador: el entorno no expone una ventana visible para `browser.screenshot`.
- Diálogo de impresión real: se interceptó `window.print()` para no abrir el diálogo del sistema; los estilos `@media print` se verificaron por revisión de CSS.
- Suite automatizada de componentes/E2E y pruebas en navegadores distintos de Chromium.
- No se pudo variar el ancho del viewport con una herramienta; la comprobación responsive se hizo con el ancho real disponible (526 px) y revisión de las media queries.

### Limitaciones documentadas (sin modificar el backend)

- El rol Profesor no puede listar estudiantes (`GET /api/estudiantes` es exclusivo de Secretaría). El formulario de calificaciones solicita el identificador numérico y sugiere estudiantes ya calificados. Se documentó una mejora futura (endpoint de solo lectura para Profesor) sin implementarla.
- No existe endpoint de boletines para Profesor; el boletín es exclusivo de Secretaría.
- No hay endpoint de estadísticas: los conteos de los paneles se calculan con los listados reales, sin cifras inventadas.

### Backend sin cambios

La Etapa 5 no modificó ningún archivo, migración, configuración ni funcionalidad de
`SistemaNotasColegio/`. Los cambios se limitan a `SistemaNotasColegio.Web/`, al `README.md`
raíz y al `.gitignore`.

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
| Etapa 4: registro/corrección de calificaciones y reglas de promedio/estado | **Finalizada** |
| Etapa 5: frontend Vue 3 + Vite | **Finalizada** |
| Etapa 6: reportes, pruebas finales, documentación y publicación en GitHub | Pendiente |

---

## Notas de implementación

- Se conservó el controlador de plantilla `WeatherForecastController` por indicación expresa de no eliminar archivos existentes; solo se ocultó de Swagger.
- No se agregó ningún rol adicional ni registro público.
- La Etapa 2 no modifica el esquema de base de datos.
- La Etapa 3 tampoco modifica el esquema: usa los campos existentes `Cedula`, `Nombres`, `Apellidos` y `Activo` en estudiantes, y `Codigo`, `Nombre` y `Activo` en materias. El modelo no define código estudiantil ni descripción de materia, por lo que no se inventaron campos.
- Estudiantes y materias no se eliminan físicamente: solo se activan o desactivan para preservar calificaciones e historial académico.
- La Etapa 4 tampoco modifica el esquema: reutiliza las entidades `Calificacion` y `AsignacionMateria`, el índice único `(EstudianteId, MateriaId, Trimestre)` y las restricciones `CK_Calificaciones_Nota` y `CK_Calificaciones_Trimestre` ya existentes.
- El promedio anual no se guarda en la base: se calcula en cada consulta del boletín, por lo que no puede quedar desactualizado.
- La corrección de notas (`PUT`) reutiliza el registro original y conserva la trazabilidad en `ActualizadoPorUsuarioId`.
- El endpoint de asignaciones del Profesor y los de calificaciones obtienen el ID del Profesor desde el JWT (`nameid`/`sub`); nunca desde el cuerpo o la query del cliente.
- La Etapa 5 agrega un proyecto hermano `SistemaNotasColegio.Web/`; el backend no se modificó. Se eligió JavaScript (no TypeScript) de forma consistente con la convención del repositorio.
- El frontend no usa Bootstrap JS ni Pinia: los modales, notificaciones, confirmaciones y paginación son componentes propios sobre estilos Bootstrap 5.
- La sesión se guarda en `localStorage` (`snc.sesion`) con token, expiración y datos del usuario; la contraseña nunca se almacena. El `rolId` del backend es la fuente de verdad para el guard de rutas.
- El boletín se imprime con `window.print()` y reglas `@media print`; no se genera PDF en el servidor.
- La base `INS600_Notas_AlvarezDB` no fue modificada ni utilizada.
