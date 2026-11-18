<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import EmptyState from '../../components/EmptyState.vue'
import EstadoBadge from '../../components/EstadoBadge.vue'
import PageHeader from '../../components/PageHeader.vue'
import { mensajeDeError } from '../../services/api'
import { listarMisAsignaciones } from '../../services/profesoresService'
import { formatearFecha } from '../../utils/format'

const router = useRouter()

const asignaciones = ref([])
const cargando = ref(true)
const errorCarga = ref('')

const activas = computed(() => asignaciones.value.filter((elemento) => elemento.materiaActiva).length)
const inactivas = computed(() => asignaciones.value.length - activas.value)

async function cargar() {
  cargando.value = true
  errorCarga.value = ''

  try {
    asignaciones.value = await listarMisAsignaciones()
  } catch (error) {
    errorCarga.value = mensajeDeError(error, 'No fue posible cargar sus asignaciones.')
  } finally {
    cargando.value = false
  }
}

onMounted(cargar)

function verCalificaciones(asignacion) {
  router.push({ path: '/profesor/calificaciones', query: { materia: asignacion.materiaId } })
}
</script>

<template>
  <div>
    <PageHeader
      titulo="Mis asignaciones"
      descripcion="Materias que la Secretaría le ha asignado para el registro de calificaciones."
    >
      <template #acciones>
        <RouterLink to="/profesor/calificaciones" class="btn btn-primary">
          <i class="bi bi-clipboard2-check"></i>
          Ir a calificaciones
        </RouterLink>
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

    <div v-if="!cargando && !errorCarga && asignaciones.length > 0" class="card-snc mb-3">
      <div class="card-snc-body py-3">
        <div class="d-flex flex-wrap align-items-center gap-2">
          <span class="badge-suave">
            <i class="bi bi-journal-check"></i>
            {{ asignaciones.length }} materia{{ asignaciones.length === 1 ? '' : 's' }} asignada{{ asignaciones.length === 1 ? '' : 's' }}
          </span>
          <span class="badge-estado activo">{{ activas }} activa{{ activas === 1 ? '' : 's' }}</span>
          <span v-if="inactivas > 0" class="badge-estado inactivo">
            {{ inactivas }} inactiva{{ inactivas === 1 ? '' : 's' }}
          </span>
          <span class="small-85 text-snc-muted ms-auto">
            La identidad del profesor se obtiene de su sesión autenticada.
          </span>
        </div>
      </div>
    </div>

    <div v-if="cargando" class="card-snc">
      <div class="table-loading">
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        Cargando sus asignaciones…
      </div>
    </div>

    <div v-else-if="!errorCarga && asignaciones.length === 0" class="card-snc">
      <EmptyState
        icono="journal-x"
        titulo="No tiene materias asignadas"
        mensaje="Comuníquese con la Secretaría para que le asigne al menos una materia antes de registrar calificaciones."
      />
    </div>

    <div v-else-if="!errorCarga" class="row g-3">
      <div v-for="asignacion in asignaciones" :key="asignacion.asignacionId" class="col-12 col-md-6 col-xl-4">
        <div class="card-snc h-100">
          <div class="card-snc-body d-flex flex-column h-100">
            <div class="d-flex align-items-start justify-content-between gap-2 mb-2">
              <div class="quick-icon">
                <i class="bi bi-journal-bookmark"></i>
              </div>
              <EstadoBadge
                :activo="asignacion.materiaActiva"
                texto-activo="Materia activa"
                texto-inactivo="Materia inactiva"
              />
            </div>

            <h3 class="h6 mb-1">{{ asignacion.materiaNombre }}</h3>
            <div class="mb-2">
              <span class="badge-suave">{{ asignacion.materiaCodigo }}</span>
            </div>

            <div class="data-list mt-auto">
              <div class="data-row">
                <span class="data-row-label">
                  <i class="bi bi-calendar3 text-snc-primary"></i>
                  Asignada el
                </span>
                <span class="data-row-value">{{ formatearFecha(asignacion.fechaRegistro) }}</span>
              </div>
              <div class="data-row">
                <span class="data-row-label">
                  <i class="bi bi-hash text-snc-primary"></i>
                  Identificador
                </span>
                <span class="data-row-value">{{ asignacion.materiaId }}</span>
              </div>
            </div>

            <button
              type="button"
              class="btn btn-outline-primary btn-sm mt-3 w-100"
              :disabled="!asignacion.materiaActiva"
              @click="verCalificaciones(asignacion)"
            >
              <i class="bi bi-clipboard2-check"></i>
              Ver calificaciones
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
