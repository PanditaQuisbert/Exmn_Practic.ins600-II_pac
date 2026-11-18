# Sistema de Notas Escolar — Frontend (Vue 3)

Aplicación web del **Sistema de Gestión de Calificaciones de un Colegio**. Consume la API
ASP.NET Core del proyecto hermano `SistemaNotasColegio/` y ofrece una interfaz diferenciada
para los roles **Secretaría** y **Profesor**.

---

## Tecnologías

| Componente | Tecnología |
|---|---|
| Framework | Vue 3 (Composition API, `<script setup>`) |
| Lenguaje | JavaScript (ES Modules) |
| Herramientas | Vite 6 |
| Enrutamiento | Vue Router 4 |
| Cliente HTTP | Axios |
| Estilos | Bootstrap 5 + Bootstrap Icons + hoja de estilos institucional |
| Estado | Stores reactivos propios (`src/stores`) |

No se utiliza Pinia ni Bootstrap JS: los modales, notificaciones y diálogos de confirmación
son componentes propios construidos sobre las clases de Bootstrap.

---

## Requisitos

- Node.js 20 o superior (probado con Node 24).
- npm 10 o superior.
- Backend del repositorio en ejecución (ver más abajo).
- Navegador moderno (Chrome, Edge o Firefox actualizados).

---

## Instalación

Desde la raíz del repositorio:

```bash
cd SistemaNotasColegio.Web
npm install
```

Dependencias declaradas en `package.json`:

- `vue`, `vue-router`, `axios`
- `bootstrap`, `bootstrap-icons`
- `vite`, `@vitejs/plugin-vue` (desarrollo)

---

## Variables de entorno

El frontend lee la URL de la API desde `VITE_API_BASE_URL`.

1. Copie el archivo de ejemplo:

```bash
copy .env.example .env
```

2. Ajuste el valor según su entorno:

```env
VITE_API_BASE_URL=http://localhost:5012
```

El archivo `.env` es local y está excluido por `.gitignore`. `.env.example` no contiene
credenciales ni claves. El valor por defecto del código es `http://localhost:5012` si la
variable no está definida.

---

## Ejecución del backend

Desde la raíz del repositorio, en una terminal aparte:

```bash
dotnet run --project SistemaNotasColegio --urls http://localhost:5012
```

Requisitos del backend:

- SQL Server Express disponible en la instancia configurada.
- Clave JWT configurada mediante User Secrets.
- Usuario inicial de Secretaría configurado mediante User Secrets.

En Development, Swagger está disponible en `http://localhost:5012/swagger`.

El backend permite CORS para `http://localhost:5173` (desarrollo) y `http://localhost:4173`
(vista previa de la compilación).

---

## Ejecución del frontend

Desarrollo con recarga en caliente:

```bash
npm run dev
```

Abrir `http://localhost:5173`.

Compilación de producción:

```bash
npm run build
```

La salida se genera en `dist/`. Para revisar la compilación:

```bash
npm run preview
```

Abrir `http://localhost:4173`.

---

## Conexión con la API

Toda la comunicación pasa por `src/services/api.js`:

- URL base tomada de `VITE_API_BASE_URL`.
- Token JWT agregado automáticamente en cada solicitud autenticada.
- Normalización de errores del backend (`{ mensaje }`, ProblemDetails y errores de red).
- Cierre automático de sesión y redirección al login ante un `401`.
- Mensaje específico ante un `403` (permisos insuficientes).
- Mensaje explícito si la API no está disponible.
- Contador global de solicitudes para la barra de carga superior.

La sesión se guarda en `localStorage` bajo la clave `snc.sesion` (token, expiración y datos
del usuario). **La contraseña nunca se almacena.** Al expirar el token, la sesión se elimina
y se solicita un nuevo inicio de sesión.

La pantalla de login verifica la conexión con `GET /api/health` e informa el estado real de la
API y de la base de datos.

---

## Roles y rutas

| Ruta | Rol | Descripción |
|---|---|---|
| `/login` | Público | Inicio de sesión |
| `/secretaria` | Secretaría | Panel principal con datos reales |
| `/secretaria/estudiantes` | Secretaría | CRUD, búsqueda y activación lógica |
| `/secretaria/materias` | Secretaría | CRUD, búsqueda y activación lógica |
| `/secretaria/usuarios` | Secretaría | CRUD de Secretaría y Profesor |
| `/secretaria/asignaciones` | Secretaría | Listado, creación y eliminación |
| `/secretaria/boletines` | Secretaría | Consulta e impresión del boletín |
| `/profesor` | Profesor | Panel principal |
| `/profesor/asignaciones` | Profesor | Materias asignadas |
| `/profesor/calificaciones` | Profesor | Registro y corrección de notas |
| `/403` | Autenticado | Acceso denegado por rol |
| `*` | Público | Página no encontrada |

El guard de rutas usa el `rolId` devuelto por la API (1 = Secretaría, 2 = Profesor) y redirige
al panel correspondiente después del login. Un usuario autenticado que intente abrir una ruta
de otro rol es enviado a `/403`.

