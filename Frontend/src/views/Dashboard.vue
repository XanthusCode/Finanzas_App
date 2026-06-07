<template>
  <div class="fade-up">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="section-label">Resumen</div>
        <h1 class="page-title">Dashboard</h1>
      </div>
      <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
    </div>

    <div v-if="store.loading" class="loading">
      <div class="loading-grid">
        <div v-for="i in 4" :key="i" class="skeleton-kpi" />
      </div>
    </div>

    <template v-else-if="store.resumen">
      <!-- KPIs -->
      <div ref="kpiRef" class="kpi-grid" style="opacity: 0">
        <div class="kpi-card" style="--ca: var(--accent)">
          <div class="kpi-top">
            <span class="kpi-label">Ingresos</span>
            <svg class="kpi-icon" style="color:var(--accent)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/></svg>
          </div>
          <div class="kpi-value" style="color: var(--accent)">{{ fmtAnim(counters.ingresos) }}</div>
        </div>
        <div class="kpi-card" style="--ca: var(--red)">
          <div class="kpi-top">
            <span class="kpi-label">Gastos fijos</span>
            <svg class="kpi-icon" style="color:var(--red)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
          </div>
          <div class="kpi-value" style="color: var(--red)">{{ fmtAnim(counters.fijos) }}</div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: pcts.fijos + '%', background: 'var(--red)' }" />
          </div>
          <div class="kpi-pct">{{ Math.round(pcts.fijos) }}% del ingreso</div>
        </div>
        <div class="kpi-card" style="--ca: var(--amber)">
          <div class="kpi-top">
            <span class="kpi-label">Gastos variables</span>
            <svg class="kpi-icon" style="color:var(--amber)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3h18v18H3z"/><path d="M3 9h18M9 21V9"/></svg>
          </div>
          <div class="kpi-value" style="color: var(--amber)">{{ fmtAnim(counters.variables) }}</div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: pcts.variables + '%', background: 'var(--amber)' }" />
          </div>
          <div class="kpi-pct">{{ Math.round(pcts.variables) }}% del ingreso</div>
        </div>
        <div class="kpi-card kpi-ahorro" style="--ca: var(--green)">
          <div class="kpi-top">
            <span class="kpi-label">Ahorro del mes</span>
            <svg class="kpi-icon" style="color:var(--green)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2a10 10 0 1 0 10 10"/><path d="M12 6v6l4 2"/></svg>
          </div>
          <div class="kpi-value" style="color: var(--green)">{{ fmtAnim(counters.ahorro) }}</div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: pcts.ahorro + '%', background: 'var(--green)' }" />
          </div>
          <div class="kpi-pct">{{ Math.round(pcts.ahorro) }}% del ingreso</div>
        </div>
      </div>

      <!-- Ahorro anual banner -->
      <div ref="bannerRef" class="anual-banner" style="opacity: 0">
        <div class="anual-left">
          <div class="kpi-label" style="margin-bottom: 0.5rem">Ahorro acumulado {{ store.anioActual }}</div>
          <div class="anual-big">{{ fmtAnim(counters.ahorroAnual) }}</div>
          <div class="anual-meta">de {{ fmt(store.resumenAnual?.totalIngresos) }} en ingresos este año</div>
        </div>
        <div class="anual-right">
          <div class="anual-stats">
            <div class="anual-stat">
              <span class="anual-stat-label">Ingresos</span>
              <strong>{{ fmt(store.resumenAnual?.totalIngresos) }}</strong>
            </div>
            <div class="anual-stat">
              <span class="anual-stat-label">Gastos</span>
              <strong>{{ fmt(store.resumenAnual?.totalGastos) }}</strong>
            </div>
            <div class="anual-stat">
              <span class="anual-stat-label">Tasa</span>
              <strong style="color: var(--green)">{{ store.resumenAnual?.pctAhorro ?? 0 }}%</strong>
            </div>
          </div>
          <div class="anual-progress-wrap">
            <div class="anual-progress-track">
              <div class="anual-progress-fill" :style="{ width: progressWidth + '%' }" />
              <div class="anual-progress-goal" />
            </div>
            <div class="anual-progress-labels">
              <span>0%</span>
              <span>Meta 20%</span>
              <span>100%</span>
            </div>
          </div>
          <span :class="['health-badge', healthClass]">{{ healthLabel }}</span>
        </div>
      </div>

      <!-- 3 columnas: donut | fijos | variables -->
      <div ref="bottomRef" class="bottom-grid">
        <div class="card" style="opacity: 0">
          <p class="card-title">Distribución del mes</p>
          <div class="donut-section">
            <DonutChart
              :data="distribucion.map(d => d.val)"
              :labels="distribucion.map(d => d.label)"
              :colors="distribucion.map(d => d.color)"
              :center-label="(resumen?.pctAhorro ?? 0) + '%'"
            />
            <div class="donut-legend">
              <div v-for="item in distribucion" :key="item.label" class="legend-row">
                <div class="legend-left">
                  <span class="legend-dot" :style="{ background: item.color }" />
                  <span class="legend-name">{{ item.label }}</span>
                </div>
                <div class="legend-right">
                  <span class="legend-amt">{{ fmt(item.val) }}</span>
                  <span class="legend-pct">{{ pct(item.val, resumen?.totalIngresos) }}%</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="card" style="opacity: 0">
          <p class="card-title">Gastos fijos <span class="tag tag-red">{{ fmt(resumen?.totalFijos) }}</span></p>
          <div v-if="store.gastosFijos.length === 0" class="empty">Sin gastos registrados</div>
          <div v-for="g in store.gastosFijos" :key="g.id" class="expense-row" style="opacity: 0">
            <div>
              <div class="expense-name">{{ g.categoria }}</div>
              <div class="expense-det">{{ g.detalle }}</div>
            </div>
            <div class="expense-amt">{{ fmt(g.monto) }}</div>
          </div>
        </div>

        <div class="card" style="opacity: 0">
          <p class="card-title">Gastos variables <span class="tag tag-amber">{{ fmt(resumen?.totalVariables) }}</span></p>
          <div v-if="store.gastosVariables.length === 0" class="empty">Sin gastos registrados</div>
          <div v-for="g in store.gastosVariables" :key="g.id" class="expense-row" style="opacity: 0">
            <div>
              <div class="expense-name">{{ g.categoria }}</div>
              <div class="expense-det">{{ g.detalle }}</div>
            </div>
            <div class="expense-amt">{{ fmt(g.monto) }}</div>
          </div>
        </div>
      </div>
    </template>

    <template v-else>
      <div class="empty-state">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" style="color: var(--text-muted); margin-bottom: 0.75rem"><path d="M9 19v-6a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2zm0 0V9a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v10m-6 0a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2m0 0V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-2a2 2 0 0 1-2-2z"/></svg>
        <div>No hay datos para este mes</div>
        <div style="font-size: 0.65rem; color: var(--text-muted); margin-top: 0.25rem">Agrega ingresos y gastos para ver el resumen</div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, reactive, watch, nextTick, onMounted } from 'vue'
