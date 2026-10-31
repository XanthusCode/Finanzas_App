<template>
  <div>
    <div class="legend">
      <span v-for="d in props.datasets" :key="d.label" class="legend-item">
        <span class="legend-dot" :style="{ background: d.color }"></span>
        {{ d.label }}
      </span>
    </div>
    <div style="position: relative; width: 100%; height: 220px;">
      <canvas ref="canvasRef" role="img" aria-label="Gráfica de ingresos, gastos y ahorros por mes">
        Ingresos, gastos y ahorros mensuales.
      </canvas>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { Chart, BarController, BarElement, CategoryScale, LinearScale, Tooltip, type ScriptableContext } from 'chart.js'
import { useAppearance } from '@/composables/useAppearance'

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip)

const props = defineProps<{
  labels: string[]
  datasets: { label: string; data: number[]; color: string }[]
  /** Oculta los números del eje Y (los valores se leen en el tooltip) */
  hideY?: boolean
}>()

const canvasRef = ref<HTMLCanvasElement>()
let chart: Chart | null = null

const { theme } = useAppearance()

function cssVar(name: string) {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim()
}

const fmtCOP = (n: number) => '$' + Math.round(n).toLocaleString('es-CO')
const fmtCorto = (v: number) =>
  v >= 1_000_000 ? (v / 1_000_000).toFixed(1) + 'M' : v >= 1000 ? (v / 1000) + 'k' : String(v)

// Degradado vertical sutil para colores hex (#rrggbb); otros formatos quedan sólidos
function fondoBarra(color: string) {
  if (!/^#[0-9a-f]{6}$/i.test(color)) return color
  return (ctx: ScriptableContext<'bar'>) => {
    const area = ctx.chart.chartArea
    if (!area) return color
    const g = ctx.chart.ctx.createLinearGradient(0, area.top, 0, area.bottom)
    g.addColorStop(0, color + 'f2')
    g.addColorStop(1, color + '73')
    return g
  }
}

function buildChart() {
  if (!canvasRef.value) return
  chart?.destroy()
  const tickColor = cssVar('--text-secondary')
  const gridColor = cssVar('--chart-grid')
  chart = new Chart(canvasRef.value, {
    type: 'bar',
    data: {
      labels: props.labels,
      datasets: props.datasets.map(d => ({
        label: d.label,
        data: d.data,
        backgroundColor: fondoBarra(d.color),
        hoverBackgroundColor: d.color,
        borderRadius: { topLeft: 5, topRight: 5 },
        borderSkipped: 'start',
        // Barras más delgadas y cada par (ingreso/gasto) más junto
        barPercentage: 0.82,
        categoryPercentage: 0.58,
        maxBarThickness: 22,
      }))
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      interaction: { mode: 'index', intersect: false },
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: cssVar('--surface'),
          borderColor: cssVar('--border-hover'),
          borderWidth: 1,
          titleColor: cssVar('--text-primary'),
          bodyColor: cssVar('--text-secondary'),
          padding: 10,
          cornerRadius: 10,
          boxPadding: 4,
          usePointStyle: true,
          callbacks: {
            label: (c) => ` ${c.dataset.label}: ${fmtCOP(Number(c.raw))}`,
          },
        },
      },
      scales: {
        x: {
          grid: { display: false },
          border: { display: false },
          ticks: { font: { size: 11 }, color: tickColor },
        },
        y: {
          grid: { color: gridColor },
          border: { display: false, dash: [4, 4] },
          ticks: {
            display: !props.hideY,
            maxTicksLimit: 5,
            font: { size: 10 }, color: tickColor,
            callback: (v) => fmtCorto(Number(v)),
          },
        },
      },
    },
  })
}

onMounted(buildChart)
watch(() => props.datasets, buildChart, { deep: true })
watch(theme, buildChart)
onBeforeUnmount(() => chart?.destroy())
</script>

<style scoped>
.legend {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.85rem;
  flex-wrap: wrap;
}
.legend-item {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.25rem 0.6rem;
  border-radius: 999px;
  background: var(--surface2);
  font-size: 0.7rem;
  color: var(--text-secondary);
}
.legend-dot {
  width: 8px;
  height: 8px;
  border-radius: 99px;
}
</style>
