import { computed, ref, watch } from 'vue'

/**
 * Paginación local reutilizable. Recibe un computed con la lista ya filtrada.
 */
export function usePaginacion(origen, tamanoPagina = 10) {
  const pagina = ref(1)

  const total = computed(() => (Array.isArray(origen.value) ? origen.value.length : 0))
  const totalPaginas = computed(() => Math.max(1, Math.ceil(total.value / tamanoPagina)))

  const elementos = computed(() => {
    const inicio = (pagina.value - 1) * tamanoPagina
    return origen.value.slice(inicio, inicio + tamanoPagina)
  })

  watch(totalPaginas, (paginas) => {
    if (pagina.value > paginas) pagina.value = paginas
    if (pagina.value < 1) pagina.value = 1
  })

  watch(total, () => {
    if (pagina.value > totalPaginas.value) pagina.value = totalPaginas.value
  })

  function reiniciar() {
    pagina.value = 1
  }

  return {
    pagina,
    total,
    totalPaginas,
    elementos,
    reiniciar,
    tamanoPagina
  }
}
