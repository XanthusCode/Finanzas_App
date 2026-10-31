<template>
  <section id="how-it-works" class="l-section how">
    <div class="l-container">
      <div class="l-section-head reveal">
        <span class="l-eyebrow">Cómo funciona</span>
        <h2 class="l-h2">Empieza en <span class="l-grad">3 pasos</span></h2>
        <p class="l-lead">Sin configuraciones eternas ni conectar tu banco.</p>
      </div>

      <ol class="steps">
        <li v-for="(s, i) in steps" :key="s.title" class="step l-glass reveal" :style="{ transitionDelay: `${i * 90}ms` }">
          <div class="step-top">
            <span class="l-icon" :style="{ '--ic': COLORS[i] }"><component :is="s.icon" :size="19" :stroke-width="1.9" /></span>
            <span class="step-num">0{{ i + 1 }}</span>
          </div>
          <h3>{{ s.title }}</h3>
          <p>{{ s.desc }}</p>
        </li>
      </ol>
    </div>
  </section>
</template>

<script setup lang="ts">
import { steps } from '@/composables/useLanding'

const COLORS = ['#60a5fa', '#34d399', '#a78bfa']
</script>

<style scoped>
.steps {
  position: relative;
  list-style: none; padding: 0;
  display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem;
}
/* Línea que conecta los pasos */
.steps::before {
  content: '';
  position: absolute; top: 44px; left: 10%; right: 10%; height: 1px;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.14), transparent);
}
.step { position: relative; display: flex; flex-direction: column; gap: 0.6rem; padding: 1.5rem; }
.step-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.6rem; }
.step-num {
  font-size: 2.2rem; font-weight: 800; letter-spacing: -0.05em; line-height: 1;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.28), rgba(255, 255, 255, 0.04));
  -webkit-background-clip: text; background-clip: text; color: transparent;
}
.step h3 { font-size: 1.05rem; font-weight: 700; letter-spacing: -0.015em; }
.step p  { font-size: 0.86rem; line-height: 1.6; color: var(--l-sub); }

@media (max-width: 800px) {
  .steps { grid-template-columns: 1fr; }
  .steps::before { display: none; }
}
</style>
