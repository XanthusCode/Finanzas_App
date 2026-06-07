<template>
  <header class="topbar">
      <span class="topbar-page">{{ pageTitle }}</span>
      <UserMenu />
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import UserMenu from '@/components/common/UserMenu.vue'

const route = useRoute()

const PAGE_TITLES: Record<string, string> = {
  '/':           'Dashboard',
  '/dashboard':  'Dashboard',
  '/gastos':     'Gastos',
  '/ingresos':   'Ingresos',
  '/categorias': 'Categorías',
}

const pageTitle = computed(() => PAGE_TITLES[route.path] ?? '')
</script>

<style scoped>
/* ── Topbar ── */
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

.topbar-page {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--text-secondary);
  letter-spacing: 0.04em;
  text-transform: uppercase;
}
</style>