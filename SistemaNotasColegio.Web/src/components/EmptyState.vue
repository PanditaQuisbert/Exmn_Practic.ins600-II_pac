<script setup>
defineProps({
  icono: { type: String, default: 'inbox' },
  titulo: { type: String, default: 'Sin registros' },
  mensaje: { type: String, default: '' }
})
</script>

<template>
  <div class="empty-state">
    <div class="empty-icon">
      <i class="bi" :class="`bi-${icono}`"></i>
    </div>
    <h3>{{ titulo }}</h3>
    <p v-if="mensaje">{{ mensaje }}</p>
    <div v-if="$slots.default" class="mt-3">
      <slot />
    </div>
  </div>
</template>
