<template>
  <div class="fade-up proyeccion">
    <div class="page-header">
      <div class="header-left">
        <div>
          <div class="section-label">Análisis</div>
          <h1 class="page-title">Proyección de cierre</h1>
        </div>
      </div>
      <MonthSelector class="month-selector" :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
    </div>

    <div v-if="store.loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="!resumen">
      <div class="empty-state">
        <span class="empty-icon"><ChartLine :size="24" :stroke-width="1.8" /></span>
        <p class="empty-title">Sin datos para este período</p>
      </div>
    </template>

    <!-- Mes futuro -->
    <template v-else-if="esFuturo">
      <div class="empty-state">
        <span class="empty-icon"><CalendarClock :size="24" :stroke-width="1.8" /></span>
        <p class="empty-title">Este mes aún no empieza</p>
        <p class="empty-sub">Selecciona el mes actual o un mes pasado para ver la proyección.</p>
      </div>
    </template>

    <!-- Mes pasado — resultados reales -->
    <template v-else-if="esPasado">
      <div class="contexto-banner">
        <Info :size="15" :stroke-width="2" />
        Mes cerrado — mostrando resultados reales
      </div>
      <div class="kpi-grid">
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">Gasto real</span><span class="kpi-icon"><Receipt :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val">{{ fmt(resumen.totalGastos) }}</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">Ingresos</span><span class="kpi-icon"><Banknote :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val">{{ fmt(resumen.totalIngresos) }}</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">Ahorro</span><span class="kpi-icon kpi-icon--pos"><PiggyBank :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val" :class="resumen.ahorro >= 0 ? 'is-pos' : 'is-neg'">{{ resumen.ahorro >= 0 ? '+' : '' }}{{ fmt(resumen.ahorro) }}</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">% Ahorro</span><span class="kpi-icon"><Percent :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val" :class="resumen.pctAhorro >= 20 ? 'is-pos' : resumen.pctAhorro >= 10 ? 'is-warn' : 'is-neg'">
            {{ resumen.pctAhorro.toFixed(1) }}%
          </span>
        </div>
      </div>
    </template>

    <!-- Mes actual — proyección -->
    <template v-else>
      <!-- Contexto temporal -->
      <div class="tiempo-bar">
        <div class="tiempo-info">
          <span class="tiempo-label">Día {{ diaActual }} de {{ diasDelMes }}</span>
          <span class="tiempo-resta">· Faltan {{ diasRestantes }} día{{ diasRestantes !== 1 ? 's' : '' }}</span>
          <span class="tiempo-pct">{{ pctMes }}% del mes</span>
        </div>
        <div class="tiempo-track">
          <div class="tiempo-fill" :style="{ width: pctMes + '%' }" />
        </div>
      </div>

      <!-- KPIs proyección -->
      <div class="kpi-grid">
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">Gastado hasta hoy</span><span class="kpi-icon"><CalendarDays :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val">{{ fmt(resumen.totalGastos) }}</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">Proyección fin de mes</span><span class="kpi-icon"><TrendingUp :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val" :class="{ 'is-neg': gastoProyectado > resumen.totalIngresos }">{{ fmt(gastoProyectado) }}</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">Ingresos</span><span class="kpi-icon"><Banknote :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val">{{ fmt(resumen.totalIngresos) }}</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top"><span class="kpi-label">Ahorro proyectado</span><span class="kpi-icon kpi-icon--pos"><PiggyBank :size="15" :stroke-width="2" /></span></div>
          <span class="kpi-val" :class="ahorroProyectado >= 0 ? 'is-pos' : 'is-neg'">
            {{ ahorroProyectado >= 0 ? '+' : '' }}{{ fmt(ahorroProyectado) }}
          </span>
        </div>
      </div>

      <!-- Ritmo de gasto: la barra completa = tus ingresos -->
      <div class="burn-card">
        <div class="burn-header">
          <div>
            <h2 class="burn-title">Ritmo de gasto</h2>
            <p class="burn-sub">La barra completa son tus ingresos; lo que queda vacío es tu ahorro.</p>
          </div>
          <span class="burn-rate"><Flame :size="14" :stroke-width="2" /> {{ fmt(tasaDiaria) }}<small>/día</small></span>
        </div>

        <div class="burn-legend">
          <span class="legend-pill legend-pill--real"><span class="dot" />Gastado <strong>{{ fmt(resumen.totalGastos) }}</strong></span>
          <span class="legend-pill legend-pill--proj"><span class="dot" />Proyectado <strong>{{ fmt(gastoRestanteProyectado) }}</strong></span>
          <span v-if="margenSinGastar > 0" class="legend-pill legend-pill--margin"><span class="dot" />Margen (ahorro) <strong>{{ fmt(margenSinGastar) }}</strong></span>
        </div>

        <div class="burn-viz">
          <div class="burn-seg burn-real" :style="{ width: pctBarra(resumen.totalGastos) + '%' }" />
          <div class="burn-seg burn-proj" :style="{ width: pctBarra(gastoRestanteProyectado) + '%' }" />
          <!-- Si te pasas, la línea marca dónde terminan tus ingresos -->
          <div v-if="gastoProyectado > resumen.totalIngresos" class="burn-ingreso" :style="{ left: pctBarra(resumen.totalIngresos) + '%' }">
            <span>Ingresos</span>
          </div>
        </div>

        <div v-if="gastoProyectado > resumen.totalIngresos" class="burn-alerta">
          <TriangleAlert :size="14" :stroke-width="2.2" />
          Al ritmo actual terminarás el mes con déficit de <strong>{{ fmt(Math.abs(ahorroProyectado)) }}</strong>
        </div>
      </div>

      <!-- Consejo -->
      <div class="consejo" :class="`consejo--${sinIngresos ? 'warn' : estadoConsejo}`">
        <span class="consejo-icon">
          <Info v-if="sinIngresos" :size="16" :stroke-width="2.6" />
          <Check v-else-if="estadoConsejo === 'ok'" :size="16" :stroke-width="2.8" />
          <TriangleAlert v-else-if="estadoConsejo === 'warn'" :size="15" :stroke-width="2.4" />
          <TrendingDown v-else :size="16" :stroke-width="2.4" />
        </span>
        <div class="consejo-body">
          <template v-if="sinIngresos">
            <p class="consejo-title">Faltan ingresos por registrar</p>
            <p class="consejo-texto">Aún no registraste ingresos este mes — la proyección de cierre no es confiable todavía.</p>
          </template>
          <template v-else-if="estadoConsejo === 'ok'">
            <p class="consejo-title">¡Vas muy bien!</p>
            <p class="consejo-texto">Proyección de ahorro: <strong class="hl">{{ fmt(ahorroProyectado) }}</strong> ({{ pctAhorroProyectado }}% del ingreso).</p>
          </template>
          <template v-else-if="estadoConsejo === 'warn'">
            <p class="consejo-title">Estás cerca del límite</p>
            <p class="consejo-texto">Vas a gastar aprox. <strong class="hl">{{ pctGastoProyectado }}%</strong> de tus ingresos registrados. Controla los gastos ocasionales (el porcentaje puede bajar si aún faltan ingresos por registrar).</p>
          </template>
          <template v-else>
            <p class="consejo-title">Vas camino a un déficit</p>
            <p class="consejo-texto">Vas a gastar aprox. <strong class="hl">{{ pctGastoProyectado }}%</strong> de tus ingresos registrados. Si aún falta cargar ingresos del mes, el porcentaje debería bajar — de lo contrario, reduce tus gastos ocasionales los próximos días para cerrar en positivo.</p>
          </template>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import {
  ChartLine, CalendarClock, CalendarDays, Info, Receipt, Banknote, PiggyBank, Percent,
  TrendingUp, TrendingDown, Flame, TriangleAlert, Check,
} from '@lucide/vue'
import { useFinanceStore } from '@/stores/finance'
import { fmtCOP as fmt } from '@/utils'
import MonthSelector from '@/components/common/MonthSelector.vue'

