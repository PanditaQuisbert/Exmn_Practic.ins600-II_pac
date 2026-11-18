<script setup>
import { computed } from 'vue'
import { estaAutenticado, rutaInicio } from '../stores/auth'

const destino = computed(() => (estaAutenticado.value ? rutaInicio() : '/login'))
const textoBoton = computed(() => (estaAutenticado.value ? 'Ir a mi panel' : 'Iniciar sesión'))
</script>

<template>
  <div class="login-page d-flex align-items-center justify-content-center p-3">
    <div class="card-snc w-100" style="max-width: 540px">
      <div class="card-snc-body text-center p-4 p-sm-5">
        <div class="empty-icon">
          <i class="bi bi-compass"></i>
        </div>
        <span class="badge-estado inactivo">Error 404</span>
        <h1 class="mt-3" style="font-size: 1.35rem">Página no encontrada</h1>
        <p class="text-snc-muted small-85 mb-4">
          La dirección solicitada no existe o ya no está disponible en el sistema.
        </p>
        <RouterLink :to="destino" class="btn btn-primary">
          <i class="bi bi-arrow-left"></i>
          {{ textoBoton }}
        </RouterLink>
      </div>
    </div>
  </div>
</template>
