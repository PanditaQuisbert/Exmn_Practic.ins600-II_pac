import { api } from './api'

export async function listarMaterias(incluirInactivas = false) {
  const { data } = await api.get('/api/materias', {
    params: { incluirInactivas }
  })
  return data
}

export async function obtenerMateria(id) {
  const { data } = await api.get(`/api/materias/${id}`)
  return data
}

export async function crearMateria(payload) {
  const { data } = await api.post('/api/materias', payload)
  return data
}

export async function actualizarMateria(id, payload) {
  const { data } = await api.put(`/api/materias/${id}`, payload)
  return data
}

export async function cambiarEstadoMateria(id, activo) {
  const { data } = await api.patch(`/api/materias/${id}/estado`, { activo })
  return data
}
