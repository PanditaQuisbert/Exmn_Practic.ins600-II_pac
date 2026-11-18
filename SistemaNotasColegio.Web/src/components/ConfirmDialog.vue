<script setup>
import { computed, onBeforeUnmount, watch } from 'vue'
import { estadoConfirmacion, responderConfirmacion } from '../stores/confirm'

const claseBoton = computed(() => {
  if (estadoConfirmacion.variante === 'danger') return 'btn-danger'
  if (estadoConfirmacion.variante === 'success') return 'btn-success'
  return 'btn-primary'
})

const claseIcono = computed(() => {
  const variantesPermitidas = ['danger', 'success', 'warning']
  return variantesPermitidas.includes(estadoConfirmacion.variante)
    ? estadoConfirmacion.variante
    : ''
})

function alPresionarTecla(evento) {
  if (evento.key === 'Escape' && estadoConfirmacion.abierta) {
    responderConfirmacion(false)
  }
}

watch(
  () => estadoConfirmacion.abierta,
  (abierta) => {
    if (abierta) document.addEventListener('keydown', alPresionarTecla)
    else document.removeEventListener('keydown', alPresionarTecla)
  }
)

onBeforeUnmount(() => {
  document.removeEventListener('keydown', alPresionarTecla)
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="estadoConfirmacion.abierta"
      class="snc-modal-backdrop"
      @click.self="responderConfirmacion(false)"
    >
      <div
        class="snc-modal modal-sm-snc"
        role="dialog"
        aria-modal="true"
        :aria-label="estadoConfirmacion.titulo"
      >
        <div class="confirm-body">
          <div class="confirm-icon" :class="claseIcono">
            <i class="bi" :class="`bi-${estadoConfirmacion.icono}`"></i>
          </div>
          <h2 class="confirm-title">{{ estadoConfirmacion.titulo }}</h2>
          <p class="confirm-message">{{ estadoConfirmacion.mensaje }}</p>
          <p v-if="estadoConfirmacion.detalle" class="confirm-detail">
            {{ estadoConfirmacion.detalle }}
          </p>
        </div>

        <div class="snc-modal-footer">
          <button type="button" class="btn btn-light-snc" @click="responderConfirmacion(false)">
            {{ estadoConfirmacion.textoCancelar }}
          </button>
          <button type="button" class="btn" :class="claseBoton" @click="responderConfirmacion(true)">
            {{ estadoConfirmacion.textoConfirmar }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
