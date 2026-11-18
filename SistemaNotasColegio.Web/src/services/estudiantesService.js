import { api } from './api'

export async function listarEstudiantes(incluirInactivos = false) {
  const { data } = await api.get('/api/estudiantes', {
    params: { incluirInactivos }
  })
  return data
}

export async function obtenerEstudiante(id) {
  const { data } = await api.get(`/api/estudiantes/${id}`)
  return data
}

export async function crearEstudiante(payload) {
  const { data } = await api.post('/api/estudiantes', payload)
  return data
}

export async function actualizarEstudiante(id, payload) {
  const { data } = await api.put(`/api/estudiantes/${id}`, payload)
  return data
}

export async function cambiarEstadoEstudiante(id, activo) {
  const { data } = await api.patch(`/api/estudiantes/${id}/estado`, { activo })
  return data
}
