<template>
  <section id="faq" class="faq">
    <div class="faq-inner">
      <div class="section-label" style="justify-content:center">Ayuda</div>
      <h2 class="faq-title">Preguntas frecuentes</h2>

      <div class="faq-list">
        <div v-for="(item, i) in faqItems" :key="item.question" class="faq-item">
          <button class="faq-question" @click="toggle(i)">
            <span>{{ item.question }}</span>
            <ChevronDown class="faq-chevron" :class="{ 'faq-chevron--open': openIndex === i }" :size="16" :stroke-width="2" />
          </button>
          <div v-if="openIndex === i" class="faq-answer">{{ item.answer }}</div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ChevronDown } from '@lucide/vue'
import { faqItems } from '@/composables/useLanding'

const openIndex = ref<number | null>(0)

function toggle(i: number) {
  openIndex.value = openIndex.value === i ? null : i
}
</script>

<style scoped>
.faq { padding: 5rem 2rem; scroll-margin-top: 5rem; }
.faq-inner { max-width: 720px; margin: 0 auto; text-align: center; }
.faq-title { font-size: 1.9rem; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0.5rem 0 2.5rem; }

.faq-list { text-align: left; display: flex; flex-direction: column; gap: 0.6rem; }
.faq-item {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
}
.faq-question {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  background: none;
  border: none;
  padding: 1rem 1.25rem;
  font-family: var(--ff-body);
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-primary);
  cursor: pointer;
  text-align: left;
}
.faq-chevron { color: var(--text-secondary); flex-shrink: 0; transition: transform 0.2s; }
.faq-chevron--open { transform: rotate(180deg); color: var(--accent); }

.faq-answer {
  padding: 0 1.25rem 1.1rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
  line-height: 1.6;
}
</style>
