<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Análisis</div>
        <h1 class="page-title">Proyección de cierre</h1>
      </div>
      <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
    </div>

    <div v-if="store.loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="!resumen">
      <p class="empty-state">Sin datos para este período.</p>
    </template>

    <!-- Mes futuro -->
    <template v-else-if="esFuturo">
      <p class="empty-state">Selecciona el mes actual o un mes pasado para ver la proyección.</p>
    </template>

    <!-- Mes pasado — resultados reales -->
    <template v-else-if="esPasado">
      <div class="contexto-banner">
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
        Mes cerrado — mostrando resultados reales
      </div>
      <div class="kpi-grid">
        <div class="kpi-card">
          <span class="kpi-lbl">Gasto real</span>
          <span class="kpi-v">{{ fmt(resumen.totalGastos) }}</span>
        </div>
        <div class="kpi-card">
          <span class="kpi-lbl">Ingresos</span>
          <span class="kpi-v">{{ fmt(resumen.totalIngresos) }}</span>
        </div>
        <div class="kpi-card">
          <span class="kpi-lbl">Ahorro</span>
          <span class="kpi-v" :style="{ color: resumen.ahorro >= 0 ? 'var(--accent)' : 'var(--red)' }">
            {{ resumen.ahorro >= 0 ? '+' : '' }}{{ fmt(resumen.ahorro) }}
          </span>
        </div>
        <div class="kpi-card">
          <span class="kpi-lbl">% Ahorro</span>
          <span class="kpi-v" :style="{ color: resumen.pctAhorro >= 20 ? 'var(--accent)' : resumen.pctAhorro >= 10 ? '#f59e0b' : 'var(--red)' }">
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
          <span class="tiempo-resta">Faltan {{ diasRestantes }} día{{ diasRestantes !== 1 ? 's' : '' }}</span>
        </div>
        <div class="tiempo-track">
          <div class="tiempo-fill" :style="{ width: pctMes + '%' }" />
        </div>
        <span class="tiempo-pct">{{ pctMes }}% del mes</span>
      </div>

      <!-- KPIs proyección -->
      <div class="kpi-grid">
        <div class="kpi-card">
          <span class="kpi-lbl">Gastado hasta hoy</span>
          <span class="kpi-v">{{ fmt(resumen.totalGastos) }}</span>
        </div>
        <div class="kpi-card">
          <span class="kpi-lbl">Proyección fin de mes</span>
          <span class="kpi-v" :style="{ color: gastoProyectado > resumen.totalIngresos ? 'var(--red)' : '' }">{{ fmt(gastoProyectado) }}</span>
        </div>
        <div class="kpi-card">
          <span class="kpi-lbl">Ingresos</span>
          <span class="kpi-v">{{ fmt(resumen.totalIngresos) }}</span>
        </div>
        <div class="kpi-card">
          <span class="kpi-lbl">Ahorro proyectado</span>
          <span class="kpi-v" :style="{ color: ahorroProyectado >= 0 ? 'var(--accent)' : 'var(--red)' }">
            {{ ahorroProyectado >= 0 ? '+' : '' }}{{ fmt(ahorroProyectado) }}
          </span>
        </div>
      </div>

      <!-- Barra de quemado -->
      <div class="burn-card">
        <div class="burn-header">
          <span class="burn-title">Ritmo de gasto</span>
          <span class="burn-rate">{{ fmt(tasaDiaria) }}/día</span>
        </div>

        <div class="burn-viz">
          <!-- Gastado -->
          <div class="burn-seg burn-real" :style="{ flex: resumen.totalGastos }" />
          <!-- Proyección restante -->
          <div class="burn-seg burn-proj" :style="{ flex: gastoRestanteProyectado }" />
          <!-- Margen de ahorro -->
          <div v-if="margenSinGastar > 0" class="burn-seg burn-margin" :style="{ flex: margenSinGastar }" />
        </div>

        <div class="burn-legend">
          <span><span class="dot dot-real" />Gastado {{ fmt(resumen.totalGastos) }}</span>
          <span><span class="dot dot-proj" />Proyectado {{ fmt(gastoRestanteProyectado) }}</span>
          <span v-if="margenSinGastar > 0"><span class="dot dot-margin" />Margen {{ fmt(margenSinGastar) }}</span>
        </div>

        <!-- Línea de ingresos -->
        <div class="burn-footer">
          <div class="burn-ingreso-mark" :style="{ left: pctIngreso + '%' }">
            <div class="bim-line" />
            <span class="bim-label">Ingresos</span>
          </div>
        </div>

        <!-- Alerta si proyección supera ingresos -->
        <div v-if="gastoProyectado > resumen.totalIngresos" class="burn-alerta">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
          Al ritmo actual terminarás el mes con déficit de {{ fmt(Math.abs(ahorroProyectado)) }}
        </div>
      </div>

      <!-- Consejo -->
      <div class="consejo-card" :class="'consejo-' + estadoConsejo">
        <span class="consejo-icon">{{ iconoConsejo }}</span>
        <p class="consejo-texto">{{ textoConsejo }}</p>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
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

const gastoProyectado = computed(() =>
  Math.round(tasaDiaria.value * diasDelMes.value)
)

const gastoRestanteProyectado = computed(() =>
  Math.max(gastoProyectado.value - (resumen.value?.totalGastos ?? 0), 0)
)

const ahorroProyectado = computed(() =>
  (resumen.value?.totalIngresos ?? 0) - gastoProyectado.value
)

const margenSinGastar = computed(() =>
  Math.max((resumen.value?.totalIngresos ?? 0) - gastoProyectado.value, 0)
)