import { animate, stagger } from 'motion'
import { useFinanceStore } from '@/stores/finance'
import MonthSelector from '@/components/common/MonthSelector.vue'
import DonutChart from '@/components/charts/DonutChart.vue'

const store = useFinanceStore()
const resumen = computed(() => store.resumen)

// Element refs
const kpiRef    = ref<HTMLElement>()
const bannerRef = ref<HTMLElement>()
const bottomRef = ref<HTMLElement>()

// Animated display values
const counters     = reactive({ ingresos: 0, fijos: 0, variables: 0, ahorro: 0, ahorroAnual: 0 })
const pcts         = reactive({ fijos: 0, variables: 0, ahorro: 0 })
const progressWidth = ref(0)

const distribucion = computed(() => [
  { label: 'Gastos fijos',     val: resumen.value?.totalFijos    ?? 0, color: '#f87171' },
  { label: 'Gastos variables', val: resumen.value?.totalVariables ?? 0, color: '#fbbf24' },
  { label: 'Ahorro',           val: resumen.value?.ahorro        ?? 0, color: '#34d399' },
])

const healthLabel = computed(() => {
  const p = resumen.value?.pctAhorro ?? 0
  if (p >= 30) return 'Excelente'
  if (p >= 20) return 'Muy bien'
  if (p >= 10) return 'Bien'
  if (p >= 0)  return 'Justo'
  return 'Déficit'
})
const healthClass = computed(() => {
  const p = resumen.value?.pctAhorro ?? 0
  if (p >= 20) return 'health-green'
  if (p >= 0)  return 'health-amber'
  return 'health-red'
})

