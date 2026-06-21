export interface Gasto {
  id?: string
  categoria: string
  detalle: string
  monto: number
  tipo: 'FIJO' | 'VARIABLE'
  mes: number
  anio: number
  esRecurrente: boolean
  numCuotas?: number
  cuotaActual?: number
  gastoOrigenId?: string
}

export interface Ingreso {
  esRecurrente?: boolean
  id?: string
  concepto: string
  monto: number
  mes: number
  anio: number
}

export interface Resumen {
  mes: number
  anio: number
  totalIngresos: number
  totalFijos: number
  totalVariables: number
  totalGastos: number
  ahorro: number
  pctAhorro: number
}

export interface Categoria {
  id?: string
  nombre: string
  tipo: 'FIJO' | 'VARIABLE'
  activa: boolean
}

export interface Presupuesto {
  id?: string
  categoria: string
  limite: number
}

export interface GastoCategoriaAnual {
  categoria: string
  datos: number[]
}

export interface Meta {
  id?: string
  nombre: string
  montoObjetivo: number
  montoActual: number
  fechaLimite?: string
  completada: boolean
  creadoEn?: string
}

export interface ResumenCategoria {
  categoria: string
  total: number
}

export interface Deuda {
  id?: string
  persona: string
  descripcion: string
  monto: number
  tipo: 'ME_DEBEN' | 'LE_DEBO'
  fechaLimite?: string
  pagada: boolean
  creadoEn?: string
}
