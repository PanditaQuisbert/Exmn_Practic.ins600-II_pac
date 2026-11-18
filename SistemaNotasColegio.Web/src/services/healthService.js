import { api } from './api'

export async function verificarSaludApi() {
  const { data } = await api.get('/api/health', { timeout: 8000 })
  return data
}
