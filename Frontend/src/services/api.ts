import axios from 'axios'
import type { Gasto, Ingreso, Resumen, Categoria } from '@/types'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

export const gastosService = {
  getAll: (mes: number, anio: number) =>
    api.get<Gasto[]>(`/gastos?mes=${mes}&anio=${anio}`),
  create: (gasto: Gasto) =>
    api.post<Gasto>('/gastos', gasto),
  update: (id: number, gasto: Gasto) =>
    api.put<Gasto>(`/gastos/${id}`, gasto),
  delete: (id: number) =>
    api.delete(`/gastos/${id}`)
}

export const ingresosService = {
  getAll: (mes: number, anio: number) =>
    api.get<Ingreso[]>(`/ingresos?mes=${mes}&anio=${anio}`),
  create: (ingreso: Ingreso) =>
    api.post<Ingreso>('/ingresos', ingreso),
  delete: (id: number) =>
    api.delete(`/ingresos/${id}`)
}

export const resumenService = {
  get: (mes: number, anio: number) =>
    api.get<Resumen>(`/resumen?mes=${mes}&anio=${anio}`)
}

export const categoriasService = {
  getAll: () =>
    api.get<Categoria[]>('/categorias'),
  create: (cat: Omit<Categoria, 'id'>) =>
    api.post<Categoria>('/categorias', cat),
  update: (id: number, cat: Omit<Categoria, 'id'>) =>
    api.put<Categoria>(`/categorias/${id}`, cat),
  delete: (id: number) =>
    api.delete(`/categorias/${id}`)
}