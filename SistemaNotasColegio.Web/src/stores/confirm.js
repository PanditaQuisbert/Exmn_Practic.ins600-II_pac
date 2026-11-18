import { reactive } from 'vue'

let resolverActual = null

export const estadoConfirmacion = reactive({
  abierta: false,
  titulo: '',
  mensaje: '',
  detalle: '',
  textoConfirmar: 'Confirmar',
  textoCancelar: 'Cancelar',
  variante: 'primary',
  icono: 'question-circle'
})

/**
 * Abre un diálogo de confirmación y resuelve true/false según la decisión.
 */
export function confirmar(opciones = {}) {
  if (resolverActual) {
    resolverActual(false)
    resolverActual = null
  }

  estadoConfirmacion.titulo = opciones.titulo ?? 'Confirmar acción'
  estadoConfirmacion.mensaje = opciones.mensaje ?? '¿Desea continuar?'
  estadoConfirmacion.detalle = opciones.detalle ?? ''
  estadoConfirmacion.textoConfirmar = opciones.textoConfirmar ?? 'Confirmar'
  estadoConfirmacion.textoCancelar = opciones.textoCancelar ?? 'Cancelar'
  estadoConfirmacion.variante = opciones.variante ?? 'primary'
  estadoConfirmacion.icono = opciones.icono ?? 'question-circle'
  estadoConfirmacion.abierta = true

  return new Promise((resolver) => {
    resolverActual = resolver
  })
}

export function responderConfirmacion(valor) {
  const resolver = resolverActual
  resolverActual = null
  estadoConfirmacion.abierta = false
  if (resolver) resolver(Boolean(valor))
}
