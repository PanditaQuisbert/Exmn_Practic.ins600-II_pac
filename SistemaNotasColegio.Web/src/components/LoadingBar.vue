<script setup>
import { hayCargaGlobal } from '../stores/loading'
</script>

<template>
  <div
    v-if="hayCargaGlobal"
    class="loading-bar"
    role="progressbar"
    aria-label="Cargando información"
  >
    <div class="loading-track"></div>
  </div>
</template>