const pctIngreso = computed(() => {
  const total = gastoProyectado.value + margenSinGastar.value
  return total > 0 ? Math.min((( resumen.value?.totalIngresos ?? 0) / total) * 100, 100) : 100
})

const estadoConsejo = computed(() => {
  const ratio = gastoProyectado.value / (resumen.value?.totalIngresos ?? 1)
  if (ratio > 1)    return 'bad'
  if (ratio > 0.85) return 'warn'
  return 'ok'
})

const iconoConsejo = computed(() => ({ ok: '✓', warn: '!', bad: '⚠' }[estadoConsejo.value]))

const textoConsejo = computed(() => {
  if (!resumen.value) return ''
  const pct = Math.round((gastoProyectado.value / resumen.value.totalIngresos) * 100)
  if (estadoConsejo.value === 'bad')
    return `Vas a gastar aprox. ${pct}% de tus ingresos. Reduce tus gastos variables los próximos días para cerrar en positivo.`
  if (estadoConsejo.value === 'warn')
    return `Vas a gastar aprox. ${pct}% de tus ingresos. Estás cerca del límite — controla los gastos variables.`
  return `Vas muy bien. Proyección de ahorro: ${fmt(ahorroProyectado.value)} (${Math.round((ahorroProyectado.value / resumen.value.totalIngresos) * 100)}% del ingreso).`
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.empty-state { text-align: center; padding: 3rem 1rem; color: var(--text-muted); font-size: 0.82rem; }

.contexto-banner {
  display: flex; align-items: center; gap: 0.5rem;
  font-size: 0.72rem; color: var(--text-muted);
  background: var(--surface); border: 1px solid var(--border);
  border-radius: 6px; padding: 0.5rem 0.85rem; margin-bottom: 1rem;
}

/* Tiempo */
.tiempo-bar  { background: var(--surface); border: 1px solid var(--border); border-radius: 8px; padding: 0.85rem 1rem; margin-bottom: 1rem; display: flex; flex-direction: column; gap: 0.4rem; }
.tiempo-info { display: flex; justify-content: space-between; }
.tiempo-label { font-size: 0.72rem; font-weight: 600; }
.tiempo-resta { font-size: 0.68rem; color: var(--text-muted); }
.tiempo-track { height: 6px; background: var(--border); border-radius: 3px; overflow: hidden; }
.tiempo-fill  { height: 100%; background: var(--accent); border-radius: 3px; transition: width 0.4s; }
.tiempo-pct   { font-size: 0.62rem; color: var(--text-muted); text-align: right; }

/* KPIs */
.kpi-grid { display: grid; grid-template-columns: repeat(4,1fr); gap: 0.75rem; margin-bottom: 1rem; }

/* Burn card */
.burn-card {
  background: var(--surface); border: 1px solid var(--border); border-radius: 10px;
  padding: 1rem 1.1rem; margin-bottom: 0.75rem;
  display: flex; flex-direction: column; gap: 0.65rem;
}
.burn-header { display: flex; justify-content: space-between; align-items: center; }
.burn-title  { font-size: 0.72rem; font-weight: 600; }
.burn-rate   { font-size: 0.72rem; color: var(--text-muted); }

.burn-viz { display: flex; height: 12px; border-radius: 6px; overflow: hidden; gap: 2px; }
.burn-seg { min-width: 2px; border-radius: 4px; }
.burn-real   { background: var(--accent); }
.burn-proj   { background: rgba(99,179,255,0.3); }
.burn-margin { background: #34d399; opacity: 0.4; }

.burn-legend { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.burn-legend span { display: flex; align-items: center; gap: 0.3rem; font-size: 0.62rem; color: var(--text-muted); }
.dot { width: 7px; height: 7px; border-radius: 50%; display: inline-block; }
.dot-real   { background: var(--accent); }
.dot-proj   { background: rgba(99,179,255,0.4); }
.dot-margin { background: #34d399; opacity: 0.6; }

.burn-footer { position: relative; height: 20px; }
.burn-ingreso-mark { position: absolute; transform: translateX(-50%); display: flex; flex-direction: column; align-items: center; gap: 1px; }
.bim-line  { width: 1px; height: 10px; background: var(--text-muted); }
.bim-label { font-size: 0.55rem; color: var(--text-muted); white-space: nowrap; }

.burn-alerta {
  display: flex; align-items: center; gap: 0.4rem;
  font-size: 0.68rem; color: var(--red);
  background: rgba(248,113,113,0.08); border: 1px solid rgba(248,113,113,0.2);
  border-radius: 6px; padding: 0.4rem 0.7rem;
}

/* Consejo */
.consejo-card {
  display: flex; align-items: flex-start; gap: 0.75rem;
  border-radius: 8px; padding: 0.9rem 1rem;
  border: 1px solid var(--border);
}
.consejo-ok   { background: rgba(52,211,153,0.06); border-color: rgba(52,211,153,0.3); }
.consejo-warn { background: rgba(245,158,11,0.06); border-color: rgba(245,158,11,0.3); }
.consejo-bad  { background: rgba(248,113,113,0.06); border-color: rgba(248,113,113,0.3); }

.consejo-icon { font-size: 1rem; flex-shrink: 0; margin-top: 0.1rem; }
.consejo-ok   .consejo-icon { color: #34d399; }
.consejo-warn .consejo-icon { color: #f59e0b; }
.consejo-bad  .consejo-icon { color: #f87171; }
.consejo-texto { font-size: 0.75rem; color: var(--text-secondary); line-height: 1.5; margin: 0; }

/* Loading */
.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 100px; border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }
</style>
