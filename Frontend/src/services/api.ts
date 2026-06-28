import axios from 'axios'
import type { Gasto, Ingreso, Resumen, GastoCategoriaAnual, Categoria, Presupuesto, Meta, ResumenCategoria, Deuda } from '@/types'

const TOKEN_KEY = 'finanzas_token'

const baseURL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}`
  : '/api'

export const api = axios.create({
  baseURL,
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
    api.delete(`/gastos/${id}`),
  copiarRecurrentes: (mes: number, anio: number) =>
    api.post<Gasto[]>(`/gastos/copiar-recurrentes?mes=${mes}&anio=${anio}`),
  importar: (mes: number, anio: number, gastos: Omit<Gasto, 'id' | 'mes' | 'anio'>[]) =>
    api.post<{ importados: number }>(`/gastos/importar?mes=${mes}&anio=${anio}`, gastos),
  getResumenCategoria: (mes: number, anio: number) =>
    api.get<ResumenCategoria[]>(`/gastos/resumen-categoria?mes=${mes}&anio=${anio}`),
  getCuotas: () =>
    api.get<Gasto[]>('/gastos/cuotas')
}

export const ingresosService = {
  getAll: (mes: number, anio: number) =>
    api.get<Ingreso[]>(`/ingresos?mes=${mes}&anio=${anio}`),
  create: (ingreso: Ingreso) =>
    api.post<Ingreso>('/ingresos', ingreso),
  update: (id: string, ingreso: Ingreso) =>
    api.put<Ingreso>(`/ingresos/${id}`, ingreso),
  delete: (id: string) =>
    api.delete(`/ingresos/${id}`),
  copiarRecurrentes: (mes: number, anio: number) =>
    api.post<Ingreso[]>(`/ingresos/copiar-recurrentes?mes=${mes}&anio=${anio}`)
}

export const resumenService = {
  get: (mes: number, anio: number) =>
    api.get<Resumen>(`/resumen?mes=${mes}&anio=${anio}`),
  getAnual: (anio: number) =>
    api.get<Resumen>(`/resumen/anual?anio=${anio}`),
  getTendencia: (anio: number) =>
    api.get<Resumen[]>(`/resumen/tendencia?anio=${anio}`),
  getGastosPorCategoria: (anio: number) =>
    api.get<GastoCategoriaAnual[]>(`/resumen/gastos-por-categoria?anio=${anio}`),
  getReporte: (mes: number, anio: number) =>
    api.get(`/resumen/reporte?mes=${mes}&anio=${anio}`, { responseType: 'blob' })
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

export const presupuestosService = {
  getAll: () =>
    api.get<Presupuesto[]>('/presupuestos'),
  upsert: (presupuesto: Omit<Presupuesto, 'id'>) =>
    api.put<Presupuesto>('/presupuestos', presupuesto),
  delete: (id: string) =>
    api.delete(`/presupuestos/${id}`)
}

export const metasService = {
  getAll: () =>
    api.get<Meta[]>('/metas'),
  create: (meta: Pick<Meta, 'nombre' | 'montoObjetivo' | 'fechaLimite'>) =>
    api.post<Meta>('/metas', meta),
  update: (id: string, meta: Pick<Meta, 'nombre' | 'montoObjetivo' | 'fechaLimite'>) =>
    api.put<Meta>(`/metas/${id}`, meta),
  abonar: (id: string, monto: number) =>
    api.post<Meta>(`/metas/${id}/abonar`, { monto }),
  delete: (id: string) =>
    api.delete(`/metas/${id}`)
}

export const deudasService = {
  getAll: () =>
    api.get<Deuda[]>('/deudas'),
  create: (deuda: Omit<Deuda, 'id' | 'pagada' | 'creadoEn'>) =>
    api.post<Deuda>('/deudas', deuda),
  togglePagada: (id: string) =>
    api.patch<Deuda>(`/deudas/${id}/pagar`),
  delete: (id: string) =>
    api.delete(`/deudas/${id}`)
}

export const perfilService = {
  updateNombre: (nombre: string) =>
    api.put<{ token: string; email: string; nombre: string; expiresIn: number }>('/auth/perfil', { nombre }),
  changePassword: (currentPassword: string, newPassword: string) =>
    api.put('/auth/password', { currentPassword, newPassword })
}
