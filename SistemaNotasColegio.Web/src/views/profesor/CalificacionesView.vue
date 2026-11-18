<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppModal from '../../components/AppModal.vue'
import EmptyState from '../../components/EmptyState.vue'
import PageHeader from '../../components/PageHeader.vue'
import PaginationBar from '../../components/PaginationBar.vue'
import SearchBox from '../../components/SearchBox.vue'
import { usePaginacion } from '../../composables/usePaginacion'
import { mensajeDeError } from '../../services/api'
import {
  actualizarCalificacion,
  crearCalificacion,
  listarCalificaciones
} from '../../services/calificacionesService'
import { listarMisAsignaciones } from '../../services/profesoresService'
import { estadoSesion } from '../../stores/auth'
import { toast } from '../../stores/toast'
import { erroresDeCampos } from '../../utils/errors'
import {
  coincideBusqueda,
  etiquetaTrimestre,
  etiquetaTrimestreCorta,
  formatearFechaHora,
  formatearNota
} from '../../utils/format'
import { validarCalificacion, validarNota } from '../../utils/validators'

const ruta = useRoute()

const calificaciones = ref([])
const asignaciones = ref([])
const cargando = ref(true)
const errorCarga = ref('')
const errorAsignaciones = ref('')

const busqueda = ref('')
const filtroMateria = ref('')
const filtroTrimestre = ref('')

const modalAbierta = ref(false)
const modoEdicion = ref(false)
const calificacionEditando = ref(null)
const guardando = ref(false)
const errores = ref({})
const errorFormulario = ref('')
const detallesFormulario = ref([])

const formulario = reactive({
  estudianteId: '',
  materiaId: 0,
  trimestre: 0,
  nota: ''
})

const numeroUsuario = computed(() => Number(estadoSesion.usuario?.id ?? 0))

const materiasAsignadas = computed(() => {
  const mapa = new Map()
  for (const asignacion of asignaciones.value) {
    if (!mapa.has(asignacion.materiaId)) mapa.set(asignacion.materiaId, asignacion)
  }
  return Array.from(mapa.values()).sort((a, b) => a.materiaNombre.localeCompare(b.materiaNombre))
})

const materiasActivas = computed(() => materiasAsignadas.value.filter((elemento) => elemento.materiaActiva))

const estudiantesConocidos = computed(() => {
  const mapa = new Map()
  for (const calificacion of calificaciones.value) {
    if (!mapa.has(calificacion.estudianteId)) {
      mapa.set(calificacion.estudianteId, {
        id: calificacion.estudianteId,
        cedula: calificacion.estudianteCedula,
        nombre: calificacion.estudianteNombreCompleto
      })
    }
  }
  return Array.from(mapa.values()).sort((a, b) => a.nombre.localeCompare(b.nombre))
})

const estudianteDetectado = computed(() => {
  const id = Number(formulario.estudianteId)
  if (!id) return null
  return estudiantesConocidos.value.find((elemento) => elemento.id === id) ?? null
})

const filtradas = computed(() =>
  calificaciones.value.filter((calificacion) => {
    if (filtroMateria.value && Number(calificacion.materiaId) !== Number(filtroMateria.value)) {
      return false
    }
    if (filtroTrimestre.value && Number(calificacion.trimestre) !== Number(filtroTrimestre.value)) {
      return false
    }

    return coincideBusqueda(
      `${calificacion.estudianteNombreCompleto} ${calificacion.estudianteCedula} ${calificacion.estudianteId} ${calificacion.materiaNombre} ${calificacion.materiaCodigo}`,
      busqueda.value
    )
  })
)

const { pagina, total, totalPaginas, elementos, reiniciar } = usePaginacion(filtradas, 10)

watch([busqueda, filtroMateria, filtroTrimestre], reiniciar)

const propias = computed(
  () => calificaciones.value.filter((elemento) => esPropia(elemento)).length
)
const ajenas = computed(() => calificaciones.value.length - propias.value)

function esPropia(calificacion) {
  return Number(calificacion.registradoPorUsuarioId) === numeroUsuario.value
}

