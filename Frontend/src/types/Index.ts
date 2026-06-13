export interface Gasto {
  id?: string
  categoria: string
  detalle: string
  monto: number
  tipo: 'FIJO' | 'VARIABLE'
  mes: number
  anio: number
  esRecurrente: boolean
}

export interface Ingreso {
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

export interface Meta {
  id?: string
  nombre: string
  montoObjetivo: number
  montoActual: number
  fechaLimite?: string
  completada: boolean
}
