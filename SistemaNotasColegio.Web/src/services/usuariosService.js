import { api } from './api'

export async function listarUsuarios({ rolId = null, incluirInactivos = false } = {}) {
  const params = { incluirInactivos }
  if (rolId !== null && rolId !== undefined && rolId !== '') {
    params.rolId = Number(rolId)
  }

  const { data } = await api.get('/api/usuarios', { params })
  return data
}

export async function obtenerUsuario(id) {
  const { data } = await api.get(`/api/usuarios/${id}`)
  return data
}

export async function crearUsuario(payload) {
  const { data } = await api.post('/api/usuarios', payload)
  return data
}

export async function actualizarUsuario(id, payload) {
  const { data } = await api.put(`/api/usuarios/${id}`, payload)
  return data
}

export async function cambiarEstadoUsuario(id, activo) {
  const { data } = await api.patch(`/api/usuarios/${id}/estado`, { activo })
  return data
}
