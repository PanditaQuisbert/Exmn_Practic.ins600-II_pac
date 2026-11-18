const FORMATO_FECHA = new Intl.DateTimeFormat('es-EC', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric'
})

const FORMATO_FECHA_HORA = new Intl.DateTimeFormat('es-EC', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit'
})

const FORMATO_FECHA_LARGA = new Intl.DateTimeFormat('es-EC', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
  year: 'numeric'
})

function convertirFecha(valor) {
  if (!valor) return null
  const fecha = valor instanceof Date ? valor : new Date(valor)
  return Number.isNaN(fecha.getTime()) ? null : fecha
}

export function formatearFecha(valor) {
  const fecha = convertirFecha(valor)
  return fecha ? FORMATO_FECHA.format(fecha) : '—'
}

export function formatearFechaHora(valor) {
  const fecha = convertirFecha(valor)
  return fecha ? FORMATO_FECHA_HORA.format(fecha) : '—'
}

export function formatearFechaLarga(valor = new Date()) {
  const fecha = convertirFecha(valor)
  return fecha ? FORMATO_FECHA_LARGA.format(fecha) : '—'
}

export function formatearNota(valor) {
  if (valor === null || valor === undefined || valor === '') return '—'
  const numero = Number(valor)
  if (Number.isNaN(numero)) return '—'
  return numero.toLocaleString('es-EC', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
  })
}

export function nombreCompleto(persona) {
  if (!persona) return ''
  return `${persona.nombres ?? ''} ${persona.apellidos ?? ''}`.trim()
}

export function normalizarTexto(valor) {
  return String(valor ?? '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim()
}

export function coincideBusqueda(valor, busqueda) {
  const termino = normalizarTexto(busqueda)
  if (!termino) return true
  return normalizarTexto(valor).includes(termino)
}

export function etiquetaTrimestre(trimestre) {
  const numero = Number(trimestre)
  if (numero === 1) return 'Primer trimestre'
  if (numero === 2) return 'Segundo trimestre'
  if (numero === 3) return 'Tercer trimestre'
  return `Trimestre ${trimestre}`
}

export function etiquetaTrimestreCorta(trimestre) {
  const numero = Number(trimestre)
  if (numero === 1) return 'T1'
  if (numero === 2) return 'T2'
  if (numero === 3) return 'T3'
  return '—'
}

export function iniciales(persona) {
  const nombres = String(persona?.nombres ?? '').trim()
  const apellidos = String(persona?.apellidos ?? '').trim()
  const primera = nombres.charAt(0)
  const segunda = apellidos.charAt(0)
  return `${primera}${segunda}`.toUpperCase() || 'SN'
}
