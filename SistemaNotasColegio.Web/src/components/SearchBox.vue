<script setup>
defineProps({
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: 'Buscar…' },
  etiqueta: { type: String, default: 'Buscar' }
})

defineEmits(['update:modelValue'])
</script>

<template>
  <div class="search-box">
    <i class="bi bi-search"></i>
    <input
      type="text"
      class="form-control"
      :value="modelValue"
      :placeholder="placeholder"
      :aria-label="etiqueta"
      @input="$emit('update:modelValue', $event.target.value)"
    />
    <button
      v-if="modelValue"
      type="button"
      class="search-clear"
      aria-label="Limpiar búsqueda"
      @click="$emit('update:modelValue', '')"
    >
      <i class="bi bi-x-lg"></i>
    </button>
  </div>
</template>
