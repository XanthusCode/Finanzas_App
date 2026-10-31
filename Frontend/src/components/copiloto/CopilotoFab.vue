<template>
  <button class="zaldo-chat-trigger" :aria-label="abierto ? 'Cerrar copiloto' : 'Abrir copiloto'" @click="toggle">
    <span v-if="abierto"><X /></span>
    <BotMessageSquare  v-else />
  </button>
  <CopilotoPanel />
</template>

<script setup lang="ts">
import { useCopiloto } from '@/composables/useCopiloto'
import CopilotoPanel from './CopilotoPanel.vue'
import { BotMessageSquare , X } from 'lucide-vue-next'

const { abierto, toggle } = useCopiloto()
</script>

<style scoped>
.zaldo-chat-trigger {
  position: fixed; bottom: 24px; right: 24px; z-index: 200;
  width: 56px; height: 56px; border-radius: 50%; border: none;
  background: var(--avatar-gradient); color: #fff; font-size: 22px;
  display: grid; place-items: center; cursor: pointer;
  box-shadow: var(--shadow-pop);
  transition: transform 0.2s var(--ease-out);
}
.zaldo-chat-trigger:hover { transform: translateY(-2px) scale(1.04); }
.zaldo-chat-trigger:focus-visible { outline: none; box-shadow: var(--focus-ring); }
</style>
