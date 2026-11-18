<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { iniciarSesionApi } from '../../services/authService'
import { verificarSaludApi } from '../../services/healthService'
import { mensajeDeError } from '../../services/api'
import { iniciarSesion } from '../../stores/auth'
import { toast } from '../../stores/toast'
import { validarLogin } from '../../utils/validators'

const router = useRouter()
const ruta = useRoute()

const formulario = reactive({
  nombreUsuario: '',
  password: ''
})

const errores = ref({})
const errorGeneral = ref('')
const enviando = ref(false)
const mostrarPassword = ref(false)
const estadoApi = ref('checking')

const sesionExpirada = computed(() => ruta.query.sesionExpirada === '1')

const textoEstadoApi = computed(() => {
  if (estadoApi.value === 'checking') return 'Verificando conexión con la API…'
  if (estadoApi.value === 'ok') return 'Servidor conectado'
  return 'Sin conexión con la API'
})

function limpiarError(campo) {
  if (errores.value[campo]) {
    const copia = { ...errores.value }
    delete copia[campo]
    errores.value = copia
  }
  errorGeneral.value = ''
}

function destinoSeguro(redireccion, rolId) {
  const rol = Number(rolId)
  const destino = typeof redireccion === 'string' ? redireccion : ''

  if (rol === 1 && destino.startsWith('/secretaria')) return destino
  if (rol === 2 && destino.startsWith('/profesor')) return destino
  if (rol === 1) return '/secretaria'
  if (rol === 2) return '/profesor'
  return '/login'
}

async function verificarServidor() {
  estadoApi.value = 'checking'
  try {
    const salud = await verificarSaludApi()
    estadoApi.value = salud?.database === 'Connected' ? 'ok' : 'fail'
  } catch {
    estadoApi.value = 'fail'
  }
}

async function enviar() {
  errorGeneral.value = ''

  const validacion = validarLogin(formulario)
  errores.value = validacion
  if (Object.keys(validacion).length > 0) return

  enviando.value = true

  try {
    const respuesta = await iniciarSesionApi({
      nombreUsuario: formulario.nombreUsuario.trim(),
      password: formulario.password
    })

    iniciarSesion(respuesta)
    toast.success(`Bienvenido, ${respuesta.usuario?.nombres ?? 'usuario'}.`)

    await router.replace(destinoSeguro(ruta.query.redirect, respuesta.usuario?.rolId))
  } catch (error) {
    errorGeneral.value = mensajeDeError(error, 'No fue posible iniciar sesión.')
    if (error?.esConexion) estadoApi.value = 'fail'
  } finally {
    enviando.value = false
  }
}

onMounted(verificarServidor)
</script>

