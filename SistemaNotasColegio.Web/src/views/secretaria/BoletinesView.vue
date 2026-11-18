<script setup>
import { computed, onMounted, ref } from 'vue'
import BoletinAcademico from '../../components/BoletinAcademico.vue'
import EmptyState from '../../components/EmptyState.vue'
import PageHeader from '../../components/PageHeader.vue'
import SearchBox from '../../components/SearchBox.vue'
import { mensajeDeError } from '../../services/api'
import { obtenerBoletin } from '../../services/boletinesService'
import { listarEstudiantes } from '../../services/estudiantesService'
import { toast } from '../../stores/toast'
import { coincideBusqueda, iniciales, nombreCompleto } from '../../utils/format'

const estudiantes = ref([])
const cargandoEstudiantes = ref(true)
const errorEstudiantes = ref('')
const busqueda = ref('')

const seleccionado = ref(null)
const boletin = ref(null)
const cargandoBoletin = ref(false)
const errorBoletin = ref('')

const filtrados = computed(() =>
  estudiantes.value.filter((estudiante) =>
    coincideBusqueda(`${estudiante.cedula} ${estudiante.nombres} ${estudiante.apellidos}`, busqueda.value)
  )
)

const nombreSeleccionado = computed(() =>
  seleccionado.value ? nombreCompleto(seleccionado.value) : ''
)

async function cargarEstudiantes() {
  cargandoEstudiantes.value = true
  errorEstudiantes.value = ''

  try {
    // Se incluyen inactivos porque la API conserva y permite consultar su historial académico.
    estudiantes.value = await listarEstudiantes(true)
  } catch (error) {
    estudiantes.value = []
    errorEstudiantes.value = mensajeDeError(error, 'No fue posible cargar los estudiantes.')
  } finally {
    cargandoEstudiantes.value = false
  }
}

async function seleccionar(estudiante) {
  seleccionado.value = estudiante
  boletin.value = null
  errorBoletin.value = ''
  cargandoBoletin.value = true

  try {
    boletin.value = await obtenerBoletin(estudiante.id)
  } catch (error) {
    errorBoletin.value = mensajeDeError(
      error,
      'No fue posible obtener el boletín del estudiante seleccionado.'
    )
  } finally {
    cargandoBoletin.value = false
  }
}

function imprimir() {
  if (!boletin.value) return
  window.print()
}

onMounted(cargarEstudiantes)
</script>

<template>
  <div>
    <PageHeader
      titulo="Boletines académicos"
      descripcion="Consulte las calificaciones trimestrales, el promedio anual y el estado académico."
    >
      <template #acciones>
        <button
          type="button"
          class="btn btn-primary"
          :disabled="!boletin || cargandoBoletin"
          title="Imprimir el boletín seleccionado"
          @click="imprimir"
        >
          <i class="bi bi-printer"></i>
          Imprimir boletín
        </button>
      </template>
    </PageHeader>

    <div class="boletin-layout">
      <div class="card-snc boletin-search-panel no-print">
        <div class="card-snc-header">
          <div>
            <h2>Estudiantes</h2>
            <p>Seleccione un estudiante para consultar su boletín.</p>
          </div>
        </div>

        <div class="p-3 pb-2">
          <SearchBox
            v-model="busqueda"
            placeholder="Buscar por cédula o nombre…"
            etiqueta="Buscar estudiantes para el boletín"
          />
        </div>

        <div v-if="cargandoEstudiantes" class="table-loading">
          <span class="spinner-border spinner-border-sm me-2" role="status"></span>
          Cargando estudiantes…
        </div>

        <div v-else-if="errorEstudiantes" class="p-3">
          <div class="alert alert-danger mb-0" role="alert">
            <div class="d-flex gap-2">
              <i class="bi bi-exclamation-octagon-fill mt-1"></i>
              <div>{{ errorEstudiantes }}</div>
            </div>
            <button type="button" class="btn btn-sm btn-outline-danger mt-2" @click="cargarEstudiantes">
              <i class="bi bi-arrow-clockwise"></i>
              Reintentar
            </button>
          </div>
        </div>

        <EmptyState
          v-else-if="filtrados.length === 0"
          icono="person-lines-fill"
          :titulo="busqueda ? 'Sin resultados' : 'No hay estudiantes registrados'"
          :mensaje="
            busqueda
              ? 'No se encontraron estudiantes que coincidan con la búsqueda.'
              : 'Registre estudiantes en la sección Estudiantes para consultar sus boletines.'
          "
        />

        <div v-else class="student-list">
          <button
            v-for="estudiante in filtrados"
            :key="estudiante.id"
            type="button"
            class="student-item"
            :class="{ seleccionado: seleccionado?.id === estudiante.id }"
            @click="seleccionar(estudiante)"
          >
            <span class="student-avatar">{{ iniciales(estudiante) }}</span>
            <span class="flex-grow-1 min-w-0">
              <span class="student-name">{{ nombreCompleto(estudiante) }}</span>
              <span class="student-meta">
                Cédula {{ estudiante.cedula }} · {{ estudiante.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </span>
            <i class="bi bi-chevron-right text-snc-muted"></i>
          </button>
        </div>
      </div>

      <div>
        <div v-if="cargandoBoletin" class="card-snc boletin-placeholder no-print">
          <div class="text-center">
            <span class="spinner-border text-primary mb-3" role="status"></span>
            <p class="text-snc-muted mb-0">Generando el boletín de {{ nombreSeleccionado }}…</p>
          </div>
        </div>

        <div v-else-if="errorBoletin" class="card-snc no-print">
          <div class="p-4">
            <div class="alert alert-danger mb-0" role="alert">
              <div class="d-flex gap-2">
                <i class="bi bi-exclamation-octagon-fill mt-1"></i>
                <div>{{ errorBoletin }}</div>
              </div>
              <button
                v-if="seleccionado"
                type="button"
                class="btn btn-sm btn-outline-danger mt-2"
                @click="seleccionar(seleccionado)"
              >
                <i class="bi bi-arrow-clockwise"></i>
                Reintentar
              </button>
            </div>
          </div>
        </div>

        <div v-else-if="boletin" class="print-area">
          <BoletinAcademico :boletin="boletin" />
        </div>

        <div v-else class="card-snc boletin-placeholder no-print">
          <EmptyState
            icono="file-earmark-text"
            titulo="Seleccione un estudiante"
            mensaje="El boletín mostrará las notas de los tres trimestres, el promedio anual y el estado de cada materia."
          />
        </div>
      </div>
    </div>
  </div>
</template>
