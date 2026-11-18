<script setup>
import { onBeforeUnmount, watch } from 'vue'

const props = defineProps({
  abierta: { type: Boolean, default: false },
  titulo: { type: String, default: '' },
  subtitulo: { type: String, default: '' },
  tamano: { type: String, default: '' },
  bloquearCierre: { type: Boolean, default: false }
})

const emitir = defineEmits(['cerrar'])

function cerrar() {
  if (!props.bloquearCierre) emitir('cerrar')
}

function alPresionarTecla(evento) {
  if (evento.key === 'Escape' && props.abierta) cerrar()
}

watch(
  () => props.abierta,
  (abierta) => {
    if (abierta) {
      document.addEventListener('keydown', alPresionarTecla)
      document.body.style.overflow = 'hidden'
    } else {
      document.removeEventListener('keydown', alPresionarTecla)
      document.body.style.overflow = ''
    }
  }
)

onBeforeUnmount(() => {
  document.removeEventListener('keydown', alPresionarTecla)
  document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <div v-if="abierta" class="snc-modal-backdrop" @click.self="cerrar">
      <div
        class="snc-modal"
        :class="{
          'modal-lg-snc': tamano === 'lg',
          'modal-sm-snc': tamano === 'sm'
        }"
        role="dialog"
        aria-modal="true"
        :aria-label="titulo"
      >
        <div class="snc-modal-header">
          <div>
            <h2>{{ titulo }}</h2>
            <p v-if="subtitulo">{{ subtitulo }}</p>
          </div>
          <button
            type="button"
            class="modal-close"
            aria-label="Cerrar ventana"
            :disabled="bloquearCierre"
            @click="cerrar"
          >
            <i class="bi bi-x-lg"></i>
          </button>
        </div>

        <div class="snc-modal-body">
          <slot />
        </div>

        <div class="snc-modal-footer">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>
