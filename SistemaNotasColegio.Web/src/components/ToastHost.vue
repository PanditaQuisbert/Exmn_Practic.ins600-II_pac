<script setup>
import { toast, toasts } from '../stores/toast'

const iconos = {
  success: 'bi-check-circle-fill',
  danger: 'bi-exclamation-octagon-fill',
  warning: 'bi-exclamation-triangle-fill',
  info: 'bi-info-circle-fill'
}
</script>

<template>
  <div class="toast-host" aria-live="polite" aria-atomic="false">
    <TransitionGroup name="toast">
      <div
        v-for="elemento in toasts"
        :key="elemento.id"
        class="snc-toast"
        :class="elemento.tipo"
        role="status"
      >
        <i class="bi toast-icon" :class="iconos[elemento.tipo] ?? iconos.info"></i>
        <div class="toast-message">{{ elemento.mensaje }}</div>
        <button
          type="button"
          class="toast-close"
          aria-label="Cerrar notificación"
          @click="toast.quitar(elemento.id)"
        >
          <i class="bi bi-x-lg"></i>
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>
