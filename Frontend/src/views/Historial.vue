<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Análisis</div>
        <h1 class="page-title">Historial anual</h1>
      </div>
      <div class="year-selector">
        <button class="year-btn" aria-label="Año anterior" @click="anio--"><ChevronLeft :size="16" :stroke-width="2" /></button>
        <span class="year-label">{{ anio }}</span>
        <button class="year-btn" aria-label="Año siguiente" :disabled="anio >= anioHoy" @click="anio++"><ChevronRight :size="16" :stroke-width="2" /></button>
      </div>
    </div>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 5" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <!-- KPIs anuales -->
      <div class="kpi-grid">
        <div class="kpi-card" style="--ca: var(--c-need)">
          <div class="kpi-top">
            <span class="kpi-label">Ingresos totales</span>
            <span class="kpi-icon"><ArrowUpRight :size="15" :stroke-width="2" /></span>
          </div>
          <div class="kpi-value">{{ fmt(totalIngresos) }}</div>
          <div class="kpi-sub">{{ mesesActivos.length }} {{ mesesActivos.length === 1 ? 'mes' : 'meses' }} con ingresos</div>
        </div>
        <div class="kpi-card" style="--ca: var(--c-neg)">
          <div class="kpi-top">
            <span class="kpi-label">Gastos totales</span>
            <span class="kpi-icon"><ShoppingBag :size="15" :stroke-width="2" /></span>
          </div>
          <div class="kpi-value is-neg">{{ fmt(totalGastos) }}</div>
          <div class="kpi-sub">{{ fmt(totalFijos) }} fijos · {{ fmt(totalVariables) }} ocasionales</div>
        </div>
        <div class="kpi-card" style="--ca: var(--c-pos)">
          <div class="kpi-top">
            <span class="kpi-label">Ahorro total</span>
            <span class="kpi-icon"><PiggyBank :size="15" :stroke-width="2" /></span>
          </div>
          <div class="kpi-value" :class="totalAhorro >= 0 ? 'is-pos' : 'is-neg'">
            {{ totalAhorro >= 0 ? '+' : '' }}{{ fmt(totalAhorro) }}
          </div>
          <div class="kpi-sub">en {{ anio }}</div>
        </div>
        <div class="kpi-card" :style="{ '--ca': colorPct(pctAhorroPromedio) }">
          <div class="kpi-top">
            <span class="kpi-label">% Ahorro promedio</span>
            <span class="kpi-icon"><Percent :size="15" :stroke-width="2" /></span>
          </div>
          <div class="kpi-value" :style="{ color: colorPct(pctAhorroPromedio) }">{{ pctAhorroPromedio.toFixed(1) }}%</div>
          <div class="kpi-bar-wrap">
            <div class="kpi-bar" :style="{ width: Math.min(Math.max(pctAhorroPromedio, 0), 100) + '%' }" />
          </div>
        </div>
      </div>

      <!-- Tendencia del año -->
      <div v-if="mesesConDatos.length > 1" class="card chart-card">
        <p class="card-title">Ingresos vs. gastos {{ anio }}</p>
        <BarChart :labels="MESES" :datasets="tendenciaDatasets" hide-y />
      </div>

      <!-- Tabla mensual -->
      <div class="tabla-card">
        <table class="tabla">
          <thead>
            <tr>
              <th>Mes</th>
              <th class="num">Ingresos</th>
              <th class="num">Gastos fijos</th>
              <th class="num">Gastos ocasionales</th>
              <th class="num">Total gastos</th>
              <th class="num">Ahorro</th>
              <th class="num">% Ahorro</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="m in datosMeses"
              :key="m.mes"
              :class="{ 'row-vacio': m.totalIngresos === 0 && m.totalGastos === 0, 'row-actual': esActual(m) }"
            >
              <td class="td-mes">
                {{ MESES[m.mes - 1] }}
                <span v-if="esActual(m)" class="actual-pill">Actual</span>
              </td>
              <td class="num num--strong">{{ m.totalIngresos > 0 ? fmt(m.totalIngresos) : '' }}</td>
              <td class="num">{{ m.totalFijos > 0 ? fmt(m.totalFijos) : '' }}</td>
              <td class="num">{{ m.totalVariables > 0 ? fmt(m.totalVariables) : '' }}</td>
              <td class="num">{{ m.totalGastos > 0 ? fmt(m.totalGastos) : '' }}</td>
              <td class="num" :class="m.totalIngresos > 0 ? (m.ahorro >= 0 ? 'aho-pos' : 'aho-neg') : ''">
                {{ m.totalIngresos > 0 ? (m.ahorro >= 0 ? '+' : '') + fmt(m.ahorro) : '' }}
              </td>
              <td class="num" :class="m.totalIngresos > 0 ? clasePct(m.pctAhorro) : ''">
                {{ m.totalIngresos > 0 ? m.pctAhorro.toFixed(1) + '%' : '' }}
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="row-total">
              <td>Total</td>
              <td class="num num--strong">{{ fmt(totalIngresos) }}</td>
              <td class="num">{{ fmt(totalFijos) }}</td>
              <td class="num">{{ fmt(totalVariables) }}</td>
              <td class="num">{{ fmt(totalGastos) }}</td>
              <td class="num" :class="totalAhorro >= 0 ? 'aho-pos' : 'aho-neg'">
                {{ totalAhorro >= 0 ? '+' : '' }}{{ fmt(totalAhorro) }}
              </td>
              <td class="num" :class="clasePct(pctAhorroPromedio)">
                {{ pctAhorroPromedio.toFixed(1) }}%
              </td>
            </tr>
          </tfoot>
        </table>
      </div>

      <!-- Tarjetas mensuales (mobile) -->
      <div class="meses-cards">
        <div
          v-for="m in datosMeses"
          :key="m.mes"
          class="mes-card"
          :class="{ 'mes-vacio': m.totalIngresos === 0 && m.totalGastos === 0, 'mes-actual': esActual(m) }"
        >
          <div class="mes-card-header">
            <span class="mes-card-nombre">{{ MESES[m.mes - 1] }}</span>
            <span class="mes-card-ahorro" :class="m.totalIngresos > 0 ? (m.ahorro >= 0 ? 'aho-pos' : 'aho-neg') : ''">
              {{ m.totalIngresos > 0 ? (m.ahorro >= 0 ? '+' : '') + fmt(m.ahorro) : '—' }}
            </span>
          </div>
          <div class="mes-card-grid">
            <div class="mes-card-item">
              <span class="mes-card-lbl">Ingresos</span>
              <span class="mes-card-val">{{ m.totalIngresos > 0 ? fmt(m.totalIngresos) : '—' }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">Gastos fijos</span>
              <span class="mes-card-val">{{ m.totalFijos > 0 ? fmt(m.totalFijos) : '—' }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">Gastos ocasionales</span>
              <span class="mes-card-val">{{ m.totalVariables > 0 ? fmt(m.totalVariables) : '—' }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">Total gastos</span>
              <span class="mes-card-val">{{ m.totalGastos > 0 ? fmt(m.totalGastos) : '—' }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">% Ahorro</span>
              <span class="mes-card-val" :class="m.totalIngresos > 0 ? clasePct(m.pctAhorro) : ''">
                {{ m.totalIngresos > 0 ? m.pctAhorro.toFixed(1) + '%' : '—' }}
              </span>
            </div>
          </div>
        </div>

        <div class="mes-card mes-card-total">
          <div class="mes-card-header">
            <span class="mes-card-nombre">Total</span>
            <span class="mes-card-ahorro" :class="totalAhorro >= 0 ? 'aho-pos' : 'aho-neg'">
              {{ totalAhorro >= 0 ? '+' : '' }}{{ fmt(totalAhorro) }}
            </span>
          </div>
          <div class="mes-card-grid">
            <div class="mes-card-item">
              <span class="mes-card-lbl">Ingresos</span>
              <span class="mes-card-val">{{ fmt(totalIngresos) }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">Gastos fijos</span>
              <span class="mes-card-val">{{ fmt(totalFijos) }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">Gastos ocasionales</span>
              <span class="mes-card-val">{{ fmt(totalVariables) }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">Total gastos</span>
              <span class="mes-card-val">{{ fmt(totalGastos) }}</span>
            </div>
            <div class="mes-card-item">
              <span class="mes-card-lbl">% Ahorro</span>
              <span class="mes-card-val" :class="clasePct(pctAhorroPromedio)">
                {{ pctAhorroPromedio.toFixed(1) }}%
              </span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { ChevronLeft, ChevronRight, ArrowUpRight, ShoppingBag, PiggyBank, Percent } from '@lucide/vue'
import { resumenService } from '@/services/api'
import { fmtCOP as fmt } from '@/utils'
import type { Resumen } from '@/types'
import BarChart from '@/components/charts/BarChart.vue'

const MESES = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']

const anioHoy = new Date().getFullYear()
const mesHoy  = new Date().getMonth() + 1
const anio    = ref(anioHoy)
const loading = ref(false)
const datos   = ref<Resumen[]>([])

const datosMeses = computed(() =>
  Array.from({ length: 12 }, (_, i) => {
    const mes = i + 1
    return datos.value.find(d => d.mes === mes) ?? {
      mes, anio: anio.value,
      totalIngresos: 0, totalFijos: 0, totalVariables: 0,
      totalGastos: 0, ahorro: 0, pctAhorro: 0
    } as Resumen
  })
)

const mesesConDatos = computed(() => datosMeses.value.filter(m => m.totalIngresos > 0))

const mesesActivos   = computed(() => datosMeses.value.filter(m => m.totalIngresos > 0))
const totalIngresos  = computed(() => mesesActivos.value.reduce((s, m) => s + m.totalIngresos, 0))
const totalFijos     = computed(() => mesesActivos.value.reduce((s, m) => s + m.totalFijos, 0))
const totalVariables = computed(() => mesesActivos.value.reduce((s, m) => s + m.totalVariables, 0))
const totalGastos    = computed(() => mesesActivos.value.reduce((s, m) => s + m.totalGastos, 0))
const totalAhorro    = computed(() =>
  datosMeses.value.filter(m => m.totalIngresos > 0).reduce((s, m) => s + m.ahorro, 0)
)
const pctAhorroPromedio = computed(() => {
  const con = mesesConDatos.value.filter(m => m.totalIngresos > 0)
  if (con.length === 0) return 0
  return con.reduce((s, m) => s + m.pctAhorro, 0) / con.length
})

const tendenciaDatasets = computed(() => [
  { label: 'Ingresos', data: datosMeses.value.map(m => m.totalIngresos), color: '#3b82f6' },
  { label: 'Gastos',   data: datosMeses.value.map(m => m.totalGastos),   color: '#f43f5e' },
])

// Semáforo del % de ahorro: ≥20% verde, ≥10% naranja, menos rojo
const clasePct = (p: number) => p >= 20 ? 'aho-pos' : p >= 10 ? 'aho-warn' : 'aho-neg'
const colorPct = (p: number) => p >= 20 ? 'var(--c-pos)' : p >= 10 ? 'var(--c-want)' : 'var(--c-neg)'

function esActual(m: Resumen): boolean {
  return anio.value === anioHoy && m.mes === mesHoy
}


async function cargar() {
  loading.value = true
  try {
    const { data } = await resumenService.getTendencia(anio.value)
    datos.value = data
  } finally {
    loading.value = false
  }
}

watch(anio, cargar)
onMounted(cargar)
</script>

<style scoped>
.fade-up { font-variant-numeric: tabular-nums; }

/* Selector de año (mismo estilo que MonthSelector) */
.year-selector {
  display: inline-flex; align-items: center; gap: 0.25rem;
  height: 38px; padding: 0 0.25rem;
  background: var(--surface2); border: 1px solid transparent; border-radius: 10px;
}
.year-selector:hover { border-color: var(--border); }
.year-btn {
  display: grid; place-items: center;
  width: 30px; height: 30px; padding: 0;
  background: none; border: none; border-radius: 8px;
  color: var(--text-secondary); cursor: pointer;
  transition: color 0.15s, background 0.15s;
}
.year-btn:hover:not(:disabled) { color: var(--text-primary); background: var(--surface); }
.year-btn:disabled { opacity: 0.3; cursor: default; }
.year-label { font-family: var(--ff-mono); font-size: 0.86rem; font-weight: 700; min-width: 56px; text-align: center; color: var(--text-primary); }

/* ── KPIs: borde neutro, color solo en ícono y monto ── */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.85rem; margin-bottom: 1rem; }
.kpi-card {
  display: flex; flex-direction: column;
  padding: 1.1rem 1.25rem 1.2rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
}
.kpi-top   { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.kpi-label { font-size: 0.68rem; font-weight: 500; color: var(--text-subtle); letter-spacing: 0.08em; text-transform: uppercase; }
.kpi-icon {
  display: grid; place-items: center; flex-shrink: 0;
  width: 30px; height: 30px; border-radius: 9px;
  color: var(--ca); background: color-mix(in srgb, var(--ca) 13%, transparent);
}
.kpi-value {
  font-family: var(--ff-mono);
  font-size: 1.4rem; font-weight: 700; letter-spacing: -0.02em;
  color: var(--text-primary);
}
.kpi-value.is-pos { color: var(--c-pos); }
.kpi-value.is-neg { color: var(--c-neg); }
.kpi-sub { font-size: 0.68rem; color: var(--text-muted); margin-top: 0.35rem; }
.kpi-bar-wrap { height: 6px; margin-top: 0.7rem; border-radius: 99px; overflow: hidden; background: color-mix(in srgb, var(--text-muted) 22%, var(--surface2)); }
.kpi-bar { height: 100%; border-radius: 99px; max-width: 100%; background: var(--ca); }

.chart-card { margin-bottom: 1rem; padding: 1.25rem 1.35rem; }
.chart-card:hover { border-color: var(--border); }
.card-title {
  font-size: 0.72rem; font-weight: 500; color: var(--text-primary);
  letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 0.85rem;
}

/* ── Tabla ── */
.tabla-card {
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
  overflow-x: auto; overflow-y: hidden; -webkit-overflow-scrolling: touch;
  padding: 0.35rem;
}
.tabla { width: 100%; border-collapse: separate; border-spacing: 0; font-size: 0.76rem; }
.tabla th {
  padding: 0.85rem 1rem 0.7rem; text-align: left;
  font-size: 11px; font-weight: 500; letter-spacing: 0.08em; text-transform: uppercase;
  color: var(--text-subtle); white-space: nowrap;
  border-bottom: 1px solid var(--border);
}
.tabla th.num, .tabla td.num { text-align: right; }
.tabla td {
  padding: 0.7rem 1rem;
  color: var(--text-secondary); white-space: nowrap;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 70%, transparent);
  transition: background 0.12s ease;
}
.tabla td.num { font-family: var(--ff-mono); }
.tabla td.num--strong { color: var(--text-primary); }
.tabla tbody tr:last-child td { border-bottom: none; }
.tabla tbody tr:hover td { background: var(--surface2); }
.tabla tbody tr td:first-child { border-radius: 10px 0 0 10px; }
.tabla tbody tr td:last-child  { border-radius: 0 10px 10px 0; }

.td-mes { font-weight: 700; color: var(--text-primary) !important; }
.row-vacio td { color: var(--text-muted); }
.row-vacio .td-mes { color: var(--text-muted) !important; font-weight: 500; }
.row-actual td { background: color-mix(in srgb, var(--accent) 7%, transparent); }
.actual-pill {
  margin-left: 0.4rem; padding: 0.08rem 0.45rem; border-radius: 999px;
  font-size: 0.58rem; font-weight: 500; letter-spacing: 0.04em; vertical-align: middle;
  color: var(--accent-strong); background: color-mix(in srgb, var(--accent) 16%, transparent);
}

.row-total td {
  font-weight: 700; color: var(--text-primary);
  border-top: 1px solid var(--border-hover);
  padding-top: 0.85rem; padding-bottom: 0.85rem;
}

.aho-pos  { color: var(--c-pos) !important; font-weight: 700; }
.aho-neg  { color: var(--c-neg) !important; font-weight: 700; }
.aho-warn { color: var(--c-want) !important; font-weight: 700; }

/* Tarjetas mensuales (mobile) */
.meses-cards { display: none; }
.mes-card {
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card);
  padding: 0.75rem 0.9rem;
  box-shadow: var(--shadow-card);
}
.mes-card-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 0.5rem; padding-bottom: 0.5rem; border-bottom: 1px solid var(--border);
}
.mes-card-nombre { font-weight: 700; color: var(--text-primary); font-size: 0.86rem; }
.mes-card-ahorro { font-family: var(--ff-mono); font-weight: 700; font-size: 0.86rem; }
.mes-card-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.5rem 0.75rem; }
.mes-card-item { display: flex; flex-direction: column; gap: 0.15rem; }
.mes-card-lbl { font-size: 0.6rem; color: var(--text-subtle); letter-spacing: 0.06em; text-transform: uppercase; }
.mes-card-val { font-family: var(--ff-mono); font-size: 0.76rem; color: var(--text-primary); font-weight: 500; }

.mes-vacio { opacity: 0.4; }
.mes-actual { border-color: color-mix(in srgb, var(--accent) 35%, var(--border)); }
.mes-actual .mes-card-nombre::after { content: ' · Actual'; font-size: 0.66rem; font-weight: 500; color: var(--accent-strong); }

.mes-card-total { background: var(--surface2); }
.mes-card-total .mes-card-nombre, .mes-card-total .mes-card-val { color: var(--text-primary); font-weight: 700; }

/* Loading */
.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row { height: 44px; border-radius: 6px; background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 900px) {
  .kpi-grid { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 500px) {
  .year-selector { width: 100%; justify-content: space-between; }
  .kpi-grid { grid-template-columns: 1fr 1fr; }

  .tabla-card { display: none; }
  .meses-cards { display: flex; flex-direction: column; gap: 0.6rem; }
}
</style>