// Custom ease-out-expo counter using rAF — avoids Motion's strict number animation types
function countUp(to: number, durationMs: number, delayMs: number, onUpdate: (v: number) => void) {
  const eased = (t: number) => t === 1 ? 1 : 1 - Math.pow(2, -10 * t)
  let start: number | null = null
  function tick(now: number) {
    if (start === null) start = now + delayMs
    if (now < start) { requestAnimationFrame(tick); return }
    const t = Math.min((now - start) / durationMs, 1)
    onUpdate(to * eased(t))
    if (t < 1) requestAnimationFrame(tick)
    else onUpdate(to)
  }
  requestAnimationFrame(tick)
}

watch(() => store.resumen, async (val) => {
  if (!val) return
  await nextTick()

  Object.assign(counters, { ingresos: 0, fijos: 0, variables: 0, ahorro: 0 })
  Object.assign(pcts,     { fijos: 0, variables: 0, ahorro: 0 })
  progressWidth.value = 0

  // 1. KPI grid entrance — Array.from fixes NodeList typing in Motion v12
  if (kpiRef.value) {
    animate(kpiRef.value, { opacity: 1 } as any, { duration: 0 })
    animate(
      Array.from(kpiRef.value.querySelectorAll('.kpi-card')),
      { opacity: [0, 1], y: [14, 0] } as any,
      { delay: stagger(0.09), duration: 0.3, ease: 'easeOut' }
    )
  }

  // 2. Number counters (custom rAF — no Motion types conflict)
  const total = val.totalIngresos
  countUp(val.totalIngresos,             1100,   0, v => { counters.ingresos  = v })
  countUp(val.totalFijos,                1100, 100, v => { counters.fijos     = v })
  countUp(val.totalVariables,            1100, 150, v => { counters.variables = v })
  countUp(val.ahorro,                    1100, 200, v => { counters.ahorro    = v })
  countUp(store.resumenAnual?.ahorro ?? 0, 1300, 300, v => { counters.ahorroAnual = v })

  // 3. KPI mini-bar pct counters
  countUp(pct(val.totalFijos,     total), 1000, 200, v => { pcts.fijos     = v })
  countUp(pct(val.totalVariables, total), 1000, 300, v => { pcts.variables = v })
  countUp(Math.max(0, val.pctAhorro),    1000, 400, v => { pcts.ahorro    = v })

  // 4. Banner slide-in (Motion on single HTMLElement is fine)
  if (bannerRef.value) {
    animate(bannerRef.value, { opacity: [0, 1], x: [-24, 0] } as any, { duration: 0.45, delay: 0.28, ease: 'easeOut' })
  }

  // 5. Annual progress bar — CSS transition driven by reactive ref
  setTimeout(() => {
    progressWidth.value = Math.min(store.resumenAnual?.pctAhorro ?? 0, 100)
  }, 500)

  // 6. Bottom grid cards stagger with manual delay offset
  if (bottomRef.value) {
    animate(
      Array.from(bottomRef.value.querySelectorAll(':scope > .card')),
      { opacity: [0, 1], y: [20, 0] } as any,
      { delay: (i: number) => 0.32 + i * 0.12, duration: 0.35, ease: 'easeOut' }
    )
  }

  // 7. Expense rows stagger
  if (bottomRef.value) {
    animate(
      Array.from(bottomRef.value.querySelectorAll('.expense-row')),
      { opacity: [0, 1], x: [-8, 0] } as any,
      { delay: (i: number) => 0.65 + i * 0.035, duration: 0.22, ease: 'easeOut' }
    )
  }
}, { immediate: true })

const fmt     = (n?: number) => n != null ? '$' + Math.round(n).toLocaleString('es-CO') : '$0'
const fmtAnim = (n: number)  => '$' + Math.round(n).toLocaleString('es-CO')
const pct     = (a?: number, b?: number) => (a && b) ? Math.round(a / b * 100) : 0

onMounted(() => { store.cargarDatos(); store.cargarResumenAnual() })
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }

