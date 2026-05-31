<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Resumen</div>
        <h1 class="page-title">Dashboard</h1>
      </div>
      <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
    </div>

    <div v-if="store.loading" class="loading">Cargando...</div>

    <template v-else-if="store.resumen">
      <!-- KPIs -->
      <div class="kpi-grid">
        <div class="kpi-card">
          <div class="kpi-label">Ingresos</div>
          <div class="kpi-value" style="color: var(--accent)">{{ fmt(resumen?.totalIngresos) }}</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Gastos fijos</div>
          <div class="kpi-value" style="color: var(--red)">{{ fmt(resumen?.totalFijos) }}</div>
          <div class="kpi-pct">{{ pct(resumen?.totalFijos, resumen?.totalIngresos) }}% del ingreso</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Gastos variables</div>
          <div class="kpi-value" style="color: var(--amber)">{{ fmt(resumen?.totalVariables) }}</div>
          <div class="kpi-pct">{{ pct(resumen?.totalVariables, resumen?.totalIngresos) }}% del ingreso</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Ahorro</div>
          <div class="kpi-value" style="color: var(--green)">{{ fmt(resumen?.ahorro) }}</div>
          <div class="kpi-pct">{{ resumen?.pctAhorro ?? 0 }}% del ingreso</div>
        </div>
      </div>

      <!-- Barras de distribucion -->
      <div class="card" style="margin-bottom: 1rem">
        <p class="card-title">Distribución del ingreso</p>
        <div class="dist-bars">
          <div v-for="item in distribucion" :key="item.label" class="bar-row">
            <div class="bar-label">
              <span>{{ item.label }}</span>
              <span>{{ pct(item.val, resumen?.totalIngresos) }}%</span>
            </div>
            <div class="bar-track">
              <div class="bar-fill" :style="{ width: pct(item.val, resumen?.totalIngresos) + '%', background: item.color }" />
            </div>
          </div>
        </div>
      </div>

      <!-- Listas lado a lado -->
      <div class="two-col">
        <div class="card">
          <p class="card-title">Gastos fijos <span class="tag tag-red">{{ fmt(resumen?.totalFijos) }}</span></p>
          <div v-if="store.gastosFijos.length === 0" class="empty">Sin gastos registrados</div>
          <div v-for="g in store.gastosFijos" :key="g.id" class="expense-row">
            <div>
              <div class="expense-name">{{ g.categoria }}</div>
              <div class="expense-det">{{ g.detalle }}</div>
            </div>
            <div class="expense-amt">{{ fmt(g.monto) }}</div>
          </div>
        </div>

        <div class="card">
          <p class="card-title">Gastos variables <span class="tag tag-amber">{{ fmt(resumen?.totalVariables) }}</span></p>
          <div v-if="store.gastosVariables.length === 0" class="empty">Sin gastos registrados</div>
          <div v-for="g in store.gastosVariables" :key="g.id" class="expense-row">
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
       <div class="empty">No hay datos para el mes seleccionado</div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import MonthSelector from '@/components/common/MonthSelector.vue'

const store = useFinanceStore()
const resumen = computed(() => store.resumen)

const distribucion = computed(() => [
  { label: 'Gastos fijos',     val: resumen.value?.totalFijos ?? 0,     color: '#f87171' },
  { label: 'Gastos variables', val: resumen.value?.totalVariables ?? 0,  color: '#fbbf24' },
  { label: 'Ahorro',           val: resumen.value?.ahorro ?? 0,          color: '#34d399' },
])

const fmt = (n?: number) => n != null ? '$' + Math.round(n).toLocaleString('es-CO') : '$0'
const pct = (a?: number, b?: number) => (a && b) ? Math.round(a / b * 100) : 0

onMounted(() => store.cargarDatos())
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.loading { color: var(--text-muted); font-size: 0.8rem; }

.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; margin-bottom: 1rem; }
.kpi-card { background: var(--surface); border: 1px solid var(--border); border-radius: 10px; padding: 1rem 1.1rem; }
.kpi-label { font-size: 0.62rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 0.4rem; }
.kpi-value { font-size: 1.3rem; font-family: var(--ff-display); font-weight: 700; }
.kpi-pct { font-size: 0.65rem; color: var(--text-muted); margin-top: 0.2rem; }

.card-title { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 1rem; display: flex; align-items: center; justify-content: space-between; }

.dist-bars { display: flex; flex-direction: column; gap: 0.75rem; }
.bar-row {}
.bar-label { display: flex; justify-content: space-between; font-size: 0.72rem; color: var(--text-secondary); margin-bottom: 0.3rem; }
.bar-track { height: 5px; background: var(--surface2); border-radius: 3px; overflow: hidden; }
.bar-fill { height: 100%; border-radius: 3px; transition: width 0.5s ease; }

.two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }

.expense-row { display: flex; justify-content: space-between; align-items: center; padding: 0.6rem 0; border-bottom: 1px solid var(--border); }
.expense-row:last-child { border-bottom: none; }
.expense-name { font-size: 0.78rem; color: var(--text-primary); }
.expense-det { font-size: 0.65rem; color: var(--text-muted); margin-top: 0.1rem; }
.expense-amt { font-size: 0.78rem; color: var(--text-primary); }

.empty { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }
</style>