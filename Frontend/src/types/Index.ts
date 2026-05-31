export interface Gasto {
  id?: number
  categoria: string
  detalle: string
  monto: number
  tipo: 'FIJO' | 'VARIABLE'
  mes: number
  anio: number
}

export interface Ingreso {
  id?: number
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

export interface categoria {
  id?: number
  categoryName: string
  CategoryType: 'FIJO' | 'VARIABLE'
  createdAt: Date
  updateAt: Date
}

export const MESES = [
  'Enero','Febrero','Marzo','Abril','Mayo','Junio',
  'Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'
]

export const CATEGORIAS_FIJAS = [
  'Hogar','Gimnasio','Universidad','Moto','Higiene',
  'Celular','Netflix','Agentes IA','Otros'
]

export const CATEGORIAS_VARIABLES = [
  'Juegos','Regalos','Accesorios','Comida','Salidas','Otros'
]