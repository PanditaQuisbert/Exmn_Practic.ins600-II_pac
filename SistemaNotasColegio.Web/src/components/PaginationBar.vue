<script setup>
import { computed } from 'vue'

const props = defineProps({
  pagina: { type: Number, required: true },
  totalPaginas: { type: Number, required: true },
  total: { type: Number, required: true },
  tamano: { type: Number, default: 10 }
})

const emitir = defineEmits(['update:pagina'])

const desde = computed(() => (props.total === 0 ? 0 : (props.pagina - 1) * props.tamano + 1))
const hasta = computed(() => Math.min(props.total, props.pagina * props.tamano))

const paginas = computed(() => {
  const totalPaginas = props.totalPaginas
  if (totalPaginas <= 7) {
    return Array.from({ length: totalPaginas }, (_, indice) => indice + 1)
  }

  let inicio = Math.max(1, props.pagina - 2)
  let fin = Math.min(totalPaginas, inicio + 4)
  if (fin - inicio < 4) inicio = Math.max(1, fin - 4)

  const resultado = []
  if (inicio > 1) {
    resultado.push(1)
    if (inicio > 2) resultado.push('…')
  }
  for (let numero = inicio; numero <= fin; numero += 1) resultado.push(numero)
  if (fin < totalPaginas) {
    if (fin < totalPaginas - 1) resultado.push('…')
    resultado.push(totalPaginas)
  }
  return resultado
})

function irA(pagina) {
  if (pagina < 1 || pagina > props.totalPaginas || pagina === props.pagina) return
  emitir('update:pagina', pagina)
}
</script>

<template>
  <div class="pagination-snc">
    <span class="table-count">
      Mostrando {{ desde }}–{{ hasta }} de {{ total }} registro{{ total === 1 ? '' : 's' }}
    </span>

    <nav v-if="totalPaginas > 1" aria-label="Paginación de registros">
      <ul class="pagination">
        <li class="page-item" :class="{ disabled: pagina <= 1 }">
          <button class="page-link" type="button" aria-label="Página anterior" @click="irA(pagina - 1)">
            <i class="bi bi-chevron-left"></i>
          </button>
        </li>
        <li
          v-for="(numero, indice) in paginas"
          :key="`${numero}-${indice}`"
          class="page-item"
          :class="{ active: numero === pagina, disabled: numero === '…' }"
        >
          <span v-if="numero === '…'" class="page-link">…</span>
          <button v-else class="page-link" type="button" @click="irA(numero)">
            {{ numero }}
          </button>
        </li>
        <li class="page-item" :class="{ disabled: pagina >= totalPaginas }">
          <button class="page-link" type="button" aria-label="Página siguiente" @click="irA(pagina + 1)">
            <i class="bi bi-chevron-right"></i>
          </button>
        </li>
      </ul>
    </nav>
  </div>
</template>
