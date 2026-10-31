<template>
  <header class="topbar-wrap">
    <div class="topbar">
      <div class="topbar-left">
        <button
          class="icon-btn"
          :aria-label="collapsed ? 'Expandir menú' : 'Colapsar menú'"
          :title="collapsed ? 'Expandir menú' : 'Colapsar menú'"
          @click="$emit('toggle-sidebar')"
        >
          <Menu class="only-mobile" :size="18" :stroke-width="1.8" />
          <PanelLeftOpen v-if="collapsed" class="only-desktop" :size="18" :stroke-width="1.8" />
          <PanelLeftClose v-else class="only-desktop" :size="18" :stroke-width="1.8" />
        </button>

        <span class="topbar-sep" aria-hidden="true" />

        <nav class="breadcrumb" aria-label="Ubicación">
          <span v-if="current.group" class="crumb-group">{{ current.group }}</span>
          <ChevronRight v-if="current.group" :size="14" :stroke-width="2" class="crumb-chevron" />
          <span class="crumb-page">
            <component :is="current.icon" v-if="current.icon" :size="16" :stroke-width="1.9" class="crumb-icon" />
            {{ current.title }}
          </span>
        </nav>
      </div>

      <div class="topbar-right">
        <button class="search-trigger" aria-label="Buscar (Ctrl + K)" @click="paletaAbierta = true">
          <Search :size="15" :stroke-width="2" />
          <span class="search-trigger-text">Buscar…</span>
          <span class="search-trigger-kbd"><kbd>{{ esMac ? '⌘' : 'Ctrl' }}</kbd><kbd>K</kbd></span>
        </button>

        <button
          class="icon-btn icon-btn--bordered"
          :aria-label="theme === 'dark' ? 'Cambiar a tema claro' : 'Cambiar a tema oscuro'"
          :title="theme === 'dark' ? 'Tema claro' : 'Tema oscuro'"
          @click="toggleTheme"
        >
          <Transition name="swap" mode="out-in">
            <Sun v-if="theme === 'dark'" key="sun" :size="16" :stroke-width="1.9" />
            <Moon v-else key="moon" :size="16" :stroke-width="1.9" />
          </Transition>
        </button>

        <span class="topbar-sep" aria-hidden="true" />

        <UserMenu />
      </div>
    </div>
    <CommandPalette v-model="paletaAbierta" />
  </header>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, onBeforeUnmount, type Component } from 'vue'
import { useRoute } from 'vue-router'
import { Sun, Moon, Menu, PanelLeftClose, PanelLeftOpen, ChevronRight, UserRound, Search } from 'lucide-vue-next'
import UserMenu from '@/components/common/UserMenu.vue'
import CommandPalette from '@/components/common/CommandPalette.vue'
import { useAppearance } from '@/composables/useAppearance'
import { menu } from '../Sidebar/SidebarItems'

defineProps<{ collapsed?: boolean }>()
defineEmits(['toggle-sidebar'])

const route = useRoute()
const { theme, toggleTheme } = useAppearance()

// Rutas fuera del menú lateral
const EXTRA_PAGES: Record<string, { group: string; title: string; icon: Component }> = {
  '/perfil': { group: 'Cuenta', title: 'Perfil', icon: UserRound },
}

const current = computed<{ group: string; title: string; icon?: Component }>(() => {
  for (const section of menu) {
    const item = section.items.find(i => i.route === route.path)
    if (item) return { group: section.label, title: item.title, icon: item.icon }
  }
  return EXTRA_PAGES[route.path] ?? { group: '', title: '' }
})

// Búsqueda global: Ctrl + K (Windows/Linux) o ⌘ + K (Mac), también "/" fuera de inputs
const paletaAbierta = ref(false)
const esMac = /Mac|iPhone|iPad/.test(navigator.platform || navigator.userAgent)

function onAtajo(e: KeyboardEvent) {
  if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
    e.preventDefault()
    paletaAbierta.value = !paletaAbierta.value
    return
  }
  const el = e.target as HTMLElement | null
  const escribiendo = !!el && (el.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(el.tagName))
  if (e.key === '/' && !escribiendo && !paletaAbierta.value) {
    e.preventDefault()
    paletaAbierta.value = true
  }
}
onMounted(() => window.addEventListener('keydown', onAtajo))
onBeforeUnmount(() => window.removeEventListener('keydown', onAtajo))
</script>

