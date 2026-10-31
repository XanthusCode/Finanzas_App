export interface Gasto {
  id?: string
  categoria: string
  detalle: string
  monto: number
  tipo: 'FIJO' | 'VARIABLE'
  mes: number
  anio: number
  fecha?: string
  esRecurrente: boolean
  numCuotas?: number
  cuotaActual?: number
  gastoOrigenId?: string
  cuentaId?: string
  actualizadoEn?: string
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
  actualizadoEn?: string
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
  activa: boolean
}

export interface ConteoCategorias {
  total: number
}

export interface CategoriaGastos {
  categoriaId: string
  categoria: string
  moneda: string
  total: number
  cantidad: number
  gastos: Gasto[]
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
  cuentaId?: string
  creadoEn?: string
  actualizadoEn?: string
}

export type TipoRetiroMeta = 'SIMPLE' | 'GASTO' | 'TRANSFERENCIA'

export interface MovimientoMetaPayload {
  monto: number
  cuentaOrigenId?: string
  tipoRetiro?: TipoRetiroMeta
  categoria?: string
  detalle?: string
  cuentaDestinoId?: string
}

export interface ResumenCategoria {
  categoria: string
  total: number
}

export interface Suscripcion {
  categoria: string
  detalle: string
  monto: number
  mesesActivo: number
}

export interface HeatmapDia {
  fecha: string
  total: number
}

export interface Insights {
  estado: 'ok' | 'sin_datos' | 'error'
  texto?: string
}

export interface Deuda {
  id?: string
  persona: string
  descripcion: string
  monto: number
  tipo: 'ME_DEBEN' | 'LE_DEBO'
  fechaLimite?: string
  cuentaId?: string
  pagada: boolean
  creadoEn?: string
  actualizadoEn?: string
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
  actualizadoEn?: string
}

export interface MovimientoCuenta {
  id?: string
  monto: number
  descripcion?: string
  fecha: string
}

// Línea del historial unificado de una cuenta (movimiento manual + ingreso + gasto)
export interface LineaCuenta {
  tipo: 'MOVIMIENTO' | 'INGRESO' | 'GASTO'
  monto: number // firmado
  descripcion?: string
  fecha: string
}

export interface MovimientoMeta {
  id?: string
  monto: number // firmado: positivo = abono, negativo = retiro
  descripcion?: string
  fecha: string
}

export interface ReciboExtraido {
  monto: number
  comercio: string | null
  descripcion: string | null
  fecha: string | null
  categoriaSugerida: string | null
}

// Resultado de la búsqueda global (Ctrl/Cmd + K) — GET /buscar?q=
export interface ResultadoBusqueda {
  tipo: 'GASTO' | 'INGRESO'
  id: string
  titulo: string          // gasto: categoría · ingreso: concepto
  detalle?: string | null // solo gastos
  monto: number
  mes: number
  anio: number
  fecha?: string | null
  tipoGasto?: 'FIJO' | 'VARIABLE' | null
}

// Chat IA
export interface ChatMensaje {
  rol: 'user' | 'assistant'
  contenido: string
}

export interface ChatRespuesta {
  estado: 'ok' | 'error'
  respuesta?: string
}
