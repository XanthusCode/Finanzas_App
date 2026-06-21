<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Análisis</div>
        <h1 class="page-title">Historial anual</h1>
      </div>
      <div class="year-selector">
        <button class="year-btn" @click="anio--">‹</button>
        <span class="year-label">{{ anio }}</span>
        <button class="year-btn" @click="anio++" :disabled="anio >= anioHoy">›</button>
      </div>
    </div>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 5" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <!-- KPIs anuales -->
      <div class="kpi-grid">
        <div class="kpi-card">
          <span class="kpi-label">Ingresos totales</span>
          <span class="kpi-val">{{ fmt(totalIngresos) }}</span>
        </div>
        <div class="kpi-card">
          <span class="kpi-label">Gastos totales</span>
          <span class="kpi-val">{{ fmt(totalGastos) }}</span>
        </div>
        <div class="kpi-card">
          <span class="kpi-label">Ahorro total</span>
          <span class="kpi-val" :style="{ color: totalAhorro >= 0 ? 'var(--accent)' : 'var(--red)' }">
            {{ totalAhorro >= 0 ? '+' : '' }}{{ fmt(totalAhorro) }}
          </span>
        </div>
        <div class="kpi-card">
          <span class="kpi-label">% Ahorro promedio</span>
          <span class="kpi-val" :style="{ color: pctAhorroPromedio >= 20 ? 'var(--accent)' : pctAhorroPromedio >= 10 ? '#f59e0b' : 'var(--red)' }">
            {{ pctAhorroPromedio.toFixed(1) }}%
          </span>
        </div>
      </div>

      <!-- Minigráfico de ahorro -->
      <div v-if="mesesConDatos.length > 1" class="sparkline-card">
        <span class="spark-label">Ahorro mes a mes</span>
        <div class="sparkline">
          <div v-for="(m, i) in datosMeses" :key="i" class="spark-col">
            <div class="spark-bar-wrap">
              <div
                class="spark-bar"
                :class="m.ahorro >= 0 ? 'spark-pos' : 'spark-neg'"
                :style="{ height: pctSpark(m.ahorro) + '%' }"
              />
            </div>
            <span class="spark-mes">{{ MESES[m.mes - 1] }}</span>
          </div>
        </div>
      </div>

      <!-- Tabla mensual -->
      <div class="tabla-card">
        <table class="tabla">
          <thead>
            <tr>
              <th>Mes</th>
              <th class="num">Ingresos</th>
              <th class="num">Gastos fijos</th>
              <th class="num">Gastos variables</th>
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
              <td class="td-mes">{{ MESES[m.mes - 1] }}</td>
              <td class="num">{{ m.totalIngresos > 0 ? fmt(m.totalIngresos) : '—' }}</td>
              <td class="num">{{ m.totalFijos > 0 ? fmt(m.totalFijos) : '—' }}</td>
              <td class="num">{{ m.totalVariables > 0 ? fmt(m.totalVariables) : '—' }}</td>
              <td class="num">{{ m.totalGastos > 0 ? fmt(m.totalGastos) : '—' }}</td>
              <td class="num" :class="m.totalIngresos > 0 ? (m.ahorro >= 0 ? 'aho-pos' : 'aho-neg') : ''">
                {{ m.totalIngresos > 0 ? (m.ahorro >= 0 ? '+' : '') + fmt(m.ahorro) : '—' }}
              </td>
              <td class="num" :class="m.totalIngresos > 0 ? (m.pctAhorro >= 20 ? 'aho-pos' : m.pctAhorro >= 10 ? 'aho-warn' : 'aho-neg') : ''">
                {{ m.totalIngresos > 0 ? m.pctAhorro.toFixed(1) + '%' : '—' }}
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="row-total">
              <td>Total</td>
              <td class="num">{{ fmt(totalIngresos) }}</td>
              <td class="num">{{ fmt(totalFijos) }}</td>
              <td class="num">{{ fmt(totalVariables) }}</td>
              <td class="num">{{ fmt(totalGastos) }}</td>
              <td class="num" :class="totalAhorro >= 0 ? 'aho-pos' : 'aho-neg'">
                {{ totalAhorro >= 0 ? '+' : '' }}{{ fmt(totalAhorro) }}
              </td>
              <td class="num" :class="pctAhorroPromedio >= 20 ? 'aho-pos' : pctAhorroPromedio >= 10 ? 'aho-warn' : 'aho-neg'">
                {{ pctAhorroPromedio.toFixed(1) }}%
              </td>
            </tr>
          </tfoot>
        </table>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { resumenService } from '@/services/api'
