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
  actualizarMateria,
  cambiarEstadoMateria,
  crearMateria,
  listarMaterias
} from '../../services/materiasService'
import { mensajeDeError } from '../../services/api'
import { confirmar } from '../../stores/confirm'
import { toast } from '../../stores/toast'
import { consumirParametroNuevo, erroresDeCampos } from '../../utils/errors'
import { coincideBusqueda, formatearFecha } from '../../utils/format'
import { validarMateria } from '../../utils/validators'

const ruta = useRoute()
const router = useRouter()

const materias = ref([])
const cargando = ref(true)
const errorCarga = ref('')
const incluirInactivas = ref(false)
const busqueda = ref('')

const modalAbierta = ref(false)
const guardando = ref(false)
const editando = ref(null)
const errores = ref({})
const errorFormulario = ref('')
const detallesFormulario = ref([])
const cambiandoEstadoId = ref(null)

const formulario = reactive({
  codigo: '',
  nombre: ''
})

const filtradas = computed(() =>
  materias.value.filter((materia) =>
    coincideBusqueda(`${materia.codigo} ${materia.nombre}`, busqueda.value)
  )
)

const { pagina, total, totalPaginas, elementos, reiniciar } = usePaginacion(filtradas, 10)

watch(busqueda, reiniciar)
watch(incluirInactivas, () => {
  cargar()
})

function limpiarErrores() {
  errores.value = {}
  errorFormulario.value = ''
  detallesFormulario.value = []
}

function abrirCrear() {
  editando.value = null
  formulario.codigo = ''
  formulario.nombre = ''
  limpiarErrores()
  modalAbierta.value = true
}

function abrirEditar(materia) {
  editando.value = materia
  formulario.codigo = materia.codigo
  formulario.nombre = materia.nombre
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
    materias.value = await listarMaterias(incluirInactivas.value)
  } catch (error) {
    errorCarga.value = mensajeDeError(error, 'No fue posible cargar las materias.')
  } finally {
    cargando.value = false
  }
}

async function guardar() {
  limpiarErrores()

  const validacion = validarMateria(formulario)
  errores.value = validacion
  if (Object.keys(validacion).length > 0) return

  guardando.value = true

  try {
    const payload = {
      codigo: formulario.codigo.trim(),
      nombre: formulario.nombre.trim()
    }

    if (editando.value) {
      await actualizarMateria(editando.value.id, payload)
      toast.success('Materia actualizada correctamente.')
    } else {
      await crearMateria(payload)
      toast.success('Materia registrada correctamente.')
    }

    modalAbierta.value = false
    await cargar()
  } catch (error) {
    errorFormulario.value = mensajeDeError(error, 'No fue posible guardar la materia.')
    detallesFormulario.value = error?.detalles ?? []
    errores.value = { ...errores.value, ...erroresDeCampos(error?.campos) }
  } finally {
    guardando.value = false
  }
}

