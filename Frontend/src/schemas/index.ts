import { z } from 'zod'

export interface PasswordRequirement {
  id: string
  label: string
  test: (value: string) => boolean
}

export const passwordRequirements: PasswordRequirement[] = [
  { id: 'length', label: 'Mínimo 8 caracteres', test: v => v.length >= 8 },
  { id: 'upper', label: 'Una letra mayúscula', test: v => /[A-Z]/.test(v) },
  { id: 'lower', label: 'Una letra minúscula', test: v => /[a-z]/.test(v) },
  { id: 'number', label: 'Un número', test: v => /[0-9]/.test(v) },
]

export const passwordSchema = z.string().refine(
  v => passwordRequirements.every(r => r.test(v)),
  { message: 'La contraseña no cumple los requisitos mínimos' }
)

export const gastoSchema = z.object({
  tipo: z.enum(['FIJO', 'VARIABLE']),
  categoria: z.string().min(1, { message: 'Selecciona una categoría' }),
  detalle: z.string().max(100, { message: 'Máximo 100 caracteres' }),
  monto: z.number({ error: 'Ingresa un monto' }).positive({ message: 'El monto debe ser mayor a $0' }),
})

export const ingresoSchema = z.object({
  concepto: z.string().min(1, { message: 'El concepto es requerido' }).max(100, { message: 'Máximo 100 caracteres' }),
  monto: z.number({ error: 'Ingresa un monto' }).positive({ message: 'El monto debe ser mayor a $0' }),
})

export type GastoFormData = z.infer<typeof gastoSchema>
export type IngresoFormData = z.infer<typeof ingresoSchema>
