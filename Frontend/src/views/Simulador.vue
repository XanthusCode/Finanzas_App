<template>
  <div class="fade-up simulador">
    <div class="page-header">
      <div class="header-left">
        <div>
          <div class="section-label">Herramientas</div>
          <h1 class="page-title">Simulador what-if</h1>
        </div>
      </div>
      <MonthSelector class="month-selector" :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
    </div>

    <div v-if="store.loading" class="loading-list">
      <div v-for="i in 5" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="filas.length === 0">
      <div class="empty-state">
        <span class="empty-icon"><SlidersHorizontal :size="24" :stroke-width="1.8" /></span>
        <p class="empty-title">Sin gastos para simular</p>
        <p class="empty-sub">Registra gastos este mes y aquí podrás probar escenarios: ¿qué pasa si gasto menos en comida?</p>
      </div>
    </template>

    <template v-else>
      <!-- Resultado: actual → simulado + diferencia -->
      <div class="resultado" :class="deltaClass">
        <div class="resultado-comparacion">
          <div class="resultado-col">
            <span class="resultado-label">Ahorro actual</span>
            <span class="resultado-val resultado-val--actual">{{ fmt(store.resumen?.ahorro ?? 0) }}</span>
          </div>
          <span class="resultado-arrow"><ArrowRight :size="22" :stroke-width="2" /></span>
          <div class="resultado-col">
            <span class="resultado-label">Ahorro simulado</span>
            <span class="resultado-val resultado-val--sim">{{ fmt(ahorroSimulado) }}</span>
          </div>
        </div>

        <div class="resultado-acciones">
          <div class="delta-badge">
            <TrendingUp v-if="deltaAhorro > 0" :size="16" :stroke-width="2.2" />
            <TrendingDown v-else-if="deltaAhorro < 0" :size="16" :stroke-width="2.2" />
            <span>{{ deltaAhorro > 0 ? '+' : '' }}{{ fmt(deltaAhorro) }}</span>
            <small>{{ deltaAhorro > 0 ? 'más de ahorro' : deltaAhorro < 0 ? 'menos de ahorro' : 'sin cambios' }}</small>
          </div>
          <button class="btn btn-sm btn-ghost" :disabled="!hayCambios" @click="resetear">
            <RotateCcw :size="14" :stroke-width="2" /> Resetear
          </button>
        </div>
      </div>

      <div class="sim-layout">
        <!-- Filas por categoría -->
        <section class="card sim-card">
          <header class="sim-head">
            <h2>Ajusta los gastos por categoría</h2>
            <span class="sim-hint"><span class="hint-marker" /> valor real del mes</span>
          </header>

          <div v-for="fila in filas" :key="fila.categoria" class="sim-row" :class="{ 'is-changed': diff(fila) !== 0 }">
            <div class="sim-cat">
              <span class="sim-cat-name">{{ fila.categoria }}</span>
              <span v-if="diff(fila) !== 0" class="sim-diff" :class="diff(fila) < 0 ? 'diff-good' : 'diff-bad'">
                {{ diff(fila) > 0 ? '+' : '' }}{{ fmt(diff(fila)) }}
              </span>
              <span v-else class="sim-diff sim-diff--muted">{{ fmt(fila.actual) }} real</span>
            </div>

            <div class="sim-slider-wrap" :style="{ '--p': pctSim(fila) + '%', '--fill': fillColor(fila) }">
              <input
                type="range"
                class="sim-slider"
                :min="0"
                :max="fila.max"
                :step="1000"
                :value="simulacion[fila.categoria] ?? fila.actual"
                :aria-label="`Gasto simulado en ${fila.categoria}`"
                @input="onSlider(fila.categoria, $event)"
              />
              <span class="sim-ghost" :style="{ left: `calc(10px + (100% - 20px) * ${pctActual(fila) / 100})` }" :title="`Real: ${fmt(fila.actual)}`" />
            </div>

            <CurrencyInput
              :model-value="simulacion[fila.categoria] ?? fila.actual"
              class="input sim-input"
              @update:model-value="v => setSim(fila.categoria, v)"
            />
          </div>
        </section>

        <!-- Totales -->
        <aside class="card totales-card">
          <p class="totales-title">Resumen del escenario</p>
          <div class="totales-row">
            <span class="totales-label">Ingresos</span>
            <span class="totales-val">{{ fmt(store.resumen?.totalIngresos ?? 0) }}</span>
          </div>
          <div class="totales-row">
            <span class="totales-label">Gastos actuales</span>
            <span class="totales-val">{{ fmt(store.resumen?.totalGastos ?? 0) }}</span>
          </div>
          <div class="totales-row">
            <span class="totales-label">Gastos simulados</span>
            <span class="totales-val">{{ fmt(totalSimulado) }}</span>
          </div>
          <div class="totales-ahorro">
            <span class="totales-label">Ahorro simulado</span>
            <span class="totales-big" :class="ahorroSimulado >= 0 ? 'is-pos' : 'is-neg'">{{ fmt(ahorroSimulado) }}</span>
            <div class="tasa-bar"><div class="tasa-fill" :class="ahorroSimulado >= 0 ? 'is-pos' : 'is-neg'" :style="{ width: Math.min(Math.max(pctAhorroSim, 0), 100) + '%' }" /></div>
            <span class="totales-pct">{{ pctAhorroSim }}% de tus ingresos</span>
          </div>
        </aside>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, watch, onMounted } from 'vue'