const store   = useFinanceStore()
const resumen = computed(() => store.resumen)

onMounted(() => { if (!store.resumen) store.cargarDatos() })

const hoy       = new Date()
const mesHoy    = hoy.getMonth() + 1
const anioHoy   = hoy.getFullYear()
const diaActual = hoy.getDate()

const diasDelMes = computed(() => {
  return new Date(store.anioActual, store.mesActual, 0).getDate()
})
const diasRestantes = computed(() => Math.max(diasDelMes.value - diaActual, 0))
const pctMes = computed(() => Math.round((diaActual / diasDelMes.value) * 100))

const esFuturo = computed(() =>
  store.anioActual > anioHoy ||
  (store.anioActual === anioHoy && store.mesActual > mesHoy)
)
const esPasado = computed(() =>
  store.anioActual < anioHoy ||
  (store.anioActual === anioHoy && store.mesActual < mesHoy)
)

const tasaDiaria = computed(() => {
  if (!resumen.value || diaActual === 0) return 0
  return resumen.value.totalGastos / diaActual
})

// Los gastos fijos (arriendo, suscripciones) normalmente ya quedan completos
// al pagarse una vez en el mes, así que no se extrapolan por día — solo los
// variables, que sí se acumulan progresivamente.
const gastoProyectado = computed(() => {
  if (!resumen.value || diaActual === 0) return resumen.value?.totalGastos ?? 0
  const tasaDiariaVariable = resumen.value.totalVariables / diaActual
  const variableProyectado = Math.round(tasaDiariaVariable * diasDelMes.value)
  return resumen.value.totalFijos + variableProyectado
})

