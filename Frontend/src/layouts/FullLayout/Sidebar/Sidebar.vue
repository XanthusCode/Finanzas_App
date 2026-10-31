<template>
  <aside class="sidebar" :class="{ 'sidebar--open': open, 'sidebar--collapsed': collapsed }">
    <div class="sidebar-panel">
      <div class="sidebar-header">
        <AppLogo class="sidebar-logo" />
        <button class="sidebar-close" aria-label="Cerrar menú" @click="$emit('close')">
          <X :size="16" :stroke-width="2" />
        </button>
      </div>

      <nav class="sidebar-nav" @scroll="hideTooltip">
        <section v-for="section in menu" :key="section.label" class="nav-group">
          <span class="nav-group-label">{{ section.label }}</span>

          <RouterLink
            v-for="item in section.items"
            :key="item.route"
            :to="item.route"
            class="nav-item"
            active-class="nav-item--active"
            :aria-label="item.title"
            @mouseenter="showTooltip($event, item.title)"
            @mouseleave="hideTooltip"
            @focus="showTooltip($event, item.title)"
            @blur="hideTooltip"
            @click="$emit('close')"
          >
            <component :is="item.icon" :size="18" :stroke-width="1.75" class="nav-icon" />
            <span class="nav-text">{{ item.title }}</span>
            <ChevronRight :size="15" :stroke-width="2" class="nav-chevron" />
          </RouterLink>
        </section>
      </nav>

      <RouterLink
        to="/perfil"
        class="sidebar-user"
        active-class="sidebar-user--active"
        @mouseenter="showTooltip($event, user?.nombre ?? 'Perfil')"
        @mouseleave="hideTooltip"
        @click="$emit('close')"
      >
        <span class="user-avatar">{{ initials }}</span>
        <span class="user-info">
          <span class="user-name">{{ user?.nombre }}</span>
          <span class="user-email">{{ user?.email }}</span>
        </span>
      </RouterLink>
    </div>

    <!-- En body: el sticky del aside crea un contexto de apilamiento y el contenido lo taparía -->
    <Teleport to="body">
      <Transition name="tooltip">
        <div
          v-if="collapsed && tooltip.visible"
          class="sidebar-tooltip"
          :style="{ top: `${tooltip.top}px`, left: `${tooltip.left}px` }"
          role="tooltip"
        >
          {{ tooltip.text }}
        </div>
      </Transition>
    </Teleport>
  </aside>
</template>

<script setup lang="ts">
import { computed, reactive } from 'vue'
import { ChevronRight, X } from 'lucide-vue-next'
import { menu } from './SidebarItems'
import AppLogo from '@/components/common/AppLogo.vue'
import { useAuthStore } from '@/stores/auth'
import { userFromToken } from '@/utils/index'

const props = defineProps<{ open: boolean; collapsed?: boolean }>()
defineEmits(['close'])

const auth = useAuthStore()
const user = computed(() => userFromToken(auth.token ?? ''))
const initials = computed(() =>
  (user.value?.nombre ?? '').split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase()
)

// Tooltip con position: fixed para que el overflow del nav no lo recorte
const tooltip = reactive({ visible: false, text: '', top: 0, left: 0 })

function showTooltip(e: Event, text: string) {
  if (!props.collapsed) return
  const rect = (e.currentTarget as HTMLElement).getBoundingClientRect()
  tooltip.text = text
  tooltip.top = rect.top + rect.height / 2
  tooltip.left = rect.right + 12
  tooltip.visible = true
}

function hideTooltip() { tooltip.visible = false }
</script>

<style scoped>
.sidebar {
  position: sticky;
  top: 0;
  z-index: 60;
  height: 100vh;
  padding: 12px 0 12px 12px;
}

.sidebar-panel {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 0 0.75rem 0.75rem;
  box-shadow: var(--shadow-card);
  overflow: hidden;
}

/* ── Header ──
   Misma altura que el navbar (56px - 1px de borde del panel) para que el logo y el
   breadcrumb queden en la misma línea y la línea inferior continúe la del navbar. */
.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 55px;
  margin: 0 -0.75rem 1.25rem;
  padding: 0 1.25rem;
  border-bottom: 1px solid var(--border);
}

.sidebar-logo {
  font-family: var(--ff-display);
  font-weight: 800;
  font-size: 1.15rem;
  letter-spacing: 0.04em;
  color: var(--text-primary);
}

.sidebar-close {
  display: none;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-muted);
  padding: 6px;
  border-radius: 8px;
  transition: color 0.15s, background 0.15s;
}
.sidebar-close:hover { color: var(--text-primary); background: var(--surface2); }

/* ── Navegación ── */
.sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1.75rem;
  overflow-y: auto;
  overflow-x: hidden;
  scrollbar-width: none;
}
.sidebar-nav::-webkit-scrollbar { display: none; }

