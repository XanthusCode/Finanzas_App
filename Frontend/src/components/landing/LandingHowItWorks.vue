<template>
  <section id="how-it-works" class="how">
    <div class="how-inner">
      <div class="section-label" style="justify-content:center">Cómo funciona</div>
      <h2 class="how-title">Empieza en 3 pasos</h2>

      <div class="how-steps">
        <div v-for="(s, i) in steps" :key="s.title" class="how-step">
          <div class="how-step-num">{{ i + 1 }}</div>
          <div class="how-step-icon">
            <component :is="s.icon" :size="20" :stroke-width="1.8" />
          </div>
          <h3 class="how-step-title">{{ s.title }}</h3>
          <p class="how-step-desc">{{ s.desc }}</p>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { steps } from '@/composables/useLanding'
</script>

<style scoped>
.how { padding: 5rem 2rem; scroll-margin-top: 5rem; }
.how-inner { max-width: 1120px; margin: 0 auto; text-align: center; }
.how-title { font-size: 1.9rem; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0.5rem 0 3rem; }

.how-steps {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
  position: relative;
  
}

.how-step { position: relative; border: 1px solid var(--border); padding: 2rem 1.5rem; border-radius: 0.5rem;transition: all 0.2s ease-in-out; }
.how-step-num {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  border: 1px solid var(--border-hover);
  color: var(--accent);
  font-size: 0.72rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  position: absolute;
  top: 10px;
  left: 10px;
}
.how-step-icon {
  width: 46px;
  height: 46px;
  border-radius: 50%;
  background: rgba(99,179,255,0.1);
  color: var(--accent);
  
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
}
.how-step-title { font-size: 0.95rem; font-weight: 700; color: var(--text-primary); margin-bottom: 0.4rem; }
.how-step-desc { font-size: 0.8rem; color: var(--text-secondary); line-height: 1.55; max-width: 260px; margin: 0 auto; }

@media (max-width: 760px) {
  .how-steps { grid-template-columns: 1fr; gap: 2rem; }
}
@media (max-width: 600px) {
  .how { padding: 3rem 1.25rem; }
  .how-title { font-size: 1.5rem; margin-bottom: 2rem; }
}
</style>
