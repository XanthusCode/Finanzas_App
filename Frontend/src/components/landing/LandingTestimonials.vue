<template>
  <section class="testi">
    <div class="testi-inner">
      <div class="section-label" style="justify-content:center">Lo que dicen</div>
      <h2 class="testi-title">Personas reales, probando en beta</h2>

      <div class="testi-grid">
        <div v-for="t in testimonials" :key="t.quote" class="testi-card">
          <p class="testi-quote">“{{ t.quote }}”</p>
          <p class="testi-author">— {{ t.author }}</p>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { testimonials } from '@/composables/useLanding'
</script>

<style scoped>
.testi { padding: 5rem 2rem; }
.testi-inner { max-width: 1120px; margin: 0 auto; text-align: center; }
.testi-title { font-size: 1.9rem; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0.5rem 0 3rem; }

.testi-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.25rem;
  text-align: left;
}
.testi-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.5rem;
}
.testi-quote { font-size: 0.85rem; color: var(--text-primary); line-height: 1.6; margin-bottom: 1rem; }
.testi-author { font-size: 0.72rem; color: var(--text-secondary); }

@media (max-width: 860px) {
  .testi-grid { grid-template-columns: 1fr; }
}
@media (max-width: 600px) {
  .testi { padding: 3rem 1.25rem; }
  .testi-title { font-size: 1.5rem; margin-bottom: 2rem; }
}
</style>
