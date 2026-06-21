<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Herramientas</div>
        <h1 class="page-title">Simulador what-if</h1>
      </div>
      <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
    </div>

    <div v-if="store.loading" class="loading-list">
      <div v-for="i in 5" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="filas.length === 0">
      <div class="empty-state">Sin gastos registrados este mes para simular.</div>
    </template>

    <template v-else>
      <!-- Banner de resultado -->
      <div class="resultado-banner" :class="deltaAhorro >= 0 ? 'banner-positivo' : 'banner-negativo'">
        <div class="resultado-col">
          <span class="resultado-label">Ahorro actual</span>
          <span class="resultado-val">{{ fmt(store.resumen?.ahorro ?? 0) }}</span>
        </div>
        <div class="resultado-arrow">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M5 12h14M12 5l7 7-7 7"/>
          </svg>
        </div>
        <div class="resultado-col">
          <span class="resultado-label">Ahorro simulado</span>
          <span class="resultado-val resultado-sim">{{ fmt(ahorroSimulado) }}</span>
        </div>
        <div class="resultado-delta" :class="deltaAhorro >= 0 ? 'delta-pos' : 'delta-neg'">
          {{ deltaAhorro >= 0 ? '+' : '' }}{{ fmt(deltaAhorro) }}
        </div>
        <button class="btn btn-sm" style="margin-left:auto" @click="resetear">Resetear</button>
      </div>

      <!-- Filas por categoría -->
      <div class="card">
        <p class="card-title">Ajusta los gastos por categoría</p>

        <div v-for="fila in filas" :key="fila.categoria" class="sim-row">
          <div class="sim-cat">{{ fila.categoria }}</div>

          <div class="sim-center">
            <input
              type="range"
              class="sim-slider"
              :min="0"
              :max="fila.max"
              :step="1000"
              :value="simulacion[fila.categoria] ?? fila.actual"
              @input="onSlider(fila.categoria, $event)"
            />
            <div class="sim-bar-track">
              <div
                class="sim-bar-fill"
                :class="pctSim(fila) > 100 ? 'fill-danger' : pctSim(fila) > 80 ? 'fill-warn' : 'fill-ok'"
                :style="{ width: Math.min(pctSim(fila), 100) + '%' }"
              />
              <div class="sim-bar-actual" :style="{ left: pctActual(fila) + '%' }" />
            </div>
          </div>

          <div class="sim-values">
            <CurrencyInput
              :model-value="simulacion[fila.categoria] ?? fila.actual"
              class="input input-inline"
              style="max-width: 130px"
              @update:model-value="v => setSim(fila.categoria, v)"
            />
            <span class="sim-diff" :class="diff(fila) < 0 ? 'diff-neg' : diff(fila) > 0 ? 'diff-pos' : ''">
              {{ diff(fila) !== 0 ? (diff(fila) > 0 ? '+' : '') + fmt(diff(fila)) : '—' }}
            </span>
          </div>
        </div>
      </div>

      <!-- Totales -->
      <div class="card totales-card">
        <div class="totales-row">
          <span class="totales-label">Total gastos actual</span>
          <span class="totales-val">{{ fmt(store.resumen?.totalGastos ?? 0) }}</span>
        </div>
        <div class="totales-row">
          <span class="totales-label">Total gastos simulado</span>
          <span class="totales-val">{{ fmt(totalSimulado) }}</span>
        </div>
        <div class="totales-sep" />
        <div class="totales-row">
          <span class="totales-label">Ingresos</span>
          <span class="totales-val">{{ fmt(store.resumen?.totalIngresos ?? 0) }}</span>
        </div>
        <div class="totales-row totales-ahorro">
          <span class="totales-label">Ahorro simulado</span>
          <span class="totales-val" :style="{ color: ahorroSimulado >= 0 ? 'var(--accent)' : 'var(--red)' }">
            {{ fmt(ahorroSimulado) }}
            <span class="totales-pct">({{ pctAhorroSim }}%)</span>
          </span>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, watch, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import { fmtCOP as fmt } from '@/utils'
import MonthSelector from '@/components/common/MonthSelector.vue'
import CurrencyInput from '@/components/common/CurrencyInput.vue'

const store = useFinanceStore()

const simulacion = reactive<Record<string, number>>({})

const gastosPorCat = computed(() => {
  const map: Record<string, number> = {}
  for (const g of store.gastos) {
    map[g.categoria] = (map[g.categoria] ?? 0) + g.monto
  }
  return map
})

function inicializar() {
  Object.keys(simulacion).forEach(k => delete simulacion[k])
  for (const [cat, monto] of Object.entries(gastosPorCat.value)) {
    simulacion[cat] = monto
  }
}

watch(gastosPorCat, inicializar, { immediate: true })

