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
import { mensajeDeError } from '../../services/api'
import { listarRoles } from '../../services/rolesService'
import {
  actualizarUsuario,
  cambiarEstadoUsuario,
  crearUsuario,
  listarUsuarios
} from '../../services/usuariosService'
import { estadoSesion } from '../../stores/auth'
import { confirmar } from '../../stores/confirm'
import { toast } from '../../stores/toast'
import { consumirParametroNuevo, erroresDeCampos } from '../../utils/errors'
import { coincideBusqueda, formatearFecha, nombreCompleto } from '../../utils/format'
import { validarUsuario } from '../../utils/validators'

const ruta = useRoute()
const router = useRouter()

const usuarios = ref([])
const roles = ref([])
const cargando = ref(true)
const errorCarga = ref('')
const errorRoles = ref('')
const incluirInactivos = ref(false)
const filtroRol = ref('')
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
  apellidos: '',
  correo: '',
  nombreUsuario: '',
  password: '',
  rolId: 0
})

const filtrados = computed(() =>
  usuarios.value.filter((usuario) =>
    coincideBusqueda(
      `${usuario.cedula} ${usuario.nombres} ${usuario.apellidos} ${usuario.correo} ${usuario.nombreUsuario} ${usuario.rol}`,
      busqueda.value
    )
  )
)

const { pagina, total, totalPaginas, elementos, reiniciar } = usePaginacion(filtrados, 10)

watch(busqueda, reiniciar)
watch([incluirInactivos, filtroRol], () => {
  cargar()
})

function limiteInferiorPassword() {
  return editando.value ? 0 : 8
}

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
  formulario.correo = ''
  formulario.nombreUsuario = ''
  formulario.password = ''
  formulario.rolId = 0
  limpiarErrores()
  modalAbierta.value = true
}

function abrirEditar(usuario) {
  editando.value = usuario
  formulario.cedula = usuario.cedula
  formulario.nombres = usuario.nombres
  formulario.apellidos = usuario.apellidos
  formulario.correo = usuario.correo
  formulario.nombreUsuario = usuario.nombreUsuario
  formulario.password = ''
  formulario.rolId = Number(usuario.rolId)
  limpiarErrores()
  modalAbierta.value = true
}

function cerrarModal() {
  if (guardando.value) return
  modalAbierta.value = false
}

async function cargarRoles() {
  errorRoles.value = ''

  try {
    roles.value = await listarRoles()
  } catch (error) {
    roles.value = []
    errorRoles.value = mensajeDeError(error, 'No fue posible cargar los roles disponibles.')
  }
}

async function cargar() {
  cargando.value = true
  errorCarga.value = ''

  try {
    usuarios.value = await listarUsuarios({
      rolId: filtroRol.value || null,
      incluirInactivos: incluirInactivos.value
    })
  } catch (error) {
    errorCarga.value = mensajeDeError(error, 'No fue posible cargar los usuarios.')
  } finally {
    cargando.value = false
  }
}

async function guardar() {
  limpiarErrores()

  const validacion = validarUsuario(formulario, Boolean(editando.value))
  errores.value = validacion
  if (Object.keys(validacion).length > 0) return

  guardando.value = true

  try {
    const payload = {
      cedula: formulario.cedula.trim(),
      nombres: formulario.nombres.trim(),
      apellidos: formulario.apellidos.trim(),
      correo: formulario.correo.trim(),
      nombreUsuario: formulario.nombreUsuario.trim(),
      rolId: Number(formulario.rolId)
    }

    if (formulario.password) {
      payload.password = formulario.password
    }

    if (editando.value) {
      await actualizarUsuario(editando.value.id, payload)
      toast.success('Usuario actualizado correctamente.')
    } else {
      await crearUsuario(payload)
      toast.success('Usuario registrado correctamente.')
    }

    modalAbierta.value = false
    await cargar()
  } catch (error) {
    errorFormulario.value = mensajeDeError(error, 'No fue posible guardar el usuario.')
    detallesFormulario.value = error?.detalles ?? []
    errores.value = { ...errores.value, ...erroresDeCampos(error?.campos) }
  } finally {
    guardando.value = false
  }
}

