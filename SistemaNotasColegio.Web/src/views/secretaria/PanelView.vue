<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import PageHeader from '../../components/PageHeader.vue'
import StatCard from '../../components/StatCard.vue'
import { listarAsignaciones } from '../../services/asignacionesService'
import { mensajeDeError } from '../../services/api'
import { listarEstudiantes } from '../../services/estudiantesService'
import { verificarSaludApi } from '../../services/healthService'
import { listarMaterias } from '../../services/materiasService'
import { listarUsuarios } from '../../services/usuariosService'
import { nombreUsuarioSesion } from '../../stores/auth'
import { formatearFechaLarga, formatearFechaHora } from '../../utils/format'

const cargando = ref(true)
const errorCarga = ref('')
const cargandoSalud = ref(false)
const salud = ref(null)
const errorSalud = ref('')

const resumen = reactive({
  estudiantesTotal: 0,
  estudiantesActivos: 0,
  estudiantesInactivos: 0,
  materiasTotal: 0,
  materiasActivas: 0,
  materiasInactivas: 0,
  usuariosTotal: 0,
  usuariosActivos: 0,
  secretariasActivas: 0,
  profesoresActivos: 0,
  asignaciones: 0
})

const nombre = computed(() => nombreUsuarioSesion())
const saludo = computed(() => {
  const hora = new Date().getHours()
  if (hora < 12) return 'Buenos días'
  if (hora < 19) return 'Buenas tardes'
  return 'Buenas noches'
})
const fechaActual = formatearFechaLarga(new Date())

const saludOperativa = computed(
  () => salud.value?.database === 'Connected' && salud.value?.status === 'Healthy'
)

const accionesRapidas = [
  {
    to: '/secretaria/estudiantes?nuevo=1',
    icono: 'person-plus',
    titulo: 'Registrar estudiante',
    texto: 'Incorpore un nuevo estudiante al sistema.'
  },
  {
    to: '/secretaria/materias?nuevo=1',
    icono: 'journal-plus',
    titulo: 'Registrar materia',
    texto: 'Cree una materia en el catálogo académico.'
  },
  {
    to: '/secretaria/usuarios?nuevo=1',
    icono: 'person-badge',
    titulo: 'Registrar usuario',
    texto: 'Cree una cuenta de Secretaría o Profesor.'
  },
  {
    to: '/secretaria/asignaciones?nuevo=1',
    icono: 'diagram-3',
    titulo: 'Asignar materia',
    texto: 'Vincule una materia con un profesor activo.'
  },
  {
    to: '/secretaria/boletines',
    icono: 'file-earmark-text',
    titulo: 'Consultar boletín',
    texto: 'Revise las notas y el estado académico.'
  }
]

async function cargarResumen() {
  cargando.value = true
  errorCarga.value = ''

  try {
    const [estudiantes, materias, usuarios, asignaciones] = await Promise.all([
      listarEstudiantes(true),
      listarMaterias(true),
      listarUsuarios({ incluirInactivos: true }),
      listarAsignaciones()
    ])

    resumen.estudiantesTotal = estudiantes.length
    resumen.estudiantesActivos = estudiantes.filter((elemento) => elemento.activo).length
    resumen.estudiantesInactivos = resumen.estudiantesTotal - resumen.estudiantesActivos

    resumen.materiasTotal = materias.length
    resumen.materiasActivas = materias.filter((elemento) => elemento.activo).length
    resumen.materiasInactivas = resumen.materiasTotal - resumen.materiasActivas

    resumen.usuariosTotal = usuarios.length
    resumen.usuariosActivos = usuarios.filter((elemento) => elemento.activo).length
    resumen.secretariasActivas = usuarios.filter(
      (elemento) => elemento.activo && Number(elemento.rolId) === 1
    ).length
    resumen.profesoresActivos = usuarios.filter(
      (elemento) => elemento.activo && Number(elemento.rolId) === 2
    ).length

    resumen.asignaciones = asignaciones.length
  } catch (error) {
    errorCarga.value = mensajeDeError(
      error,
      'No fue posible cargar el resumen del sistema.'
    )
  } finally {
    cargando.value = false
  }
}

async function cargarSalud() {
  cargandoSalud.value = true
  errorSalud.value = ''

  try {
    salud.value = await verificarSaludApi()
  } catch (error) {
    salud.value = null
    errorSalud.value = mensajeDeError(error, 'No fue posible verificar la API.')
  } finally {
    cargandoSalud.value = false
  }
}

onMounted(() => {
  cargarResumen()
  cargarSalud()
})
</script>

