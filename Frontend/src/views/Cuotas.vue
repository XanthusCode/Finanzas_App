<template>
  <div class="fade-up cuotas">
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
        <span class="empty-icon"><CreditCard :size="24" :stroke-width="1.8" /></span>
        <p class="empty-title">Sin cuotas registradas</p>
        <p class="empty-sub">Cuando registres un gasto en cuotas aparecerá aquí con su seguimiento.</p>
      </div>
    </template>

    <template v-else>
      <!-- Resumen -->
      <div class="resumen-grid">
        <div class="res-card">
          <div class="res-top">
            <span class="res-label">Planes activos</span>
            <span class="res-icon"><CreditCard :size="15" :stroke-width="2" /></span>
          </div>
          <span class="res-val">{{ planesActivos.length }}</span>
          <span class="res-sub">{{ planesCompletados.length }} completado{{ planesCompletados.length !== 1 ? 's' : '' }}</span>
        </div>
        <div class="res-card">
          <div class="res-top">
            <span class="res-label">Comprometido / mes</span>
            <span class="res-icon"><CalendarClock :size="15" :stroke-width="2" /></span>
          </div>
          <span class="res-val">{{ fmt(comprometidoMes) }}</span>
          <span class="res-sub">en cuotas este mes</span>
        </div>
        <div class="res-card">
          <div class="res-top">
            <span class="res-label">Total pendiente</span>
            <span class="res-icon res-icon--warn"><Wallet :size="15" :stroke-width="2" /></span>
          </div>
          <span class="res-val">{{ fmt(totalPendiente) }}</span>
          <span class="res-sub">por pagar en total</span>
        </div>
      </div>

      <!-- Planes activos: una sola tarjeta, un plan por fila -->
      <section v-if="planesActivos.length > 0" class="planes-card">
        <header class="planes-head">
          <h2>En curso</h2>
          <span class="planes-count">{{ planesActivos.length }}</span>
        </header>

        <div v-for="plan in planesActivos" :key="plan.origenId" class="plan-row">
          <div class="plan-top">
            <div class="plan-main">
              <span class="plan-icon" :style="{ background: guessColor(plan.categoria) + '22', color: guessColor(plan.categoria) }">
                <component :is="guessIcono(plan.categoria)" :size="18" :stroke-width="1.9" />
              </span>
              <div class="plan-info">
                <span class="plan-detalle">{{ plan.detalle || plan.categoria }}</span>
                <span class="plan-cat">{{ plan.categoria }}</span>
              </div>
            </div>
            <div class="plan-right">
              <span class="plan-monto">{{ fmt(plan.montoCuota) }}<small>/mes</small></span>
              <span class="plan-cuotas">{{ plan.cuotasPasadas }}/{{ plan.numCuotas }} cuotas</span>
            </div>
          </div>

          <div class="plan-progress">
            <div class="plan-bar-wrap" :title="`${pctPlan(plan)}% pagado`">
              <div class="plan-bar" :style="{ width: Math.max(pctPlan(plan), 2) + '%' }" />
            </div>
            <span class="plan-pct">{{ pctPlan(plan) }}%</span>
          </div>

          <div class="plan-footer">
            <span class="plan-meta">
              <CircleCheck :size="13" :stroke-width="2" />
              Pagado: <strong>{{ fmt(plan.montoCuota * plan.cuotasPasadas) }}</strong>
              <span class="plan-meta-sep">·</span>
              Faltan <strong>{{ fmt(plan.montoCuota * (plan.numCuotas - plan.cuotasPasadas)) }}</strong>
            </span>
            <span class="plan-meta">
              <CalendarDays :size="13" :stroke-width="2" />
              Termina {{ labelMesFin(plan) }}
            </span>
          </div>
        </div>
      </section>

      <!-- Planes completados -->
      <section v-if="planesCompletados.length > 0" class="planes-card planes-card--done">
        <button class="planes-head planes-head--toggle" :aria-expanded="verCompletados" @click="verCompletados = !verCompletados">
          <h2>Completados</h2>
          <span class="planes-count">{{ planesCompletados.length }}</span>
          <ChevronDown :size="15" :stroke-width="2" class="toggle-chevron" :class="{ 'is-open': verCompletados }" />
        </button>

        <template v-if="verCompletados">
          <div v-for="plan in planesCompletados" :key="plan.origenId" class="plan-row plan-row--done">
            <div class="plan-top">
              <div class="plan-main">
                <span class="plan-icon plan-icon--done"><Check :size="18" :stroke-width="2.2" /></span>
                <div class="plan-info">
                  <span class="plan-detalle">{{ plan.detalle || plan.categoria }}</span>
                  <span class="plan-cat">{{ plan.categoria }} · {{ plan.numCuotas }} cuotas</span>
                </div>
              </div>
              <div class="plan-right">
                <span class="plan-monto">{{ fmt(plan.montoCuota * plan.numCuotas) }}</span>
                <span class="plan-cuotas">Finalizó {{ labelMesFin(plan) }}</span>
              </div>
            </div>
          </div>
        </template>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { CreditCard, CalendarClock, Wallet, CalendarDays, CircleCheck, Check, ChevronDown } from '@lucide/vue'
import { gastosService } from '@/services/api'
import { fmtCOP as fmt } from '@/utils'
import { guessIcono, guessColor } from '@/composables/useCategoriaOptions'
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

function pctPlan(plan: Plan) {
  return plan.numCuotas > 0 ? Math.round(plan.cuotasPasadas / plan.numCuotas * 100) : 0
}

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
.cuotas { font-variant-numeric: tabular-nums; }