<style scoped>
/* Envoltura sticky: degradado del fondo para que el contenido se desvanezca bajo el navbar */
.topbar-wrap {
  position: sticky;
  top: 0;
  z-index: 50;
  padding: 12px 12px 0;
  background: linear-gradient(to bottom, var(--bg) 60%, transparent);
}

.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  height: 56px;
  padding: 0 0.6rem 0 0.5rem;
  background: color-mix(in srgb, var(--surface) 78%, transparent);
  backdrop-filter: blur(12px) saturate(140%);
  -webkit-backdrop-filter: blur(12px) saturate(140%);
  border: 1px solid var(--border);
  border-radius: 16px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1), 0 10px 28px -18px rgba(0, 0, 0, 0.35);
}

.topbar-left,
.topbar-right {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  min-width: 0;
}

.topbar-sep {
  width: 1px;
  height: 22px;
  background: var(--border-hover);
  flex-shrink: 0;
}

/* ── Botones de ícono ── */
.icon-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  width: 38px;
  height: 38px;
  padding: 0;
  color: var(--text-secondary);
  background: transparent;
  border: 1px solid transparent;
  border-radius: 10px;
  cursor: pointer;
  transition: color 0.15s ease, background 0.15s ease, border-color 0.15s ease, transform 0.1s ease;
}
.icon-btn--bordered { border-color: var(--border); }
.icon-btn:hover {
  color: var(--text-primary);
  background: var(--surface2);
  border-color: var(--border-hover);
}
.icon-btn:active { transform: scale(0.95); }
.icon-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 2px; }

.only-mobile { display: none; }

/* ── Breadcrumb ── */
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  min-width: 0;
  white-space: nowrap;
}
.crumb-group {
  font-size: 0.8rem;
  color: var(--text-subtle);
  letter-spacing: 0.02em;
}
.crumb-chevron { color: var(--text-muted); flex-shrink: 0; opacity: 0.8; }
.crumb-page {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  font-size: 0.88rem;
  font-weight: 600;
  color: var(--text-primary);
  letter-spacing: 0.02em;
  overflow: hidden;
  text-overflow: ellipsis;
}
.crumb-icon { color: var(--accent); flex-shrink: 0; }

/* ── Disparador de búsqueda ── */
.search-trigger {
  display: inline-flex; align-items: center; gap: 0.55rem;
  height: 38px; min-width: 220px; padding: 0 0.5rem 0 0.75rem;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  color: var(--text-muted);
  font: inherit; font-size: 0.78rem;
  cursor: pointer;
  transition: border-color 0.15s ease, color 0.15s ease;
}
.search-trigger:hover { border-color: var(--border-hover); color: var(--text-secondary); }
.search-trigger:focus-visible { outline: none; box-shadow: var(--focus-ring); }
.search-trigger-text { flex: 1; text-align: left; }
.search-trigger-kbd { display: inline-flex; gap: 3px; }
.search-trigger kbd {
  display: inline-grid; place-items: center;
  min-width: 20px; height: 20px; padding: 0 0.3rem;
  font-family: var(--ff-mono); font-size: 0.6rem; color: var(--text-secondary);
  background: var(--surface); border: 1px solid var(--border-hover); border-radius: 5px;
}

/* Cambio de ícono sol/luna */
.swap-enter-active, .swap-leave-active { transition: opacity 0.12s ease, transform 0.15s ease; }
.swap-enter-from { opacity: 0; transform: rotate(-60deg) scale(0.7); }
.swap-leave-to { opacity: 0; transform: rotate(60deg) scale(0.7); }

/* ── Mobile / tablet vertical ── */
@media (max-width: 768px), (max-width: 1024px) and (orientation: portrait) {
  .topbar-wrap { padding: 10px 10px 0; }
  .topbar { height: 52px; padding: 0 0.45rem; border-radius: 14px; }

  .only-mobile { display: block; }
  .only-desktop { display: none; }

  .search-trigger { min-width: 0; width: 38px; padding: 0; justify-content: center; }
  .search-trigger-text,
  .search-trigger-kbd,
  .crumb-group,
  .crumb-chevron,
  .topbar-right .topbar-sep { display: none; }
}

@media (max-width: 400px) {
  .topbar-left > .topbar-sep { display: none; }
}
</style>