<template>
  <div>
    <PageHeader
      titulo="Panel principal"
      descripcion="Resumen del estado académico y administrativo del colegio."
    >
      <template #acciones>
        <button
          type="button"
          class="btn btn-light-snc btn-sm"
          :disabled="cargando"
          @click="cargarResumen"
        >
          <i class="bi bi-arrow-clockwise" :class="{ spin: cargando }"></i>
          Actualizar
        </button>
      </template>
    </PageHeader>

    <div v-if="errorCarga" class="alert alert-danger d-flex gap-2" role="alert">
      <i class="bi bi-exclamation-octagon-fill mt-1"></i>
      <div class="flex-grow-1">
        {{ errorCarga }}
        <div class="mt-2">
          <button type="button" class="btn btn-sm btn-outline-danger" @click="cargarResumen">
            <i class="bi bi-arrow-clockwise"></i>
            Reintentar
          </button>
        </div>
      </div>
    </div>

    <section class="hero-card mb-4">
      <div class="hero-icon"><i class="bi bi-buildings"></i></div>
      <span class="hero-badge">
        <i class="bi bi-shield-check"></i>
        Sesión de Secretaría
      </span>
      <h2>{{ saludo }}, {{ nombre }}</h2>
      <p>
        {{ fechaActual }}. Desde este panel puede administrar estudiantes, materias,
        usuarios, asignaciones y consultar los boletines académicos.
      </p>
      <div class="hero-actions">
        <RouterLink to="/secretaria/estudiantes?nuevo=1" class="btn btn-light-snc btn-sm">
          <i class="bi bi-person-plus"></i>
          Nuevo estudiante
        </RouterLink>
        <RouterLink to="/secretaria/boletines" class="btn btn-outline-light btn-sm">
          <i class="bi bi-file-earmark-text"></i>
          Consultar boletín
        </RouterLink>
      </div>
    </section>

    <div class="row g-3 mb-4">
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Estudiantes activos"
          :valor="resumen.estudiantesActivos"
          icono="people"
          :pista="`${resumen.estudiantesTotal} estudiantes registrados en total`"
          :cargando="cargando"
        />
      </div>
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Materias activas"
          :valor="resumen.materiasActivas"
          icono="journal-bookmark"
          variante="success"
          :pista="`${resumen.materiasTotal} materias en el catálogo`"
          :cargando="cargando"
        />
      </div>
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Profesores activos"
          :valor="resumen.profesoresActivos"
          icono="person-video3"
          variante="violet"
          :pista="`${resumen.usuariosActivos} usuarios activos en el sistema`"
          :cargando="cargando"
        />
      </div>
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Asignaciones"
          :valor="resumen.asignaciones"
          icono="diagram-3"
          variante="warning"
          pista="Materias vinculadas a profesores"
          :cargando="cargando"
        />
      </div>
    </div>

    <div class="row g-3">
      <div class="col-12 col-lg-7">
        <div class="card-snc h-100">
          <div class="card-snc-header">
            <div>
              <h2>Accesos rápidos</h2>
              <p>Operaciones frecuentes de la Secretaría.</p>
            </div>
          </div>
          <div class="card-snc-body">
            <div class="row g-3">
              <div v-for="accion in accionesRapidas" :key="accion.to" class="col-12 col-md-6">
                <RouterLink :to="accion.to" class="quick-action">
                  <div class="quick-icon">
                    <i class="bi" :class="`bi-${accion.icono}`"></i>
                  </div>
                  <div>
                    <strong>{{ accion.titulo }}</strong>
                    <small>{{ accion.texto }}</small>
                  </div>
                </RouterLink>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 col-lg-5">
        <div class="card-snc h-100">
          <div class="card-snc-header">
            <div>
              <h2>Estado del sistema</h2>
              <p>Verificación real de la API y la base de datos.</p>
            </div>
            <button
              type="button"
              class="btn btn-light-snc btn-icon"
              title="Verificar nuevamente"
              :disabled="cargandoSalud"
              @click="cargarSalud"
            >
              <i class="bi bi-arrow-clockwise" :class="{ spin: cargandoSalud }"></i>
            </button>
          </div>
          <div class="card-snc-body">
            <div v-if="errorSalud" class="callout danger mb-3">
              <i class="bi bi-wifi-off"></i>
              <span>{{ errorSalud }}</span>
            </div>

            <div v-else class="d-flex flex-wrap align-items-center gap-2 mb-3">
              <span class="badge-estado" :class="saludOperativa ? 'activo' : 'reprobado'">
                {{ saludOperativa ? 'API operativa' : 'API no disponible' }}
              </span>
              <span class="small-85 text-snc-muted">
                Base de datos:
                {{ salud?.database === 'Connected' ? 'conectada' : 'sin conexión' }}
              </span>
            </div>

            <div v-if="salud?.timestamp" class="small-85 text-snc-muted mb-3">
              Última verificación: {{ formatearFechaHora(salud.timestamp) }}
            </div>

            <div class="data-list">
              <div class="data-row">
                <span class="data-row-label">
                  <i class="bi bi-person-badge text-snc-primary"></i>
                  Secretarías activas
                </span>
                <span class="data-row-value">{{ resumen.secretariasActivas }}</span>
              </div>
              <div class="data-row">
                <span class="data-row-label">
                  <i class="bi bi-person-video3 text-snc-primary"></i>
                  Profesores activos
                </span>
                <span class="data-row-value">{{ resumen.profesoresActivos }}</span>
              </div>
              <div class="data-row">
                <span class="data-row-label">
                  <i class="bi bi-people text-snc-primary"></i>
                  Estudiantes inactivos
                </span>
                <span class="data-row-value">{{ resumen.estudiantesInactivos }}</span>
              </div>
              <div class="data-row">
                <span class="data-row-label">
                  <i class="bi bi-journal-x text-snc-primary"></i>
                  Materias inactivas
                </span>
                <span class="data-row-value">{{ resumen.materiasInactivas }}</span>
              </div>
              <div class="data-row">
                <span class="data-row-label">
                  <i class="bi bi-people-fill text-snc-primary"></i>
                  Usuarios registrados
                </span>
                <span class="data-row-value">{{ resumen.usuariosTotal }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