/* ── Resumen ── */
.resumen-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.85rem; margin-bottom: 1.5rem; }
.res-card {
  display: flex; flex-direction: column; gap: 0.3rem;
  padding: 1.15rem 1.25rem 1.2rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
}
.res-top   { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.4rem; }
.res-label { font-size: 0.64rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.res-icon {
  --ic: var(--accent-strong);
  display: grid; place-items: center;
  width: 30px; height: 30px; border-radius: 9px;
  color: var(--ic); background: color-mix(in srgb, var(--ic) 13%, transparent);
}
.res-icon--warn { --ic: var(--c-want); }
.res-val {
  font-family: var(--ff-mono);
  font-size: 1.45rem; font-weight: 700; letter-spacing: -0.02em;
  color: var(--text-primary);
}
.res-sub { font-size: 0.68rem; color: var(--text-muted); }

/* ── Tarjeta contenedora de planes ── */
.planes-card {
  margin-bottom: 1rem;
  padding: 0.35rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
}
.planes-head {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.85rem 0.9rem 0.6rem;
}
.planes-head h2 {
  font-family: var(--ff-body);
  font-size: 0.76rem; font-weight: 500; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--text-primary);
}
.planes-count {
  font-size: 0.64rem; font-weight: 500; color: var(--text-secondary);
  padding: 0.1rem 0.5rem; border-radius: 999px; background: var(--surface2);
}
.planes-head--toggle {
  width: 100%; background: none; border: none; border-radius: 12px;
  font: inherit; cursor: pointer; text-align: left;
  padding-bottom: 0.85rem;
}
.planes-head--toggle:hover { background: var(--surface2); }
.toggle-chevron { margin-left: auto; color: var(--text-muted); transition: transform 0.2s ease; }
.toggle-chevron.is-open { transform: rotate(180deg); }

/* ── Fila de plan ── */
.plan-row {
  position: relative;
  display: flex; flex-direction: column; gap: 0.85rem;
  padding: 1rem 0.9rem;
  border-radius: 12px;
  transition: background 0.12s ease;
}
.plan-row::before {
  content: '';
  position: absolute; top: 0; left: 0.9rem; right: 0.9rem;
  height: 1px; background: var(--border);
}
.plan-row:hover { background: var(--surface2); }
.plan-row:hover::before,
.plan-row:hover + .plan-row::before { background: transparent; }

.plan-top  { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
.plan-main { display: flex; align-items: center; gap: 0.85rem; min-width: 0; }
.plan-icon {
  display: grid; place-items: center; flex-shrink: 0;
  width: 40px; height: 40px; border-radius: 10px;
}
.plan-icon--done { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 13%, transparent); }
.plan-info { display: flex; flex-direction: column; gap: 0.2rem; min-width: 0; }
.plan-detalle {
  font-size: 0.9rem; font-weight: 700; color: var(--text-primary);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.plan-cat { font-size: 0.72rem; color: var(--text-subtle); }

.plan-right  { display: flex; flex-direction: column; align-items: flex-end; gap: 0.2rem; flex-shrink: 0; }
.plan-monto  {
  font-family: var(--ff-mono);
  font-size: 1rem; font-weight: 700; color: var(--text-primary); white-space: nowrap;
}
.plan-monto small { font-size: 0.7rem; font-weight: 400; color: var(--text-subtle); margin-left: 1px; }
.plan-cuotas { font-size: 0.7rem; color: var(--text-subtle); }

.plan-progress { display: flex; align-items: center; gap: 0.85rem; padding-left: calc(40px + 0.85rem); }
.plan-bar-wrap {
  flex: 1; height: 10px; border-radius: 99px; overflow: hidden;
  background: color-mix(in srgb, var(--text-muted) 22%, var(--surface2));
}
.plan-bar {
  height: 100%; border-radius: 99px;
  background: linear-gradient(90deg, color-mix(in srgb, var(--c-need) 70%, transparent), var(--c-need));
  transition: width 0.6s cubic-bezier(0.2, 0, 0, 1);
}
.plan-pct { font-family: var(--ff-mono); font-size: 0.82rem; font-weight: 700; color: var(--text-primary); min-width: 40px; text-align: right; }

.plan-footer {
  display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap;
  padding-left: calc(40px + 0.85rem);
}
.plan-meta { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.72rem; color: var(--text-subtle); }
.plan-meta svg { color: var(--text-muted); }
.plan-meta strong { font-family: var(--ff-mono); font-weight: 700; color: var(--text-primary); }
.plan-meta-sep { color: var(--text-muted); }

.plan-row--done { opacity: 0.75; }
.plan-row--done:hover { opacity: 1; }

/* ── Vacío ── */
.empty-state { display: flex; flex-direction: column; align-items: center; padding: 4rem 0; gap: 0.4rem; text-align: center; }
.empty-icon {
  display: grid; place-items: center;
  width: 52px; height: 52px; margin-bottom: 0.4rem; border-radius: 999px;
  color: var(--text-secondary); background: var(--surface2);
}
.empty-title { font-size: 0.88rem; font-weight: 500; color: var(--text-primary); }
.empty-sub   { font-size: 0.72rem; color: var(--text-muted); max-width: 300px; }

/* ── Loading ── */
.loading-list { display: flex; flex-direction: column; gap: 0.6rem; }
.skeleton-row { height: 100px; border-radius: var(--radius-card); background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 768px) {
  .resumen-grid { grid-template-columns: 1fr 1fr; }
  .resumen-grid > :first-child { grid-column: 1 / -1; }
}

@media (max-width: 500px) {
  .resumen-grid { grid-template-columns: 1fr; }
  .plan-top { align-items: flex-start; }
  .plan-progress, .plan-footer { padding-left: 0; }
  .plan-footer { flex-direction: column; align-items: flex-start; gap: 0.4rem; }
}
</style>