import { fmtCOP as fmt } from '@/utils'
import type { Resumen } from '@/types'

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

const maxAbsAhorro = computed(() =>
  Math.max(...datosMeses.value.map(m => Math.abs(m.ahorro)), 1)
)

function pctSpark(ahorro: number): number {
  return Math.round((Math.abs(ahorro) / maxAbsAhorro.value) * 80)
}

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
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }

.year-selector { display: flex; align-items: center; gap: 0.5rem; }
.year-btn {
  background: var(--surface); border: 1px solid var(--border); border-radius: 6px;
  width: 30px; height: 30px; font-size: 1.1rem; color: var(--text-secondary);
  cursor: pointer; display: flex; align-items: center; justify-content: center; transition: background 0.15s;
}
.year-btn:hover:not(:disabled) { background: var(--surface2); color: var(--text-primary); }
.year-btn:disabled { opacity: 0.3; cursor: default; }
.year-label { font-size: 1rem; font-weight: 700; min-width: 48px; text-align: center; }

/* KPIs */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; margin-bottom: 1rem; }
.kpi-card { background: var(--surface); border: 1px solid var(--border); border-radius: 8px; padding: 0.85rem 1rem; display: flex; flex-direction: column; gap: 0.25rem; }
.kpi-label { font-size: 0.6rem; color: var(--text-muted); letter-spacing: 0.1em; text-transform: uppercase; }
.kpi-val   { font-size: 1.05rem; font-weight: 700; }

/* Sparkline */
.sparkline-card { background: var(--surface); border: 1px solid var(--border); border-radius: 10px; padding: 0.85rem 1rem; margin-bottom: 1rem; }
.spark-label { font-size: 0.6rem; color: var(--text-muted); letter-spacing: 0.1em; text-transform: uppercase; display: block; margin-bottom: 0.5rem; }
.sparkline { display: flex; align-items: flex-end; gap: 4px; height: 56px; }
.spark-col { display: flex; flex-direction: column; align-items: center; flex: 1; gap: 3px; height: 100%; }
.spark-bar-wrap { flex: 1; display: flex; align-items: flex-end; width: 100%; }
.spark-bar { width: 100%; border-radius: 3px 3px 0 0; min-height: 2px; transition: height 0.4s ease; }
.spark-pos { background: #34d399; }
.spark-neg { background: var(--red); opacity: 0.7; }
.spark-mes { font-size: 0.52rem; color: var(--text-muted); white-space: nowrap; }

/* Tabla */
.tabla-card { background: var(--surface); border: 1px solid var(--border); border-radius: 10px; overflow: hidden; }
.tabla { width: 100%; border-collapse: collapse; font-size: 0.72rem; }
.tabla th {
  padding: 0.6rem 0.9rem; text-align: left; font-size: 0.6rem;
  font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase;
  color: var(--text-muted); border-bottom: 1px solid var(--border);
  background: var(--surface2);
}
.tabla th.num, .tabla td.num { text-align: right; }
.tabla td { padding: 0.55rem 0.9rem; border-bottom: 1px solid var(--border); color: var(--text-secondary); }
.tabla tbody tr:last-child td { border-bottom: none; }

.td-mes { font-weight: 600; color: var(--text-primary) !important; }
.row-vacio td { opacity: 0.35; }
.row-actual td { background: rgba(99,179,255,0.04); }
.row-actual .td-mes::after { content: ' ·'; color: var(--accent); }

.row-total td { font-weight: 700; color: var(--text-primary); border-top: 1px solid var(--border); background: var(--surface2); }

.aho-pos  { color: #34d399 !important; font-weight: 600; }
.aho-neg  { color: var(--red) !important; font-weight: 600; }
.aho-warn { color: #f59e0b !important; font-weight: 600; }

/* Loading */
.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row { height: 44px; border-radius: 6px; background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
@keyframes shimmer { to { background-position: -200% 0; } }
</style>
