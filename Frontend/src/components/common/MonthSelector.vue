<template>
  <div class="month-selector">
    <button class="arrow" aria-label="Mes anterior" @click="prev"><ChevronLeft :size="16" :stroke-width="2" /></button>
    <span class="label">{{ label }}</span>
    <button class="arrow" aria-label="Mes siguiente" @click="next"><ChevronRight :size="16" :stroke-width="2" /></button>
  </div>
</template>

<script setup lang="ts">
import { ChevronLeft, ChevronRight } from '@lucide/vue'
import { computed } from 'vue'
import { format } from 'date-fns'
import { es } from 'date-fns/locale'

const props = defineProps<{ mes: number; anio: number }>()
const emit = defineEmits<{ change: [mes: number, anio: number] }>()

const label = computed(() => {
  const s = format(new Date(props.anio, props.mes - 1, 1), 'MMMM yyyy', { locale: es })
  return s.charAt(0).toUpperCase() + s.slice(1)
})

function prev() {
  if (props.mes === 1) emit('change', 12, props.anio - 1)
  else emit('change', props.mes - 1, props.anio)
}

function next() {
  if (props.mes === 12) emit('change', 1, props.anio + 1)
  else emit('change', props.mes + 1, props.anio)
}
</script>

<style scoped>
/* Sutil: no compite con el botón principal de cada vista */
.month-selector {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  height: 38px;
  padding: 0 0.25rem;
  background: var(--surface2);
  border: 1px solid transparent;
  border-radius: 10px;
  transition: border-color 0.15s ease;
}
.month-selector:hover { border-color: var(--border); }
.label {
  font-size: 0.78rem;
  font-weight: 500;
  color: var(--text-primary);
  min-width: 110px;
  text-align: center;
  text-transform: capitalize;
  font-variant-numeric: tabular-nums;
}
.arrow {
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;
  padding: 0;
  background: none;
  border: none;
  border-radius: 8px;
  color: var(--text-secondary);
  cursor: pointer;
  transition: color 0.15s ease, background 0.15s ease;
}
.arrow:hover { color: var(--text-primary); background: var(--surface); }
.arrow:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }

@media (max-width: 500px) {
  .month-selector { justify-content: space-between; }
}
</style>
