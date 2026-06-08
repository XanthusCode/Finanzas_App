import axios from 'axios'
import type { Gasto, Ingreso, Resumen, Categoria } from '@/types'

const TOKEN_KEY = 'finanzas_token'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem('finanzas_user')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

export const gastosService = {
  getAll: (mes: number, anio: number) =>
    api.get<Gasto[]>(`/gastos?mes=${mes}&anio=${anio}`),
  create: (gasto: Gasto) =>
    api.post<Gasto>('/gastos', gasto),
  update: (id: string, gasto: Gasto) =>
    api.put<Gasto>(`/gastos/${id}`, gasto),
  delete: (id: string) =>
    api.delete(`/gastos/${id}`)
}

export const ingresosService = {
  getAll: (mes: number, anio: number) =>
    api.get<Ingreso[]>(`/ingresos?mes=${mes}&anio=${anio}`),
  create: (ingreso: Ingreso) =>
    api.post<Ingreso>('/ingresos', ingreso),
  update: (id: string, ingreso: Ingreso) =>
    api.put<Ingreso>(`/ingresos/${id}`, ingreso),
  delete: (id: string) =>
    api.delete(`/ingresos/${id}`)
}

export const resumenService = {
  get: (mes: number, anio: number) =>
    api.get<Resumen>(`/resumen?mes=${mes}&anio=${anio}`),
  getAnual: (anio: number) =>
    api.get<Resumen>(`/resumen/anual?anio=${anio}`)
}

export const categoriasService = {
  getAll: () =>
    api.get<Categoria[]>('/categorias'),
  create: (cat: Omit<Categoria, 'id'>) =>
    api.post<Categoria>('/categorias', cat),
  update: (id: string, cat: Omit<Categoria, 'id'>) =>
    api.put<Categoria>(`/categorias/${id}`, cat),
  delete: (id: string) =>
    api.delete(`/categorias/${id}`)
}
