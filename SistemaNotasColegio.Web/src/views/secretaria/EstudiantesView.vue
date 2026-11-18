<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppModal from '../../components/AppModal.vue'
import EmptyState from '../../components/EmptyState.vue'
import EstadoBadge from '../../components/EstadoBadge.vue'
import PageHeader from '../../components/PageHeader.vue'
import PaginationBar from '../../components/PaginationBar.vue'
import SearchBox from '../../components/SearchBox.vue'
import { usePaginacion } from '../../composables/usePaginacion'
import {
  actualizarEstudiante,
  cambiarEstadoEstudiante,
  crearEstudiante,
  listarEstudiantes
} from '../../services/estudiantesService'
import { mensajeDeError } from '../../services/api'
import { confirmar } from '../../stores/confirm'
import { toast } from '../../stores/toast'
import { consumirParametroNuevo, erroresDeCampos } from '../../utils/errors'
import { coincideBusqueda, formatearFecha, nombreCompleto } from '../../utils/format'
import { validarEstudiante } from '../../utils/validators'

const ruta = useRoute()
const router = useRouter()

const estudiantes = ref([])
const cargando = ref(true)
const errorCarga = ref('')
const incluirInactivos = ref(false)
const busqueda = ref('')

const modalAbierta = ref(false)
const guardando = ref(false)
const editando = ref(null)
const errores = ref({})
const errorFormulario = ref('')
const detallesFormulario = ref([])
const cambiandoEstadoId = ref(null)

const formulario = reactive({
  cedula: '',
  nombres: '',
  apellidos: ''
})

const filtrados = computed(() =>
  estudiantes.value.filter((estudiante) =>
    coincideBusqueda(
      `${estudiante.cedula} ${estudiante.nombres} ${estudiante.apellidos}`,
      busqueda.value
    )
  )
)

const { pagina, total, totalPaginas, elementos, reiniciar } = usePaginacion(filtrados, 10)

watch(busqueda, reiniciar)
watch(incluirInactivos, () => {
  cargar()
})

function limpiarErrores() {
  errores.value = {}
  errorFormulario.value = ''
  detallesFormulario.value = []
}

function abrirCrear() {
  editando.value = null
  formulario.cedula = ''
  formulario.nombres = ''
  formulario.apellidos = ''
  limpiarErrores()
  modalAbierta.value = true
}

function abrirEditar(estudiante) {
  editando.value = estudiante
  formulario.cedula = estudiante.cedula
  formulario.nombres = estudiante.nombres
  formulario.apellidos = estudiante.apellidos
  limpiarErrores()
  modalAbierta.value = true
}

function cerrarModal() {
  if (guardando.value) return
  modalAbierta.value = false
}

async function cargar() {
  cargando.value = true
  errorCarga.value = ''

  try {
    estudiantes.value = await listarEstudiantes(incluirInactivos.value)
  } catch (error) {
    errorCarga.value = mensajeDeError(error, 'No fue posible cargar los estudiantes.')
  } finally {
    cargando.value = false
  }
}

async function guardar() {
  limpiarErrores()

  const validacion = validarEstudiante(formulario)
  errores.value = validacion
  if (Object.keys(validacion).length > 0) return

  guardando.value = true

  try {
    const payload = {
      cedula: formulario.cedula.trim(),
      nombres: formulario.nombres.trim(),
      apellidos: formulario.apellidos.trim()
    }

    if (editando.value) {
      await actualizarEstudiante(editando.value.id, payload)
      toast.success('Estudiante actualizado correctamente.')
    } else {
      await crearEstudiante(payload)
      toast.success('Estudiante registrado correctamente.')
    }

    modalAbierta.value = false
    await cargar()
  } catch (error) {
    errorFormulario.value = mensajeDeError(error, 'No fue posible guardar el estudiante.')
    detallesFormulario.value = error?.detalles ?? []
    errores.value = { ...errores.value, ...erroresDeCampos(error?.campos) }
  } finally {
    guardando.value = false
  }
}

async function alternarEstado(estudiante) {
  const desactivar = estudiante.activo
  const accion = desactivar ? 'Desactivar' : 'Activar'

  const confirmado = await confirmar({
    titulo: `${accion} estudiante`,
    mensaje: `¿Desea ${accion.toLowerCase()} a ${nombreCompleto(estudiante)}?`,
    detalle: desactivar
      ? 'El estudiante dejará de aparecer en el listado activo. Su historial académico y sus calificaciones se conservan.'
      : 'El estudiante volverá a estar disponible para el registro de calificaciones.',
    textoConfirmar: accion,
    variante: desactivar ? 'danger' : 'success',
    icono: desactivar ? 'person-dash' : 'person-check'
  })

  if (!confirmado) return

  cambiandoEstadoId.value = estudiante.id

  try {
    await cambiarEstadoEstudiante(estudiante.id, !estudiante.activo)
    toast.success(`Estudiante ${desactivar ? 'desactivado' : 'activado'} correctamente.`)
    await cargar()
  } catch (error) {
    toast.error(mensajeDeError(error, 'No fue posible cambiar el estado del estudiante.'))
  } finally {
    cambiandoEstadoId.value = null
  }
}

onMounted(async () => {
  await cargar()

  if (consumirParametroNuevo(ruta, router)) {
    abrirCrear()
  }
})
</script>