import { ArrowRight, TrendingUp, TrendingDown, RotateCcw, SlidersHorizontal } from '@lucide/vue'
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

const deltaClass = computed(() =>
  deltaAhorro.value > 0 ? 'is-pos' : deltaAhorro.value < 0 ? 'is-neg' : 'is-zero'
)

const hayCambios = computed(() => filas.value.some(f => diff(f) !== 0))

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

type Fila = { categoria: string; actual: number; max: number }

const pctSim = (f: Fila) =>
  f.max > 0 ? Math.min(((simulacion[f.categoria] ?? f.actual) / f.max) * 100, 100) : 0

const pctActual = (f: Fila) =>
  f.max > 0 ? Math.min((f.actual / f.max) * 100, 100) : 0

const diff = (f: { categoria: string; actual: number }) =>
  (simulacion[f.categoria] ?? f.actual) - f.actual

// Gastar menos que lo real = verde; más = naranja; igual = azul
function fillColor(f: Fila) {
  const d = diff(f)
  return d < 0 ? 'var(--c-pos)' : d > 0 ? 'var(--c-want)' : 'var(--c-need)'
}

onMounted(() => { if (!store.resumen) store.cargarDatos() })
</script>

<style scoped>
.simulador { font-variant-numeric: tabular-nums; }
.header-left { display: flex; align-items: flex-end; gap: 1.25rem; flex-wrap: wrap; }

/* ── Resultado ── */
.resultado {
  --dc: var(--text-secondary);
  display: flex; align-items: center; justify-content: space-between; gap: 1.5rem;
  margin-bottom: 1.25rem;
  padding: 1.25rem 1.5rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  box-shadow: var(--shadow-card);
  transition: border-color 0.2s ease;
}
.resultado.is-pos { --dc: var(--c-pos); border-color: color-mix(in srgb, var(--c-pos) 30%, var(--border)); }
.resultado.is-neg { --dc: var(--c-neg); border-color: color-mix(in srgb, var(--c-neg) 30%, var(--border)); }

