<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Gastos</div>
        <h1 class="page-title">Cuotas activas</h1>
      </div>
    </div>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 4" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="planes.length === 0">
      <div class="empty-state">
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.3" style="color: var(--text-muted)"><rect x="2" y="5" width="20" height="14" rx="2"/><path d="M2 10h20"/></svg>
        <p class="empty-title">Sin cuotas registradas</p>
        <p class="empty-sub">Cuando registres un gasto en cuotas aparecerá aquí con su seguimiento.</p>
      </div>
    </template>

    <template v-else>
      <!-- Resumen -->
      <div class="resumen-grid">
        <div class="res-card">
          <span class="res-label">Planes activos</span>
          <span class="res-val">{{ planesActivos.length }}</span>
        </div>
        <div class="res-card">
          <span class="res-label">Comprometido/mes</span>
          <span class="res-val accent">{{ fmt(comprometidoMes) }}</span>
        </div>
        <div class="res-card">
          <span class="res-label">Total pendiente</span>
          <span class="res-val warn">{{ fmt(totalPendiente) }}</span>
        </div>
      </div>

      <!-- Planes activos -->
      <div v-if="planesActivos.length > 0" class="section-titulo">En curso</div>
      <div class="planes-list">
        <div v-for="plan in planesActivos" :key="plan.origenId" class="plan-card">
          <div class="plan-top">
            <div class="plan-info">
              <span class="plan-detalle">{{ plan.detalle }}</span>
              <span class="plan-cat">{{ plan.categoria }}</span>
            </div>
            <div class="plan-right">
              <span class="plan-monto">{{ fmt(plan.montoCuota) }}/mes</span>
              <span class="plan-cuotas">{{ plan.cuotasPasadas }}/{{ plan.numCuotas }} cuotas</span>
            </div>
          </div>

          <div class="plan-bar-wrap">
            <div class="plan-bar" :style="{ width: (plan.cuotasPasadas / plan.numCuotas * 100) + '%' }" />
          </div>

          <div class="plan-footer">
            <span class="plan-pagado">Pagado: {{ fmt(plan.montoCuota * plan.cuotasPasadas) }}</span>
            <span class="plan-fin">
              <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
              Termina {{ labelMesFin(plan) }}
            </span>
          </div>
        </div>
      </div>

      <!-- Planes completados -->
      <template v-if="planesCompletados.length > 0">
        <div class="section-titulo completados-titulo" @click="verCompletados = !verCompletados" style="cursor:pointer">
          Completados ({{ planesCompletados.length }})
          <svg :style="{ transform: verCompletados ? 'rotate(180deg)' : '', transition: 'transform .2s' }" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="6 9 12 15 18 9"/></svg>
        </div>
        <div v-if="verCompletados" class="planes-list completados">
          <div v-for="plan in planesCompletados" :key="plan.origenId" class="plan-card plan-card-done">
            <div class="plan-top">
              <div class="plan-info">
                <span class="plan-detalle">{{ plan.detalle }}</span>
                <span class="plan-cat">{{ plan.categoria }}</span>
              </div>
              <div class="plan-right">
                <span class="badge-done">✓ Completado</span>
                <span class="plan-cuotas">{{ plan.numCuotas }} cuotas</span>
              </div>
            </div>
            <div class="plan-bar-wrap"><div class="plan-bar plan-bar-done" style="width:100%" /></div>
            <div class="plan-footer">
              <span class="plan-pagado">Total pagado: {{ fmt(plan.montoCuota * plan.numCuotas) }}</span>
              <span class="plan-fin">Finalizó {{ labelMesFin(plan) }}</span>
            </div>
          </div>
        </div>
      </template>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { gastosService } from '@/services/api'
import { fmtCOP as fmt } from '@/utils'
import type { Gasto } from '@/types'

const MESES = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']

const loading       = ref(false)
const verCompletados = ref(false)
const gastos        = ref<Gasto[]>([])

const hoy     = new Date()
const mesHoy  = hoy.getMonth() + 1
const anioHoy = hoy.getFullYear()

interface Plan {
  origenId: string
  detalle: string
  categoria: string
  montoCuota: number
  numCuotas: number
  cuotasPasadas: number
  mesFin: number
  anioFin: number
  activo: boolean
}

