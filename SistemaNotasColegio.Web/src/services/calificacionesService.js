import { api } from './api'

export async function listarCalificaciones({ estudianteId = null, materiaId = null, trimestre = null } = {}) {
  const params = {}
  if (estudianteId) params.estudianteId = Number(estudianteId)
  if (materiaId) params.materiaId = Number(materiaId)
  if (trimestre) params.trimestre = Number(trimestre)

  const { data } = await api.get('/api/calificaciones', { params })
  return data
}

export async function obtenerCalificacion(id) {
  const { data } = await api.get(`/api/calificaciones/${id}`)
  return data
}

export async function crearCalificacion(payload) {
  const { data } = await api.post('/api/calificaciones', payload)
  return data
}

export async function actualizarCalificacion(id, nota) {
  const { data } = await api.put(`/api/calificaciones/${id}`, { nota })
  return data
}