function limpiarErrores() {
  errores.value = {}
  errorFormulario.value = ''
  detallesFormulario.value = []
}

function abrirCrear() {
  modoEdicion.value = false
  calificacionEditando.value = null
  formulario.estudianteId = ''
  formulario.materiaId = materiasActivas.value.length === 1 ? materiasActivas.value[0].materiaId : 0
  formulario.trimestre = 0
  formulario.nota = ''
  limpiarErrores()
  modalAbierta.value = true
}

function abrirEditar(calificacion) {
  if (!esPropia(calificacion)) return

  modoEdicion.value = true
  calificacionEditando.value = calificacion
  formulario.estudianteId = calificacion.estudianteId
  formulario.materiaId = calificacion.materiaId
  formulario.trimestre = calificacion.trimestre
  formulario.nota = String(calificacion.nota)
  limpiarErrores()
  modalAbierta.value = true
}

function cerrarModal() {
  if (guardando.value) return
  modalAbierta.value = false
}

function buscarDuplicado(estudianteId, materiaId, trimestre, excluirId = null) {
  return calificaciones.value.find(
    (elemento) =>
      Number(elemento.estudianteId) === Number(estudianteId) &&
      Number(elemento.materiaId) === Number(materiaId) &&
      Number(elemento.trimestre) === Number(trimestre) &&
      Number(elemento.id) !== Number(excluirId)
  )
}

async function cargarCalificaciones() {
  cargando.value = true
  errorCarga.value = ''

  try {
    calificaciones.value = await listarCalificaciones()
  } catch (error) {
    errorCarga.value = mensajeDeError(error, 'No fue posible cargar las calificaciones.')
  } finally {
    cargando.value = false
  }
}

async function cargarAsignaciones() {
  errorAsignaciones.value = ''

  try {
    asignaciones.value = await listarMisAsignaciones()
  } catch (error) {
    errorAsignaciones.value = mensajeDeError(
      error,
      'No fue posible cargar sus materias asignadas.'
    )
  }
}

async function guardar() {
  limpiarErrores()

  const validacion = modoEdicion.value
    ? (() => {
        const resultado = {}
        const errorNota = validarNota(formulario.nota)
        if (errorNota) resultado.nota = errorNota
        return resultado
      })()
    : validarCalificacion(formulario)

  errores.value = validacion
  if (Object.keys(validacion).length > 0) return

  if (!modoEdicion.value) {
    const duplicado = buscarDuplicado(
      formulario.estudianteId,
      formulario.materiaId,
      formulario.trimestre
    )

    if (duplicado) {
      errorFormulario.value =
        'Ya existe una calificación para ese estudiante, materia y trimestre. ' +
        'Si necesita cambiarla, use la opción Corregir sobre el registro existente.'
      return
    }

    const materia = materiasAsignadas.value.find(
      (elemento) => Number(elemento.materiaId) === Number(formulario.materiaId)
    )

    if (!materia) {
      errorFormulario.value = 'La materia seleccionada no está entre sus asignaciones.'
      return
    }

    if (!materia.materiaActiva) {
      errorFormulario.value = 'La materia seleccionada está inactiva y no permite nuevas notas.'
      return
    }
  }

  guardando.value = true

  try {
    if (modoEdicion.value) {
      await actualizarCalificacion(calificacionEditando.value.id, Number(formulario.nota))
      toast.success('Calificación corregida correctamente.')
    } else {
      await crearCalificacion({
        estudianteId: Number(formulario.estudianteId),
        materiaId: Number(formulario.materiaId),
        trimestre: Number(formulario.trimestre),
        nota: Number(formulario.nota)
      })
      toast.success('Calificación registrada correctamente.')
    }

    modalAbierta.value = false
    await cargarCalificaciones()
  } catch (error) {
    errorFormulario.value = mensajeDeError(
      error,
      modoEdicion.value
        ? 'No fue posible corregir la calificación.'
        : 'No fue posible registrar la calificación.'
    )
    detallesFormulario.value = error?.detalles ?? []
    errores.value = { ...errores.value, ...erroresDeCampos(error?.campos) }
  } finally {
    guardando.value = false
  }
}

