<template>
  <div class="dash fade-up">
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

    <!-- Skeleton: misma forma que el dashboard real para que la carga se sienta inmediata -->
    <div v-if="store.loading" class="dash-skeleton" aria-busy="true" aria-label="Cargando resumen">
      <div class="kpi-grid">
        <div v-for="i in 4" :key="i" class="skeleton sk-kpi">
          <div class="sk-row"><span class="skeleton-line" style="width: 45%" /><span class="sk-icon" /></div>
          <span class="skeleton-line sk-big" style="width: 68%" />
          <span class="skeleton-line sk-bar" />
          <span class="skeleton-line" style="width: 38%" />
        </div>
      </div>
      <div class="skeleton sk-banner">
        <div class="sk-col"><span class="skeleton-line" style="width: 120px" /><span class="skeleton-line sk-huge" style="width: 200px" /></div>
        <div class="sk-col sk-grow"><span class="skeleton-line" style="width: 40%" /><span class="skeleton-line sk-bar" /></div>
        <div class="sk-col"><span class="skeleton-line" style="width: 150px" /><span class="skeleton-line" style="width: 110px" /></div>
      </div>
      <div class="skeleton sk-card">
        <div class="sk-row"><span class="skeleton-line" style="width: 180px" /><span class="skeleton-line sk-btn" /></div>
        <span class="skeleton-line" style="width: 90%" />
        <span class="skeleton-line" style="width: 70%" />
      </div>
      <div class="two-bottom-grid">
        <div v-for="i in 2" :key="i" class="skeleton sk-card">
          <span class="skeleton-line" style="width: 40%" />
          <span v-for="j in 3" :key="j" class="skeleton-line sk-bar" />
        </div>
      </div>
      <div class="skeleton sk-chart">
        <span class="skeleton-line" style="width: 160px" />
        <div class="sk-bars">
          <span v-for="(h, i) in SK_BARRAS" :key="i" class="sk-bar-v" :style="{ height: h + '%' }" />
        </div>
      </div>
    </div>

    <!-- Sin movimientos en el mes: bienvenida (usuario nuevo) o accesos rápidos -->
    <EmptyStart v-else-if="store.resumen && mesVacio" :variante="esUsuarioNuevo ? 'nuevo' : 'mes'" />

    <template v-else-if="store.resumen">
      <!-- KPIs -->
      <div ref="kpiRef" class="kpi-grid" style="opacity: 0">
        <div v-for="k in kpiCards" :key="k.key" class="kpi-card" :style="{ '--ca': k.color }">
          <div class="kpi-top">
            <span class="kpi-label">{{ k.label }}</span>
            <span class="kpi-icon"><component :is="k.icon" :size="15" :stroke-width="2" /></span>
          </div>
          <div class="kpi-value-row">
            <span class="kpi-value">{{ fmtAnim(k.value) }}</span>
            <span
              v-if="k.variacion"
              class="kpi-var"
              :class="k.variacion.esBueno ? 'kpi-var--good' : 'kpi-var--bad'"
              title="Frente al mes anterior"
            >
              <ArrowUpRight v-if="k.variacion.v >= 0" :size="12" :stroke-width="2.5" />
              <ArrowDownRight v-else :size="12" :stroke-width="2.5" />
              {{ Math.abs(k.variacion.v) }}%
            </span>
          </div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: (k.pct ?? 100) + '%' }" />
          </div>
          <div class="kpi-pct">
            <template v-if="k.pct != null"><strong>{{ Math.round(k.pct) }}%</strong> del ingreso</template>
            <template v-else>Base del mes</template>
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
        <div class="anual-center">
          <div class="anual-progress-head">
            <span class="kpi-label">Tasa de ahorro {{ store.anioActual }}</span>
            <span :class="['health-badge', healthClass]" title="Según el ahorro de este mes">{{ healthLabel }}</span>
          </div>
          <div class="anual-progress-track">
            <div class="anual-progress-fill" :style="{ width: progressWidth + '%' }" />
            <div class="anual-progress-goal" title="Meta 20%" />
          </div>
          <div class="anual-progress-labels">
            <span>0%</span>
            <span class="anual-goal-label">Meta 20%</span>
            <span>100%</span>
          </div>
        </div>

        <div class="anual-stats">
          <div class="anual-stat">
            <span class="anual-stat-label">Ingresos</span>
            <strong>{{ fmt(store.resumenAnual?.totalIngresos) }}</strong>
          </div>
          <span class="anual-stat-sep" aria-hidden="true" />
          <div class="anual-stat">
            <span class="anual-stat-label">Gastos</span>
            <strong>{{ fmt(store.resumenAnual?.totalGastos) }}</strong>
          </div>
          <span class="anual-stat-sep" aria-hidden="true" />
          <div class="anual-stat">
            <span class="anual-stat-label">Tasa</span>
            <strong class="anual-stat-pos">{{ store.resumenAnual?.pctAhorro ?? 0 }}%</strong>
          </div>
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
          <div class="insights-title">
            <span class="ai-icon"><Sparkles :size="15" :stroke-width="2" /></span>
            <div>
              <p class="card-title ai-title">Análisis con IA</p>
              <p class="insights-hint">Patrones y recomendaciones de tu mes</p>
            </div>
          </div>
          <button class="btn btn-sm btn-ai" :disabled="cargandoInsights" @click="generarInsights">
            <Sparkles :size="13" :stroke-width="2.2" :class="{ 'ai-spin': cargandoInsights }" />
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
              <div class="regla-main">
                <span class="regla-label">
                  <span class="regla-dot" :style="{ background: r.color }" />
                  {{ r.label }}
                </span>
                <div class="regla-bar-wrap">
                  <div class="regla-bar" :style="{ width: Math.min(r.pct, 100) + '%', background: r.color }" />
                  <div class="regla-goal" :style="{ left: r.meta + '%' }" />
                </div>
              </div>
              <div class="regla-vals">
                <strong>{{ r.pct }}%</strong>
                <span>meta {{ r.meta }}%</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Alertas de deudas -->
        <DeudasAlerta style="margin-bottom: 1rem" />
     </div>

      <!-- Alertas de presupuesto -->
      <div v-if="alertasPresupuesto.length > 0" class="alerts-scroller" :class="{ 'can-left': alertsScroll.left, 'can-right': alertsScroll.right }">
        <button v-show="alertsScroll.left" class="scroll-arrow scroll-arrow--left" aria-label="Ver anteriores" @click="scrollAlerts(-1)">
          <ChevronLeft :size="16" :stroke-width="2" />
        </button>
        <div ref="alertsRef" class="presupuesto-alerts" @scroll="updateAlertsScroll">
          <RouterLink
            v-for="a in alertasPresupuesto"
            :key="a.categoria"
            to="/presupuesto"
            class="presupuesto-alert"
            :class="`alert-${alertaEstado(a.pct)}`"
          >
            <div class="alert-top">
              <span class="alert-cat">{{ a.categoria }}</span>
              <span class="alert-pct-badge">{{ a.pct }}%</span>
            </div>
            <div class="alert-bar-wrap">
              <div class="alert-bar" :style="{ width: Math.min(a.pct, 100) + '%' }" />
            </div>
            <div class="alert-nums">
              <strong>{{ fmtCorto(a.gastado) }}</strong> / {{ fmtCorto(a.limite) }}
            </div>
          </RouterLink>
        </div>
        <button v-show="alertsScroll.right" class="scroll-arrow scroll-arrow--right" aria-label="Ver más" @click="scrollAlerts(1)">
          <ChevronRight :size="16" :stroke-width="2" />
        </button>
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
import { computed, ref, reactive, watch, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { animate, stagger } from 'motion'
import {
  TrendingUp, Lock, Shuffle, PiggyBank, ArrowUpRight, ArrowDownRight,
  Sparkles, ChevronLeft, ChevronRight,
} from 'lucide-vue-next'
import { useFinanceStore } from '@/stores/finance'
import MonthSelector from '@/components/common/MonthSelector.vue'
import DeudasAlerta from '@/components/common/DeudasAlerta.vue'
import DonutChart from '@/components/charts/DonutChart.vue'
import BarChart from '@/components/charts/BarChart.vue'
import GastoHeatmap from '@/components/charts/GastoHeatmap.vue'
import EmptyStart from '@/components/common/EmptyStart.vue'
import { usePresupuestoMensual } from '@/composables/usePresupuestoMensual'
import { COLOR_PALETTE } from '@/composables/useCategoriaOptions'
import { resumenService, gastosService } from '@/services/api'
import type { Suscripcion, HeatmapDia } from '@/types'

const MESES = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']

const store = useFinanceStore()
const resumen = computed(() => store.resumen)

// Estado vacío: el mes no tiene ni ingresos ni gastos. Si además el año está en cero
// y no hay cuentas, se asume usuario nuevo y se muestra la bienvenida.
const mesVacio = computed(() =>
  (store.resumen?.totalIngresos ?? 0) === 0 && (store.resumen?.totalGastos ?? 0) === 0
)
const esUsuarioNuevo = computed(() =>
  (store.resumenAnual?.totalIngresos ?? 0) === 0 &&
  (store.resumenAnual?.totalGastos ?? 0) === 0 &&
  store.cuentas.length === 0
)
// Alturas fijas de las barras del skeleton del gráfico (12 meses)
const SK_BARRAS = [45, 62, 38, 70, 55, 80, 48, 66, 58, 74, 42, 60]

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

// Mismos tonos que --c-need / --c-want / --c-pos (el donut necesita hex)
const distribucion = computed(() => [
  { label: 'Gastos fijos',     val: resumen.value?.totalFijos    ?? 0, color: '#60a5fa' },
  { label: 'Gastos ocasionales', val: resumen.value?.totalVariables ?? 0, color: '#fb923c' },
  { label: 'Dinero restante',    val: resumen.value?.ahorro        ?? 0, color: '#10b981' },
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
    { label: 'Necesidades', pct: pct(r.totalFijos),     meta: 50, color: 'var(--c-need)' },
    { label: 'Gustos',      pct: pct(r.totalVariables), meta: 30, color: 'var(--c-want)' },
    { label: 'Ahorro',      pct: Math.max(0, pct(r.ahorro)), meta: 20, color: 'var(--c-pos)' },
  ]
})