async function alternarEstado(materia) {
  const desactivar = materia.activo
  const accion = desactivar ? 'Desactivar' : 'Activar'

  const confirmado = await confirmar({
    titulo: `${accion} materia`,
    mensaje: `¿Desea ${accion.toLowerCase()} la materia ${materia.nombre}?`,
    detalle: desactivar
      ? 'La materia dejará de estar disponible para nuevas asignaciones y calificaciones. Las notas existentes se conservan.'
      : 'La materia volverá a estar disponible para asignaciones y nuevas calificaciones.',
    textoConfirmar: accion,
    variante: desactivar ? 'danger' : 'success',
    icono: desactivar ? 'journal-x' : 'journal-check'
  })

  if (!confirmado) return

  cambiandoEstadoId.value = materia.id

  try {
    await cambiarEstadoMateria(materia.id, !materia.activo)
    toast.success(`Materia ${desactivar ? 'desactivada' : 'activada'} correctamente.`)
    await cargar()
  } catch (error) {
    toast.error(mensajeDeError(error, 'No fue posible cambiar el estado de la materia.'))
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
      titulo="Gestión de materias"
      descripcion="Administre el catálogo de materias activas e inactivas del colegio."
    >
      <template #acciones>
        <button type="button" class="btn btn-primary" @click="abrirCrear">
          <i class="bi bi-journal-plus"></i>
          Nueva materia
        </button>
      </template>
    </PageHeader>

    <div class="card-snc">
      <div class="list-toolbar">
        <div class="toolbar-group">
          <SearchBox
            v-model="busqueda"
            placeholder="Buscar por código o nombre…"
            etiqueta="Buscar materias"
          />
        </div>

        <div class="toolbar-group">
          <span class="table-count">{{ total }} registro{{ total === 1 ? '' : 's' }}</span>
          <div class="form-check form-switch mb-0">
            <input
              id="incluirInactivasMaterias"
              v-model="incluirInactivas"
              class="form-check-input"
              type="checkbox"
            />
            <label class="form-check-label" for="incluirInactivasMaterias">
              Incluir inactivas
            </label>
          </div>
        </div>
      </div>

      <div v-if="cargando" class="table-loading">
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        Cargando materias…
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
          icono="journal-bookmark"
          :titulo="busqueda ? 'Sin resultados' : 'No hay materias registradas'"
          :mensaje="
            busqueda
              ? 'No se encontraron materias que coincidan con la búsqueda.'
              : 'Registre la primera materia para organizar la carga académica.'
          "
        >
          <button v-if="busqueda" type="button" class="btn btn-light-snc btn-sm" @click="busqueda = ''">
            <i class="bi bi-x-circle"></i>
            Limpiar búsqueda
          </button>
          <button v-else type="button" class="btn btn-primary btn-sm" @click="abrirCrear">
            <i class="bi bi-journal-plus"></i>
            Registrar materia
          </button>
        </EmptyState>

        <template v-else>
          <div class="table-responsive-snc">
            <table class="table-snc">
              <thead>
                <tr>
                  <th>Materia</th>
                  <th>Código</th>
                  <th>Estado</th>
                  <th>Fecha de registro</th>
                  <th class="cell-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="materia in elementos" :key="materia.id">
                  <td>
                    <span class="cell-strong">{{ materia.nombre }}</span>
                    <span class="cell-sub">ID interno: {{ materia.id }}</span>
                  </td>
                  <td class="cell-nowrap"><span class="badge-suave">{{ materia.codigo }}</span></td>
                  <td><EstadoBadge :activo="materia.activo" /></td>
                  <td class="cell-nowrap">{{ formatearFecha(materia.fechaRegistro) }}</td>
                  <td>
                    <div class="table-actions">
                      <button
                        type="button"
                        class="btn btn-light-snc btn-icon"
                        title="Editar materia"
                        aria-label="Editar materia"
                        @click="abrirEditar(materia)"
                      >
                        <i class="bi bi-pencil-square"></i>
                      </button>
                      <button
                        type="button"
                        class="btn btn-icon"
                        :class="materia.activo ? 'btn-danger-soft' : 'btn-success-soft'"
                        :title="materia.activo ? 'Desactivar materia' : 'Activar materia'"
                        :aria-label="materia.activo ? 'Desactivar materia' : 'Activar materia'"
                        :disabled="cambiandoEstadoId === materia.id"
                        @click="alternarEstado(materia)"
                      >
                        <span
                          v-if="cambiandoEstadoId === materia.id"
                          class="spinner-border spinner-border-sm"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        <i
                          v-else
                          class="bi"
                          :class="materia.activo ? 'bi-journal-x' : 'bi-journal-check'"
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
      :titulo="editando ? 'Editar materia' : 'Registrar materia'"
      :subtitulo="
        editando
          ? 'Actualice el código y el nombre de la materia.'
          : 'Complete la información para registrar una nueva materia.'
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

      <form id="formularioMateria" novalidate @submit.prevent="guardar">
        <div class="row g-3">
          <div class="col-12 col-sm-5">
            <label for="materiaCodigo" class="form-label">
              Código<span class="required-mark">*</span>
            </label>
            <input
              id="materiaCodigo"
              v-model="formulario.codigo"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.codigo }"
              maxlength="20"
              placeholder="Ej. MAT-01"
              @input="errores.codigo = ''"
            />
            <div v-if="errores.codigo" class="invalid-feedback">{{ errores.codigo }}</div>
            <div v-else class="form-text">Entre 2 y 20 caracteres.</div>
          </div>

          <div class="col-12 col-sm-7">
            <label for="materiaNombre" class="form-label">
              Nombre de la materia<span class="required-mark">*</span>
            </label>
            <input
              id="materiaNombre"
              v-model="formulario.nombre"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.nombre }"
              maxlength="150"
              placeholder="Ej. Matemáticas"
              @input="errores.nombre = ''"
            />
            <div v-if="errores.nombre" class="invalid-feedback">{{ errores.nombre }}</div>
          </div>
        </div>
      </form>

      <template #footer>
        <button type="button" class="btn btn-light-snc" :disabled="guardando" @click="cerrarModal">
          Cancelar
        </button>
        <button type="submit" form="formularioMateria" class="btn btn-primary" :disabled="guardando">
          <span
            v-if="guardando"
            class="spinner-border spinner-border-sm"
            role="status"
            aria-hidden="true"
          ></span>
          <i v-else class="bi bi-check2-circle"></i>
          {{ guardando ? 'Guardando…' : editando ? 'Guardar cambios' : 'Registrar materia' }}
        </button>
      </template>
    </AppModal>
  </div>
</template>
