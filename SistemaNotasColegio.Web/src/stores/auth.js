import { computed, reactive } from 'vue'

const CLAVE_ALMACENAMIENTO = 'snc.sesion'

function leerSesionGuardada() {
  try {
    const texto = localStorage.getItem(CLAVE_ALMACENAMIENTO)
    if (!texto) return null

    const datos = JSON.parse(texto)
    if (!datos?.token || !datos?.usuario) return null

    return datos
  } catch {
    return null
  }
}

/**
 * El backend emite el token con DateTime.UtcNow. Si la fecha serializada no
 * incluye zona horaria, se interpreta como UTC para no adelantar la expiración.
 */
function normalizarExpiracion(valor) {
  if (!valor) return null

  const texto = String(valor)
  const tieneZona = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(texto)
  const fecha = new Date(tieneZona ? texto : `${texto}Z`)

  return Number.isNaN(fecha.getTime()) ? null : fecha.toISOString()
}

const guardada = leerSesionGuardada()

export const estadoSesion = reactive({
  token: guardada?.token ?? null,
  tipoToken: guardada?.tipoToken ?? 'Bearer',
  expiraEn: guardada?.expiraEn ?? null,
  usuario: guardada?.usuario ?? null
})

export function sesionVigente() {
  if (!estadoSesion.token || !estadoSesion.usuario) return false
  if (!estadoSesion.expiraEn) return true

  const expiracion = Date.parse(estadoSesion.expiraEn)
  if (Number.isNaN(expiracion)) return true

  return expiracion - Date.now() > 5000
}

export const estaAutenticado = computed(() => sesionVigente())
export const usuarioActual = computed(() => estadoSesion.usuario)
export const rolActual = computed(() => Number(estadoSesion.usuario?.rolId ?? 0))

export function obtenerToken() {
  return sesionVigente() ? estadoSesion.token : null
}

export function tieneRol(...roles) {
  if (!sesionVigente()) return false
  return roles.map(Number).includes(rolActual.value)
}

export function esSecretaria() {
  return rolActual.value === 1
}

export function esProfesor() {
  return rolActual.value === 2
}

export function rutaInicio() {
  if (rolActual.value === 1) return '/secretaria'
  if (rolActual.value === 2) return '/profesor'
  return '/login'
}

export function nombreUsuarioSesion() {
  const usuario = estadoSesion.usuario
  if (!usuario) return ''
  return `${usuario.nombres ?? ''} ${usuario.apellidos ?? ''}`.trim() || usuario.nombreUsuario || ''
}

export function iniciarSesion(respuesta) {
  if (!respuesta?.token || !respuesta?.usuario) {
    throw new Error('La respuesta de inicio de sesión no contiene la sesión esperada.')
  }

  estadoSesion.token = respuesta.token
  estadoSesion.tipoToken = respuesta.tipoToken || 'Bearer'
  estadoSesion.expiraEn = normalizarExpiracion(respuesta.expiraEn)
  estadoSesion.usuario = respuesta.usuario

  localStorage.setItem(
    CLAVE_ALMACENAMIENTO,
    JSON.stringify({
      token: estadoSesion.token,
      tipoToken: estadoSesion.tipoToken,
      expiraEn: estadoSesion.expiraEn,
      usuario: estadoSesion.usuario
    })
  )
}

export function cerrarSesion() {
  estadoSesion.token = null
  estadoSesion.tipoToken = 'Bearer'
  estadoSesion.expiraEn = null
  estadoSesion.usuario = null
  localStorage.removeItem(CLAVE_ALMACENAMIENTO)
}

/**
 * Limpia la sesión y notifica únicamente si realmente existía una sesión activa,
 * evitando mensajes repetidos por solicitudes paralelas que reciban 401.
 */
export function expirarSesion() {
  const existiaSesion = Boolean(estadoSesion.token)
  cerrarSesion()
  return existiaSesion
}
