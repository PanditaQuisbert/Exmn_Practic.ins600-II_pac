import { api } from './api'

export async function iniciarSesionApi(credenciales) {
  const { data } = await api.post('/api/auth/login', credenciales)
  return data
}
