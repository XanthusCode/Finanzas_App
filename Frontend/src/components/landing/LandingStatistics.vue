<template>
  <section class="stats">
    <div class="stats-inner">
      <div v-for="s in stats" :key="s.label" class="stat-item">
        <div class="stat-value">{{ s.value }}</div>
        <div class="stat-label">{{ s.label }}</div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { stats } from '@/composables/useLanding'
</script>

<style scoped>
.stats {
  padding: 2.5rem 2rem;
  border-top: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  background: var(--surface);
}
.stats-inner {
  max-width: 1120px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
  text-align: center;
}
.stat-value {
  font-family: var(--ff-display);
  font-size: 2.1rem;
  font-weight: 800;
  color: var(--accent);
}
.stat-label { font-size: 0.78rem; color: var(--text-secondary); margin-top: 0.35rem; }

@media (max-width: 640px) {
  .stats-inner { grid-template-columns: 1fr; gap: 1.75rem; }
}
@media (max-width: 600px) {
  .stats { padding: 2rem 1.25rem; }
  .stat-value { font-size: 1.7rem; }
}
</style>