.resultado-comparacion { display: flex; align-items: center; gap: 1.5rem; }
.resultado-col   { display: flex; flex-direction: column; gap: 0.35rem; }
.resultado-label { font-size: 0.64rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.resultado-val   { font-family: var(--ff-mono); font-weight: 700; letter-spacing: -0.02em; line-height: 1; }
.resultado-val--actual { font-size: 1.2rem; color: var(--text-subtle); }
.resultado-val--sim    { font-size: 1.85rem; color: var(--text-primary); }
.resultado-arrow {
  display: grid; place-items: center;
  width: 38px; height: 38px; border-radius: 999px;
  color: var(--text-secondary); background: var(--surface2);
}

.resultado-acciones { display: flex; align-items: center; gap: 0.75rem; }
.delta-badge {
  display: inline-flex; align-items: center; gap: 0.5rem;
  padding: 0.6rem 1rem;
  border-radius: 12px;
  color: var(--dc);
  background: color-mix(in srgb, var(--dc) 12%, transparent);
  transition: color 0.2s ease, background 0.2s ease;
}
.delta-badge span { font-family: var(--ff-mono); font-size: 1.1rem; font-weight: 700; }
.delta-badge small { font-size: 0.68rem; opacity: 0.85; }
.btn-ghost { background: transparent; border-color: transparent; color: var(--text-secondary); }
.btn-ghost:hover:not(:disabled) { background: var(--surface2); border-color: var(--border); color: var(--text-primary); }

/* ── Layout ── */
.sim-layout { display: grid; grid-template-columns: minmax(0, 1fr) 300px; gap: 1rem; align-items: start; }
.sim-card { padding: 1.1rem 1.1rem 0.6rem; }
.sim-card:hover { border-color: var(--border); }

.sim-head { display: flex; justify-content: space-between; align-items: center; gap: 1rem; padding: 0 0.4rem 0.6rem; }
.sim-head h2 {
  font-family: var(--ff-body);
  font-size: 0.74rem; font-weight: 500; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--text-primary);
}
.sim-hint { display: inline-flex; align-items: center; gap: 0.4rem; font-size: 0.66rem; color: var(--text-muted); }
.hint-marker { width: 2px; height: 12px; border-radius: 2px; background: var(--text-primary); opacity: 0.7; }

/* ── Fila: [categoría 160px] [slider] [input 130px] ── */
.sim-row {
  display: grid;
  grid-template-columns: 160px minmax(0, 1fr) 130px;
  align-items: center;
  gap: 1.25rem;
  padding: 0.55rem 0.4rem;
  border-radius: 10px;
  transition: background 0.12s ease;
}
.sim-row:hover,
.sim-row:focus-within { background: var(--surface2); }

