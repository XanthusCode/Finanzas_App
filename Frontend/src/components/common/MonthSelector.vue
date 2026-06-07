<template>
  <div class="month-selector">
    <button class="arrow" @click="prev">&#8249;</button>
    <span class="label">{{ label }}</span>
    <button class="arrow" @click="next">&#8250;</button>
  </div>
</template>

<script setup lang="ts">
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
.month-selector {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 6px;
  padding: 0.35rem 0.75rem;
}
.label {
  font-size: 0.78rem;
  color: var(--text-primary);
  min-width: 110px;
  text-align: center;
}
.arrow {
  background: none;
  border: none;
  color: var(--text-secondary);
  font-size: 1.1rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
  transition: color 0.2s;
}
.arrow:hover { color: var(--text-primary); }
</style>