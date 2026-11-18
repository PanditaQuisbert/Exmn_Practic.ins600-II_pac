<script setup>
import { useRouter } from 'vue-router'
import { cerrarSesion, rutaInicio } from '../stores/auth'

const router = useRouter()

function salir() {
  cerrarSesion()
  router.replace({ name: 'login' })
}
</script>

<template>
  <div class="login-page d-flex align-items-center justify-content-center p-3">
    <div class="card-snc w-100" style="max-width: 560px">
      <div class="card-snc-body text-center p-4 p-sm-5">
        <div class="empty-icon" style="background: #fff4e0; color: #8f5c00">
          <i class="bi bi-shield-lock"></i>
        </div>
        <span class="badge-estado incompleto">Error 403</span>
        <h1 class="mt-3" style="font-size: 1.35rem">Acceso no autorizado</h1>
        <p class="text-snc-muted small-85 mb-4">
          Su rol actual no tiene permisos para acceder a esta sección del sistema. Si cree que
          se trata de un error, comuníquese con la Secretaría.
        </p>
        <div class="d-flex flex-wrap gap-2 justify-content-center">
          <RouterLink :to="rutaInicio()" class="btn btn-primary">
            <i class="bi bi-house-door"></i>
            Ir a mi panel
          </RouterLink>
          <button type="button" class="btn btn-light-snc" @click="salir">
            <i class="bi bi-box-arrow-right"></i>
            Cerrar sesión
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