const planes = computed<Plan[]>(() => {
  const map = new Map<string, Gasto[]>()
  for (const g of gastos.value) {
    const key = g.gastoOrigenId ?? g.id ?? ''
    if (!map.has(key)) map.set(key, [])
    map.get(key)!.push(g)
  }

  const result: Plan[] = []
  for (const [origenId, cuotas] of map) {
    cuotas.sort((a, b) => a.cuotaActual! - b.cuotaActual!)
    const primera = cuotas[0]
    const ultima  = cuotas[cuotas.length - 1]
    if (!primera || !ultima) continue
    const numCuotas = primera.numCuotas!

    const cuotasPasadas = cuotas.filter(c =>
      c.anio < anioHoy || (c.anio === anioHoy && c.mes <= mesHoy)
    ).length

    const activo = ultima.anio > anioHoy || (ultima.anio === anioHoy && ultima.mes >= mesHoy)

    result.push({
      origenId,
      detalle:  primera.detalle,
      categoria: primera.categoria,
      montoCuota: primera.monto,
      numCuotas,
      cuotasPasadas,
      mesFin:  ultima.mes,
      anioFin: ultima.anio,
      activo,
    })
  }
  return result.sort((a, b) => {
    if (a.activo !== b.activo) return a.activo ? -1 : 1
    return a.anioFin !== b.anioFin ? a.anioFin - b.anioFin : a.mesFin - b.mesFin
  })
})

const planesActivos    = computed(() => planes.value.filter(p => p.activo))
const planesCompletados = computed(() => planes.value.filter(p => !p.activo))

const comprometidoMes = computed(() =>
  planesActivos.value.reduce((s, p) => s + p.montoCuota, 0)
)

const totalPendiente = computed(() =>
  planesActivos.value.reduce((s, p) =>
    s + p.montoCuota * (p.numCuotas - p.cuotasPasadas), 0)
)

function labelMesFin(plan: Plan): string {
  return `${MESES[plan.mesFin - 1]} ${plan.anioFin}`
}

onMounted(async () => {
  loading.value = true
  try {
    const { data } = await gastosService.getCuotas()
    gastos.value = data
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }

/* Resumen */
.resumen-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem; margin-bottom: 1.25rem; }
.res-card { background: var(--surface); border: 1px solid var(--border); border-radius: 8px; padding: 0.85rem 1rem; display: flex; flex-direction: column; gap: 0.25rem; }
.res-label { font-size: 0.6rem; color: var(--text-muted); letter-spacing: 0.1em; text-transform: uppercase; }
.res-val   { font-size: 1.1rem; font-weight: 700; }
.res-val.accent { color: var(--accent); }
.res-val.warn   { color: #f59e0b; }

.section-titulo {
  font-size: 0.65rem; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--text-muted); margin-bottom: 0.5rem; display: flex; align-items: center; gap: 0.4rem;
}
.completados-titulo { margin-top: 1rem; }

/* Planes */
.planes-list { display: flex; flex-direction: column; gap: 0.6rem; margin-bottom: 0.5rem; }

.plan-card {
  background: var(--surface); border: 1px solid var(--border);
  border-radius: 10px; padding: 0.9rem 1.1rem;
  display: flex; flex-direction: column; gap: 0.5rem;
}
.plan-card-done { opacity: 0.65; }
.completados .plan-card { border-style: dashed; }

.plan-top  { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; }
.plan-info { display: flex; flex-direction: column; gap: 0.1rem; }
.plan-detalle { font-size: 0.82rem; font-weight: 600; color: var(--text-primary); }
.plan-cat     { font-size: 0.62rem; color: var(--text-muted); }

.plan-right  { display: flex; flex-direction: column; align-items: flex-end; gap: 0.15rem; }
.plan-monto  { font-size: 0.82rem; font-weight: 700; color: var(--text-primary); white-space: nowrap; }
.plan-cuotas { font-size: 0.62rem; color: var(--text-muted); }
.badge-done  { font-size: 0.62rem; background: rgba(99,179,255,0.12); color: var(--accent); padding: 0.15rem 0.5rem; border-radius: 4px; }

.plan-bar-wrap { height: 5px; background: var(--border); border-radius: 3px; overflow: hidden; }
.plan-bar      { height: 100%; background: var(--accent); border-radius: 3px; transition: width 0.5s ease; }
.plan-bar-done { background: #34d399; }

.plan-footer { display: flex; justify-content: space-between; align-items: center; }
.plan-pagado { font-size: 0.62rem; color: var(--text-muted); }
.plan-fin    { display: flex; align-items: center; gap: 0.3rem; font-size: 0.62rem; color: var(--text-muted); }

/* Empty */
.empty-state { display: flex; flex-direction: column; align-items: center; padding: 4rem 0; gap: 0.4rem; text-align: center; }
.empty-title { font-size: 0.85rem; font-weight: 600; color: var(--text-secondary); margin-top: 0.5rem; }
.empty-sub   { font-size: 0.72rem; color: var(--text-muted); max-width: 300px; }

/* Loading */
.loading-list { display: flex; flex-direction: column; gap: 0.6rem; }
.skeleton-row { height: 100px; border-radius: 10px; background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
@keyframes shimmer { to { background-position: -200% 0; } }
</style>
