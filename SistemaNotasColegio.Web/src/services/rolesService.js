import { api } from './api'

export async function listarRoles() {
  const { data } = await api.get('/api/roles')
  return data
}
