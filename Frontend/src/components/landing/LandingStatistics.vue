<template>
  <section class="stats">
    <div class="l-container">
      <div class="stats-strip l-glass reveal">
        <div v-for="s in stats" :key="s.label" class="stat">
          <span class="stat-value">{{ s.value }}</span>
          <span class="stat-label">{{ s.label }}</span>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { stats } from '@/composables/useLanding'
</script>

<style scoped>
.stats { position: relative; padding: 1rem 0 2rem; }
.stats-strip {
  display: grid; grid-template-columns: repeat(3, 1fr);
  padding: 1.6rem 1rem;
}
.stat {
  display: flex; flex-direction: column; align-items: center; gap: 0.35rem;
  padding: 0 1rem; text-align: center;
}
.stat + .stat { border-left: 1px solid var(--l-line); }
.stat-value {
  font-size: clamp(2rem, 4vw, 2.8rem); font-weight: 800; letter-spacing: -0.04em; line-height: 1;
  background: linear-gradient(180deg, #fff, #94a3b8);
  -webkit-background-clip: text; background-clip: text; color: transparent;
}
.stat-label { font-size: 0.84rem; color: var(--l-sub); max-width: 220px; }

@media (max-width: 640px) {
  .stats-strip { grid-template-columns: 1fr; gap: 1.25rem; padding: 1.4rem 1rem; }
  .stat + .stat { border-left: none; border-top: 1px solid var(--l-line); padding-top: 1.25rem; }
}
</style>
