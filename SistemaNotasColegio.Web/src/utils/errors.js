/**
 * Toma los errores de validación del ProblemDetails del backend y los adapta
 * a las claves usadas por los formularios del frontend.
 */
export function erroresDeCampos(campos = {}) {
  const resultado = {}

  for (const [clave, mensaje] of Object.entries(campos ?? {})) {
    if (typeof mensaje !== 'string' || !mensaje.trim()) continue
    const claveLocal = clave.charAt(0).toLowerCase() + clave.slice(1)
    resultado[claveLocal] = mensaje
  }

  return resultado
}

/**
 * Consume el parámetro ?nuevo=1 usado por los accesos rápidos del panel.
 * Devuelve true una sola vez para que la vista abra su formulario.
 */
export function consumirParametroNuevo(ruta, router) {
  if (ruta.query.nuevo !== '1') return false

  const query = { ...ruta.query }
  delete query.nuevo
  router.replace({ name: ruta.name, query })

  return true
}
