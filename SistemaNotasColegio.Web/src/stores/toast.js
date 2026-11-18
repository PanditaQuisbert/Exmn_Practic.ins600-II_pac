import { reactive } from 'vue'

let consecutivo = 0
const MAXIMO_VISIBLES = 5

export const toasts = reactive([])

function quitar(id) {
  const indice = toasts.findIndex((elemento) => elemento.id === id)
  if (indice >= 0) toasts.splice(indice, 1)
}

function agregar(tipo, mensaje, duracion = 4500) {
  const texto = String(mensaje ?? '').trim()
  if (!texto) return null

  const id = ++consecutivo
  toasts.push({ id, tipo, mensaje: texto })

  while (toasts.length > MAXIMO_VISIBLES) {
    toasts.shift()
  }

  if (duracion > 0) {
    window.setTimeout(() => quitar(id), duracion)
  }

  return id
}

export const toast = {
  success: (mensaje, duracion) => agregar('success', mensaje, duracion),
  error: (mensaje, duracion) => agregar('danger', mensaje, duracion ?? 6500),
  warning: (mensaje, duracion) => agregar('warning', mensaje, duracion),
  info: (mensaje, duracion) => agregar('info', mensaje, duracion),
  quitar
}
