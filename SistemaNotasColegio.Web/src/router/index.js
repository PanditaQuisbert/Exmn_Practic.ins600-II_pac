import { createRouter, createWebHistory } from 'vue-router'
import {
  cerrarSesion,
  estaAutenticado,
  estadoSesion,
  rutaInicio,
  tieneRol
} from '../stores/auth'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/auth/LoginView.vue'),
    meta: { publica: true, titulo: 'Iniciar sesión' }
  },
  {
    path: '/403',
    name: 'prohibido',
    component: () => import('../views/ForbiddenView.vue'),
    meta: { requiereSesion: true, titulo: 'Acceso denegado' }
  },
  {
    path: '/secretaria',
    component: () => import('../layouts/MainLayout.vue'),
    meta: { requiereSesion: true, roles: [1] },
    children: [
      {
        path: '',
        name: 'secretaria-panel',
        component: () => import('../views/secretaria/PanelView.vue'),
        meta: { titulo: 'Panel principal', icono: 'grid-1x2' }
      },
      {
        path: 'estudiantes',
        name: 'secretaria-estudiantes',
        component: () => import('../views/secretaria/EstudiantesView.vue'),
        meta: { titulo: 'Estudiantes', icono: 'people' }
      },
      {
        path: 'materias',
        name: 'secretaria-materias',
        component: () => import('../views/secretaria/MateriasView.vue'),
        meta: { titulo: 'Materias', icono: 'journal-bookmark' }
      },
      {
        path: 'usuarios',
        name: 'secretaria-usuarios',
        component: () => import('../views/secretaria/UsuariosView.vue'),
        meta: { titulo: 'Usuarios', icono: 'person-badge' }
      },
      {
        path: 'asignaciones',
        name: 'secretaria-asignaciones',
        component: () => import('../views/secretaria/AsignacionesView.vue'),
        meta: { titulo: 'Asignaciones', icono: 'diagram-3' }
      },
      {
        path: 'boletines',
        name: 'secretaria-boletines',
        component: () => import('../views/secretaria/BoletinesView.vue'),
        meta: { titulo: 'Boletines académicos', icono: 'file-earmark-text' }
      }
    ]
  },
  {
    path: '/profesor',
    component: () => import('../layouts/MainLayout.vue'),
    meta: { requiereSesion: true, roles: [2] },
    children: [
      {
        path: '',
        name: 'profesor-panel',
        component: () => import('../views/profesor/PanelView.vue'),
        meta: { titulo: 'Panel del profesor', icono: 'grid-1x2' }
      },
      {
        path: 'asignaciones',
        name: 'profesor-asignaciones',
        component: () => import('../views/profesor/MisAsignacionesView.vue'),
        meta: { titulo: 'Mis asignaciones', icono: 'journal-check' }
      },
      {
        path: 'calificaciones',
        name: 'profesor-calificaciones',
        component: () => import('../views/profesor/CalificacionesView.vue'),
        meta: { titulo: 'Calificaciones', icono: 'clipboard2-check' }
      }
    ]
  },
  {
    path: '/',
    redirect: () => (estaAutenticado.value ? rutaInicio() : '/login')
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'no-encontrado',
    component: () => import('../views/NotFoundView.vue'),
    meta: { publica: true, titulo: 'Página no encontrada' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  }
})

router.beforeEach((destino) => {
  const requiereSesion = destino.matched.some((registro) => registro.meta.requiereSesion)
  const esPublica = destino.matched.some((registro) => registro.meta.publica)

  if (esPublica) {
    if (destino.name === 'login' && estaAutenticado.value) {
      return rutaInicio()
    }
    return true
  }

  if (requiereSesion && !estaAutenticado.value) {
    const habiaToken = Boolean(estadoSesion.token)
    if (habiaToken) cerrarSesion()

    return {
      name: 'login',
      query: habiaToken
        ? { sesionExpirada: '1' }
        : destino.fullPath !== '/'
          ? { redirect: destino.fullPath }
          : {}
    }
  }

  const rolesPermitidos = destino.matched.flatMap((registro) => registro.meta.roles ?? [])
  if (rolesPermitidos.length && !tieneRol(...rolesPermitidos)) {
    return { name: 'prohibido' }
  }

  return true
})

router.afterEach((destino) => {
  const titulo = destino.meta.titulo ? `${destino.meta.titulo} · Sistema de Notas` : 'Sistema de Notas'
  document.title = titulo
})

export default router
