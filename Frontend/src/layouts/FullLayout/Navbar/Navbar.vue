<template>
  <header class="topbar">
    <div class="topbar-left">
      <button class="hamburger" @click="$emit('toggle-sidebar')">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="18" x2="21" y2="18"/></svg>
      </button>
      <span class="topbar-page">{{ pageTitle }}</span>
    </div>
    <div class="topbar-right">
      <button class="theme-toggle" title="Cambiar tema" @click="toggleTheme">
        <Sun v-if="theme === 'dark'" :size="16" :stroke-width="2" />
        <Moon v-else :size="16" :stroke-width="2" />
      </button>
      <UserMenu />
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { Sun, Moon } from 'lucide-vue-next'
import UserMenu from '@/components/common/UserMenu.vue'
import { useAppearance } from '@/composables/useAppearance'

defineEmits(['toggle-sidebar'])

const route = useRoute()
const { theme, toggleTheme } = useAppearance()

const PAGE_TITLES: Record<string, string> = {
  '/dashboard':   'Dashboard',
  '/gastos':      'Gastos',
  '/ingresos':    'Ingresos',
  '/categorias':  'Categorías',
  '/presupuesto': 'Presupuesto',
  '/metas':       'Metas',
  '/deudas':      'Deudas',
  '/cuotas':      'Cuotas',
  '/cuentas':     'Cuentas',
  '/simulador':   'Simulador',
  '/proyeccion':  'Proyección',
  '/historial':   'Historial',
}

const pageTitle = computed(() => PAGE_TITLES[route.path] ?? '')
</script>

<style scoped>
.topbar {
  position: sticky;
  top: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 2.5rem;
  height: 56px;
  background: var(--bg);
  border-bottom: 1px solid var(--border);
}

.topbar-left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.topbar-page {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--text-secondary);
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.hamburger {
  display: none;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-secondary);
  padding: 4px;
  border-radius: 6px;
  transition: color 0.15s, background 0.15s;
  align-items: center;
  justify-content: center;
}
.hamburger:hover { color: var(--text-primary); background: var(--surface2); }

.topbar-right {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.theme-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-secondary);
  padding: 6px;
  border-radius: 6px;
  transition: color 0.15s, background 0.15s;
}
.theme-toggle:hover { color: var(--accent); background: var(--surface2); }

@media (max-width: 768px), (max-width: 1024px) and (orientation: portrait) {
  .topbar {
    padding: 0 1rem;
  }

  .hamburger {
    display: flex;
  }
}
</style>