const gastoRestanteProyectado = computed(() =>
  Math.max(gastoProyectado.value - (resumen.value?.totalGastos ?? 0), 0)
)

const ahorroProyectado = computed(() =>
  (resumen.value?.totalIngresos ?? 0) - gastoProyectado.value
)

const margenSinGastar = computed(() =>
  Math.max((resumen.value?.totalIngresos ?? 0) - gastoProyectado.value, 0)
)

// Base de la barra: los ingresos (o lo proyectado si te pasas, para que quepa todo)
const baseBarra = computed(() =>
  Math.max(resumen.value?.totalIngresos ?? 0, gastoProyectado.value, 1)
)
const pctBarra = (n: number) => Math.min((n / baseBarra.value) * 100, 100)

const sinIngresos = computed(() => (resumen.value?.totalIngresos ?? 0) === 0)

const estadoConsejo = computed(() => {
  const ingresos = resumen.value?.totalIngresos ?? 0
  if (ingresos === 0) return 'bad'
  const ratio = gastoProyectado.value / ingresos
  if (ratio > 1)    return 'bad'
  if (ratio > 0.85) return 'warn'
  return 'ok'
})

const pctGastoProyectado = computed(() => {
  const ing = resumen.value?.totalIngresos ?? 0
  return ing > 0 ? Math.round((gastoProyectado.value / ing) * 100) : 0
})
const pctAhorroProyectado = computed(() => {
  const ing = resumen.value?.totalIngresos ?? 0
  return ing > 0 ? Math.round((ahorroProyectado.value / ing) * 100) : 0
})
</script>

<style scoped>
.proyeccion { font-variant-numeric: tabular-nums; }
.header-left { display: flex; align-items: flex-end; gap: 1.25rem; flex-wrap: wrap; }

.contexto-banner {
  display: flex; align-items: center; gap: 0.5rem;
  font-size: 0.76rem; color: var(--text-secondary);
  background: var(--surface2);
  border-radius: 10px; padding: 0.65rem 0.95rem; margin-bottom: 1rem;
}
.contexto-banner svg { color: var(--accent-strong); }

/* ── Tiempo ── */
.tiempo-bar {
  display: flex; flex-direction: column; gap: 0.65rem;
  margin-bottom: 1rem; padding: 1rem 1.25rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
}
.tiempo-info  { display: flex; align-items: baseline; gap: 0.5rem; }
.tiempo-label { font-size: 0.86rem; font-weight: 700; color: var(--text-primary); }
.tiempo-resta { font-size: 0.78rem; color: var(--text-subtle); }
.tiempo-pct   { margin-left: auto; font-family: var(--ff-mono); font-size: 0.74rem; font-weight: 700; color: var(--text-primary); }
.tiempo-track { height: 6px; border-radius: 99px; overflow: hidden; background: color-mix(in srgb, var(--text-muted) 22%, var(--surface2)); }
.tiempo-fill  { height: 100%; border-radius: 99px; background: var(--c-need); transition: width 0.4s ease; }

/* ── KPIs ── */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.85rem; margin-bottom: 1rem; }
.kpi-card {
  display: flex; flex-direction: column; gap: 0.35rem;
  padding: 1.1rem 1.25rem 1.2rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
}
.kpi-top   { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; margin-bottom: 0.4rem; }
.kpi-label { font-size: 0.7rem; font-weight: 500; color: var(--text-subtle); letter-spacing: 0.06em; text-transform: uppercase; }
.kpi-icon {
  --ic: var(--accent-strong);
  display: grid; place-items: center; flex-shrink: 0;
  width: 30px; height: 30px; border-radius: 9px;
  color: var(--ic); background: color-mix(in srgb, var(--ic) 13%, transparent);
}
.kpi-icon--pos { --ic: var(--c-pos); }
.kpi-val {
  font-family: var(--ff-mono);
  font-size: 1.4rem; font-weight: 700; letter-spacing: -0.02em;
  color: var(--text-primary);
}
.kpi-val.is-pos  { color: var(--c-pos); }
.kpi-val.is-warn { color: var(--c-want); }
.kpi-val.is-neg  { color: var(--c-neg); }

