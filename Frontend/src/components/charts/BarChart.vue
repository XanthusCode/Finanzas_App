<template>
  <div>
    <div class="legend">
      <span v-for="d in datasets" :key="d.label" class="legend-item">
        <span class="legend-dot" :style="{ background: d.color }"></span>
        {{ d.label }}
      </span>
    </div>
    <div style="position: relative; width: 100%; height: 200px;">
      <canvas ref="canvasRef" role="img" aria-label="Gráfica de ingresos, gastos y ahorros por mes">
        Ingresos, gastos y ahorros mensuales.
      </canvas>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { Chart, BarController, BarElement, CategoryScale, LinearScale, Tooltip } from 'chart.js'

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip)

const props = defineProps<{
  labels: string[]
  datasets: { label: string; data: number[]; color: string }[]
}>()

const canvasRef = ref<HTMLCanvasElement>()
let chart: Chart | null = null

const datasets = props.datasets

function buildChart() {
  if (!canvasRef.value) return
  chart?.destroy()
  chart = new Chart(canvasRef.value, {
    type: 'bar',
    data: {
      labels: props.labels,
      datasets: props.datasets.map(d => ({
        label: d.label,
        data: d.data,
        backgroundColor: d.color,
        borderRadius: 4,
        borderSkipped: false,
      }))
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false }, tooltip: { mode: 'index' } },
      scales: {
        x: { grid: { display: false }, ticks: { font: { size: 11 }, color: '#7a8aa0' } },
        y: {
          grid: { color: 'rgba(99,179,255,0.05)' },
          ticks: {
            font: { size: 11 }, color: '#7a8aa0',
            callback: (v) => Number(v) >= 1000000
              ? (Number(v) / 1000000).toFixed(1) + 'M'
              : Number(v) >= 1000 ? (Number(v) / 1000) + 'k' : v
          }
        }
      }
    }
  })
}

onMounted(buildChart)
watch(() => props.datasets, buildChart, { deep: true })
</script>

<style scoped>
.legend {
  display: flex;
  gap: 1rem;
  margin-bottom: 0.75rem;
  flex-wrap: wrap;
}
.legend-item {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.68rem;
  color: var(--text-secondary);
}
.legend-dot {
  width: 10px;
  height: 10px;
  border-radius: 2px;
}
</style>