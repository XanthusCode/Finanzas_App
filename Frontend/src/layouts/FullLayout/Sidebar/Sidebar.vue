<template>
  <aside class="sidebar" :class="{ 'sidebar--open': open }">
    <div class="sidebar-header">
      <div class="sidebar-logo">
        FINANZAS
      </div>
      <button class="sidebar-close" @click="$emit('close')">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
      </button>
    </div>

   <nav class="sidebar-nav">
      <template
          v-for="(section, index) in menu"
          :key="index"
      >
          <span class="nav-group-label">{{ section.label }}</span>

          <RouterLink
            v-for="item in section.items"
            :key="item.route"
            :to="item.route"
            class="nav-item"
            active-class="nav-item--active"
            @click="$emit('close')"
          >
            <component
              :is="item.icon"
              :size="16"
            />
            {{ item.title }}
          </RouterLink>

          <div
            v-if="index !== menu.length - 1"
            class="nav-divider"
          />
      </template>
    </nav>

    <div class="sidebar-footer">
      <div class="sidebar-year">{{ anio }}</div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { menu } from './SidebarItems'

defineProps<{ open: boolean }>()
defineEmits(['close'])

const anio = new Date().getFullYear()
</script>

<style scoped>
.sidebar {
  background: var(--surface);
  border-right: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  padding: 1.5rem 1rem;
  position: sticky;
  top: 0;
  height: 100vh;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 2.5rem;
  padding: 0 0.5rem;
}

.sidebar-logo {
  font-family: var(--ff-display);
  font-weight: 800;
  font-size: 1.1rem;
  letter-spacing: 0.04em;
  color: var(--text-primary);
}

.sidebar-close {
  display: none;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-muted);
  padding: 4px;
  border-radius: 6px;
  transition: color 0.15s, background 0.15s;
}
.sidebar-close:hover { color: var(--text-primary); background: var(--surface2); }

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  flex: 1;
  overflow-y: auto;
}

.nav-group-label {
  font-size: 0.6rem;
  color: var(--text-muted);
  letter-spacing: 0.1em;
  text-transform: uppercase;
  padding: 0 0.75rem;
  margin: 0.5rem 0 0.15rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.6rem 0.75rem;
  padding-left: calc(0.75rem - 2px);
  border-left: 2px solid transparent;
  border-radius: 6px;
  font-size: 0.78rem;
  color: var(--text-secondary);
  text-decoration: none;
  transition: background 0.15s, color 0.15s, border-color 0.15s;
  letter-spacing: 0.03em;
}
.nav-item:hover { background: var(--surface2); color: var(--text-primary); }
.nav-item--active {
  background: color-mix(in srgb, var(--accent) 8%, transparent);
  color: var(--accent);
  border-left-color: var(--accent);
}

.nav-divider {
  height: 1px;
  background: var(--border);
  margin: 0.5rem 0;
}

.sidebar-footer {
  padding: 0 0.5rem;
}
.sidebar-year {
  font-size: 0.62rem;
  color: var(--text-muted);
  letter-spacing: 0.1em;
}

/* ── Mobile / tablet vertical ── */
@media (max-width: 768px), (max-width: 1024px) and (orientation: portrait) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    height: 100dvh;
    width: 260px;
    z-index: 100;
    transform: translateX(-100%);
    transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    overflow-y: auto;
    padding: 1.25rem 1rem;
  }

  .sidebar--open {
    transform: translateX(0);
  }

  .sidebar-close {
    display: flex;
    align-items: center;
    justify-content: center;
  }
}
</style>

