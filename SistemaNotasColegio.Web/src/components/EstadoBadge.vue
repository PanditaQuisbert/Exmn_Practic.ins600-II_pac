<script setup>
defineProps({
  activo: { type: Boolean, default: false },
  textoActivo: { type: String, default: 'Activo' },
  textoInactivo: { type: String, default: 'Inactivo' }
})
</script>

<template>
  <span class="badge-estado" :class="activo ? 'activo' : 'inactivo'">
    {{ activo ? textoActivo : textoInactivo }}
  </span>
</template>