<template>
  <div>
    <PageHeader
      titulo="Gestión de estudiantes"
      descripcion="Registre, consulte y actualice los estudiantes del colegio."
    >
      <template #acciones>
        <button type="button" class="btn btn-primary" @click="abrirCrear">
          <i class="bi bi-person-plus"></i>
          Nuevo estudiante
        </button>
      </template>
    </PageHeader>

    <div class="card-snc">
      <div class="list-toolbar">
        <div class="toolbar-group">
          <SearchBox
            v-model="busqueda"
            placeholder="Buscar por cédula, nombres o apellidos…"
            etiqueta="Buscar estudiantes"
          />
        </div>

        <div class="toolbar-group">
          <span class="table-count">{{ total }} registro{{ total === 1 ? '' : 's' }}</span>
          <div class="form-check form-switch mb-0">
            <input
              id="incluirInactivosEstudiantes"
              v-model="incluirInactivos"
              class="form-check-input"
              type="checkbox"
            />
            <label class="form-check-label" for="incluirInactivosEstudiantes">
              Incluir inactivos
            </label>
          </div>
        </div>
      </div>

      <div v-if="cargando" class="table-loading">
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        Cargando estudiantes…
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
          icono="people"
          :titulo="busqueda ? 'Sin resultados' : 'No hay estudiantes registrados'"
          :mensaje="
            busqueda
              ? 'No se encontraron estudiantes que coincidan con la búsqueda.'
              : 'Registre el primer estudiante para comenzar la gestión académica.'
          "
        >
          <button v-if="busqueda" type="button" class="btn btn-light-snc btn-sm" @click="busqueda = ''">
            <i class="bi bi-x-circle"></i>
            Limpiar búsqueda
          </button>
          <button v-else type="button" class="btn btn-primary btn-sm" @click="abrirCrear">
            <i class="bi bi-person-plus"></i>
            Registrar estudiante
          </button>
        </EmptyState>

        <template v-else>
          <div class="table-responsive-snc">
            <table class="table-snc">
              <thead>
                <tr>
                  <th>Estudiante</th>
                  <th>Cédula</th>
                  <th>Estado</th>
                  <th>Fecha de registro</th>
                  <th class="cell-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="estudiante in elementos" :key="estudiante.id">
                  <td>
                    <span class="cell-strong">{{ nombreCompleto(estudiante) }}</span>
                    <span class="cell-sub">ID interno: {{ estudiante.id }}</span>
                  </td>
                  <td class="cell-nowrap">{{ estudiante.cedula }}</td>
                  <td>
                    <EstadoBadge :activo="estudiante.activo" />
                  </td>
                  <td class="cell-nowrap">{{ formatearFecha(estudiante.fechaRegistro) }}</td>
                  <td>
                    <div class="table-actions">
                      <button
                        type="button"
                        class="btn btn-light-snc btn-icon"
                        title="Editar estudiante"
                        aria-label="Editar estudiante"
                        @click="abrirEditar(estudiante)"
                      >
                        <i class="bi bi-pencil-square"></i>
                      </button>
                      <button
                        type="button"
                        class="btn btn-icon"
                        :class="estudiante.activo ? 'btn-danger-soft' : 'btn-success-soft'"
                        :title="estudiante.activo ? 'Desactivar estudiante' : 'Activar estudiante'"
                        :aria-label="estudiante.activo ? 'Desactivar estudiante' : 'Activar estudiante'"
                        :disabled="cambiandoEstadoId === estudiante.id"
                        @click="alternarEstado(estudiante)"
                      >
                        <span
                          v-if="cambiandoEstadoId === estudiante.id"
                          class="spinner-border spinner-border-sm"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        <i
                          v-else
                          class="bi"
                          :class="estudiante.activo ? 'bi-person-dash' : 'bi-person-check'"
                        ></i>
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
      :titulo="editando ? 'Editar estudiante' : 'Registrar estudiante'"
      :subtitulo="
        editando
          ? 'Actualice los datos personales del estudiante.'
          : 'Complete la información para registrar un nuevo estudiante.'
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

      <form id="formularioEstudiante" novalidate @submit.prevent="guardar">
        <div class="row g-3">
          <div class="col-12">
            <label for="estudianteCedula" class="form-label">
              Cédula<span class="required-mark">*</span>
            </label>
            <input
              id="estudianteCedula"
              v-model="formulario.cedula"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.cedula }"
              maxlength="20"
              placeholder="Ej. 0102030405"
              @input="errores.cedula = ''"
            />
            <div v-if="errores.cedula" class="invalid-feedback">{{ errores.cedula }}</div>
            <div v-else class="form-text">Entre 5 y 20 caracteres. Letras, números y guiones.</div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="estudianteNombres" class="form-label">
              Nombres<span class="required-mark">*</span>
            </label>
            <input
              id="estudianteNombres"
              v-model="formulario.nombres"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.nombres }"
              maxlength="100"
              placeholder="Nombres del estudiante"
              @input="errores.nombres = ''"
            />
            <div v-if="errores.nombres" class="invalid-feedback">{{ errores.nombres }}</div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="estudianteApellidos" class="form-label">
              Apellidos<span class="required-mark">*</span>
            </label>
            <input
              id="estudianteApellidos"
              v-model="formulario.apellidos"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.apellidos }"
              maxlength="100"
              placeholder="Apellidos del estudiante"
              @input="errores.apellidos = ''"
            />
            <div v-if="errores.apellidos" class="invalid-feedback">{{ errores.apellidos }}</div>
          </div>
        </div>
      </form>

      <template #footer>
        <button type="button" class="btn btn-light-snc" :disabled="guardando" @click="cerrarModal">
          Cancelar
        </button>
        <button type="submit" form="formularioEstudiante" class="btn btn-primary" :disabled="guardando">
          <span
            v-if="guardando"
            class="spinner-border spinner-border-sm"
            role="status"
            aria-hidden="true"
          ></span>
          <i v-else class="bi bi-check2-circle"></i>
          {{ guardando ? 'Guardando…' : editando ? 'Guardar cambios' : 'Registrar estudiante' }}
        </button>
      </template>
    </AppModal>
  </div>
</template>
