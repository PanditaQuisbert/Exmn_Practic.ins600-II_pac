import { api } from './api'

export async function listarMisAsignaciones() {
  const { data } = await api.get('/api/profesores/mis-asignaciones')
  return data
}
