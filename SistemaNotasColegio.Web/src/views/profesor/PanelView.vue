<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import PageHeader from '../../components/PageHeader.vue'
import StatCard from '../../components/StatCard.vue'
import { mensajeDeError } from '../../services/api'
import { listarCalificaciones } from '../../services/calificacionesService'
import { listarMisAsignaciones } from '../../services/profesoresService'
import { estadoSesion, nombreUsuarioSesion } from '../../stores/auth'
import {
  etiquetaTrimestre,
  formatearFechaLarga,
  formatearFechaHora,
  formatearNota
} from '../../utils/format'

const asignaciones = ref([])
const calificaciones = ref([])
const cargando = ref(true)
const errorCarga = ref('')

const resumen = reactive({
  asignaciones: 0,
  materiasActivas: 0,
  calificaciones: 0,
  estudiantes: 0
})

const nombre = computed(() => nombreUsuarioSesion())
const saludo = computed(() => {
  const hora = new Date().getHours()
  if (hora < 12) return 'Buenos días'
  if (hora < 19) return 'Buenas tardes'
  return 'Buenas noches'
})
const fechaActual = formatearFechaLarga(new Date())

const recientes = computed(() =>
  calificaciones.value
    .slice()
    .sort((a, b) => {
      const fechaA = new Date(a.fechaActualizacion ?? a.fechaRegistro).getTime()
      const fechaB = new Date(b.fechaActualizacion ?? b.fechaRegistro).getTime()
      return fechaB - fechaA
    })
    .slice(0, 6)
)

const sinAsignaciones = computed(() => !cargando.value && asignaciones.value.length === 0)

async function cargar() {
  cargando.value = true
  errorCarga.value = ''

  try {
    const [misAsignaciones, misCalificaciones] = await Promise.all([
      listarMisAsignaciones(),
      listarCalificaciones()
    ])

    asignaciones.value = misAsignaciones
    calificaciones.value = misCalificaciones

    resumen.asignaciones = misAsignaciones.length
    resumen.materiasActivas = misAsignaciones.filter((elemento) => elemento.materiaActiva).length
    resumen.calificaciones = misCalificaciones.length
    resumen.estudiantes = new Set(misCalificaciones.map((elemento) => elemento.estudianteId)).size
  } catch (error) {
    errorCarga.value = mensajeDeError(
      error,
      'No fue posible cargar la información del panel del profesor.'
    )
  } finally {
    cargando.value = false
  }
}

onMounted(cargar)
</script>

