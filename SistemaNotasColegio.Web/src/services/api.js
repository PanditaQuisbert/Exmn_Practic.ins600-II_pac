import axios from 'axios'
import {
  expirarSesion,
  obtenerToken
} from '../stores/auth'
import { loadingStore } from '../stores/loading'

const URL_BASE = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5012').replace(/\/+$/, '')

export const api = axios.create({
  baseURL: URL_BASE,
  timeout: 20000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json'
  }
})

function extraerDetalles(datos) {
  if (!datos || typeof datos !== 'object') return []

  const errores = datos.errors
  if (!errores || typeof errores !== 'object') return []

  return Object.values(errores)
    .flat()
    .filter((mensaje) => typeof mensaje === 'string' && mensaje.trim())
}

function extraerCampos(datos) {
  const campos = {}
  const errores = datos?.errors

  if (!errores || typeof errores !== 'object') return campos

  for (const [campo, mensajes] of Object.entries(errores)) {
    const primero = Array.isArray(mensajes) ? mensajes[0] : mensajes
    if (typeof primero === 'string' && primero.trim()) {
      campos[String(campo).toLowerCase()] = primero
    }
  }

  return campos
}

export function normalizarError(error) {
  if (error?.name === 'ApiError') return error

  const respuesta = error?.response
  const datos = respuesta?.data
  const estado = respuesta?.status ?? 0

  if (!respuesta) {
    const mensaje =
      error?.code === 'ECONNABORTED'
        ? 'La solicitud tardó demasiado. Intente nuevamente.'
        : `No se pudo conectar con la API en ${URL_BASE}. Verifique que el backend esté en ejecución.`

    const errorConexion = new Error(mensaje)
    errorConexion.name = 'ApiError'
    errorConexion.status = 0
    errorConexion.mensaje = mensaje
    errorConexion.detalles = []
    errorConexion.campos = {}
    errorConexion.esConexion = true
    return errorConexion
  }

  const detalles = extraerDetalles(datos)
  const campos = extraerCampos(datos)
  let mensaje = ''

  if (typeof datos === 'string' && datos.trim()) {
    mensaje = datos
  } else if (datos?.mensaje) {
    mensaje = datos.mensaje
  } else if (detalles.length) {
    mensaje = 'Revise los datos ingresados.'
  } else if (datos?.detail) {
    mensaje = datos.detail
  } else if (datos?.title && !datos.title.toLowerCase().includes('validation')) {
    mensaje = datos.title
  }

  if (!mensaje) {
    if (estado === 400) mensaje = 'Los datos enviados no son válidos.'
    else if (estado === 401) mensaje = 'Su sesión ha expirado o las credenciales no son válidas.'
    else if (estado === 403) mensaje = 'No tiene permisos para realizar esta acción.'
    else if (estado === 404) mensaje = 'El recurso solicitado no existe.'
    else if (estado === 409) mensaje = 'La operación entra en conflicto con los datos existentes.'
    else if (estado >= 500) mensaje = 'Ocurrió un error interno en el servidor. Intente nuevamente.'
    else mensaje = 'No fue posible completar la solicitud.'
  }

  const errorNormalizado = new Error(mensaje)
  errorNormalizado.name = 'ApiError'
  errorNormalizado.status = estado
  errorNormalizado.mensaje = mensaje
  errorNormalizado.detalles = detalles
  errorNormalizado.campos = campos
  errorNormalizado.esConexion = false
  return errorNormalizado
}

api.interceptors.request.use(
  (configuracion) => {
    const token = obtenerToken()

    if (token) {
      configuracion.headers.Authorization = `Bearer ${token}`
    }

    loadingStore.iniciar()
    return configuracion
  },
  (error) => {
    loadingStore.finalizar()
    return Promise.reject(normalizarError(error))
  }
)

api.interceptors.response.use(
  (respuesta) => {
    loadingStore.finalizar()
    return respuesta
  },
  (error) => {
    loadingStore.finalizar()

    const normalizado = normalizarError(error)
    const esLogin = String(error?.config?.url ?? '').includes('/api/auth/login')

    if (normalizado.status === 401 && !esLogin) {
      const existiaSesion = expirarSesion()
      if (existiaSesion) {
        window.dispatchEvent(new CustomEvent('snc:sesion-expirada'))
      }
    }

    return Promise.reject(normalizado)
  }
)

export function mensajeDeError(error, respaldo = 'No fue posible completar la solicitud.') {
  return error?.mensaje || error?.message || respaldo
}

export function urlApi() {
  return URL_BASE
}
