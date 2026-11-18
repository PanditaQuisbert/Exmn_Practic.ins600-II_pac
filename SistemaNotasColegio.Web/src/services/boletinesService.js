import { api } from './api'

export async function obtenerBoletin(estudianteId) {
  const { data } = await api.get(`/api/boletines/estudiantes/${estudianteId}`)
  return data
}
