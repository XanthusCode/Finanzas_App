import type { Meta } from '@/types'

export interface MetaProjection {
  fechaEstimada: Date | null
  urgencia: 'ok' | 'warn' | 'danger' | null
  diasRestantes: number | null
}

export function calcularProyeccionMeta(meta: Meta): MetaProjection {
  if (meta.completada) {
    return { fechaEstimada: null, urgencia: null, diasRestantes: null }
  }

  const hoy = new Date()

  let fechaEstimada: Date | null = null
  if (meta.creadoEn && meta.montoActual > 0) {
    const diasDesdeCreacion = Math.max(
      1,
      Math.floor((hoy.getTime() - new Date(meta.creadoEn).getTime()) / 86_400_000)
    )
    const tasaDiaria = meta.montoActual / diasDesdeCreacion
    if (tasaDiaria > 0 && meta.montoActual < meta.montoObjetivo) {
      const diasNecesarios = (meta.montoObjetivo - meta.montoActual) / tasaDiaria
      fechaEstimada = new Date(hoy.getTime() + diasNecesarios * 86_400_000)
    }
  }

  let urgencia: MetaProjection['urgencia'] = null
  let diasRestantes: number | null = null

  if (meta.fechaLimite) {
    diasRestantes = Math.floor(
      (new Date(meta.fechaLimite).getTime() - hoy.getTime()) / 86_400_000
    )
    const pct = meta.montoObjetivo > 0
      ? (meta.montoActual / meta.montoObjetivo) * 100
      : 0

    if (diasRestantes < 0) {
      urgencia = 'danger'
    } else if (diasRestantes < 30 && pct < 50) {
      urgencia = 'danger'
    } else if (diasRestantes < 60 && pct < 75) {
      urgencia = 'warn'
    } else {
      urgencia = 'ok'
    }
  }

  return { fechaEstimada, urgencia, diasRestantes }
}
