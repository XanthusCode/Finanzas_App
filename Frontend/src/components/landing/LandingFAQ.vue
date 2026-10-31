<template>
  <section id="faq" class="l-section faq">
    <div class="l-container faq-inner">
      <div class="l-section-head reveal">
        <span class="l-eyebrow">Ayuda</span>
        <h2 class="l-h2">Preguntas frecuentes</h2>
        <p class="l-lead">¿Algo más? Escríbenos a <a href="mailto:soporte@zaldo.app">soporte@zaldo.app</a>.</p>
      </div>

      <div class="faq-list l-glass reveal">
        <div v-for="(item, i) in faqItems" :key="item.question" class="faq-item" :class="{ 'is-open': openIndex === i }">
          <button
            :id="`faq-q-${i}`"
            class="faq-question"
            :aria-expanded="openIndex === i"
            :aria-controls="`faq-a-${i}`"
            @click="toggle(i)"
          >
            <span>{{ item.question }}</span>
            <span class="faq-toggle"><Plus :size="16" :stroke-width="2.2" /></span>
          </button>
          <div :id="`faq-a-${i}`" class="faq-answer" role="region" :aria-labelledby="`faq-q-${i}`" :aria-hidden="openIndex !== i" :inert="openIndex !== i">
            <div class="faq-answer-inner"><p>{{ item.answer }}</p></div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Plus } from 'lucide-vue-next'
import { faqItems } from '@/composables/useLanding'

const openIndex = ref<number | null>(0)

function toggle(i: number) {
  openIndex.value = openIndex.value === i ? null : i
}
</script>

<style scoped>
.faq-inner { max-width: 820px; }
.l-lead a { color: var(--l-blue); text-decoration: none; }
.l-lead a:hover { text-decoration: underline; }

.faq-list { padding: 0.4rem; }
.faq-item { border-radius: 14px; transition: background 0.2s ease; }
.faq-item + .faq-item { margin-top: 2px; }
.faq-item.is-open { background: rgba(255, 255, 255, 0.04); }
.faq-question {
  display: flex; align-items: center; justify-content: space-between; gap: 1rem;
  width: 100%; padding: 1.05rem 1.1rem;
  background: none; border: none; border-radius: 14px;
  font-size: 0.95rem; font-weight: 600; color: var(--l-text); text-align: left;
  cursor: pointer;
}
.faq-question:hover { background: rgba(255, 255, 255, 0.03); }
.faq-question:focus-visible { outline: 2px solid var(--l-blue); outline-offset: -2px; }
.faq-toggle {
  display: grid; place-items: center; flex-shrink: 0;
  width: 28px; height: 28px; border-radius: 9px;
  color: var(--l-sub); background: rgba(255, 255, 255, 0.05);
  transition: transform 0.25s var(--ease-out), color 0.2s ease, background 0.2s ease;
}
.is-open .faq-toggle { transform: rotate(45deg); color: var(--l-text); background: rgba(96, 165, 250, 0.18); }

/* Altura animada con grid-template-rows 0fr → 1fr */
.faq-answer { display: grid; grid-template-rows: 0fr; transition: grid-template-rows 0.3s var(--ease-out); }
.is-open .faq-answer { grid-template-rows: 1fr; }
.faq-answer-inner { overflow: hidden; }
.faq-answer p { padding: 0 1.1rem 1.1rem; font-size: 0.88rem; line-height: 1.65; color: var(--l-sub); }
</style>
