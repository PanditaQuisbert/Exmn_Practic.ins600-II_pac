<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppModal from '../../components/AppModal.vue'
import EmptyState from '../../components/EmptyState.vue'
import PageHeader from '../../components/PageHeader.vue'
import PaginationBar from '../../components/PaginationBar.vue'
import SearchBox from '../../components/SearchBox.vue'
import { usePaginacion } from '../../composables/usePaginacion'
import {
  crearAsignacion,
  eliminarAsignacion,
  listarAsignaciones
} from '../../services/asignacionesService'
import { mensajeDeError } from '../../services/api'
import { listarMaterias } from '../../services/materiasService'
import { listarUsuarios } from '../../services/usuariosService'
import { confirmar } from '../../stores/confirm'
import { toast } from '../../stores/toast'
import { consumirParametroNuevo, erroresDeCampos } from '../../utils/errors'
import { coincideBusqueda, formatearFecha } from '../../utils/format'
import { validarAsignacion } from '../../utils/validators'

const ruta = useRoute()
const router = useRouter()

const asignaciones = ref([])
const profesores = ref([])
const materias = ref([])
const cargando = ref(true)
const errorCarga = ref('')
const errorCatalogos = ref('')
const busqueda = ref('')
const filtroProfesor = ref('')
const filtroMateria = ref('')

const modalAbierta = ref(false)
const guardando = ref(false)
const errores = ref({})
const errorFormulario = ref('')
const detallesFormulario = ref([])
const eliminandoId = ref(null)

const formulario = reactive({
  profesorId: 0,
  materiaId: 0
})

const profesoresActivos = computed(() =>
  profesores.value
    .filter((profesor) => profesor.activo)
    .slice()
    .sort((a, b) => `${a.apellidos} ${a.nombres}`.localeCompare(`${b.apellidos} ${b.nombres}`))
)

const materiasActivas = computed(() =>
  materias.value
    .filter((materia) => materia.activo)
    .slice()
    .sort((a, b) => a.nombre.localeCompare(b.nombre))
)

const filtradas = computed(() =>
  asignaciones.value.filter((asignacion) => {
    if (filtroProfesor.value && Number(asignacion.profesorId) !== Number(filtroProfesor.value)) {
      return false
    }
    if (filtroMateria.value && Number(asignacion.materiaId) !== Number(filtroMateria.value)) {
      return false
    }

    return coincideBusqueda(
      `${asignacion.profesorNombre} ${asignacion.profesorNombreUsuario} ${asignacion.materiaCodigo} ${asignacion.materiaNombre}`,
      busqueda.value
    )
  })
)

const { pagina, total, totalPaginas, elementos, reiniciar } = usePaginacion(filtradas, 10)

watch([busqueda, filtroProfesor, filtroMateria], reiniciar)

function limpiarErrores() {
  errores.value = {}
  errorFormulario.value = ''
  detallesFormulario.value = []
}

function abrirCrear() {
  formulario.profesorId = 0
  formulario.materiaId = 0
  limpiarErrores()
  modalAbierta.value = true
}

function cerrarModal() {
  if (guardando.value) return
  modalAbierta.value = false
}

async function cargarCatalogos() {
  errorCatalogos.value = ''

  try {
    const [usuarios, catalogoMaterias] = await Promise.all([
      listarUsuarios({ rolId: 2, incluirInactivos: true }),
      listarMaterias(true)
    ])

    profesores.value = usuarios
    materias.value = catalogoMaterias
  } catch (error) {
    errorCatalogos.value = mensajeDeError(
      error,
      'No fue posible cargar los profesores y materias disponibles.'
    )
  }
}

async function cargar() {
  cargando.value = true
  errorCarga.value = ''

  try {
    asignaciones.value = await listarAsignaciones()
  } catch (error) {
    errorCarga.value = mensajeDeError(error, 'No fue posible cargar las asignaciones.')
  } finally {
    cargando.value = false
  }
}

async function guardar() {
  limpiarErrores()

  const validacion = validarAsignacion(formulario)
  errores.value = validacion
  if (Object.keys(validacion).length > 0) return

  guardando.value = true

  try {
    await crearAsignacion({
      profesorId: Number(formulario.profesorId),
      materiaId: Number(formulario.materiaId)
    })

    toast.success('Asignación registrada correctamente.')
    modalAbierta.value = false
    await cargar()
  } catch (error) {
    errorFormulario.value = mensajeDeError(error, 'No fue posible registrar la asignación.')
    detallesFormulario.value = error?.detalles ?? []
    errores.value = { ...errores.value, ...erroresDeCampos(error?.campos) }
  } finally {
    guardando.value = false
  }
}