async function alternarEstado(usuario) {
  const desactivar = usuario.activo
  const accion = desactivar ? 'Desactivar' : 'Activar'
  const esCuentaPropia = Number(usuario.id) === Number(estadoSesion.usuario?.id)

  const confirmado = await confirmar({
    titulo: `${accion} usuario`,
    mensaje: `¿Desea ${accion.toLowerCase()} la cuenta de ${nombreCompleto(usuario)}?`,
    detalle: esCuentaPropia
      ? 'Está a punto de cambiar el estado de su propia cuenta. La API valida la protección de la última Secretaría activa.'
      : desactivar
        ? 'El usuario no podrá iniciar sesión mientras permanezca inactivo. La API protege la última Secretaría activa.'
        : 'El usuario podrá iniciar sesión nuevamente.',
    textoConfirmar: accion,
    variante: desactivar ? 'danger' : 'success',
    icono: desactivar ? 'person-dash' : 'person-check'
  })

  if (!confirmado) return

  cambiandoEstadoId.value = usuario.id

  try {
    await cambiarEstadoUsuario(usuario.id, !usuario.activo)
    toast.success(`Usuario ${desactivar ? 'desactivado' : 'activado'} correctamente.`)
    await cargar()
  } catch (error) {
    toast.error(mensajeDeError(error, 'No fue posible cambiar el estado del usuario.'))
  } finally {
    cambiandoEstadoId.value = null
  }
}

onMounted(async () => {
  await Promise.all([cargar(), cargarRoles()])

  if (consumirParametroNuevo(ruta, router)) {
    abrirCrear()
  }
})
</script>

