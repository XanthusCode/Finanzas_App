<template>
  <button class="zaldo-chat-trigger" :aria-label="abierto ? 'Cerrar copiloto' : 'Abrir copiloto'" @click="toggle">
    <X v-if="abierto" :size="20" :stroke-width="1.5" />
    <MessageCircle v-else :size="20" :stroke-width="1.5" />
  </button>
  <CopilotoPanel />
</template>

<script setup lang="ts">
import { useCopiloto } from '@/composables/useCopiloto'
import CopilotoPanel from './CopilotoPanel.vue'
import { MessageCircle, X } from '@lucide/vue'

const { abierto, toggle } = useCopiloto()
</script>

<style scoped>
.zaldo-chat-trigger {
  position: fixed; bottom: 24px; right: 24px; z-index: 200;
  width: 56px; height: 56px; border-radius: 50%; border: none;
  background: var(--surface2); color: var(--text-primary);
  outline: 1px solid var(--border-hover);
  display: grid; place-items: center; cursor: pointer;
  box-shadow: var(--shadow-pop);
  transition: background-color 0.15s var(--ease-out), transform 0.12s var(--ease-out);
}
.zaldo-chat-trigger:hover { background: var(--border-hover); }
.zaldo-chat-trigger:active { transform: scale(0.97); }
.zaldo-chat-trigger:focus-visible { outline: none; box-shadow: var(--focus-ring); }
</style>
