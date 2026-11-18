<script setup>
import { computed } from 'vue'

const props = defineProps({
  estado: { type: String, default: 'INCOMPLETO' }
})

const clase = computed(() => {
  const valor = String(props.estado ?? '').toUpperCase()
  if (valor === 'APROBADO') return 'aprobado'
  if (valor === 'REPROBADO') return 'reprobado'
  return 'incompleto'
})

const texto = computed(() => {
  const valor = String(props.estado ?? '').toUpperCase()
  if (valor === 'APROBADO') return 'Aprobado'
  if (valor === 'REPROBADO') return 'Reprobado'
  return 'Incompleto'
})
</script>

<template>
  <span class="badge-estado" :class="clase">{{ texto }}</span>
</template>