<template>
  <div>
    <PageHeader
      titulo="Gestión de usuarios"
      descripcion="Administre las cuentas de Secretaría y Profesor, sus roles y su estado."
    >
      <template #acciones>
        <button type="button" class="btn btn-primary" @click="abrirCrear">
          <i class="bi bi-person-plus"></i>
          Nuevo usuario
        </button>
      </template>
    </PageHeader>

    <div v-if="errorRoles" class="alert alert-warning d-flex gap-2" role="alert">
      <i class="bi bi-exclamation-triangle-fill mt-1"></i>
      <div>{{ errorRoles }} Puede reintentar al abrir el formulario de usuario.</div>
    </div>

    <div class="card-snc">
      <div class="list-toolbar">
        <div class="toolbar-group">
          <SearchBox
            v-model="busqueda"
            placeholder="Buscar por nombre, usuario, cédula o correo…"
            etiqueta="Buscar usuarios"
          />
        </div>

        <div class="toolbar-group">
          <span class="table-count">{{ total }} registro{{ total === 1 ? '' : 's' }}</span>

          <select v-model="filtroRol" class="form-select form-select-sm" aria-label="Filtrar por rol">
            <option value="">Todos los roles</option>
            <option v-for="rol in roles" :key="rol.id" :value="rol.id">{{ rol.nombre }}</option>
          </select>

          <div class="form-check form-switch mb-0">
            <input
              id="incluirInactivosUsuarios"
              v-model="incluirInactivos"
              class="form-check-input"
              type="checkbox"
            />
            <label class="form-check-label" for="incluirInactivosUsuarios">
              Incluir inactivos
            </label>
          </div>
        </div>
      </div>

      <div v-if="cargando" class="table-loading">
        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
        Cargando usuarios…
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
          icono="person-badge"
          :titulo="busqueda || filtroRol ? 'Sin resultados' : 'No hay usuarios registrados'"
          :mensaje="
            busqueda || filtroRol
              ? 'No se encontraron usuarios que coincidan con los filtros seleccionados.'
              : 'Registre el primer usuario para habilitar el acceso al sistema.'
          "
        >
          <button
            v-if="busqueda || filtroRol"
            type="button"
            class="btn btn-light-snc btn-sm"
            @click="
              busqueda = '';
              filtroRol = ''
            "
          >
            <i class="bi bi-x-circle"></i>
            Limpiar filtros
          </button>
          <button v-else type="button" class="btn btn-primary btn-sm" @click="abrirCrear">
            <i class="bi bi-person-plus"></i>
            Registrar usuario
          </button>
        </EmptyState>

        <template v-else>
          <div class="table-responsive-snc">
            <table class="table-snc">
              <thead>
                <tr>
                  <th>Usuario</th>
                  <th>Correo</th>
                  <th>Rol</th>
                  <th>Estado</th>
                  <th>Registro</th>
                  <th class="cell-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="usuario in elementos" :key="usuario.id">
                  <td>
                    <span class="cell-strong">{{ nombreCompleto(usuario) }}</span>
                    <span class="cell-sub">
                      {{ usuario.nombreUsuario }} · Cédula {{ usuario.cedula }}
                    </span>
                  </td>
                  <td>{{ usuario.correo }}</td>
                  <td>
                    <span class="badge-rol" :class="{ profesor: Number(usuario.rolId) === 2 }">
                      <i
                        class="bi"
                        :class="Number(usuario.rolId) === 1 ? 'bi-person-badge' : 'bi-person-video3'"
                      ></i>
                      {{ usuario.rol }}
                    </span>
                  </td>
                  <td><EstadoBadge :activo="usuario.activo" /></td>
                  <td class="cell-nowrap">{{ formatearFecha(usuario.fechaRegistro) }}</td>
                  <td>
                    <div class="table-actions">
                      <button
                        type="button"
                        class="btn btn-light-snc btn-icon"
                        title="Editar usuario"
                        aria-label="Editar usuario"
                        @click="abrirEditar(usuario)"
                      >
                        <i class="bi bi-pencil-square"></i>
                      </button>
                      <button
                        type="button"
                        class="btn btn-icon"
                        :class="usuario.activo ? 'btn-danger-soft' : 'btn-success-soft'"
                        :title="usuario.activo ? 'Desactivar usuario' : 'Activar usuario'"
                        :aria-label="usuario.activo ? 'Desactivar usuario' : 'Activar usuario'"
                        :disabled="cambiandoEstadoId === usuario.id"
                        @click="alternarEstado(usuario)"
                      >
                        <span
                          v-if="cambiandoEstadoId === usuario.id"
                          class="spinner-border spinner-border-sm"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        <i
                          v-else
                          class="bi"
                          :class="usuario.activo ? 'bi-person-dash' : 'bi-person-check'"
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
      :titulo="editando ? 'Editar usuario' : 'Registrar usuario'"
      :subtitulo="
        editando
          ? 'Actualice los datos de la cuenta. La contraseña es opcional.'
          : 'Complete la información para crear una nueva cuenta de acceso.'
      "
      :bloquear-cierre="guardando"
      tamano="lg"
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

      <div v-if="errorRoles" class="callout warning mb-3">
        <i class="bi bi-exclamation-triangle"></i>
        <div class="flex-grow-1">
          {{ errorRoles }}
          <button type="button" class="btn btn-sm btn-light-snc ms-2" @click="cargarRoles">
            Reintentar
          </button>
        </div>
      </div>

      <form id="formularioUsuario" novalidate @submit.prevent="guardar">
        <div class="row g-3">
          <div class="col-12 col-sm-6">
            <label for="usuarioCedula" class="form-label">
              Cédula<span class="required-mark">*</span>
            </label>
            <input
              id="usuarioCedula"
              v-model="formulario.cedula"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.cedula }"
              maxlength="20"
              @input="errores.cedula = ''"
            />
            <div v-if="errores.cedula" class="invalid-feedback">{{ errores.cedula }}</div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="usuarioNombreUsuario" class="form-label">
              Nombre de usuario<span class="required-mark">*</span>
            </label>
            <input
              id="usuarioNombreUsuario"
              v-model="formulario.nombreUsuario"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.nombreUsuario }"
              maxlength="50"
              autocomplete="off"
              @input="errores.nombreUsuario = ''"
            />
            <div v-if="errores.nombreUsuario" class="invalid-feedback">
              {{ errores.nombreUsuario }}
            </div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="usuarioNombres" class="form-label">
              Nombres<span class="required-mark">*</span>
            </label>
            <input
              id="usuarioNombres"
              v-model="formulario.nombres"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.nombres }"
              maxlength="100"
              @input="errores.nombres = ''"
            />
            <div v-if="errores.nombres" class="invalid-feedback">{{ errores.nombres }}</div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="usuarioApellidos" class="form-label">
              Apellidos<span class="required-mark">*</span>
            </label>
            <input
              id="usuarioApellidos"
              v-model="formulario.apellidos"
              type="text"
              class="form-control"
              :class="{ 'is-invalid': errores.apellidos }"
              maxlength="100"
              @input="errores.apellidos = ''"
            />
            <div v-if="errores.apellidos" class="invalid-feedback">{{ errores.apellidos }}</div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="usuarioCorreo" class="form-label">
              Correo electrónico<span class="required-mark">*</span>
            </label>
            <input
              id="usuarioCorreo"
              v-model="formulario.correo"
              type="email"
              class="form-control"
              :class="{ 'is-invalid': errores.correo }"
              maxlength="150"
              autocomplete="off"
              @input="errores.correo = ''"
            />
            <div v-if="errores.correo" class="invalid-feedback">{{ errores.correo }}</div>
          </div>

          <div class="col-12 col-sm-6">
            <label for="usuarioRol" class="form-label">
              Rol<span class="required-mark">*</span>
            </label>
            <select
              id="usuarioRol"
              v-model.number="formulario.rolId"
              class="form-select"
              :class="{ 'is-invalid': errores.rolId }"
              :disabled="roles.length === 0"
              @change="errores.rolId = ''"
            >
              <option :value="0" disabled>Seleccione un rol</option>
              <option v-for="rol in roles" :key="rol.id" :value="rol.id">{{ rol.nombre }}</option>
            </select>
            <div v-if="errores.rolId" class="invalid-feedback">{{ errores.rolId }}</div>
          </div>

          <div class="col-12">
            <label for="usuarioPassword" class="form-label">
              {{ editando ? 'Nueva contraseña' : 'Contraseña' }}
              <span v-if="!editando" class="required-mark">*</span>
            </label>
            <input
              id="usuarioPassword"
              v-model="formulario.password"
              type="password"
              class="form-control"
              :class="{ 'is-invalid': errores.password }"
              maxlength="100"
              autocomplete="new-password"
              :placeholder="editando ? 'Deje vacío para conservar la contraseña actual' : 'Mínimo 8 caracteres'"
              @input="errores.password = ''"
            />
            <div v-if="errores.password" class="invalid-feedback">{{ errores.password }}</div>
            <div v-else class="form-text">
              {{
                editando
                  ? 'Opcional. Si la completa, debe tener al menos 8 caracteres.'
                  : 'Mínimo 8 caracteres. La contraseña no se muestra ni se almacena en el navegador.'
              }}
            </div>
          </div>
        </div>
      </form>

      <template #footer>
        <button type="button" class="btn btn-light-snc" :disabled="guardando" @click="cerrarModal">
          Cancelar
        </button>
        <button type="submit" form="formularioUsuario" class="btn btn-primary" :disabled="guardando">
          <span
            v-if="guardando"
            class="spinner-border spinner-border-sm"
            role="status"
            aria-hidden="true"
          ></span>
          <i v-else class="bi bi-check2-circle"></i>
          {{ guardando ? 'Guardando…' : editando ? 'Guardar cambios' : 'Registrar usuario' }}
        </button>
      </template>
    </AppModal>
  </div>
</template>
