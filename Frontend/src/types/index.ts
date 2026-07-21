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
  cuentaId?: string
}

export type FrecuenciaIngreso = 'DIARIO' | 'SEMANAL' | 'QUINCENAL' | 'MENSUAL' | 'BIMESTRAL' | 'TRIMESTRAL' | 'ANUAL' | 'PERSONALIZADO'

export interface Ingreso {
  esRecurrente?: boolean
  id?: string
  concepto: string
  monto: number
  mes: number
  anio: number
  cuentaId?: string
  frecuencia?: FrecuenciaIngreso
  diaPago?: number
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
  montoPagado: number
  montoRestante: number
}

export interface Abono {
  id?: string
  monto: number
  fecha: string
}

export type TipoCuenta = 'EFECTIVO' | 'BANCO' | 'BILLETERA_DIGITAL' | 'TARJETA_CREDITO' | 'ALCANCIA' | 'CRIPTO' | 'OTRO'

export interface Cuenta {
  id?: string
  nombre: string
  tipo: TipoCuenta
  activa: boolean
  saldo: number
  creadoEn?: string
}

export interface MovimientoCuenta {
  id?: string
  monto: number
  descripcion?: string
  fecha: string
}

export interface MovimientoMeta {
  id?: string
  monto: number // firmado: positivo = abono, negativo = retiro
  fecha: string
}