async function cargarTodo() {
  await Promise.all([cargarCalificaciones(), cargarAsignaciones()])
}

onMounted(async () => {
  const materiaQuery = Number(ruta.query.materia)
  if (materiaQuery) filtroMateria.value = materiaQuery

  await cargarTodo()

  if (ruta.query.nuevo === '1') {
    abrirCrear()
  }
})
</script>

<template>
  <div>
    <PageHeader
      titulo="Calificaciones"
      descripcion="Registre y corrija las notas trimestrales de sus materias asignadas."
    >
      <template #acciones>
        <button type="button" class="btn btn-primary" @click="abrirCrear">
          <i class="bi bi-clipboard2-plus"></i>
          Registrar calificación
        </button>
      </template>
    </PageHeader>

    <div v-if="errorAsignaciones" class="alert alert-warning d-flex gap-2" role="alert">
      <i class="bi bi-exclamation-triangle-fill mt-1"></i>
      <div class="flex-grow-1">
        {{ errorAsignaciones }}
        <button type="button" class="btn btn-sm btn-light-snc ms-2" @click="cargarAsignaciones">
          Reintentar
        </button>
      </div>
    </div>

    <div class="row g-3 mb-3">
      <div class="col-12 col-md-4">
        <div class="card-snc">
          <div class="card-snc-body py-3 d-flex align-items-center gap-3">
            <div class="quick-icon"><i class="bi bi-journal-check"></i></div>
            <div>
              <div class="stat-value">{{ materiasAsignadas.length }}</div>
              <div class="stat-label">Materias a cargo</div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="card-snc">
          <div class="card-snc-body py-3 d-flex align-items-center gap-3">
            <div class="quick-icon"><i class="bi bi-pencil-square"></i></div>
            <div>
              <div class="stat-value">{{ propias }}</div>
              <div class="stat-label">Notas registradas por usted</div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="card-snc">
          <div class="card-snc-body py-3 d-flex align-items-center gap-3">
            <div class="quick-icon"><i class="bi bi-lock"></i></div>
            <div>
              <div class="stat-value">{{ ajenas }}</div>
              <div class="stat-label">Notas de solo lectura</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="card-snc">
      <div class="list-toolbar">
        <div class="toolbar-group">
          <SearchBox
            v-model="busqueda"
            placeholder="Buscar por estudiante, cédula o materia…"
            etiqueta="Buscar calificaciones"
          />
        </div>

        <div class="toolbar-group">
          <span class="table-count">{{ total }} registro{{ total === 1 ? '' : 's' }}</span>

          <select v-model="filtroMateria" class="form-select form-select-sm" aria-label="Filtrar por materia">
            <option value="">Todas las materias</option>
            <option v-for="materia in materiasAsignadas" :key="materia.materiaId" :value="materia.materiaId">
              {{ materia.materiaCodigo }} · {{ materia.materiaNombre }}
            </option>
          </select>

          <select v-model="filtroTrimestre" class="form-select form-select-sm" aria-label="Filtrar por trimestre">
            <option value="">Todos los trimestres</option>
            <option value="1">Primer trimestre</option>
            <option value="2">Segundo trimestre</option>
            <option value="3">Tercer trimestre</option>
          </select>
        </div>
      </div>

      <div v-if="cargando" class="table-loading">
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        Cargando calificaciones…
      </div>

      <div v-else-if="errorCarga" class="p-3">
        <div class="alert alert-danger d-flex gap-2 mb-0" role="alert">
          <i class="bi bi-exclamation-octagon-fill mt-1"></i>
          <div class="flex-grow-1">
            {{ errorCarga }}
            <div class="mt-2">
              <button type="button" class="btn btn-sm btn-outline-danger" @click="cargarCalificaciones">
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
          icono="clipboard2-check"
          :titulo="busqueda || filtroMateria || filtroTrimestre ? 'Sin resultados' : 'Aún no hay calificaciones'"
          :mensaje="
            busqueda || filtroMateria || filtroTrimestre
              ? 'No se encontraron calificaciones que coincidan con los filtros seleccionados.'
              : materiasAsignadas.length === 0
                ? 'No tiene materias asignadas. La Secretaría debe asignarle una materia para registrar notas.'
                : 'Registre la primera calificación de sus materias asignadas.'
          "
        >
          <button
            v-if="busqueda || filtroMateria || filtroTrimestre"
            type="button"
            class="btn btn-light-snc btn-sm"
            @click="
              busqueda = '';
              filtroMateria = '';
              filtroTrimestre = ''
            "
          >
            <i class="bi bi-x-circle"></i>
            Limpiar filtros
          </button>
          <button
            v-else-if="materiasAsignadas.length > 0"
            type="button"
            class="btn btn-primary btn-sm"
            @click="abrirCrear"
          >
            <i class="bi bi-clipboard2-plus"></i>
            Registrar calificación
          </button>
        </EmptyState>

        <template v-else>
          <div class="table-responsive-snc">
            <table class="table-snc">
              <thead>
                <tr>
                  <th>Estudiante</th>
                  <th>Materia</th>
                  <th>Trimestre</th>
                  <th class="cell-right">Nota</th>
                  <th>Registrada por</th>
                  <th>Última actualización</th>
                  <th class="cell-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="calificacion in elementos" :key="calificacion.id">
                  <td>
                    <span class="cell-strong">{{ calificacion.estudianteNombreCompleto }}</span>
                    <span class="cell-sub">
                      {{ calificacion.estudianteCedula }} · ID {{ calificacion.estudianteId }}
                    </span>
                  </td>
                  <td>
                    <span class="cell-strong">{{ calificacion.materiaNombre }}</span>
                    <span class="cell-sub">{{ calificacion.materiaCodigo }}</span>
                  </td>
                  <td class="cell-nowrap">
                    <span class="badge-suave" :title="etiquetaTrimestre(calificacion.trimestre)">
                      {{ etiquetaTrimestreCorta(calificacion.trimestre) }}
                    </span>
                  </td>
                  <td class="cell-right">
                    <span class="fw-700 text-snc-primary">{{ formatearNota(calificacion.nota) }}</span>
                  </td>
                  <td>
                    <span class="cell-strong">{{ calificacion.registradoPorNombreCompleto }}</span>
                    <span v-if="esPropia(calificacion)" class="cell-sub text-success">Registro propio</span>
                    <span v-else class="cell-sub">Solo lectura</span>
                  </td>
                  <td class="cell-nowrap">
                    {{ formatearFechaHora(calificacion.fechaActualizacion ?? calificacion.fechaRegistro) }}
                  </td>
                  <td>
                    <div class="table-actions">
                      <button
                        type="button"
                        class="btn btn-light-snc btn-icon"
                        title="Corregir nota"
                        aria-label="Corregir nota"
                        :disabled="!esPropia(calificacion)"
                        @click="abrirEditar(calificacion)"
                      >
                        <i class="bi" :class="esPropia(calificacion) ? 'bi-pencil-square' : 'bi-lock'"></i>
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
      :titulo="modoEdicion ? 'Corregir calificación' : 'Registrar calificación'"
      :subtitulo="
        modoEdicion
          ? 'Solo puede corregir las notas que usted registró.'
          : 'La nota debe estar entre 0 y 100 y no puede duplicarse por estudiante, materia y trimestre.'
      "
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

      <form id="formularioCalificacion" novalidate @submit.prevent="guardar">
        <div class="row g-3">
          <div class="col-12 col-sm-6">
            <label for="calificacionEstudiante" class="form-label">
              Identificador del estudiante<span class="required-mark">*</span>
            </label>
            <input
              id="calificacionEstudiante"
              v-model="formulario.estudianteId"
              type="number"
              min="1"
              step="1"
              class="form-control"
              :class="{ 'is-invalid': errores.estudianteId }"
              list="estudiantesSugeridos"
              placeholder="Ej. 12"
              :disabled="modoEdicion"
              @input="errores.estudianteId = ''"
            />
            <datalist id="estudiantesSugeridos">
              <option
                v-for="estudiante in estudiantesConocidos"
                :key="estudiante.id"
                :value="estudiante.id"
              >
                {{ estudiante.nombre }} · {{ estudiante.cedula }}
              </option>
            </datalist>
            <div v-if="errores.estudianteId" class="invalid-feedback">{{ errores.estudianteId }}</div>
            <div v-else-if="estudianteDetectado" class="form-text text-success">
              {{ estudianteDetectado.nombre }} · Cédula {{ estudianteDetectado.cedula }}
            </div>
            <div v-else class="form-text">
              La API no expone el listado de estudiantes al rol Profesor. Ingrese el identificador
              numérico del estudiante o seleccione uno de los ya calificados.
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="calificacionMateria" class="form-label">
              Materia asignada<span class="required-mark">*</span>
            </label>
            <select
              id="calificacionMateria"
              v-model.number="formulario.materiaId"
              class="form-select"
              :class="{ 'is-invalid': errores.materiaId }"
              :disabled="modoEdicion"
              @change="errores.materiaId = ''"
            >
              <option :value="0" disabled>Seleccione una materia</option>
              <option
                v-for="materia in modoEdicion ? materiasAsignadas : materiasActivas"
                :key="materia.materiaId"
                :value="materia.materiaId"
              >
                {{ materia.materiaCodigo }} · {{ materia.materiaNombre }}
              </option>
            </select>
            <div v-if="errores.materiaId" class="invalid-feedback">{{ errores.materiaId }}</div>
            <div v-else-if="materiasActivas.length === 0" class="form-text text-danger">
              No tiene materias activas asignadas para registrar calificaciones.
            </div>
          </div>

          <div class="col-12 col-sm-4">
            <label for="calificacionTrimestre" class="form-label">
              Trimestre<span class="required-mark">*</span>
            </label>
            <select
              id="calificacionTrimestre"
              v-model.number="formulario.trimestre"
              class="form-select"
              :class="{ 'is-invalid': errores.trimestre }"
              :disabled="modoEdicion"
              @change="errores.trimestre = ''"
            >
              <option :value="0" disabled>Seleccione</option>
              <option :value="1">Primer trimestre</option>
              <option :value="2">Segundo trimestre</option>
              <option :value="3">Tercer trimestre</option>
            </select>
            <div v-if="errores.trimestre" class="invalid-feedback">{{ errores.trimestre }}</div>
          </div>

          <div class="col-12 col-sm-4">
            <label for="calificacionNota" class="form-label">
              Nota (0–100)<span class="required-mark">*</span>
            </label>
            <input
              id="calificacionNota"
              v-model="formulario.nota"
              type="number"
              min="0"
              max="100"
              step="0.01"
              class="form-control"
              :class="{ 'is-invalid': errores.nota }"
              placeholder="Ej. 85"
              @input="errores.nota = ''"
            />
            <div v-if="errores.nota" class="invalid-feedback">{{ errores.nota }}</div>
            <div v-else class="form-text">Hasta dos decimales.</div>
          </div>

          <div class="col-12 col-sm-4">
            <label class="form-label">Profesor</label>
            <input
              type="text"
              class="form-control"
              :value="estadoSesion.usuario?.nombreUsuario ?? ''"
              disabled
            />
            <div class="form-text">Tomado de su sesión autenticada.</div>
          </div>
        </div>

        <div class="callout mt-3">
          <i class="bi bi-info-circle"></i>
          <span>
            El backend valida que el estudiante y la materia estén activos, que la materia esté
            asignada a usted y que no exista una nota duplicada para el mismo trimestre.
          </span>
        </div>
      </form>

      <template #footer>
        <button type="button" class="btn btn-light-snc" :disabled="guardando" @click="cerrarModal">
          Cancelar
        </button>
        <button
          type="submit"
          form="formularioCalificacion"
          class="btn btn-primary"
          :disabled="guardando"
        >
          <span
            v-if="guardando"
            class="spinner-border spinner-border-sm"
            role="status"
            aria-hidden="true"
          ></span>
          <i v-else class="bi bi-check2-circle"></i>
          {{ guardando ? 'Guardando…' : modoEdicion ? 'Guardar corrección' : 'Registrar calificación' }}
        </button>
      </template>
    </AppModal>
  </div>
</template>
