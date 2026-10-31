<template>
  <div class="user-menu" ref="menuRef">
    <button @mouseenter="showTooltip($event, 'Cerrar sesión')" @mouseleave="hideTooltip" class="dropdown-item dropdown-item--danger" @click="handleLogout">
      <LogOut :size="20" />
    </button>

     <!-- En body: el sticky del aside crea un contexto de apilamiento y el contenido lo taparía -->
    <Teleport to="body">
      <Transition name="tooltip">
        <div
          v-if="tooltip.visible"
          class="sidebar-tooltip"
          :style="{ top: `${tooltip.top}px`, left: `${tooltip.left}px` }"
          role="tooltip"
        >
          {{ tooltip.text }}
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { LogOut } from 'lucide-vue-next'
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const menuRef = ref<HTMLElement | null>(null)
const tooltip = reactive({ visible: false, text: '', top: 0, left: 0 })

function handleLogout() {
  auth.logout()
  router.push('/login')
}

function showTooltip(e: Event, text: string) {
  const rect = (e.currentTarget as HTMLElement).getBoundingClientRect()
  tooltip.text = text
  tooltip.top = rect.bottom + 10
  tooltip.left = rect.left + rect.width / 2
  tooltip.visible = true
}

function hideTooltip() { tooltip.visible = false }
</script>

<style scoped>
.user-menu {
  position: relative;
}

.dropdown-item {
  display: flex;
  align-items: center;
  width: 100%;
  padding: 0.65rem;
  font-size: 0.78rem;
  color: var(--text-secondary);
  background: none;
  border: none;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
  text-align: left;
}
.dropdown-item:hover { background: var(--surface2); color: var(--text-primary); }
.dropdown-item--danger:hover { background: color-mix(in srgb, var(--red) 8%, transparent); color: var(--red); }

/* Dropdown animation */
.dropdown-enter-active, .dropdown-leave-active { transition: opacity 0.15s, transform 0.15s; }
.dropdown-enter-from, .dropdown-leave-to { opacity: 0; transform: translateY(-6px); }

/* ── Tooltip ) ── */
.sidebar-tooltip {
  position: fixed;
  z-index: 200;
  transform: translateX(-65%);   /* ✅ centra sobre el punto */
  padding: 0.4rem 0.7rem;
  border-radius: 8px;
  background: var(--surface);
  border: 1px solid var(--border-hover);
  color: var(--text-primary);
  font-size: 0.78rem;
  white-space: nowrap;
  pointer-events: none;
  box-shadow: 0 8px 24px -8px rgba(0, 0, 0, 0.4);
}

.tooltip-enter-active { transition: opacity 0.12s ease, transform 0.12s ease; }
.tooltip-leave-active { transition: opacity 0.08s ease; }

/* animación desde arriba (porque ahora aparece abajo) */
.tooltip-enter-from { opacity: 0; transform: translate(-50%, -4px); }
.tooltip-leave-to   { opacity: 0; }   /* deja el transform base intacto */
</style>