async function eliminar(asignacion) {
  const confirmado = await confirmar({
    titulo: 'Eliminar asignación',
    mensaje: `¿Desea eliminar la asignación de ${asignacion.profesorNombre} en ${asignacion.materiaNombre}?`,
    detalle:
      'La API no permite eliminar asignaciones cuando el profesor ya registró calificaciones en la materia.',
    textoConfirmar: 'Eliminar',
    variante: 'danger',
    icono: 'trash3'
  })

  if (!confirmado) return

  eliminandoId.value = asignacion.id

  try {
    await eliminarAsignacion(asignacion.id)
    toast.success('Asignación eliminada correctamente.')
    await cargar()
  } catch (error) {
    toast.error(mensajeDeError(error, 'No fue posible eliminar la asignación.'))
  } finally {
    eliminandoId.value = null
  }
}

onMounted(async () => {
  await Promise.all([cargar(), cargarCatalogos()])

  if (consumirParametroNuevo(ruta, router)) {
    abrirCrear()
  }
})
</script>

<template>
  <div>
    <PageHeader
      titulo="Asignaciones de materias"
      descripcion="Vincule las materias del catálogo con los profesores activos del colegio."
    >
      <template #acciones>
        <button type="button" class="btn btn-primary" @click="abrirCrear">
          <i class="bi bi-diagram-3"></i>
          Nueva asignación
        </button>
      </template>
    </PageHeader>

    <div v-if="errorCatalogos" class="alert alert-warning d-flex gap-2" role="alert">
      <i class="bi bi-exclamation-triangle-fill mt-1"></i>
      <div class="flex-grow-1">
        {{ errorCatalogos }}
        <button type="button" class="btn btn-sm btn-light-snc ms-2" @click="cargarCatalogos">
          Reintentar
        </button>
      </div>
    </div>

    <div class="card-snc">
      <div class="list-toolbar">
        <div class="toolbar-group">
          <SearchBox
            v-model="busqueda"
            placeholder="Buscar por profesor o materia…"
            etiqueta="Buscar asignaciones"
          />
        </div>

        <div class="toolbar-group">
          <span class="table-count">{{ total }} registro{{ total === 1 ? '' : 's' }}</span>

          <select
            v-model="filtroProfesor"
            class="form-select form-select-sm"
            aria-label="Filtrar por profesor"
          >
            <option value="">Todos los profesores</option>
            <option v-for="profesor in profesores" :key="profesor.id" :value="profesor.id">
              {{ profesor.nombres }} {{ profesor.apellidos }}{{ profesor.activo ? '' : ' (inactivo)' }}
            </option>
          </select>

          <select
            v-model="filtroMateria"
            class="form-select form-select-sm"
            aria-label="Filtrar por materia"
          >
            <option value="">Todas las materias</option>
            <option v-for="materia in materias" :key="materia.id" :value="materia.id">
              {{ materia.codigo }} · {{ materia.nombre }}{{ materia.activo ? '' : ' (inactiva)' }}
            </option>
          </select>
        </div>
      </div>

      <div v-if="cargando" class="table-loading">
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        Cargando asignaciones…
      </div>

      <div v-else-if="errorCarga" class="p-3">
        <div class="alert alert-danger d-flex gap-2 mb-0" role="alert">
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
      </div>

      <template v-else>
        <EmptyState
          v-if="elementos.length === 0"
          icono="diagram-3"
          :titulo="busqueda || filtroProfesor || filtroMateria ? 'Sin resultados' : 'No hay asignaciones registradas'"
          :mensaje="
            busqueda || filtroProfesor || filtroMateria
              ? 'No se encontraron asignaciones que coincidan con los filtros seleccionados.'
              : 'Asigne una materia a un profesor para habilitar el registro de calificaciones.'
          "
        >
          <button
            v-if="busqueda || filtroProfesor || filtroMateria"
            type="button"
            class="btn btn-light-snc btn-sm"
            @click="
              busqueda = '';
              filtroProfesor = '';
              filtroMateria = ''
            "
          >
            <i class="bi bi-x-circle"></i>
            Limpiar filtros
          </button>
          <button v-else type="button" class="btn btn-primary btn-sm" @click="abrirCrear">
            <i class="bi bi-diagram-3"></i>
            Registrar asignación
          </button>
        </EmptyState>

        <template v-else>
          <div class="table-responsive-snc">
            <table class="table-snc">
              <thead>
                <tr>
                  <th>Profesor</th>
                  <th>Materia</th>
                  <th>Fecha de asignación</th>
                  <th class="cell-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="asignacion in elementos" :key="asignacion.id">
                  <td>
                    <span class="cell-strong">{{ asignacion.profesorNombre }}</span>
                    <span class="cell-sub">{{ asignacion.profesorNombreUsuario }}</span>
                  </td>
                  <td>
                    <span class="cell-strong">{{ asignacion.materiaNombre }}</span>
                    <span class="cell-sub">{{ asignacion.materiaCodigo }}</span>
                  </td>
                  <td class="cell-nowrap">{{ formatearFecha(asignacion.fechaRegistro) }}</td>
                  <td>
                    <div class="table-actions">
                      <button
                        type="button"
                        class="btn btn-danger-soft btn-icon"
                        title="Eliminar asignación"
                        aria-label="Eliminar asignación"
                        :disabled="eliminandoId === asignacion.id"
                        @click="eliminar(asignacion)"
                      >
                        <span
                          v-if="eliminandoId === asignacion.id"
                          class="spinner-border spinner-border-sm"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        <i v-else class="bi bi-trash3"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <PaginationBar
            v-model:pagina="pagina"
            :total-paginas="totalPaginas"
            :total="total"
            :tamano="10"
          />
        </template>
      </template>
    </div>

    <AppModal
      :abierta="modalAbierta"
      titulo="Nueva asignación"
      subtitulo="Seleccione un profesor activo y una materia activa."
      :bloquear-cierre="guardando"
      @cerrar="cerrarModal"
    >
      <div v-if="errorFormulario" class="alert alert-danger" role="alert">
        <div class="d-flex gap-2">
          <i class="bi bi-exclamation-octagon-fill mt-1"></i>
          <div>{{ errorFormulario }}</div>
        </div>
        <ul v-if="detallesFormulario.length" class="mt-2 mb-0">
          <li v-for="(detalle, indice) in detallesFormulario" :key="indice">{{ detalle }}</li>
        </ul>
      </div>

      <form id="formularioAsignacion" novalidate @submit.prevent="guardar">
        <div class="row g-3">
          <div class="col-12">
            <label for="asignacionProfesor" class="form-label">
              Profesor<span class="required-mark">*</span>
            </label>
            <select
              id="asignacionProfesor"
              v-model.number="formulario.profesorId"
              class="form-select"
              :class="{ 'is-invalid': errores.profesorId }"
              @change="errores.profesorId = ''"
            >
              <option :value="0" disabled>Seleccione un profesor activo</option>
              <option v-for="profesor in profesoresActivos" :key="profesor.id" :value="profesor.id">
                {{ profesor.nombres }} {{ profesor.apellidos }} · {{ profesor.nombreUsuario }}
              </option>
            </select>
            <div v-if="errores.profesorId" class="invalid-feedback">{{ errores.profesorId }}</div>
            <div v-else-if="profesoresActivos.length === 0" class="form-text text-danger">
              No hay profesores activos disponibles. Cree o active un profesor en Usuarios.
            </div>
          </div>

          <div class="col-12">
            <label for="asignacionMateria" class="form-label">
              Materia<span class="required-mark">*</span>
            </label>
            <select
              id="asignacionMateria"
              v-model.number="formulario.materiaId"
              class="form-select"
              :class="{ 'is-invalid': errores.materiaId }"
              @change="errores.materiaId = ''"
            >
              <option :value="0" disabled>Seleccione una materia activa</option>
              <option v-for="materia in materiasActivas" :key="materia.id" :value="materia.id">
                {{ materia.codigo }} · {{ materia.nombre }}
              </option>
            </select>
            <div v-if="errores.materiaId" class="invalid-feedback">{{ errores.materiaId }}</div>
            <div v-else-if="materiasActivas.length === 0" class="form-text text-danger">
              No hay materias activas disponibles. Cree o active una materia en Materias.
            </div>
          </div>
        </div>

        <div class="callout mt-3">
          <i class="bi bi-info-circle"></i>
          <span>
            Solo se muestran profesores y materias activos. La API valida que la asignación no
            esté duplicada.
          </span>
        </div>
      </form>

      <template #footer>
        <button type="button" class="btn btn-light-snc" :disabled="guardando" @click="cerrarModal">
          Cancelar
        </button>
        <button
          type="submit"
          form="formularioAsignacion"
          class="btn btn-primary"
          :disabled="guardando || profesoresActivos.length === 0 || materiasActivas.length === 0"
        >
          <span
            v-if="guardando"
            class="spinner-border spinner-border-sm"
            role="status"
            aria-hidden="true"
          ></span>
          <i v-else class="bi bi-check2-circle"></i>
          {{ guardando ? 'Guardando…' : 'Registrar asignación' }}
        </button>
      </template>
    </AppModal>
  </div>
</template>
