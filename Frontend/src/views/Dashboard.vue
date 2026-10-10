<template>
  <div class="fade-up">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="section-label">Resumen</div>
        <h1 class="page-title">Dashboard</h1>
      </div>
      <div style="display:flex;align-items:center;gap:0.5rem">
        <button class="btn btn-sm" :disabled="descargandoPDF" @click="descargarPDF">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-right:4px"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          {{ descargandoPDF ? 'Generando…' : 'Generar PDF' }}
        </button>
        <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
      </div>
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
          <div class="kpi-value-row">
            <span class="kpi-value">{{ fmtAnim(counters.ingresos) }}</span>
            <span v-if="variacion?.ingresos" class="kpi-var" :class="variacion.ingresos.esBueno ? 'kpi-var--good' : 'kpi-var--bad'">{{ variacion.ingresos.v >= 0 ? '▲' : '▼' }}{{ Math.abs(variacion.ingresos.v) }}%</span>
          </div>
        </div>
        <div class="kpi-card" style="--ca: var(--red)">
          <div class="kpi-top">
            <span class="kpi-label">Gastos fijos</span>
            <svg class="kpi-icon" style="color:var(--red)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
          </div>
          <div class="kpi-value-row">
            <span class="kpi-value">{{ fmtAnim(counters.fijos) }}</span>
            <span v-if="variacion?.fijos" class="kpi-var" :class="variacion.fijos.esBueno ? 'kpi-var--good' : 'kpi-var--bad'">{{ variacion.fijos.v >= 0 ? '▲' : '▼' }}{{ Math.abs(variacion.fijos.v) }}%</span>
          </div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: pcts.fijos + '%', background: 'var(--red)' }" />
          </div>
          <div class="kpi-pct">
            <span class="kpi-pct-badge kpi-pct-badge--red">{{ Math.round(pcts.fijos) }}%</span> del ingreso
          </div>
        </div>
        <div class="kpi-card" style="--ca: var(--amber)">
          <div class="kpi-top">
            <span class="kpi-label">Gastos ocasionales</span>
            <svg class="kpi-icon" style="color:var(--amber)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3h18v18H3z"/><path d="M3 9h18M9 21V9"/></svg>
          </div>
          <div class="kpi-value-row">
            <span class="kpi-value">{{ fmtAnim(counters.variables) }}</span>
            <span v-if="variacion?.variables" class="kpi-var" :class="variacion.variables.esBueno ? 'kpi-var--good' : 'kpi-var--bad'">{{ variacion.variables.v >= 0 ? '▲' : '▼' }}{{ Math.abs(variacion.variables.v) }}%</span>
          </div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: pcts.variables + '%', background: 'var(--amber)' }" />
          </div>
          <div class="kpi-pct">
            <span class="kpi-pct-badge kpi-pct-badge--amber">{{ Math.round(pcts.variables) }}%</span> del ingreso
          </div>
        </div>
        <div class="kpi-card kpi-ahorro" style="--ca: var(--green)">
          <div class="kpi-top">
            <span class="kpi-label">Dinero restante del mes</span>
            <svg class="kpi-icon" style="color:var(--green)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2a10 10 0 1 0 10 10"/><path d="M12 6v6l4 2"/></svg>
          </div>
          <div class="kpi-value-row">
            <span class="kpi-value">{{ fmtAnim(counters.ahorro) }}</span>
            <span v-if="variacion?.ahorro" class="kpi-var" :class="variacion.ahorro.esBueno ? 'kpi-var--good' : 'kpi-var--bad'">{{ variacion.ahorro.v >= 0 ? '▲' : '▼' }}{{ Math.abs(variacion.ahorro.v) }}%</span>
          </div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: pcts.ahorro + '%', background: 'var(--green)' }" />
          </div>
          <div class="kpi-pct">
            <span class="kpi-pct-badge kpi-pct-badge--green">{{ Math.round(pcts.ahorro) }}%</span> del ingreso
          </div>
        </div>
      </div>

      <!-- Ahorro anual banner -->
      <div ref="bannerRef" class="anual-banner" style="opacity: 0">
          <div class="anual-left">
            <template v-if="store.cuentas.length > 0">
              <div class="kpi-label" style="margin-bottom: 0.5rem">Total en cuentas</div>
              <div class="anual-big" :class="{ 'anual-big--neg': totalCuentas < 0 }">{{ fmt(totalCuentas) }}</div>
              <div class="anual-meta">
                en {{ store.cuentas.length }} cuenta{{ store.cuentas.length !== 1 ? 's' : '' }} ·
                <button type="button" class="anual-ahorro-btn" @click="mostrarDesglose = true">
                  ahorro {{ store.anioActual }}: {{ fmt(store.resumenAnual?.ahorro) }} ({{ store.resumenAnual?.pctAhorro ?? 0 }}%) ⓘ
                </button>
              </div>
            </template>
            <template v-else>
              <div class="kpi-label" style="margin-bottom: 0.5rem">Ahorro {{ store.anioActual }}</div>
              <div class="anual-big">{{ fmtAnim(counters.ahorroAnual) }}</div>
              <div class="anual-meta">de {{ fmt(store.resumenAnual?.totalIngresos) }} en ingresos este año</div>
              <RouterLink to="/cuentas" class="anual-cta">Crea tus cuentas para ver tu dinero real →</RouterLink>
            </template>
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

      <!-- Desglose del ahorro anual -->
      <Teleport to="body">
        <div v-if="mostrarDesglose" class="dd-overlay" @click.self="mostrarDesglose = false">
          <div class="dd-modal">
            <p class="dd-title">Cómo se calcula el ahorro {{ store.anioActual }}</p>
            <p class="dd-sub">Suma de (ingresos − gastos) de cada mes que tiene ingresos registrados. No incluye saldos de cuentas, saldo inicial ni préstamos.</p>
            <div class="dd-table-wrap">
              <table class="dd-table">
                <thead>
                  <tr><th>Mes</th><th>Ingresos</th><th>Gastos</th><th>Ahorro</th></tr>
                </thead>
                <tbody>
                  <tr v-for="r in mesesConIngreso" :key="r.mes">
                    <td>{{ MESES[r.mes - 1] }}</td>
                    <td class="dd-in">{{ fmt(r.totalIngresos) }}</td>
                    <td class="dd-out">−{{ fmt(r.totalGastos) }}</td>
                    <td :class="r.ahorro >= 0 ? 'dd-in' : 'dd-out'">{{ fmt(r.ahorro) }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr>
                    <td>Total</td>
                    <td class="dd-in">{{ fmt(desgloseIngresos) }}</td>
                    <td class="dd-out">−{{ fmt(desgloseGastos) }}</td>
                    <td :class="desgloseAhorro >= 0 ? 'dd-in' : 'dd-out'">{{ fmt(desgloseAhorro) }}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
            <div v-if="mesesExcluidos.length > 0" class="dd-note">
              <strong>No cuentan todavía</strong> (tienen gasto pero aún no hay ingreso ese mes):
              {{ mesesExcluidos.map(r => `${MESES[r.mes - 1]} −${fmt(r.totalGastos)}`).join(' · ') }}
            </div>
            <button class="btn" style="align-self:flex-end" @click="mostrarDesglose = false">Cerrar</button>
          </div>
        </div>
      </Teleport>

      <!-- Análisis con IA -->
      <div class="card insights-card" style="margin-bottom: 1rem">
        <div class="insights-header">
          <p class="card-title" style="margin-bottom: 0">Análisis con IA</p>
          <button class="btn btn-sm" :disabled="cargandoInsights" @click="generarInsights">
            {{ cargandoInsights ? 'Analizando…' : (insights ? 'Actualizar análisis' : 'Analizar mi mes') }}
          </button>
        </div>
        <div v-if="insights" class="insights-lineas">
          <div v-for="(linea, i) in insightsLineas" :key="i" class="insights-linea">
            <svg class="insights-icon" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 18h6M10 22h4M15.09 14c.18-1 .48-1.6 1.14-2.3A6 6 0 1 0 7.77 11.7c.66.7.96 1.3 1.14 2.3"/></svg>
            <span>{{ linea }}</span>
          </div>
        </div>
        <p v-else-if="insightsNoDisponible" class="insights-empty">No hay suficientes datos este mes para generar el análisis.</p>
        <p v-else-if="insightsError" class="insights-empty">No se pudo generar el análisis.</p>
        <p v-else class="insights-empty">Analiza tus hábitos de gasto este mes con IA.</p>
      </div>

     <div class="two-bottom-grid">
        <!-- Regla 50/30/20 -->
        <div v-if="regla5030.length > 0" class="card" style="margin-bottom: 1rem">
          <p class="card-title">Regla 50/30/20</p>
          <div class="regla-rows">
            <div v-for="r in regla5030" :key="r.label" class="regla-row">
              <div class="regla-top">
                <span class="regla-label">{{ r.label }}</span>
                <span class="regla-vals">{{ r.pct }}%<span class="regla-meta"> · meta {{ r.meta }}%</span></span>
              </div>
              <div class="regla-bar-wrap">
                <div class="regla-bar" :style="{ width: Math.min(r.pct, 100) + '%', background: r.color }" />
                <div class="regla-goal" :style="{ left: r.meta + '%' }" />
              </div>
            </div>
          </div>
        </div>

        <!-- Alertas de deudas -->
        <DeudasAlerta style="margin-bottom: 1rem" />
     </div>

      <!-- Alertas de presupuesto -->
      <div v-if="alertasPresupuesto.length > 0" class="presupuesto-alerts">
        <div
          v-for="a in alertasPresupuesto"
          :key="a.categoria"
          class="presupuesto-alert"
          :class="a.pct >= 100 ? 'alert-danger' : 'alert-warn'"
        >
          <div class="alert-top">
            <span class="alert-cat">{{ a.categoria }}</span>
            <span class="alert-pct-badge" :class="a.pct >= 100 ? 'alert-pct-badge--red' : 'alert-pct-badge--amber'">{{ a.pct }}%</span>
          </div>
          <div class="alert-bar-wrap">
            <div class="alert-bar" :style="{ width: Math.min(a.pct, 100) + '%' }" />
          </div>
          <div class="alert-nums">{{ fmt(a.gastado) }} de {{ fmt(a.limite) }}</div>
        </div>
      </div>

      <!-- Tendencia del año -->
      <div class="card" style="margin-bottom: 1rem">
        <p class="card-title">Tendencia {{ store.anioActual }}</p>
        <BarChart :labels="tendenciaLabels" :datasets="tendenciaDatasets" />
      </div>

      <!-- Heatmap de gastos -->
      <div v-if="heatmapData.length > 0" class="card" style="margin-bottom: 1rem">
        <p class="card-title">Mapa de gastos {{ store.anioActual }}</p>
        <GastoHeatmap :data="heatmapData" :anio="store.anioActual" />
      </div>

      <!-- Gasto por categoría anual -->
      <div v-if="categoriaDatasets.length > 0" class="card" style="margin-bottom: 1rem">
        <p class="card-title">Gasto por categoría {{ store.anioActual }}</p>
        <BarChart :labels="tendenciaLabels" :datasets="categoriaDatasets.map(d => ({ ...d, color: d.color || '#000' }))" />
      </div>

      <!-- 3 columnas: donut | fijos | variables -->
      <div ref="bottomRef" class="bottom-grid">
        <div class="card" style="opacity: 0; max-height: 400px; overflow-y: auto">
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

        <div class="card" style="opacity: 0; max-height: 400px; overflow-y: auto">
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

        <div class="card" style="opacity: 0; max-height: 400px; overflow-y: auto">
          <p class="card-title">Gastos ocasionales <span class="tag tag-amber">{{ fmt(resumen?.totalVariables) }}</span></p>
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

      <div class="two-bottom-grid">
        <!-- Top categorías -->
        <div v-if="topCategorias.length > 0" class="card" style="margin-top: 0.75rem">
          <p class="card-title">Top categorías este mes</p>
          <div class="top-cat-list">
            <div v-for="(cat, i) in topCategorias" :key="cat.nombre" class="top-cat-row">
              <div class="top-cat-left">
                <span class="top-cat-rank">#{{ i + 1 }}</span>
                <span class="top-cat-nombre">{{ cat.nombre }}</span>
              </div>
              <div class="top-cat-right">
                <div class="top-cat-bar-wrap">
                  <div class="top-cat-bar" :style="{ width: (cat.total / maxCategoria * 100) + '%' }" />
                </div>
                <span class="top-cat-monto">{{ fmt(cat.total) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Suscripciones olvidadas -->
        <div v-if="suscripciones.length > 0" class="card" style="margin-top: 0.75rem">
          <p class="card-title">¿Sigues usando esto?</p>
          <div class="susc-list">
            <div v-for="s in suscripciones" :key="s.categoria + s.detalle + s.monto" class="susc-row">
              <div>
                <div class="susc-nombre">{{ s.categoria }}<span v-if="s.detalle"> · {{ s.detalle }}</span></div>
                <div class="susc-meses">Llevas {{ s.mesesActivo }} {{ s.mesesActivo === 1 ? 'mes' : 'meses' }} pagando esto</div>
              </div>
              <div class="susc-monto">{{ fmt(s.monto) }}</div>
            </div>
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
import { computed, ref, reactive, watch, onMounted } from 'vue'
import { animate, stagger } from 'motion'
import { useFinanceStore } from '@/stores/finance'
import MonthSelector from '@/components/common/MonthSelector.vue'
import DeudasAlerta from '@/components/common/DeudasAlerta.vue'
import DonutChart from '@/components/charts/DonutChart.vue'
import BarChart from '@/components/charts/BarChart.vue'
import GastoHeatmap from '@/components/charts/GastoHeatmap.vue'
import { usePresupuestoMensual } from '@/composables/usePresupuestoMensual'
import { COLOR_PALETTE } from '@/composables/useCategoriaOptions'
import { resumenService, gastosService } from '@/services/api'
import type { Suscripcion, HeatmapDia } from '@/types'

const MESES = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']

const store = useFinanceStore()
const resumen = computed(() => store.resumen)

// Plata real total = suma de saldos de las cuentas (si el usuario tiene cuentas)
const totalCuentas = computed(() => store.cuentas.reduce((s, c) => s + c.saldo, 0))

// Desglose del ahorro anual (qué meses e importes lo forman)
const mostrarDesglose  = ref(false)
const mesesConIngreso  = computed(() => store.tendencia.filter(r => r.totalIngresos > 0))
const mesesExcluidos   = computed(() => store.tendencia.filter(r => r.totalIngresos === 0 && r.totalGastos > 0))
const desgloseIngresos = computed(() => mesesConIngreso.value.reduce((s, r) => s + r.totalIngresos, 0))
const desgloseGastos   = computed(() => mesesConIngreso.value.reduce((s, r) => s + r.totalGastos, 0))
const desgloseAhorro   = computed(() => desgloseIngresos.value - desgloseGastos.value)

const mesDash  = computed(() => store.mesActual)
const anioDash = computed(() => store.anioActual)
const { items: itemsPresupuesto, cargar: cargarPresupuesto } = usePresupuestoMensual(mesDash, anioDash)

// Element refs
const kpiRef    = ref<HTMLElement>()
const bannerRef = ref<HTMLElement>()
const bottomRef = ref<HTMLElement>()

// Animated display values
const counters     = reactive({ ingresos: 0, fijos: 0, variables: 0, ahorro: 0, ahorroAnual: 0 })
const pcts         = reactive({ fijos: 0, variables: 0, ahorro: 0 })
const progressWidth = ref(0)

const distribucion = computed(() => [
  { label: 'Gastos fijos',     val: resumen.value?.totalFijos    ?? 0, color: '#c17165' },
  { label: 'Gastos ocasionales', val: resumen.value?.totalVariables ?? 0, color: '#c2a06b' },
  { label: 'Dinero restante',    val: resumen.value?.ahorro        ?? 0, color: '#5c9678' },
])

const variacion = computed(() => {
  const ant = store.resumenAnterior
  const act = resumen.value
  if (!ant || !act) return null

  const build = (actual: number, anterior: number, masEsMalo: boolean) => {
    if (anterior <= 0) return null
    const v = Math.round((actual - anterior) / anterior * 100)
    return { v, esBueno: masEsMalo ? v <= 0 : v >= 0 }
  }

  return {
    ingresos:  build(act.totalIngresos,  ant.totalIngresos,  false),
    fijos:     build(act.totalFijos,     ant.totalFijos,     true),
    variables: build(act.totalVariables, ant.totalVariables, true),
    ahorro:    build(act.ahorro,         ant.ahorro,         false),
  }
})

const regla5030 = computed(() => {
  const r = resumen.value
  if (!r || r.totalIngresos <= 0) return []
  const pct = (n: number) => Math.round(n / r.totalIngresos * 100)
  return [
    { label: 'Necesidades', pct: pct(r.totalFijos),     meta: 50, color: 'var(--red)' },
    { label: 'Gustos',      pct: pct(r.totalVariables), meta: 30, color: 'var(--amber)' },
    { label: 'Ahorro',      pct: Math.max(0, pct(r.ahorro)), meta: 20, color: 'var(--green)' },
  ]
})

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
  countUp(val.totalIngresos,  1100,   0, v => { counters.ingresos  = v })
  countUp(val.totalFijos,     1100, 100, v => { counters.fijos     = v })
  countUp(val.totalVariables, 1100, 150, v => { counters.variables = v })
  countUp(val.ahorro,         1100, 200, v => { counters.ahorro    = v })

  // 3. KPI mini-bar pct counters
  countUp(pct(val.totalFijos,     total), 1000, 200, v => { pcts.fijos     = v })
  countUp(pct(val.totalVariables, total), 1000, 300, v => { pcts.variables = v })
  countUp(Math.max(0, val.pctAhorro),    1000, 400, v => { pcts.ahorro    = v })

  // 4. Banner slide-in (Motion on single HTMLElement is fine)
  if (bannerRef.value) {
    animate(bannerRef.value, { opacity: [0, 1], x: [-24, 0] } as any, { duration: 0.45, delay: 0.28, ease: 'easeOut' })
  }

  // 5. Bottom grid cards stagger with manual delay offset
  if (bottomRef.value) {
    animate(
      Array.from(bottomRef.value.querySelectorAll(':scope > .card')),
      { opacity: [0, 1], y: [20, 0] } as any,
      { delay: (i: number) => 0.32 + i * 0.12, duration: 0.35, ease: 'easeOut' }
    )
  }

  // 6. Expense rows stagger
  if (bottomRef.value) {
    animate(
      Array.from(bottomRef.value.querySelectorAll('.expense-row')),
      { opacity: [0, 1], x: [-8, 0] } as any,
      { delay: (i: number) => 0.65 + i * 0.035, duration: 0.22, ease: 'easeOut' }
    )
  }
}, { flush: 'post' })

watch(() => store.resumenAnual, (val) => {
  if (!val) return
  countUp(val.ahorro, 1300, 0, v => { counters.ahorroAnual = v })
  setTimeout(() => { progressWidth.value = Math.min(val.pctAhorro ?? 0, 100) }, 500)
}, { flush: 'post' })

const fmt     = (n?: number) => n != null ? '$' + Math.round(n).toLocaleString('es-CO') : '$0'
const fmtAnim = (n: number)  => '$' + Math.round(n).toLocaleString('es-CO')
const pct     = (a?: number, b?: number) => (a && b) ? Math.round(a / b * 100) : 0

const tendenciaLabels   = computed(() => MESES)
const tendenciaDatasets = computed(() => [
  { label: 'Ingresos', data: store.tendencia.map(r => r.totalIngresos), color: 'rgba(91,122,149,0.75)' },
  { label: 'Gastos',   data: store.tendencia.map(r => r.totalGastos),   color: 'rgba(193,113,101,0.75)' },
])

const topCategorias = computed(() => {
  const map: Record<string, number> = {}
  for (const g of store.gastos) {
    map[g.categoria] = (map[g.categoria] ?? 0) + g.monto
  }
  return Object.entries(map)
    .map(([nombre, total]) => ({ nombre, total }))
    .sort((a, b) => b.total - a.total)
    .slice(0, 5)
})

const maxCategoria = computed(() => topCategorias.value[0]?.total ?? 1)

const alertasPresupuesto = computed(() =>
  itemsPresupuesto.value
    .filter(i => i.estado === 'warn' || i.estado === 'danger')
    .map(i => ({ categoria: i.categoria, gastado: i.gastado, limite: i.limite ?? 0, pct: i.pct }))
    .sort((a, b) => b.pct - a.pct)
)

const CAT_COLORS = COLOR_PALETTE

const categoriaDatasets = computed(() =>
  store.gastosPorCategoria.map((c, i) => ({
    label: c.categoria,
    data:  c.datos.map(Number),
    color: CAT_COLORS[i % CAT_COLORS.length],
  }))
)

const suscripciones = ref<Suscripcion[]>([])
const heatmapData   = ref<HeatmapDia[]>([])

async function cargarHeatmap() {
  const { data } = await gastosService.getHeatmap(store.anioActual)
  heatmapData.value = data
}

onMounted(async () => {
  store.cargarDatos()
  store.cargarResumenAnual()
  store.cargarTendencia()
  store.cargarGastosPorCategoria()
  store.cargarCuentas()   // siempre fresco: el saldo pudo cambiar en otra pantalla
  await store.cargarPresupuestos()
  await cargarPresupuesto()
  await cargarHeatmap()

  const { data } = await gastosService.getSuscripciones()
  suscripciones.value = data

  cargarInsightsCache()
})

watch(anioDash, cargarHeatmap)

watch([mesDash, anioDash], cargarPresupuesto)

const descargandoPDF = ref(false)

const insights = ref<string | null>(null)
const cargandoInsights = ref(false)
const insightsNoDisponible = ref(false)
const insightsError = ref(false)

const insightsLineas = computed(() => insights.value?.split('\n').filter(Boolean) ?? [])

function insightsKey(mes: number, anio: number) {
  return `finanzas_insights_${anio}_${mes}`
}

function cargarInsightsCache() {
  insights.value = localStorage.getItem(insightsKey(store.mesActual, store.anioActual))
  insightsNoDisponible.value = false
  insightsError.value = false
}

watch([mesDash, anioDash], cargarInsightsCache)

async function generarInsights() {
  cargandoInsights.value = true
  insightsNoDisponible.value = false
  insightsError.value = false
  try {
    const { data } = await resumenService.getInsights(store.mesActual, store.anioActual)
    if (data.estado === 'ok' && data.texto) {
      insights.value = data.texto
      localStorage.setItem(insightsKey(store.mesActual, store.anioActual), data.texto)
    }
    else if (data.estado === 'sin_datos') insightsNoDisponible.value = true
    else insightsError.value = true
  } catch {
    insightsError.value = true
  } finally {
    cargandoInsights.value = false
  }
}

async function descargarPDF() {
  descargandoPDF.value = true
  try {
    const { data } = await resumenService.getReporte(store.mesActual, store.anioActual)
    const url = URL.createObjectURL(data as Blob)
    const a = document.createElement('a')
    a.href = url
    const meses = ['','Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre']
    a.download = `reporte-${meses[store.mesActual]?.toLowerCase()}-${store.anioActual}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  } finally {
    descargandoPDF.value = false
  }
}
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.btn-sm { padding: 0.3rem 0.7rem; font-size: 0.72rem; display: inline-flex; align-items: center; }

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
  border-left: 3px solid var(--ca, var(--border));
  border-radius: 10px;
  padding: 1rem 1.1rem;
  overflow: hidden;
}
.kpi-ahorro {
  background: linear-gradient(135deg, var(--surface) 40%, color-mix(in srgb, var(--green) 5%, transparent) 100%);
  border-color: color-mix(in srgb, var(--green) 15%, transparent);
  border-left-color: var(--green);
}
.kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
.kpi-label { font-size: 0.62rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.kpi-icon { opacity: 0.7; flex-shrink: 0; }
.kpi-value-row { display: flex; align-items: baseline; gap: 0.4rem; margin-bottom: 0.5rem; }
.kpi-value { font-size: 1.25rem; font-family: var(--ff-display); font-weight: 700; color: var(--text-primary); }
.kpi-var { font-size: 0.62rem; font-weight: 700; }
.kpi-var--good { color: var(--green); }
.kpi-var--bad  { color: var(--red); }
.kpi-bar-wrap { height: 3px; background: var(--surface2); border-radius: 2px; overflow: hidden; margin-bottom: 0.35rem; }
.kpi-bar { height: 100%; border-radius: 2px; max-width: 100%; }
.kpi-pct { font-size: 0.62rem; color: var(--text-muted); display: flex; align-items: center; gap: 0.35rem; }
.kpi-pct-badge { font-size: 0.62rem; font-weight: 700; padding: 0.1rem 0.4rem; border-radius: 4px; }
.kpi-pct-badge--red   { background: color-mix(in srgb, var(--red) 12%, transparent);   color: var(--red); }
.kpi-pct-badge--amber { background: color-mix(in srgb, var(--amber) 14%, transparent); color: var(--amber); }
.kpi-pct-badge--green { background: color-mix(in srgb, var(--green) 12%, transparent); color: var(--green); }

/* Ahorro anual banner */
.anual-banner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 2rem;
  background: var(--surface);
  border: 1px solid color-mix(in srgb, var(--green) 15%, transparent);
  border-left: 3px solid var(--green);
  border-radius: 12px;
  padding: 1.25rem 1.5rem;
  margin-bottom: 1rem;
}
.anual-left { flex-shrink: 0; }
.anual-big { font-size: 2rem; font-family: var(--ff-display); font-weight: 800; color: var(--text-primary); line-height: 1; margin-bottom: 0.35rem; }
.anual-meta { font-size: 0.68rem; color: var(--text-muted); }
.anual-big--neg { color: var(--red); }
.anual-cta {
  display: inline-block; margin-top: 0.7rem; font-size: 0.68rem; font-weight: 600;
  color: var(--accent); text-decoration: none;
}
.anual-cta:hover { text-decoration: underline; }
.anual-ahorro-btn {
  background: none; border: none; padding: 0; cursor: pointer;
  font: inherit; color: var(--text-secondary); text-decoration: underline dotted;
}
.anual-ahorro-btn:hover { color: var(--text-primary); }

/* Desglose ahorro */
.dd-overlay {
  position: fixed; inset: 0; background: rgba(8,12,16,0.72); backdrop-filter: blur(3px);
  display: flex; align-items: center; justify-content: center; z-index: 500; padding: 1rem;
}
.dd-modal {
  background: var(--surface); border: 1px solid var(--border); border-radius: 12px;
  padding: 1.4rem; width: 460px; max-width: 100%; max-height: 85vh; overflow-y: auto;
  display: flex; flex-direction: column; gap: 0.75rem;
}
.dd-title { font-family: var(--ff-display); font-weight: 700; font-size: 1rem; }
.dd-sub   { font-size: 0.68rem; color: var(--text-muted); line-height: 1.4; }
.dd-table-wrap { overflow-x: auto; }
.dd-table { width: 100%; border-collapse: collapse; font-size: 0.74rem; }
.dd-table th, .dd-table td { padding: 0.4rem 0.5rem; text-align: right; white-space: nowrap; }
.dd-table th:first-child, .dd-table td:first-child { text-align: left; }
.dd-table thead th { color: var(--text-muted); font-weight: 600; border-bottom: 1px solid var(--border); }
.dd-table tbody td { border-bottom: 1px solid var(--border); }
.dd-table tfoot td { font-weight: 700; padding-top: 0.55rem; }
.dd-in  { color: var(--green); }
.dd-out { color: var(--red); }
.dd-note {
  font-size: 0.68rem; color: var(--text-secondary); line-height: 1.4;
  background: var(--surface2); border-radius: 8px; padding: 0.6rem 0.75rem;
}
.anual-right { flex: 1; display: flex; flex-direction: column; gap: 0.75rem; align-items: flex-end; }
.anual-stats { display: flex; gap: 1.5rem; }
.anual-stat { display: flex; flex-direction: column; align-items: flex-end; gap: 0.15rem; }
.anual-stat-label { font-size: 0.6rem; color: var(--text-muted); letter-spacing: 0.1em; text-transform: uppercase; }
.anual-stat strong { font-size: 0.82rem; color: var(--text-primary); }
.anual-progress-wrap { width: 100%; }
.anual-progress-track { position: relative; height: 4px; background: var(--surface2); border-radius: 3px; overflow: visible; margin-bottom: 0.3rem; }
.anual-progress-fill { height: 100%; background: linear-gradient(90deg, var(--green), color-mix(in srgb, var(--green) 50%, transparent)); border-radius: 3px; transition: width 1.4s cubic-bezier(0.16, 1, 0.3, 1); }
.anual-progress-goal {
  position: absolute;
  top: -4px; bottom: -4px; left: 20%;
  width: 1px;
  background: color-mix(in srgb, var(--amber) 50%, transparent);
}
.anual-progress-goal::after { content: '20%'; position: absolute; top: -14px; left: 3px; font-size: 0.55rem; color: var(--amber); white-space: nowrap; }
.anual-progress-labels { display: flex; justify-content: space-between; font-size: 0.58rem; color: var(--text-muted); }

.health-badge { font-size: 0.6rem; letter-spacing: 0.12em; text-transform: uppercase; padding: 0.2rem 0.65rem; border-radius: 2px; border: 1px solid; }
.health-green { border-color: color-mix(in srgb, var(--green) 35%, transparent); color: var(--green); background: color-mix(in srgb, var(--green) 5%, transparent); }
.health-amber { border-color: color-mix(in srgb, var(--amber) 35%, transparent); color: var(--amber); background: color-mix(in srgb, var(--amber) 5%, transparent); }
.health-red   { border-color: color-mix(in srgb, var(--red) 35%, transparent);   color: var(--red);   background: color-mix(in srgb, var(--red) 5%, transparent); }

/* Insights con IA */
.insights-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.85rem; }
.insights-lineas { display: flex; flex-direction:row; justify-content: space-between; gap: 0.5rem; }
.insights-linea {
  max-width: 450px;
  display: grid;
  grid-template-columns: auto 1fr;
  align-items: flex-start;
  gap: 0.6rem;
  font-size: 0.78rem;
  color: var(--text-primary);
  line-height: 1.5;
  padding: 0.65rem 0.85rem;
  background: var(--surface2);
  border-left: 3px solid var(--accent);
  border-radius: 8px;
}
.insights-icon { flex-shrink: 0; margin-top: 0.15rem; color: var(--accent); }
.insights-empty { font-size: 0.75rem; color: var(--text-muted); }

/* Regla 50/30/20 */
.regla-rows { display: flex; flex-direction: column; gap: 0.85rem; }
.regla-row { display: flex; flex-direction: column; gap: 0.35rem; }
.regla-top { display: flex; justify-content: space-between; align-items: baseline; }
.regla-label { font-size: 0.72rem; color: var(--text-primary); font-weight: 500; }
.regla-vals { font-size: 0.7rem; color: var(--text-primary); font-weight: 600; }
.regla-meta { font-size: 0.62rem; color: var(--text-muted); font-weight: 400; }
.regla-bar-wrap { position: relative; height: 5px; background: var(--surface2); border-radius: 3px; overflow: visible; }
.regla-bar { height: 100%; border-radius: 3px; max-width: 100%; transition: width 0.5s ease; }
.regla-goal { position: absolute; top: -3px; bottom: -3px; width: 2px; background: var(--text-muted); border-radius: 1px; }

/* 3-col bottom */
.bottom-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.75rem; }

/* 2-col bottom */
.two-bottom-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }

.card-title { font-size: 0.62rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem; justify-content: space-between; }

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

/* Top categorías */
.top-cat-list  { display: flex; flex-direction: column; gap: 0.55rem; }
.top-cat-row   { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
.top-cat-left  { display: flex; align-items: center; gap: 0.5rem; min-width: 160px; }
.top-cat-rank  { font-size: 0.62rem; color: var(--text-muted); width: 18px; flex-shrink: 0; }
.top-cat-nombre { font-size: 0.75rem; color: var(--text-primary); }
.top-cat-right { display: flex; align-items: center; gap: 0.75rem; flex: 1; }
.top-cat-bar-wrap { flex: 1; height: 4px; background: var(--surface2); border-radius: 2px; overflow: hidden; }
.top-cat-bar  { height: 100%; background: var(--accent); border-radius: 2px; transition: width 0.4s ease; }
.top-cat-monto { font-size: 0.72rem; color: var(--text-secondary); min-width: 80px; text-align: right; flex-shrink: 0; }

/* Suscripciones olvidadas */
.susc-list { display: flex; flex-direction: column; }
.susc-row { display: flex; justify-content: space-between; align-items: center; padding: 0.55rem 0; border-bottom: 1px solid var(--border); }
.susc-row:last-child { border-bottom: none; }
.susc-nombre { font-size: 0.75rem; color: var(--text-primary); }
.susc-meses { font-size: 0.62rem; color: var(--text-muted); margin-top: 0.1rem; }
.susc-monto { font-size: 0.75rem; color: var(--amber); font-weight: 600; flex-shrink: 0; margin-left: 0.5rem; }

/* Alertas de presupuesto */
.presupuesto-alerts {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 0.5rem;
  margin-bottom: 1rem;
}
.presupuesto-alert {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0.65rem 0.85rem;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}
.presupuesto-alert.alert-warn   { border-left: 3px solid var(--amber); }
.presupuesto-alert.alert-danger { border-left: 3px solid var(--red); }
.alert-top  { display: flex; justify-content: space-between; align-items: center; }
.alert-cat  { font-size: 0.72rem; color: var(--text-primary); font-weight: 500; }
.alert-pct-badge { font-size: 0.65rem; font-weight: 700; padding: 0.15rem 0.45rem; border-radius: 4px; }
.alert-pct-badge--amber { background: color-mix(in srgb, var(--amber) 14%, transparent); color: var(--amber); }
.alert-pct-badge--red   { background: color-mix(in srgb, var(--red) 14%, transparent);   color: var(--red); }
.alert-bar-wrap { height: 3px; background: var(--surface2); border-radius: 2px; overflow: hidden; }
.alert-warn   .alert-bar { height: 100%; background: var(--amber); border-radius: 2px; }
.alert-danger .alert-bar { height: 100%; background: var(--red);   border-radius: 2px; }
.alert-nums { font-size: 0.62rem; color: var(--text-muted); }

@media (max-width: 768px) {
  .page-header { flex-direction: column; gap: 1rem; align-items: flex-start; margin-bottom: 1.25rem; }
  .page-title { font-size: 1.4rem; }

  .loading-grid { grid-template-columns: repeat(2, 1fr); }
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }

  .anual-banner { flex-direction: column; align-items: flex-start; gap: 1rem; }
  .anual-right { align-items: flex-start; width: 100%; }
  .anual-stats { gap: 1rem; }
  .anual-stat { align-items: flex-start; }
  .anual-progress-wrap { width: 100%; }

  .bottom-grid { grid-template-columns: 1fr; }

  .top-cat-left { min-width: 120px; }
  .top-cat-monto { min-width: 70px; }
}

@media (max-width: 480px) {
  .kpi-grid { grid-template-columns: 1fr; }
  .anual-big { font-size: 1.5rem; }

  .anual-progress-wrap { 
    margin-top: 1rem;
  }

  /* Insights con IA */
.insights-lineas { flex-direction: column; }
}

</style>
