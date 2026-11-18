<script setup>
import { computed, ref, watch } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import {
  cerrarSesion,
  estadoSesion,
  nombreUsuarioSesion,
  rolActual
} from '../stores/auth'
import { confirmar } from '../stores/confirm'
import { toast } from '../stores/toast'
import { iniciales } from '../utils/format'

const ruta = useRoute()
const router = useRouter()
const menuAbierto = ref(false)

const esSecretaria = computed(() => rolActual.value === 1)
const nombreRol = computed(() => (esSecretaria.value ? 'Secretaría' : 'Profesor'))
const nombre = computed(() => nombreUsuarioSesion())
const inicialesUsuario = computed(() => iniciales(estadoSesion.usuario))

const menu = computed(() =>
  esSecretaria.value
    ? [
        { to: '/secretaria', etiqueta: 'Panel principal', icono: 'grid-1x2', exacto: true },
        { to: '/secretaria/estudiantes', etiqueta: 'Estudiantes', icono: 'people' },
        { to: '/secretaria/materias', etiqueta: 'Materias', icono: 'journal-bookmark' },
        { to: '/secretaria/usuarios', etiqueta: 'Usuarios', icono: 'person-badge' },
        { to: '/secretaria/asignaciones', etiqueta: 'Asignaciones', icono: 'diagram-3' },
        { to: '/secretaria/boletines', etiqueta: 'Boletines', icono: 'file-earmark-text' }
      ]
    : [
        { to: '/profesor', etiqueta: 'Panel principal', icono: 'grid-1x2', exacto: true },
        { to: '/profesor/asignaciones', etiqueta: 'Mis asignaciones', icono: 'journal-check' },
        { to: '/profesor/calificaciones', etiqueta: 'Calificaciones', icono: 'clipboard2-check' }
      ]
)

const subtituloTopbar = computed(() =>
  esSecretaria.value ? 'Gestión académica y administrativa' : 'Registro y control de calificaciones'
)

function estaActivo(elemento) {
  if (elemento.exacto) return ruta.path === elemento.to
  return ruta.path === elemento.to || ruta.path.startsWith(`${elemento.to}/`)
}

watch(
  () => ruta.fullPath,
  () => {
    menuAbierto.value = false
  }
)

async function salir() {
  const confirmado = await confirmar({
    titulo: 'Cerrar sesión',
    mensaje: '¿Desea finalizar la sesión actual?',
    textoConfirmar: 'Cerrar sesión',
    variante: 'danger',
    icono: 'box-arrow-right'
  })

  if (!confirmado) return

  cerrarSesion()
  toast.info('Sesión cerrada correctamente.')
  router.replace({ name: 'login' })
}
</script>

<template>
  <div class="app-shell">
    <aside class="sidebar" :class="{ 'sidebar-abierta': menuAbierto }">
      <div class="sidebar-brand">
        <div class="brand-logo">
          <i class="bi bi-mortarboard-fill"></i>
        </div>
        <div>
          <div class="brand-title">Sistema de Notas</div>
          <div class="brand-subtitle">Gestión académica</div>
        </div>
      </div>

      <div class="sidebar-scroll">
        <div class="sidebar-section-label">Menú principal</div>
        <nav aria-label="Navegación principal">
          <ul class="sidebar-nav">
            <li v-for="elemento in menu" :key="elemento.to">
              <RouterLink
                :to="elemento.to"
                class="nav-link"
                :class="{ 'nav-activo': estaActivo(elemento) }"
              >
                <i class="bi" :class="`bi-${elemento.icono}`"></i>
                <span>{{ elemento.etiqueta }}</span>
              </RouterLink>
            </li>
          </ul>
        </nav>
      </div>

      <div class="sidebar-footer">
        <div><strong>{{ nombreRol }}</strong> · Sesión protegida</div>
        <div class="mt-1">Sistema de Notas v1.0</div>
      </div>
    </aside>

    <div v-if="menuAbierto" class="sidebar-backdrop" @click="menuAbierto = false"></div>

    <div class="main-area">
      <header class="topbar">
        <button
          type="button"
          class="btn btn-light-snc btn-icon d-lg-none"
          aria-label="Abrir menú"
          @click="menuAbierto = true"
        >
          <i class="bi bi-list"></i>
        </button>

        <div class="min-w-0">
          <h1 class="topbar-title">{{ ruta.meta.titulo ?? 'Panel' }}</h1>
          <p class="topbar-subtitle">{{ subtituloTopbar }}</p>
        </div>

        <div class="topbar-spacer"></div>

        <div class="topbar-user">
          <div class="user-chip">
            <div class="avatar-circle">{{ inicialesUsuario }}</div>
            <div class="user-meta">
              <div class="user-name" :title="nombre">{{ nombre }}</div>
              <div class="user-role">{{ nombreRol }}</div>
            </div>
          </div>

          <button type="button" class="btn btn-light-snc btn-sm" title="Cerrar sesión" @click="salir">
            <i class="bi bi-box-arrow-right"></i>
            <span class="btn-logout-text">Salir</span>
          </button>
        </div>
      </header>

      <main class="page-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>