<template>
  <div>
    <PageHeader
      titulo="Panel del profesor"
      descripcion="Resumen de sus materias asignadas y calificaciones registradas."
    >
      <template #acciones>
        <button
          type="button"
          class="btn btn-light-snc btn-sm"
          :disabled="cargando"
          @click="cargar"
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
          <button type="button" class="btn btn-sm btn-outline-danger" @click="cargar">
            <i class="bi bi-arrow-clockwise"></i>
            Reintentar
          </button>
        </div>
      </div>
    </div>

    <section class="hero-card mb-4">
      <div class="hero-icon"><i class="bi bi-person-video3"></i></div>
      <span class="hero-badge">
        <i class="bi bi-shield-check"></i>
        Sesión de Profesor
      </span>
      <h2>{{ saludo }}, {{ nombre }}</h2>
      <p>
        {{ fechaActual }}. Registre y corrija las calificaciones trimestrales de las materias
        que tiene asignadas.
      </p>
      <div class="hero-actions">
        <RouterLink to="/profesor/calificaciones?nuevo=1" class="btn btn-light-snc btn-sm">
          <i class="bi bi-clipboard2-plus"></i>
          Registrar calificación
        </RouterLink>
        <RouterLink to="/profesor/asignaciones" class="btn btn-outline-light btn-sm">
          <i class="bi bi-journal-check"></i>
          Ver mis asignaciones
        </RouterLink>
      </div>
    </section>

    <div class="row g-3 mb-4">
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Materias asignadas"
          :valor="resumen.asignaciones"
          icono="journal-check"
          :pista="`${resumen.materiasActivas} materias activas`"
          :cargando="cargando"
        />
      </div>
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Calificaciones registradas"
          :valor="resumen.calificaciones"
          icono="clipboard2-check"
          variante="success"
          pista="Notas registradas por usted"
          :cargando="cargando"
        />
      </div>
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Estudiantes calificados"
          :valor="resumen.estudiantes"
          icono="people"
          variante="violet"
          pista="Estudiantes con al menos una nota"
          :cargando="cargando"
        />
      </div>
      <div class="col-12 col-sm-6 col-xl-3">
        <StatCard
          etiqueta="Usuario en sesión"
          :valor="estadoSesion.usuario?.nombreUsuario ?? '—'"
          icono="person-badge"
          variante="warning"
          pista="Profesor autenticado"
          :cargando="cargando"
        />
      </div>
    </div>

    <div class="row g-3">
      <div class="col-12 col-lg-5">
        <div class="card-snc h-100">
          <div class="card-snc-header">
            <div>
              <h2>Accesos rápidos</h2>
              <p>Operaciones disponibles para su rol.</p>
            </div>
          </div>
          <div class="card-snc-body">
            <div class="d-flex flex-column gap-3">
              <RouterLink to="/profesor/calificaciones?nuevo=1" class="quick-action">
                <div class="quick-icon"><i class="bi bi-clipboard2-plus"></i></div>
                <div>
                  <strong>Registrar calificación</strong>
                  <small>Ingrese una nota de 0 a 100 para un trimestre.</small>
                </div>
              </RouterLink>

              <RouterLink to="/profesor/calificaciones" class="quick-action">
                <div class="quick-icon"><i class="bi bi-pencil-square"></i></div>
                <div>
                  <strong>Corregir calificaciones</strong>
                  <small>Actualice las notas que usted registró.</small>
                </div>
              </RouterLink>

              <RouterLink to="/profesor/asignaciones" class="quick-action">
                <div class="quick-icon"><i class="bi bi-journal-check"></i></div>
                <div>
                  <strong>Mis asignaciones</strong>
                  <small>Consulte las materias que tiene a cargo.</small>
                </div>
              </RouterLink>
            </div>

            <div v-if="sinAsignaciones" class="callout warning mt-3">
              <i class="bi bi-exclamation-triangle"></i>
              <span>
                Aún no tiene materias asignadas. Comuníquese con la Secretaría para que le asigne
                al menos una materia antes de registrar calificaciones.
              </span>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 col-lg-7">
        <div class="card-snc h-100">
          <div class="card-snc-header">
            <div>
              <h2>Calificaciones recientes</h2>
              <p>Últimas notas registradas o corregidas por usted.</p>
            </div>
            <RouterLink to="/profesor/calificaciones" class="btn btn-light-snc btn-sm">
              Ver todas
            </RouterLink>
          </div>

          <div v-if="cargando" class="table-loading">
            <span class="spinner-border spinner-border-sm me-2" role="status"></span>
            Cargando calificaciones…
          </div>

          <div v-else-if="recientes.length === 0" class="card-snc-body">
            <div class="empty-state py-4">
              <div class="empty-icon"><i class="bi bi-clipboard2"></i></div>
              <h3>Sin calificaciones registradas</h3>
              <p>Registre la primera nota para visualizarla en este resumen.</p>
            </div>
          </div>

          <div v-else class="table-responsive-snc">
            <table class="table-snc">
              <thead>
                <tr>
                  <th>Estudiante</th>
                  <th>Materia</th>
                  <th>Trimestre</th>
                  <th class="cell-right">Nota</th>
                  <th>Registro</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="calificacion in recientes" :key="calificacion.id">
                  <td>
                    <span class="cell-strong">{{ calificacion.estudianteNombreCompleto }}</span>
                    <span class="cell-sub">{{ calificacion.estudianteCedula }}</span>
                  </td>
                  <td>
                    <span class="cell-strong">{{ calificacion.materiaNombre }}</span>
                    <span class="cell-sub">{{ calificacion.materiaCodigo }}</span>
                  </td>
                  <td class="cell-nowrap">{{ etiquetaTrimestre(calificacion.trimestre) }}</td>
                  <td class="cell-right">
                    <span class="fw-700 text-snc-primary">{{ formatearNota(calificacion.nota) }}</span>
                  </td>
                  <td class="cell-nowrap">
                    {{ formatearFechaHora(calificacion.fechaActualizacion ?? calificacion.fechaRegistro) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
