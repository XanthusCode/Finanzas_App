<template>
  <section class="l-section testi">
    <div class="l-container">
      <div class="l-section-head reveal">
        <span class="l-eyebrow">Lo que dicen</span>
        <h2 class="l-h2">Personas reales, <span class="l-grad">probando en beta</span></h2>
      </div>

      <div class="grid">
        <figure v-for="(t, i) in testimonials" :key="t.quote" class="card l-glass reveal" :style="{ transitionDelay: `${i * 90}ms` }">
          <Quote :size="22" :stroke-width="1.8" class="quote-icon" />
          <blockquote>{{ t.quote }}</blockquote>
          <figcaption>
            <span class="avatar" :style="{ background: AVATARS[i % AVATARS.length] }">{{ t.author.charAt(0) }}</span>
            <span class="who"><b>{{ t.author }}</b><small>Zaldo beta · Colombia</small></span>
          </figcaption>
        </figure>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { Quote } from 'lucide-vue-next'
import { testimonials } from '@/composables/useLanding'

const AVATARS = [
  'linear-gradient(135deg, #3b82f6, #8b5cf6)',
  'linear-gradient(135deg, #10b981, #3b82f6)',
  'linear-gradient(135deg, #8b5cf6, #f43f5e)',
]
</script>

<style scoped>
.grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; }
.card { display: flex; flex-direction: column; gap: 1rem; margin: 0; padding: 1.5rem; }
.quote-icon { color: var(--l-blue); opacity: 0.7; }
blockquote { flex: 1; margin: 0; font-size: 0.95rem; line-height: 1.6; color: var(--l-text); }
figcaption { display: flex; align-items: center; gap: 0.7rem; padding-top: 1rem; border-top: 1px solid var(--l-line); }
.avatar { display: grid; place-items: center; width: 36px; height: 36px; border-radius: 999px; font-size: 0.82rem; font-weight: 700; color: #fff; }
.who { display: flex; flex-direction: column; gap: 1px; }
.who b { font-size: 0.84rem; font-weight: 600; }
.who small { font-size: 0.72rem; color: var(--l-muted); }

@media (max-width: 860px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
