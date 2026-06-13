<template>
  <Teleport to="body">
    <div class="toast-stack" aria-live="polite" aria-atomic="false">
      <TransitionGroup name="toast">
        <div
          v-for="t in toasts"
          :key="t.id"
          :class="['toast', `toast--${t.type}`]"
          role="status"
          @click="dismiss(t.id)"
        >
          <svg v-if="t.type === 'success'" class="toast-icon" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
          <svg v-else-if="t.type === 'error'" class="toast-icon" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          <svg v-else class="toast-icon" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
          <span class="toast-msg">{{ t.message }}</span>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { useToast } from '@/composables/useToast'

const { toasts, dismiss } = useToast()
</script>

<style scoped>
.toast-stack {
  position: fixed;
  bottom: 1.75rem;
  right: 1.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
  z-index: 9999;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.55rem 0.85rem;
  border-radius: 6px;
  border: 1px solid;
  font-family: var(--ff-mono);
  font-size: 0.72rem;
  letter-spacing: 0.03em;
  pointer-events: all;
  cursor: pointer;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
  max-width: 300px;
  background: var(--surface);
}

.toast--success { border-color: rgba(52, 211, 153, 0.35); }
.toast--error   { border-color: rgba(248, 113, 113, 0.35); }
.toast--info    { border-color: rgba(99, 179, 255, 0.35); }

.toast-icon { flex-shrink: 0; }
.toast--success .toast-icon { color: var(--green); }
.toast--error   .toast-icon { color: var(--red); }
.toast--info    .toast-icon { color: var(--accent); }

.toast-msg { color: var(--text-primary); font-size: 0.78rem; }

.toast-enter-active { transition: opacity 0.22s ease, transform 0.22s ease; }
.toast-leave-active { transition: opacity 0.18s ease, transform 0.18s ease; }
.toast-enter-from   { opacity: 0; transform: translateX(16px); }
.toast-leave-to     { opacity: 0; transform: translateX(16px); }
.toast-move         { transition: transform 0.22s ease; }
</style>