.sim-cat { display: flex; flex-direction: column; gap: 0.15rem; min-width: 0; }
.sim-cat-name { font-size: 0.8rem; font-weight: 500; color: var(--text-primary); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.sim-diff { font-family: var(--ff-mono); font-size: 0.66rem; font-weight: 700; }
.sim-diff--muted { color: var(--text-muted); font-weight: 400; }
.diff-good { color: var(--c-pos); }
.diff-bad  { color: var(--c-want); }

/* Slider: un solo track con relleno + marcador fantasma del valor real */
.sim-slider-wrap { position: relative; height: 22px; display: flex; align-items: center; }
.sim-slider {
  -webkit-appearance: none; appearance: none;
  width: 100%; height: 8px; margin: 0;
  border-radius: 99px;
  background: linear-gradient(to right, var(--fill) 0 var(--p), color-mix(in srgb, var(--text-muted) 22%, var(--surface2)) var(--p) 100%);
  cursor: pointer;
  outline: none;
}
.sim-slider::-webkit-slider-thumb {
  -webkit-appearance: none; appearance: none;
  position: relative; z-index: 2;
  width: 20px; height: 20px; border-radius: 999px;
  background: #fff;
  border: 2px solid var(--fill);
  box-shadow: 0 2px 6px rgba(0,0,0,0.35);
  transition: transform 0.12s ease, box-shadow 0.12s ease;
}
.sim-slider::-moz-range-thumb {
  width: 16px; height: 16px; border-radius: 999px;
  background: #fff;
  border: 2px solid var(--fill);
  box-shadow: 0 2px 6px rgba(0,0,0,0.35);
}
.sim-slider:hover::-webkit-slider-thumb { transform: scale(1.1); }
.sim-slider:active::-webkit-slider-thumb { transform: scale(1.15); box-shadow: 0 0 0 6px color-mix(in srgb, var(--fill) 25%, transparent), 0 2px 6px rgba(0,0,0,0.35); }
.sim-slider:focus-visible::-webkit-slider-thumb { box-shadow: var(--focus-ring), 0 2px 6px rgba(0,0,0,0.35); }

/* Marcador del valor real: misma escala que el centro del thumb (10px … ancho−10px) */
.sim-ghost {
  position: absolute; top: 50%;
  width: 2px; height: 14px; border-radius: 2px;
  background: var(--text-primary); opacity: 0.55;
  transform: translate(-50%, -50%);
  pointer-events: none;
}

.sim-input {
  height: 36px; min-height: 36px;
  padding: 0 0.7rem;
  font-family: var(--ff-mono); font-size: 0.8rem; font-weight: 700;
  text-align: right;
  background: transparent;
  border: 1px solid var(--border-hover);
  border-radius: 8px;
}
.sim-row:hover .sim-input { background: var(--surface); }

/* ── Totales ── */
.totales-card { position: sticky; top: 88px; padding: 1.2rem 1.3rem; }
.totales-card:hover { border-color: var(--border); }
.totales-title { font-size: 0.66rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 0.6rem; }
.totales-row   { display: flex; justify-content: space-between; align-items: center; padding: 0.6rem 0; border-bottom: 1px solid var(--border); }
.totales-label { font-size: 0.75rem; color: var(--text-secondary); }
.totales-val   { font-family: var(--ff-mono); font-size: 0.82rem; font-weight: 700; color: var(--text-primary); }
.totales-ahorro { display: flex; flex-direction: column; gap: 0.4rem; padding-top: 0.9rem; }
.totales-big { font-family: var(--ff-mono); font-size: 1.5rem; font-weight: 700; letter-spacing: -0.02em; }
.totales-big.is-pos { color: var(--c-pos); }
.totales-big.is-neg { color: var(--c-neg); }
.tasa-bar  { height: 8px; border-radius: 99px; overflow: hidden; background: color-mix(in srgb, var(--text-muted) 22%, var(--surface2)); margin-top: 0.2rem; }
.tasa-fill { height: 100%; border-radius: 99px; transition: width 0.25s ease; }
.tasa-fill.is-pos { background: var(--c-pos); }
.tasa-fill.is-neg { background: var(--c-neg); }
.totales-pct { font-size: 0.68rem; color: var(--text-muted); }

/* ── Vacío / carga ── */
.empty-state { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 0.4rem; padding: 4rem 1rem; }
.empty-icon {
  display: grid; place-items: center;
  width: 52px; height: 52px; margin-bottom: 0.4rem; border-radius: 999px;
  color: var(--text-secondary); background: var(--surface2);
}
.empty-title { font-size: 0.88rem; font-weight: 500; color: var(--text-primary); }
.empty-sub   { font-size: 0.72rem; color: var(--text-muted); max-width: 340px; }

.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 52px; border-radius: 10px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 1024px) {
  .sim-layout { grid-template-columns: 1fr; }
  .totales-card { position: static; }
}

@media (max-width: 640px) {
  .resultado { flex-direction: column; align-items: stretch; gap: 1rem; padding: 1.1rem; }
  .resultado-comparacion { justify-content: space-between; gap: 0.75rem; }
  .resultado-val--sim { font-size: 1.5rem; }
  .resultado-acciones { justify-content: space-between; }

  .sim-row {
    grid-template-columns: 1fr 120px;
    grid-template-areas: "cat input" "slider slider";
    row-gap: 0.5rem;
    padding: 0.75rem 0.4rem;
  }
  .sim-cat { grid-area: cat; }
  .sim-slider-wrap { grid-area: slider; }
  .sim-input { grid-area: input; }
}

@media (max-width: 500px) {
  .header-left { width: 100%; }
  .month-selector { width: 100%; }
  .month-selector :deep(.label) { flex: 1; }
}
</style>