---

## Funcionalidades por rol

### Secretaría

- **Panel:** conteos reales de estudiantes, materias, usuarios y asignaciones, estado de la API
  y accesos rápidos.
- **Estudiantes:** listado con búsqueda, paginación, registro, edición y activación/desactivación.
- **Materias:** listado con búsqueda, paginación, registro, edición y activación/desactivación.
- **Usuarios:** listado con filtro por rol, registro, edición, cambio de contraseña opcional y
  activación/desactivación. La protección de la última Secretaría activa la valida el backend.
- **Asignaciones:** filtros por profesor y materia, creación con profesores y materias activos,
  eliminación con confirmación. Si el profesor ya registró notas, la API responde `409` y el
  frontend muestra el mensaje correspondiente.
- **Boletines:** búsqueda de estudiantes (incluye inactivos), visualización de notas T1–T3,
  promedio anual y estado por materia, e impresión con `window.print()` y estilos `@media print`.
  Es una pantalla de solo consulta.

### Profesor

- **Panel:** bienvenida, materias asignadas, notas registradas, estudiantes calificados y últimas
  calificaciones, todo con datos reales.
- **Mis asignaciones:** materias obtenidas de `GET /api/profesores/mis-asignaciones`, con estado
  activa/inactiva y acceso directo a sus calificaciones.
- **Calificaciones:** filtros por materia, trimestre y búsqueda por estudiante; registro de notas
  de 0 a 100; corrección mediante `PUT`; validación previa de duplicados por estudiante, materia y
  trimestre. Solo se habilitan las notas registradas por el propio profesor
  (`registradoPorUsuarioId`); las demás se muestran en modo lectura.

La identidad del profesor siempre proviene del token JWT. El formulario no permite elegir ni
escribir el profesor que registra la nota.

---

## Endpoints utilizados

| Método | Ruta | Uso |
|---|---|---|
| `POST` | `/api/auth/login` | Inicio de sesión |
| `GET` | `/api/health` | Estado de API y base de datos |
| `GET/POST/PUT/PATCH` | `/api/estudiantes` | Gestión de estudiantes |
| `GET/POST/PUT/PATCH` | `/api/materias` | Gestión de materias |
| `GET/POST/PUT/PATCH` | `/api/usuarios` | Gestión de usuarios |
| `GET` | `/api/roles` | Catálogo de roles |
| `GET/POST/DELETE` | `/api/asignaciones` | Gestión de asignaciones |
| `GET` | `/api/boletines/estudiantes/{id}` | Boletín académico |
| `GET` | `/api/profesores/mis-asignaciones` | Materias del profesor autenticado |
| `GET/POST/PUT` | `/api/calificaciones` | Calificaciones del profesor |

No se inventaron rutas, campos ni respuestas: todos los contratos fueron verificados en los
controladores, DTOs y servicios del backend.

---

## Limitaciones conocidas (sin modificar el backend)

1. **El Profesor no puede listar estudiantes.** `GET /api/estudiantes` está restringido al rol
   Secretaría. Para registrar una nota, el formulario solicita el **identificador numérico** del
   estudiante y ofrece sugerencias con los estudiantes que ya tienen calificaciones en las
   materias del profesor. No se creó ni simuló un endpoint inexistente. Una mejora futura
   requeriría un endpoint de solo lectura para Profesor (por ejemplo, estudiantes de sus materias)
   o la inclusión del nombre del estudiante en la respuesta de calificaciones (ya incluida).
2. **No existe un endpoint de boletines para Profesor.** El boletín es exclusivo de Secretaría;
   el Profesor trabaja con la lista de calificaciones.
3. **No hay endpoint de estadísticas agregadas.** Los conteos del panel se calculan en el
   frontend a partir de los listados reales; no se muestran cifras inventadas.
4. **La impresión usa el diálogo del navegador.** No se genera PDF en el servidor.

---

## Estructura del proyecto

```text
SistemaNotasColegio.Web/
├── public/
│   └── favicon.svg
├── src/
│   ├── components/        # Componentes reutilizables (tablas, modales, badges, boletín)
│   ├── composables/       # Paginación local
│   ├── layouts/           # Layout principal con sidebar y topbar
│   ├── router/            # Rutas y guard de roles
│   ├── services/          # Axios central y servicios por recurso
│   ├── stores/            # Sesión, carga, notificaciones y confirmaciones
│   ├── styles/            # Estilos institucionales y reglas de impresión
│   ├── utils/             # Formatos, validaciones y errores
│   ├── views/             # Vistas por rol
│   ├── App.vue
│   └── main.js
├── .env.example
├── .gitignore
├── index.html
├── package.json
└── vite.config.js
```

---

## Verificación

```bash
npm run build
```

La compilación debe finalizar sin errores ni advertencias. La verificación funcional se realiza
contra la API real con datos temporales y se documenta en el README principal del repositorio.
