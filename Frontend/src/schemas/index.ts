import { z } from 'zod'

export const gastoSchema = z.object({
  tipo: z.enum(['FIJO', 'VARIABLE']),
  categoria: z.string().min(1, { message: 'Selecciona una categoría' }),
  detalle: z.string().min(1, { message: 'El detalle es requerido' }).max(100, { message: 'Máximo 100 caracteres' }),
  monto: z.number({ error: 'Ingresa un monto' }).positive({ message: 'El monto debe ser mayor a $0' }),
})

export const ingresoSchema = z.object({
  concepto: z.string().min(1, { message: 'El concepto es requerido' }).max(100, { message: 'Máximo 100 caracteres' }),
  monto: z.number({ error: 'Ingresa un monto' }).positive({ message: 'El monto debe ser mayor a $0' }),
})

export type GastoFormData = z.infer<typeof gastoSchema>
export type IngresoFormData = z.infer<typeof ingresoSchema>
