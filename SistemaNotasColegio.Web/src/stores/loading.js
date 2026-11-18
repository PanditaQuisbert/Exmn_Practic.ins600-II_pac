import { computed, reactive } from 'vue'

export const estadoCarga = reactive({
  pendientes: 0
})

export const hayCargaGlobal = computed(() => estadoCarga.pendientes > 0)

export const loadingStore = {
  iniciar() {
    estadoCarga.pendientes += 1
  },
  finalizar() {
    estadoCarga.pendientes = Math.max(0, estadoCarga.pendientes - 1)
  },
  reiniciar() {
    estadoCarga.pendientes = 0
  }
}