.nav-group {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.nav-group-label {
  font-size: 11px;
  font-weight: 600;
  color: #64748b;
  letter-spacing: 0.16em;
  text-transform: uppercase;
  padding: 0 0.75rem;
  margin-bottom: 0.5rem;
  white-space: nowrap;
}

.nav-item {
  position: relative;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  height: 40px;
  padding: 0 0.75rem;
  border-radius: 10px;
  font-size: 0.82rem;
  color: var(--text-secondary);
  text-decoration: none;
  letter-spacing: 0.02em;
  transition: background 0.15s ease, color 0.15s ease, transform 0.1s ease;
}
.nav-item:hover { background: var(--surface2); color: var(--text-primary); }
.nav-item:active { transform: scale(0.98); }
.nav-item:focus-visible { outline: 2px solid var(--accent); outline-offset: 2px; }

.nav-icon { flex-shrink: 0; }
.nav-text { flex: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.nav-chevron {
  flex-shrink: 0;
  opacity: 0;
  transform: translateX(-4px);
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.nav-item:hover .nav-chevron { opacity: 0.5; transform: translateX(0); }

/* Activo: tinte de acento + barra lateral, sin gritar */
.nav-item--active,
.nav-item--active:hover {
  background: color-mix(in srgb, var(--accent) 14%, transparent);
  color: var(--accent-strong);
  font-weight: 600;
}
.nav-item--active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  width: 3px;
  height: 18px;
  border-radius: 0 3px 3px 0;
  background: var(--accent-strong);
  transform: translateY(-50%);
}
.nav-item--active .nav-chevron { opacity: 0.8; transform: translateX(0); }

/* ── Usuario ── */
.sidebar-user {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-top: 0.75rem;
  padding: 0.7rem 0.75rem;
  border-radius: 12px;
  border: 1px solid var(--border);
  background: transparent;
  text-decoration: none;
  transition: border-color 0.15s ease, background 0.15s ease;
}
.sidebar-user:hover,
.sidebar-user--active { background: var(--surface2); border-color: var(--border-hover); }

.user-avatar {
  flex-shrink: 0;
  display: grid;
  place-items: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: var(--avatar-gradient);
  box-shadow: 0 0 0 2px var(--surface), 0 0 0 3px var(--border-hover);
  color: #fff;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.04em;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}
.user-name,
.user-email {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.user-name { font-size: 0.78rem; font-weight: 600; color: var(--text-primary); }
.user-name { line-height: 1.2; }
.user-email { font-size: 0.68rem; line-height: 1.2; color: var(--text-subtle); }

/* ── Tooltip (solo colapsado) ── */
.sidebar-tooltip {
  position: fixed;
  z-index: 200;
  transform: translateY(-50%);
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
.tooltip-enter-from { opacity: 0; transform: translate(-4px, -50%); }
.tooltip-leave-to { opacity: 0; }

/* ── Escritorio colapsado: riel de íconos ── */
.sidebar--collapsed .sidebar-panel { padding: 0 0.5rem 0.5rem; }
.sidebar--collapsed .sidebar-header { justify-content: center; padding: 0; margin: 0 -0.5rem 1.25rem; }
.sidebar--collapsed .sidebar-nav { gap: 1rem; }
.sidebar--collapsed .sidebar-logo :deep(.app-logo-text),
.sidebar--collapsed .nav-text,
.sidebar--collapsed .nav-chevron,
.sidebar--collapsed .user-info { display: none; }
.sidebar--collapsed .nav-group-label {
  /* se conserva el espacio para que los grupos no salten al colapsar */
  visibility: hidden;
  height: 1px;
  margin: 0;
  padding: 0;
  background: var(--border);
}
.sidebar--collapsed .nav-group { gap: 4px; }
.sidebar--collapsed .nav-item {
  justify-content: center;
  width: 44px;
  height: 44px;
  padding: 0;
  margin: 0 auto;
  border-radius: 12px;
}
.sidebar--collapsed .sidebar-user {
  justify-content: center;
  padding: 0;
  border: none;
  background: none;
}
.sidebar--collapsed .user-avatar { width: 44px; height: 44px; border-radius: 12px; }

/* ── Mobile / tablet vertical: drawer ── */
@media (max-width: 768px), (max-width: 1024px) and (orientation: portrait) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    height: 100dvh;
    width: 280px;
    z-index: 100;
    padding: 10px;
    transform: translateX(-105%);
    transition: transform 0.28s cubic-bezier(0.32, 0.72, 0, 1);
  }
  .sidebar--open { transform: translateX(0); }

  .sidebar-close { display: flex; }

  /* En móvil siempre se muestra completo, aunque en escritorio esté colapsado */
  .sidebar--collapsed .sidebar-panel { padding: 0 0.75rem 0.75rem; }
  .sidebar--collapsed .sidebar-header { justify-content: space-between; padding: 0 1.25rem; margin: 0 -0.75rem 1.25rem; }
  .sidebar--collapsed .sidebar-nav { gap: 1.75rem; }
  .sidebar--collapsed .sidebar-logo :deep(.app-logo-text),
  .sidebar--collapsed .nav-text { display: inline; }
  .sidebar--collapsed .nav-chevron { display: block; }
  .sidebar--collapsed .user-info { display: flex; }
  .sidebar--collapsed .nav-group-label {
    visibility: visible;
    height: auto;
    padding: 0 0.75rem;
    margin-bottom: 0.5rem;
    background: none;
  }
  .sidebar--collapsed .nav-group { gap: 2px; }
  .sidebar--collapsed .nav-item {
    justify-content: flex-start;
    width: auto;
    height: 40px;
    padding: 0 0.75rem;
    margin: 0;
    border-radius: 10px;
  }
  .sidebar--collapsed .sidebar-user {
    justify-content: flex-start;
    padding: 0.7rem 0.75rem;
    border: 1px solid var(--border);
    background: transparent;
  }
  .sidebar--collapsed .user-avatar { width: 36px; height: 36px; border-radius: 10px; }
}
</style>