/* ── Ritmo de gasto ── */
.burn-card {
  display: flex; flex-direction: column; gap: 1rem;
  margin-bottom: 1rem; padding: 1.25rem 1.35rem 1.35rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
}
.burn-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; }
.burn-title {
  font-family: var(--ff-body);
  font-size: 0.78rem; font-weight: 500; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--text-primary);
}
.burn-sub { font-size: 0.7rem; color: var(--text-muted); margin-top: 0.25rem; }
.burn-rate {
  display: inline-flex; align-items: center; gap: 0.35rem; flex-shrink: 0;
  padding: 0.4rem 0.75rem; border-radius: 10px;
  background: var(--surface2); border: 1px solid var(--border);
  font-family: var(--ff-mono); font-size: 0.86rem; font-weight: 700; color: var(--text-primary);
}
.burn-rate svg { color: var(--c-want); }
.burn-rate small { font-size: 0.7rem; font-weight: 400; color: var(--text-subtle); }

.burn-legend { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.legend-pill {
  --lc: var(--c-need);
  display: inline-flex; align-items: center; gap: 0.45rem;
  padding: 0.35rem 0.7rem; border-radius: 999px;
  font-size: 0.72rem; color: var(--text-secondary);
  background: var(--surface2);
}
.legend-pill strong { font-family: var(--ff-mono); font-weight: 700; color: var(--text-primary); }
.legend-pill .dot { width: 10px; height: 10px; border-radius: 3px; background: var(--lc); flex-shrink: 0; }
.legend-pill--proj .dot {
  background: repeating-linear-gradient(135deg, var(--text-secondary) 0 2px, transparent 2px 4px);
  border: 1px solid var(--text-secondary);
}
.legend-pill--margin .dot { background: transparent; border: 1px solid var(--border-hover); }

.burn-viz {
  position: relative;
  display: flex;
  height: 16px;
  border-radius: 99px;
  background: color-mix(in srgb, var(--text-muted) 16%, var(--surface2));
}
.burn-seg { height: 100%; transition: width 0.5s cubic-bezier(0.2, 0, 0, 1); }
.burn-seg:first-child { border-radius: 99px 0 0 99px; }
.burn-real { background: var(--c-need); }
.burn-proj {
  border-radius: 0 99px 99px 0;
  background:
    repeating-linear-gradient(135deg, color-mix(in srgb, var(--text-secondary) 55%, transparent) 0 3px, transparent 3px 7px),
    color-mix(in srgb, var(--text-secondary) 18%, transparent);
}
.burn-ingreso {
  position: absolute; top: -6px; bottom: -6px;
  width: 2px; background: var(--c-neg); border-radius: 2px;
  transform: translateX(-1px);
}
.burn-ingreso span {
  position: absolute; top: calc(100% + 4px); left: 50%; transform: translateX(-50%);
  font-size: 0.62rem; font-weight: 500; color: var(--c-neg); white-space: nowrap;
}

.burn-alerta {
  display: flex; align-items: center; gap: 0.5rem;
  margin-top: 0.6rem; padding: 0.6rem 0.85rem; border-radius: 10px;
  font-size: 0.76rem; color: var(--c-neg);
  background: color-mix(in srgb, var(--c-neg) 10%, transparent);
}
.burn-alerta strong { font-family: var(--ff-mono); }

/* ── Consejo ── */
.consejo {
  --cc: var(--c-pos);
  display: flex; align-items: flex-start; gap: 0.85rem;
  padding: 1rem 1.15rem; border-radius: 14px;
  background: color-mix(in srgb, var(--cc) 10%, transparent);
}
.consejo--warn { --cc: var(--c-want); }
.consejo--bad  { --cc: var(--c-neg); }
.consejo-icon {
  display: grid; place-items: center; flex-shrink: 0;
  width: 30px; height: 30px; border-radius: 999px;
  color: #fff; background: var(--cc);
  box-shadow: 0 0 0 4px color-mix(in srgb, var(--cc) 18%, transparent);
}
.consejo-body  { display: flex; flex-direction: column; gap: 0.2rem; padding-top: 0.15rem; }
.consejo-title { font-size: 0.86rem; font-weight: 700; color: var(--text-primary); }
.consejo-texto { font-size: 0.76rem; color: var(--text-secondary); line-height: 1.55; }
.consejo-texto .hl { font-family: var(--ff-mono); font-weight: 700; color: var(--cc); }

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
  height: 100px; border-radius: var(--radius-card);
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 900px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 500px) {
  .header-left { width: 100%; }
  .month-selector { width: 100%; }
  .month-selector :deep(.label) { flex: 1; }
  .burn-header { flex-direction: column; }
  .tiempo-info { flex-wrap: wrap; }
}
</style>