const filas = computed(() =>
  Object.entries(gastosPorCat.value).map(([categoria, actual]) => ({
    categoria,
    actual,
    max: Math.max(actual * 2, 50_000),
  })).sort((a, b) => b.actual - a.actual)
)

const totalSimulado = computed(() =>
  Object.values(simulacion).reduce((s, v) => s + v, 0)
)

const ahorroSimulado = computed(() =>
  (store.resumen?.totalIngresos ?? 0) - totalSimulado.value
)

const deltaAhorro = computed(() =>
  ahorroSimulado.value - (store.resumen?.ahorro ?? 0)
)

const pctAhorroSim = computed(() => {
  const ing = store.resumen?.totalIngresos ?? 0
  return ing > 0 ? Math.round(ahorroSimulado.value / ing * 100) : 0
})

function onSlider(cat: string, e: Event) {
  simulacion[cat] = Number((e.target as HTMLInputElement).value)
}

function setSim(cat: string, val: number) {
  simulacion[cat] = val
}

function resetear() { inicializar() }

const pctSim    = (f: { categoria: string; actual: number; max: number }) =>
  f.max > 0 ? ((simulacion[f.categoria] ?? f.actual) / f.max) * 100 : 0

const pctActual = (f: { actual: number; max: number }) =>
  f.max > 0 ? Math.min((f.actual / f.max) * 100, 100) : 0

const diff = (f: { categoria: string; actual: number }) =>
  (simulacion[f.categoria] ?? f.actual) - f.actual

onMounted(() => { if (!store.resumen) store.cargarDatos() })
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 1rem; }

.empty-state { text-align: center; color: var(--text-muted); font-size: 0.8rem; padding: 3rem 0; }

/* Banner resultado */
.resultado-banner {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  border-radius: 10px;
  padding: 1rem 1.25rem;
  margin-bottom: 1rem;
  border: 1px solid;
}
.banner-positivo { background: rgba(52,211,153,0.06); border-color: rgba(52,211,153,0.2); }
.banner-negativo { background: rgba(248,113,113,0.06); border-color: rgba(248,113,113,0.2); }

.resultado-col    { display: flex; flex-direction: column; gap: 0.2rem; }
.resultado-label  { font-size: 0.62rem; color: var(--text-muted); letter-spacing: 0.08em; text-transform: uppercase; }
.resultado-val    { font-size: 1.1rem; font-weight: 700; color: var(--text-primary); }
.resultado-sim    { color: var(--accent); }
.resultado-arrow  { color: var(--text-muted); }
.resultado-delta  { font-size: 1rem; font-weight: 700; padding: 0.25rem 0.65rem; border-radius: 6px; }
.delta-pos { color: var(--accent); background: rgba(52,211,153,0.1); }
.delta-neg { color: var(--red);   background: rgba(248,113,113,0.1); }

/* Filas */
.sim-row {
  display: grid;
  grid-template-columns: 140px 1fr auto;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--border);
}
.sim-row:last-child { border-bottom: none; }

.sim-cat { font-size: 0.78rem; color: var(--text-primary); }

.sim-center { display: flex; flex-direction: column; gap: 0.4rem; }

.sim-slider {
  width: 100%;
  accent-color: var(--accent);
  cursor: pointer;
}

.sim-bar-track {
  position: relative;
  height: 4px;
  background: var(--surface2);
  border-radius: 2px;
  overflow: visible;
}
.sim-bar-fill {
  height: 100%;
  border-radius: 2px;
  transition: width 0.15s ease;
}
.fill-ok     { background: var(--accent); }
.fill-warn   { background: #f59e0b; }
.fill-danger { background: var(--red); }

.sim-bar-actual {
  position: absolute;
  top: -3px;
  width: 2px;
  height: 10px;
  background: var(--text-muted);
  border-radius: 1px;
  transform: translateX(-50%);
}

.sim-values {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
  min-width: 140px;
}

.input-inline { height: 30px; font-size: 0.78rem; padding: 0 0.6rem; }

.sim-diff   { font-size: 0.7rem; font-weight: 600; }
.diff-neg   { color: var(--accent); }
.diff-pos   { color: var(--red); }

/* Totales */
.totales-card   { margin-top: 0.75rem; }
.totales-row    { display: flex; justify-content: space-between; align-items: center; padding: 0.45rem 0; }
.totales-label  { font-size: 0.78rem; color: var(--text-secondary); }
.totales-val    { font-size: 0.82rem; font-weight: 600; color: var(--text-primary); }
.totales-pct    { font-size: 0.68rem; color: var(--text-muted); font-weight: 400; margin-left: 0.3rem; }
.totales-sep    { height: 1px; background: var(--border); margin: 0.25rem 0; }
.totales-ahorro .totales-label { font-weight: 600; }
.totales-ahorro .totales-val   { font-size: 0.9rem; }

.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 52px; border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

.btn-sm { padding: 0.3rem 0.7rem; font-size: 0.72rem; }
</style>
