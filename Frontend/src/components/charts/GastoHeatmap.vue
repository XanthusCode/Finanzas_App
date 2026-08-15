<template>
  <div class="heatmap-wrap">
    <div class="heatmap-grid">
      <div v-for="(semana, wi) in weeks" :key="wi" class="heatmap-week">
        <div
          v-for="(dia, di) in semana"
          :key="di"
          class="heatmap-day"
          :class="{ 'heatmap-day--empty': !dia.enAnio }"
          :style="dia.enAnio ? { background: colorFor(dia.total) } : {}"
          :title="dia.enAnio ? `${dia.fecha}: ${fmt(dia.total)}` : ''"
        />
      </div>
    </div>
    <div class="heatmap-legend">
      <span>Menos</span>
      <span class="heatmap-day" :style="{ background: colorFor(0) }" />
      <span class="heatmap-day" :style="{ background: colorFor(max * 0.15) }" />
      <span class="heatmap-day" :style="{ background: colorFor(max * 0.4) }" />
      <span class="heatmap-day" :style="{ background: colorFor(max * 0.65) }" />
      <span class="heatmap-day" :style="{ background: colorFor(max) }" />
      <span>Más</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { HeatmapDia } from '@/types'

const props = defineProps<{ data: HeatmapDia[]; anio: number }>()

interface Celda { fecha: string; total: number; enAnio: boolean }

function toDateStr(d: Date) {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const mapaTotales = computed(() => {
  const m: Record<string, number> = {}
  for (const d of props.data) m[d.fecha.slice(0, 10)] = d.total
  return m
})

const weeks = computed<Celda[][]>(() => {
  const inicioAnio = new Date(props.anio, 0, 1)
  const finAnio = new Date(props.anio, 11, 31)

  // Alinea al lunes anterior (o igual) al 1 de enero, para que las semanas empiecen en lunes.
  const diaSemana = (inicioAnio.getDay() + 6) % 7
  const cursor = new Date(inicioAnio)
  cursor.setDate(cursor.getDate() - diaSemana)

  const result: Celda[][] = []
  while (cursor <= finAnio) {
    const semana: Celda[] = []
    for (let i = 0; i < 7; i++) {
      const enAnio = cursor >= inicioAnio && cursor <= finAnio
      const fecha = toDateStr(cursor)
      semana.push({ fecha, total: mapaTotales.value[fecha] ?? 0, enAnio })
      cursor.setDate(cursor.getDate() + 1)
    }
    result.push(semana)
  }
  return result
})

const max = computed(() => Math.max(1, ...props.data.map(d => d.total)))

function colorFor(total: number) {
  if (total <= 0) return 'var(--border)'
  const ratio = total / max.value
  if (ratio > 0.65) return 'var(--red)'
  if (ratio > 0.4)  return 'color-mix(in srgb, var(--red) 70%, transparent)'
  if (ratio > 0.15) return 'color-mix(in srgb, var(--red) 45%, transparent)'
  return 'color-mix(in srgb, var(--red) 22%, transparent)'
}

const fmt = (n: number) => '$' + Math.round(n).toLocaleString('es-CO')
</script>

<style scoped>
.heatmap-wrap { display: flex; flex-direction: column; gap: 0.6rem; }
.heatmap-grid { display: flex; gap: 3px; overflow-x: auto; padding-bottom: 0.25rem; }
.heatmap-week { display: flex; flex-direction: column; gap: 3px; }
.heatmap-day { width: 11px; height: 11px; border-radius: 2px; flex-shrink: 0; }
.heatmap-day--empty { background: transparent !important; }
.heatmap-legend { display: flex; align-items: center; gap: 4px; font-size: 0.62rem; color: var(--text-muted); }
.heatmap-legend .heatmap-day { width: 10px; height: 10px; }
</style>