/* Skeletons */
.loading-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; }
.skeleton-kpi {
  height: 90px;
  border: 1px solid var(--border);
  border-radius: 10px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

/* KPI Grid */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; margin-bottom: 1rem; }
.kpi-card {
  position: relative;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1rem 1.1rem;
  overflow: hidden;
}
.kpi-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 2px;
  background: var(--ca, var(--border));
  border-radius: 10px 10px 0 0;
}
.kpi-ahorro {
  background: linear-gradient(135deg, var(--surface) 40%, rgba(52,211,153,0.05) 100%);
  border-color: rgba(52,211,153,0.15);
}
.kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
.kpi-label { font-size: 0.62rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.kpi-icon { opacity: 0.7; flex-shrink: 0; }
.kpi-value { font-size: 1.25rem; font-family: var(--ff-display); font-weight: 700; margin-bottom: 0.5rem; }
.kpi-bar-wrap { height: 3px; background: var(--surface2); border-radius: 2px; overflow: hidden; margin-bottom: 0.35rem; }
.kpi-bar { height: 100%; border-radius: 2px; max-width: 100%; }
.kpi-pct { font-size: 0.62rem; color: var(--text-muted); }

/* Ahorro anual banner */
.anual-banner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 2rem;
  background: var(--surface);
  border: 1px solid rgba(52,211,153,0.15);
  border-left: 3px solid var(--green);
  border-radius: 12px;
  padding: 1.25rem 1.5rem;
  margin-bottom: 1rem;
}
.anual-left { flex-shrink: 0; }
.anual-big { font-size: 2rem; font-family: var(--ff-display); font-weight: 800; color: var(--green); line-height: 1; margin-bottom: 0.35rem; }
.anual-meta { font-size: 0.68rem; color: var(--text-muted); }
.anual-right { flex: 1; display: flex; flex-direction: column; gap: 0.75rem; align-items: flex-end; }
.anual-stats { display: flex; gap: 1.5rem; }
.anual-stat { display: flex; flex-direction: column; align-items: flex-end; gap: 0.15rem; }
.anual-stat-label { font-size: 0.6rem; color: var(--text-muted); letter-spacing: 0.1em; text-transform: uppercase; }
.anual-stat strong { font-size: 0.82rem; color: var(--text-primary); }
.anual-progress-wrap { width: 100%; }
.anual-progress-track { position: relative; height: 4px; background: var(--surface2); border-radius: 3px; overflow: visible; margin-bottom: 0.3rem; }
.anual-progress-fill { height: 100%; background: linear-gradient(90deg, var(--green), rgba(52,211,153,0.5)); border-radius: 3px; transition: width 1.4s cubic-bezier(0.16, 1, 0.3, 1); }
.anual-progress-goal {
  position: absolute;
  top: -4px; bottom: -4px; left: 20%;
  width: 1px;
  background: rgba(251,191,36,0.5);
}
.anual-progress-goal::after { content: '20%'; position: absolute; top: -14px; left: 3px; font-size: 0.55rem; color: var(--amber); white-space: nowrap; }
.anual-progress-labels { display: flex; justify-content: space-between; font-size: 0.58rem; color: var(--text-muted); }

.health-badge { font-size: 0.6rem; letter-spacing: 0.12em; text-transform: uppercase; padding: 0.2rem 0.65rem; border-radius: 2px; border: 1px solid; }
.health-green { border-color: rgba(52,211,153,0.35); color: var(--green); background: rgba(52,211,153,0.05); }
.health-amber { border-color: rgba(251,191,36,0.35); color: var(--amber); background: rgba(251,191,36,0.05); }
.health-red   { border-color: rgba(248,113,113,0.35); color: var(--red);   background: rgba(248,113,113,0.05); }

/* 3-col bottom */
.bottom-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.75rem; }

.card-title { font-size: 0.62rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem; }

/* Donut section */
.donut-section { display: flex; flex-direction: column; align-items: center; gap: 1rem; }
.donut-legend { width: 100%; display: flex; flex-direction: column; gap: 0.6rem; }
.legend-row { display: flex; justify-content: space-between; align-items: center; }
.legend-left { display: flex; align-items: center; gap: 0.5rem; }
.legend-dot { width: 8px; height: 8px; border-radius: 2px; flex-shrink: 0; }
.legend-name { font-size: 0.7rem; color: var(--text-secondary); }
.legend-right { display: flex; align-items: center; gap: 0.5rem; }
.legend-amt { font-size: 0.7rem; color: var(--text-primary); }
.legend-pct { font-size: 0.62rem; color: var(--text-muted); min-width: 30px; text-align: right; }

/* Expense rows */
.expense-row { display: flex; justify-content: space-between; align-items: center; padding: 0.55rem 0; border-bottom: 1px solid var(--border); }
.expense-row:last-child { border-bottom: none; }
.expense-name { font-size: 0.75rem; color: var(--text-primary); }
.expense-det { font-size: 0.62rem; color: var(--text-muted); margin-top: 0.1rem; }
.expense-amt { font-size: 0.75rem; color: var(--text-primary); flex-shrink: 0; margin-left: 0.5rem; }

/* Empty states */
.empty { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }
.empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 4rem 0; color: var(--text-secondary); font-size: 0.8rem; }
</style>
