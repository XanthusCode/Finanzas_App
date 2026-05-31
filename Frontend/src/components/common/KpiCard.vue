<template>
  <div class="kpi">
    <div class="kpi-label">{{ label }}</div>
    <div class="kpi-value" :style="{ color: color }">{{ formatted }}</div>
    <div v-if="sub" class="kpi-sub">{{ sub }}</div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  label: string
  value: number
  color?: string
  sub?: string
}>()

const formatted = computed(() =>
  '$' + Math.round(props.value).toLocaleString('es-CO')
)
</script>

<style scoped>
.kpi {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1rem 1.1rem;
}
.kpi-label {
  font-size: 0.62rem;
  color: var(--text-secondary);
  letter-spacing: 0.1em;
  text-transform: uppercase;
  margin-bottom: 0.4rem;
}
.kpi-value {
  font-family: var(--ff-display);
  font-size: 1.3rem;
  font-weight: 700;
  color: var(--text-primary);
}
.kpi-sub {
  font-size: 0.65rem;
  color: var(--text-muted);
  margin-top: 0.25rem;
}
</style>