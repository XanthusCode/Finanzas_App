<template>
  <div class="donut-wrap">
    <canvas ref="canvasRef" />
    <div v-if="centerLabel" class="donut-center">
      <div class="donut-pct">{{ centerLabel }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onBeforeUnmount } from 'vue'
import { Chart, DoughnutController, ArcElement, Tooltip } from 'chart.js'

Chart.register(DoughnutController, ArcElement, Tooltip)

const props = defineProps<{
  data: number[]
  labels: string[]
  colors: string[]
  centerLabel?: string
}>()

const canvasRef = ref<HTMLCanvasElement>()
let chart: Chart | null = null

function buildChart() {
  if (!canvasRef.value) return
  chart?.destroy()
  chart = new Chart(canvasRef.value, {
    type: 'doughnut',
    data: {
      labels: props.labels,
      datasets: [{
        data: props.data,
        backgroundColor: props.colors,
        borderWidth: 0,
        hoverOffset: 4,
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      cutout: '72%',
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (ctx) => ` ${ctx.label}: $${Math.round(ctx.raw as number).toLocaleString('es-CO')}`
          }
        }
      }
    }
  })
}

onMounted(buildChart)
watch(() => props.data, buildChart, { deep: true })
onBeforeUnmount(() => chart?.destroy())
</script>

<style scoped>
.donut-wrap {
  position: relative;
  width: 160px;
  height: 160px;
  flex-shrink: 0;
}
.donut-center {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}
.donut-pct {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--text-primary);
}
</style>
