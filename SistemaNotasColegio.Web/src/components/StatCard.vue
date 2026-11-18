<script setup>
defineProps({
  etiqueta: { type: String, required: true },
  valor: { type: [Number, String], default: 0 },
  icono: { type: String, default: 'bar-chart' },
  variante: { type: String, default: '' },
  pista: { type: String, default: '' },
  cargando: { type: Boolean, default: false }
})
</script>

<template>
  <div class="stat-card" :class="variante ? `variant-${variante}` : ''">
    <div class="d-flex align-items-start justify-content-between gap-2">
      <div class="min-w-0">
        <div class="stat-label">{{ etiqueta }}</div>
        <div class="stat-value mt-2">
          <span v-if="cargando" class="spinner-border spinner-border-sm text-secondary" role="status">
            <span class="visually-hidden">Cargando</span>
          </span>
          <span v-else>{{ valor }}</span>
        </div>
      </div>
      <div class="stat-icon">
        <i class="bi" :class="`bi-${icono}`"></i>
      </div>
    </div>
    <div v-if="pista" class="stat-hint mt-2">{{ pista }}</div>
  </div>
</template>
