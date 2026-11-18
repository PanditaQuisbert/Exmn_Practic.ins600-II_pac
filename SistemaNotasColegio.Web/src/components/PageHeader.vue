<script setup>
defineProps({
  titulo: { type: String, required: true },
  descripcion: { type: String, default: '' }
})
</script>

<template>
  <div class="page-header">
    <div>
      <h1>{{ titulo }}</h1>
      <p v-if="descripcion">{{ descripcion }}</p>
    </div>
    <div class="page-header-actions">
      <slot name="acciones" />
    </div>
  </div>
</template>