<template>
  <div class="login-page">
    <div class="login-shell">
      <section class="login-brand-panel" aria-hidden="true">
        <div class="login-brand-content">
          <div class="login-brand-top">
            <div class="brand-logo">
              <i class="bi bi-mortarboard-fill"></i>
            </div>
            <div>
              <div class="brand-title">Sistema de Notas</div>
              <div class="brand-subtitle">Gestión académica institucional</div>
            </div>
          </div>

          <h1>Información académica clara y segura.</h1>
          <p class="lead-snc">
            Plataforma institucional para la administración de estudiantes, materias,
            asignaciones, calificaciones trimestrales y boletines académicos.
          </p>

          <div class="login-features">
            <div class="login-feature">
              <div class="login-feature-icon"><i class="bi bi-people"></i></div>
              <div>
                <strong>Gestión académica centralizada</strong>
                <span>Estudiantes, materias, usuarios y asignaciones en un solo lugar.</span>
              </div>
            </div>
            <div class="login-feature">
              <div class="login-feature-icon"><i class="bi bi-clipboard2-check"></i></div>
              <div>
                <strong>Calificaciones trimestrales</strong>
                <span>Registro y corrección de notas por parte de cada profesor.</span>
              </div>
            </div>
            <div class="login-feature">
              <div class="login-feature-icon"><i class="bi bi-file-earmark-text"></i></div>
              <div>
                <strong>Boletines institucionales</strong>
                <span>Consulta de notas, promedio anual y estado académico.</span>
              </div>
            </div>
          </div>
        </div>

        <div class="login-brand-footer">
          Acceso exclusivo para personal autorizado · Secretaría y Profesor
        </div>
      </section>

      <section class="login-form-panel">
        <div class="login-card">
          <div class="login-logo-mobile">
            <div class="brand-logo">
              <i class="bi bi-mortarboard-fill"></i>
            </div>
            <div>
              <div class="brand-title">Sistema de Notas</div>
              <div class="brand-subtitle">Gestión académica</div>
            </div>
          </div>

          <h2>Iniciar sesión</h2>
          <p class="login-subtitle">Ingrese sus credenciales institucionales para continuar.</p>

          <div v-if="sesionExpirada" class="alert alert-warning d-flex gap-2" role="alert">
            <i class="bi bi-clock-history mt-1"></i>
            <div>Su sesión expiró. Inicie sesión nuevamente para continuar.</div>
          </div>

          <div v-if="errorGeneral" class="alert alert-danger d-flex gap-2" role="alert">
            <i class="bi bi-exclamation-octagon-fill mt-1"></i>
            <div>{{ errorGeneral }}</div>
          </div>

          <form novalidate @submit.prevent="enviar">
            <div class="mb-3">
              <label for="nombreUsuario" class="form-label">
                Usuario<span class="required-mark">*</span>
              </label>
              <input
                id="nombreUsuario"
                v-model="formulario.nombreUsuario"
                type="text"
                class="form-control"
                :class="{ 'is-invalid': errores.nombreUsuario }"
                autocomplete="username"
                maxlength="50"
                placeholder="Nombre de usuario"
                autofocus
                @input="limpiarError('nombreUsuario')"
              />
              <div v-if="errores.nombreUsuario" class="invalid-feedback">
                {{ errores.nombreUsuario }}
              </div>
            </div>

            <div class="mb-3">
              <label for="password" class="form-label">
                Contraseña<span class="required-mark">*</span>
              </label>
              <div class="password-field">
                <input
                  id="password"
                  v-model="formulario.password"
                  :type="mostrarPassword ? 'text' : 'password'"
                  class="form-control"
                  :class="{ 'is-invalid': errores.password }"
                  autocomplete="current-password"
                  maxlength="100"
                  placeholder="Contraseña"
                  @input="limpiarError('password')"
                />
                <button
                  type="button"
                  class="password-toggle"
                  :aria-label="mostrarPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                  @click="mostrarPassword = !mostrarPassword"
                >
                  <i class="bi" :class="mostrarPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
              <div v-if="errores.password" class="invalid-feedback d-block">
                {{ errores.password }}
              </div>
            </div>

            <button type="submit" class="btn btn-primary w-100 mt-2" :disabled="enviando">
              <span
                v-if="enviando"
                class="spinner-border spinner-border-sm"
                role="status"
                aria-hidden="true"
              ></span>
              <i v-else class="bi bi-box-arrow-in-right"></i>
              {{ enviando ? 'Ingresando…' : 'Ingresar al sistema' }}
            </button>
          </form>

          <button
            type="button"
            class="api-status mt-3"
            :class="estadoApi"
            :title="
              estadoApi === 'fail'
                ? 'Verifique que el backend esté en ejecución en http://localhost:5012'
                : ''
            "
            @click="verificarServidor"
          >
            <span
              v-if="estadoApi === 'checking'"
              class="spinner-border spinner-border-sm"
              aria-hidden="true"
            ></span>
            <i
              v-else
              class="bi"
              :class="estadoApi === 'ok' ? 'bi-check-circle-fill' : 'bi-exclamation-circle-fill'"
            ></i>
            {{ textoEstadoApi }}
          </button>

          <div class="login-security-note">
            <i class="bi bi-shield-lock"></i>
            <span>
              Sus credenciales se validan directamente contra la API. La contraseña no se
              almacena en el navegador.
            </span>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>
