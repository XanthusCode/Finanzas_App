<template>
  <section id="features" class="features">
    <div class="features-inner">
      <div class="section-label" style="justify-content:center">Funciones</div>
      <h2 class="features-title">Todo lo que necesitas para controlar tu dinero</h2>
      <p class="features-sub">Desde el gasto de todos los días hasta tus metas a largo plazo.</p>

      <div class="features-grid">
        <div v-for="f in features" :key="f.title" class="feature-card">
          <div class="feature-icon">
            <component :is="f.icon" :size="18" :stroke-width="1.8" />
          </div>
          <h3 class="feature-title">{{ f.title }}</h3>
          <p class="feature-desc">{{ f.desc }}</p>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { features } from '@/composables/useLanding'
</script>

<style scoped>
.features { padding: 5rem 2rem; scroll-margin-top: 5rem; }
.features-inner { max-width: 1120px; margin: 0 auto; text-align: center; }

.features-title {
  font-size: 1.9rem;
  font-weight: 800;
  letter-spacing: -0.01em;
  color: var(--text-primary);
  margin: 0.5rem 0 0.6rem;
}
.features-sub { color: var(--text-secondary); font-size: 0.88rem; margin-bottom: 3rem; }

.features-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.25rem;
  text-align: left;
}

.feature-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.5rem;
  transition: border-color 0.2s, transform 0.2s;
}
.feature-card:hover { border-color: var(--border-hover); transform: translateY(-2px); }

.feature-icon {
  width: 38px;
  height: 38px;
  border-radius: 9px;
  background: rgba(99,179,255,0.1);
  color: var(--accent);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1rem;
}

.feature-title { font-size: 0.95rem; font-weight: 700; color: var(--text-primary); margin-bottom: 0.4rem; }
.feature-desc { font-size: 0.8rem; color: var(--text-secondary); line-height: 1.55; }

@media (max-width: 860px) {
  .features-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 560px) {
  .features-grid { grid-template-columns: 1fr; }
}
@media (max-width: 600px) {
  .features { padding: 3rem 1.25rem; }
  .features-title { font-size: 1.5rem; }
  .features-sub { margin-bottom: 2rem; }
}
</style>