const kpiCards = computed(() => [
  { key: 'ingresos',  label: 'Ingresos',                icon: TrendingUp, color: 'var(--accent-strong)', value: counters.ingresos,  variacion: variacion.value?.ingresos,  pct: null },
  { key: 'fijos',     label: 'Gastos fijos',            icon: Lock,       color: 'var(--c-need)',        value: counters.fijos,     variacion: variacion.value?.fijos,     pct: pcts.fijos },
  { key: 'variables', label: 'Gastos ocasionales',      icon: Shuffle,    color: 'var(--c-want)',        value: counters.variables, variacion: variacion.value?.variables, pct: pcts.variables },
  { key: 'ahorro',    label: 'Dinero restante del mes', icon: PiggyBank,  color: 'var(--c-pos)',         value: counters.ahorro,    variacion: variacion.value?.ahorro,    pct: pcts.ahorro },
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

// $200k / $1,2M para las tarjetas compactas
function fmtCorto(n: number) {
  const abs = Math.abs(n)
  const sign = n < 0 ? '-' : ''
  if (abs >= 1_000_000) return `${sign}$${(abs / 1_000_000).toLocaleString('es-CO', { maximumFractionDigits: 1 })}M`
  if (abs >= 1_000)     return `${sign}$${Math.round(abs / 1_000).toLocaleString('es-CO')}k`
  return `${sign}$${Math.round(abs)}`
}

// Al 100% justo el presupuesto está completo (neutro); solo pasarse es alerta
function alertaEstado(p: number) {
  if (p > 100) return 'over'
  if (p === 100) return 'full'
  return 'warn'
}

const tendenciaLabels   = computed(() => MESES)
const tendenciaDatasets = computed(() => [
  { label: 'Ingresos', data: store.tendencia.map(r => r.totalIngresos), color: '#3b82f6' },
  { label: 'Gastos',   data: store.tendencia.map(r => r.totalGastos),   color: '#f43f5e' },
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

// Scroll horizontal de alertas: flechas solo cuando hay contenido oculto a ese lado
const alertsRef = ref<HTMLElement>()
const alertsScroll = reactive({ left: false, right: false })

function updateAlertsScroll() {
  const el = alertsRef.value
  if (!el) { alertsScroll.left = alertsScroll.right = false; return }
  alertsScroll.left  = el.scrollLeft > 4
  alertsScroll.right = el.scrollLeft + el.clientWidth < el.scrollWidth - 4
}

function scrollAlerts(dir: 1 | -1) {
  const el = alertsRef.value
  if (el) el.scrollBy({ left: dir * el.clientWidth * 0.8, behavior: 'smooth' })
}

watch(alertasPresupuesto, () => nextTick(updateAlertsScroll), { flush: 'post' })
onMounted(() => window.addEventListener('resize', updateAlertsScroll))
onBeforeUnmount(() => window.removeEventListener('resize', updateAlertsScroll))

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

/* Skeletons (la clase base .skeleton vive en main.css) */
.dash-skeleton { display: flex; flex-direction: column; gap: 1rem; }
.dash-skeleton .kpi-grid { margin-bottom: 0; }
.sk-kpi    { display: flex; flex-direction: column; gap: 0.8rem; padding: 1.15rem 1.25rem; min-height: 150px; }
.sk-row    { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
.sk-icon   { width: 30px; height: 30px; border-radius: 9px; background: var(--surface2); }
.sk-big    { height: 22px; }
.sk-huge   { height: 30px; }
.sk-bar    { height: 6px; width: 100%; }
.sk-btn    { width: 120px; height: 32px; border-radius: 8px; }
.sk-banner { display: flex; align-items: center; gap: 2.25rem; padding: 1.35rem 1.6rem; }
.sk-col    { display: flex; flex-direction: column; gap: 0.7rem; }
.sk-grow   { flex: 1; }
.sk-card   { display: flex; flex-direction: column; gap: 0.85rem; padding: 1.25rem; }
.sk-chart  { display: flex; flex-direction: column; gap: 1rem; padding: 1.25rem; }
.sk-bars   { display: flex; align-items: flex-end; gap: 4%; height: 160px; padding: 0 1%; }
.sk-bar-v  { flex: 1; border-radius: 5px 5px 0 0; background: var(--surface2); }

/* La paleta --c-* vive en main.css */
.dash { font-variant-numeric: tabular-nums; }

/* Números: Iosevka es monoespaciada -> columnas alineadas; 700 para montos */
.kpi-value, .anual-big, .anual-stat strong, .regla-vals strong, .alert-nums,
.legend-amt, .expense-amt, .top-cat-monto, .susc-monto {
  font-family: var(--ff-mono);
}

/* KPI Grid */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.85rem; margin-bottom: 1rem; }
.kpi-card {
  position: relative;
  display: flex;
  flex-direction: column;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  padding: 1.15rem 1.25rem 1.1rem;
  overflow: hidden;
  box-shadow: var(--shadow-card);
  transition: border-color 0.2s ease;
}
.kpi-card:hover { border-color: var(--border-hover); }
.kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.85rem; }
.kpi-label { font-size: 0.66rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.kpi-icon {
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;
  border-radius: 9px;
  flex-shrink: 0;
  color: var(--ca);
  background: color-mix(in srgb, var(--ca) 13%, transparent);
}
.kpi-value-row { display: flex; align-items: center; flex-wrap: wrap; gap: 0.5rem; margin-bottom: 1rem; }
.kpi-value { font-size: 1.45rem; font-weight: 700; color: var(--text-primary); letter-spacing: -0.02em; line-height: 1.1; }
.kpi-var {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  font-size: 0.66rem;
  font-weight: 500;
  padding: 0.15rem 0.45rem 0.15rem 0.3rem;
  border-radius: 999px;
}
.kpi-var--good { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 13%, transparent); }
.kpi-var--bad  { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 13%, transparent); }
.kpi-bar-wrap { height: 6px; background: var(--surface2); border-radius: 99px; overflow: hidden; margin-top: auto; margin-bottom: 0.55rem; }
.kpi-bar { height: 100%; border-radius: 99px; max-width: 100%; background: var(--ca); }
.kpi-pct { font-size: 0.68rem; font-weight: 400; color: var(--text-muted); }
.kpi-pct strong { font-weight: 500; color: var(--text-secondary); }

/* Ahorro anual banner: saldo | progreso | métricas */
.anual-banner {
  display: grid;
  grid-template-columns: auto minmax(200px, 1fr) auto;
  align-items: center;
  gap: 2.25rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  padding: 1.35rem 1.6rem;
  margin-bottom: 1rem;
  box-shadow: var(--shadow-card);
}
.anual-left { min-width: 0; }
.anual-big { font-size: 2rem; font-weight: 700; color: var(--text-primary); line-height: 1; letter-spacing: -0.03em; margin-bottom: 0.45rem; }
.anual-meta { font-size: 0.68rem; color: var(--text-muted); }
.anual-big--neg { color: var(--c-neg); }
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
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-modal);
  padding: 1.4rem; width: 460px; max-width: 100%; max-height: 85vh; overflow-y: auto;
  display: flex; flex-direction: column; gap: 0.75rem;
  box-shadow: var(--shadow-pop);
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
.dd-in  { color: var(--c-pos); }
.dd-out { color: var(--c-neg); }
.dd-note {
  font-size: 0.68rem; color: var(--text-secondary); line-height: 1.4;
  background: var(--surface2); border-radius: 8px; padding: 0.6rem 0.75rem;
}
.anual-center { display: flex; flex-direction: column; gap: 0.6rem; min-width: 0; }
.anual-progress-head { display: flex; justify-content: space-between; align-items: center; gap: 0.75rem; }
.anual-progress-track { position: relative; height: 8px; background: var(--surface2); border-radius: 99px; }
.anual-progress-fill {
  height: 100%;
  max-width: 100%;
  background: linear-gradient(90deg, color-mix(in srgb, var(--c-pos) 70%, transparent), var(--c-pos));
  border-radius: 99px;
  transition: width 1.4s cubic-bezier(0.16, 1, 0.3, 1);
}
.anual-progress-goal {
  position: absolute;
  top: -3px; bottom: -3px; left: 20%;
  width: 2px;
  border-radius: 2px;
  background: var(--text-secondary);
}
.anual-progress-labels { position: relative; display: flex; justify-content: space-between; font-size: 0.6rem; color: var(--text-muted); }
.anual-goal-label { position: absolute; left: 20%; transform: translateX(-50%); color: var(--text-secondary); }

.anual-stats { display: flex; align-items: center; gap: 1.25rem; }
.anual-stat { display: flex; flex-direction: column; gap: 0.3rem; }
.anual-stat-label { font-size: 0.62rem; font-weight: 500; color: var(--text-subtle); letter-spacing: 0.1em; text-transform: uppercase; }
.anual-stat strong { font-size: 0.92rem; font-weight: 700; color: var(--text-primary); }
.anual-stat .anual-stat-pos { color: var(--c-pos); }
.anual-stat-sep { width: 1px; height: 30px; background: var(--border-hover); }

.health-badge {
  display: inline-flex;
  align-items: center;
  font-size: 0.64rem;
  font-weight: 500;
  letter-spacing: 0.04em;
  padding: 0.2rem 0.65rem;
  border-radius: 999px;
}
.health-green { color: var(--c-pos);  background: color-mix(in srgb, var(--c-pos) 12%, transparent); }
.health-amber { color: var(--c-want); background: color-mix(in srgb, var(--c-want) 12%, transparent); }
.health-red   { color: var(--c-neg);  background: color-mix(in srgb, var(--c-neg) 12%, transparent); }

/* Insights con IA */
.insights-card {
  border-color: color-mix(in srgb, var(--c-ai) 22%, var(--border));
  background:
    radial-gradient(120% 140% at 0% 0%, color-mix(in srgb, var(--c-ai) 7%, transparent), transparent 55%),
    var(--surface);
}
.insights-header { display: flex; justify-content: space-between; align-items: center; gap: 1rem; margin-bottom: 1rem; }
.insights-title { display: flex; align-items: center; gap: 0.75rem; }
.ai-icon {
  display: grid; place-items: center;
  width: 34px; height: 34px; border-radius: 10px; flex-shrink: 0;
  color: var(--c-ai);
  background: color-mix(in srgb, var(--c-ai) 14%, transparent);
}
.card-title.ai-title { margin-bottom: 0; color: var(--c-ai); font-weight: 500; }
.insights-hint { font-size: 0.7rem; color: var(--text-muted); margin-top: 0.2rem; }
.btn-ai {
  border: none;
  color: #fff;
  background: linear-gradient(135deg, #8b5cf6, #3b82f6);
  box-shadow: 0 4px 14px -4px color-mix(in srgb, #8b5cf6 55%, transparent), inset 0 1px 0 rgba(255,255,255,0.18);
}
.btn-ai:hover { color: #fff; background: linear-gradient(135deg, #9b70f8, #5592f7); }
.ai-spin { animation: aiSpin 1.2s linear infinite; }
@keyframes aiSpin { to { transform: rotate(360deg); } }
.insights-lineas { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 0.6rem; }
.insights-linea {
  display: grid;
  grid-template-columns: auto 1fr;
  align-items: flex-start;
  gap: 0.6rem;
  font-size: 0.78rem;
  color: var(--text-primary);
  line-height: 1.55;
  padding: 0.75rem 0.9rem;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 12px;
}
.insights-icon { flex-shrink: 0; margin-top: 0.15rem; color: var(--c-ai); }
.insights-empty { font-size: 0.78rem; color: var(--text-secondary); }

/* Regla 50/30/20 */
.regla-rows { display: flex; flex-direction: column; gap: 1.1rem; }
.regla-row { display: flex; align-items: center; gap: 1.25rem; }
.regla-main { flex: 1; display: flex; flex-direction: column; gap: 0.5rem; min-width: 0; }
.regla-label { display: flex; align-items: center; gap: 0.5rem; font-size: 0.76rem; color: var(--text-primary); font-weight: 500; }
.regla-dot { width: 8px; height: 8px; border-radius: 99px; flex-shrink: 0; }
.regla-vals { display: flex; flex-direction: column; align-items: flex-end; gap: 0.15rem; min-width: 64px; }
.regla-vals strong { font-size: 0.95rem; font-weight: 700; color: var(--text-primary); }
.regla-vals span { font-size: 0.62rem; color: var(--text-muted); }
.regla-bar-wrap { position: relative; height: 6px; background: var(--surface2); border-radius: 99px; }
.regla-bar { height: 100%; border-radius: 99px; max-width: 100%; transition: width 0.5s ease; }
.regla-goal { position: absolute; top: -3px; bottom: -3px; width: 2px; background: var(--text-secondary); border-radius: 2px; transform: translateX(-1px); }

/* 3-col bottom */
.bottom-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.85rem; }

/* 2-col bottom */
.two-bottom-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }

.card-title { font-size: 0.66rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem; justify-content: space-between; }

/* Donut section */
.donut-section { display: flex; flex-direction: column; align-items: center; gap: 1rem; }
.donut-legend { width: 100%; display: flex; flex-direction: column; gap: 0.6rem; }
.legend-row { display: flex; justify-content: space-between; align-items: center; }
.legend-left { display: flex; align-items: center; gap: 0.5rem; }
.legend-dot { width: 8px; height: 8px; border-radius: 99px; flex-shrink: 0; }
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
.top-cat-bar-wrap { flex: 1; height: 6px; background: var(--surface2); border-radius: 99px; overflow: hidden; }
.top-cat-bar  { height: 100%; background: var(--accent-strong); border-radius: 99px; transition: width 0.4s ease; }
.top-cat-monto { font-size: 0.72rem; color: var(--text-secondary); min-width: 80px; text-align: right; flex-shrink: 0; }

/* Suscripciones olvidadas */
.susc-list { display: flex; flex-direction: column; }
.susc-row { display: flex; justify-content: space-between; align-items: center; padding: 0.55rem 0; border-bottom: 1px solid var(--border); }
.susc-row:last-child { border-bottom: none; }
.susc-nombre { font-size: 0.75rem; color: var(--text-primary); }
.susc-meses { font-size: 0.62rem; color: var(--text-muted); margin-top: 0.1rem; }
.susc-monto { font-size: 0.75rem; color: var(--c-want); font-weight: 600; flex-shrink: 0; margin-left: 0.5rem; }

/* Alertas de presupuesto: fila con scroll horizontal + degradado en bordes */
.alerts-scroller { position: relative; margin-bottom: 1rem; }
.presupuesto-alerts {
  display: flex;
  gap: 0.75rem;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  scrollbar-width: none;
  padding: 2px 2px 4px;
}
.presupuesto-alerts::-webkit-scrollbar { display: none; }
.alerts-scroller.can-right .presupuesto-alerts { mask-image: linear-gradient(to right, #000 85%, transparent); }
.alerts-scroller.can-left .presupuesto-alerts  { mask-image: linear-gradient(to right, transparent, #000 15%); }
.alerts-scroller.can-left.can-right .presupuesto-alerts { mask-image: linear-gradient(to right, transparent, #000 12%, #000 88%, transparent); }

.presupuesto-alert {
  --st: var(--c-warn);
  flex: 0 0 220px;
  scroll-snap-align: start;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  padding: 0.95rem 1.05rem;
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  text-decoration: none;
  box-shadow: var(--shadow-card);
  transition: border-color 0.15s ease, transform 0.15s ease;
}
.presupuesto-alert:hover { border-color: var(--border-hover); transform: translateY(-1px); }
.alert-warn { --st: var(--c-warn); }
.alert-full { --st: var(--text-secondary); }
.alert-over { --st: var(--c-neg); }
.alert-top  { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
.alert-cat  { font-size: 0.8rem; color: var(--text-primary); font-weight: 500; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.alert-pct-badge {
  flex-shrink: 0;
  font-size: 0.64rem; font-weight: 500;
  padding: 0.15rem 0.5rem; border-radius: 999px;
  color: var(--st);
  background: color-mix(in srgb, var(--st) 13%, transparent);
}
.alert-bar-wrap { height: 6px; background: var(--surface2); border-radius: 99px; overflow: hidden; }
.alert-bar { height: 100%; background: var(--st); border-radius: 99px; }
.alert-nums { font-size: 0.7rem; color: var(--text-muted); }
.alert-nums strong { color: var(--text-primary); font-weight: 700; }

.scroll-arrow {
  position: absolute;
  top: 50%;
  z-index: 2;
  display: grid; place-items: center;
  width: 30px; height: 30px;
  border-radius: 999px;
  border: 1px solid var(--border-hover);
  background: var(--surface);
  color: var(--text-secondary);
  cursor: pointer;
  box-shadow: var(--shadow-card);
  transform: translateY(-50%);
  transition: color 0.15s ease, border-color 0.15s ease;
}
.scroll-arrow:hover { color: var(--text-primary); border-color: var(--accent); }
.scroll-arrow--left  { left: -10px; }
.scroll-arrow--right { right: -10px; }

@media (max-width: 768px) {

  .sk-banner { flex-direction: column; align-items: stretch; gap: 1rem; }
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }

  .anual-banner { grid-template-columns: 1fr; gap: 1.25rem; padding: 1.15rem 1.2rem; }
  .anual-stats { gap: 1rem; justify-content: space-between; }

  .bottom-grid { grid-template-columns: 1fr; }

  .top-cat-left { min-width: 120px; }
  .top-cat-monto { min-width: 70px; }
}

@media (max-width: 480px) {
  .kpi-grid { grid-template-columns: 1fr; }
  .anual-big { font-size: 1.5rem; }

  .insights-header { flex-direction: column; align-items: stretch; }
  .anual-stat-sep { display: none; }
}

</style>
