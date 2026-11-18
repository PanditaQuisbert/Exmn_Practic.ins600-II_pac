import { api } from './api'

export async function listarAsignaciones({ profesorId = null, materiaId = null } = {}) {
  const params = {}
  if (profesorId) params.profesorId = Number(profesorId)
  if (materiaId) params.materiaId = Number(materiaId)

  const { data } = await api.get('/api/asignaciones', { params })
  return data
}

export async function crearAsignacion(payload) {
  const { data } = await api.post('/api/asignaciones', payload)
  return data
}

export async function eliminarAsignacion(id) {
  await api.delete(`/api/asignaciones/${id}`)
}
