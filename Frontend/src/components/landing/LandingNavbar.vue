<template>
  <header class="lnav" :class="{ 'lnav--scrolled': scrolled }">
    <div class="lnav-inner">
      <div class="lnav-logo">FINANZAS</div>

      <nav class="lnav-links">
        <button
          v-for="item in items"
          :key="item.id"
          class="lnav-link"
          :class="{ 'lnav-link--active': activeSection === item.id }"
          @click="goToSection(item.id)"
        >
          {{ item.label }}
        </button>
      </nav>

      <div class="lnav-actions">
        <RouterLink to="/login" class="btn" style="text-decoration:none">
          Iniciar sesión
        </RouterLink>
        <RouterLink to="/register" class="btn btn-primary" style="text-decoration:none">
          Regístrate
        </RouterLink>
      </div>

      <button class="hamburger" @click="drawerOpen = true">
        <Menu :size="20" :stroke-width="2" />
      </button>
    </div>
  </header>

  <Transition name="fade">
    <div v-if="drawerOpen" class="ldrawer-overlay" @click="drawerOpen = false" />
  </Transition>
  <aside class="ldrawer" :class="{ 'ldrawer--open': drawerOpen }">
    <div class="ldrawer-header">
      <span class="lnav-logo">FINANZAS</span>
      <button class="icon-btn" @click="drawerOpen = false">
        <X :size="18" :stroke-width="2" />
      </button>
    </div>

    <nav class="ldrawer-nav">
       <button
        v-for="item in items"
        :key="item.id"
        class="ldrawer-link"
        :class="{ 'ldrawer-link--active': activeSection === item.id }"
        @click="goToSection(item.id)"
      >
        <component :is="item.icon" :size="16" :stroke-width="2" />
        {{ item.label }}
      </button>
    </nav>

    <div class="ldrawer-auth">
      <RouterLink to="/login" class="btn ldrawer-btn-full" @click="drawerOpen = false">
        <LogIn :size="14" :stroke-width="2" />
        Iniciar sesión
      </RouterLink>
      <RouterLink to="/register" class="btn btn-primary ldrawer-btn-full" @click="drawerOpen = false">
        <UserPlus :size="14" :stroke-width="2" />
        Regístrate
      </RouterLink>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Menu, X, LayoutGrid, ListChecks, HelpCircle, Mail, LogIn, UserPlus, Home } from 'lucide-vue-next'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { scrollToSection } from '@/composables/useLanding'

const scrolled = ref(false)
const drawerOpen = ref(false)
const route = useRoute()
const router = useRouter()
const activeSection = ref('hero')
const sectionIds = ['hero', 'features', 'how-it-works', 'faq', 'footer']

const items = [
  { label: 'Inicio', icon: Home, id: 'hero' },
  { label: 'Funciones', icon: LayoutGrid, id: 'features' },
  { label: 'Cómo funciona', icon: ListChecks, id: 'how-it-works' },
  { label: 'Ayuda', icon: HelpCircle, id: 'faq' },
  { label: 'Contacto', icon: Mail, id: 'footer' },
]

const goToSection = async (id: string) => {
  // Cerrar el menú
  drawerOpen.value = false

  // Ir a páginas
  if (id === 'login' || id === 'register') {
    await router.push(`/${id}`)
    return
  }

  // Si no estamos en la landing, ir a ella
  if (route.path !== '/') {
    await router.push('/')
  }

  // Activar los estilos a la sección seleccionada
  activeSection.value = id
  // Ir a la sección
  scrollToSection(id)
}

const handleScroll = () => {
  const scrollPosition = window.scrollY + 120

  for (const id of sectionIds) {
    const section = document.getElementById(id)

    if (
      section &&
      scrollPosition >= section.offsetTop &&
      scrollPosition < section.offsetTop + section.offsetHeight
    ) {
      activeSection.value = id
    }
  }
}

onMounted(() => window.addEventListener('scroll', handleScroll))
onUnmounted(() => window.removeEventListener('scroll', handleScroll))
</script>

<style scoped>
.lnav {
  position: sticky;
  top: 0;
  z-index: 100;
  background: var(--bg);
  border-bottom: 1px solid var(--border);
  transition: background 0.2s, border-color 0.2s;
}
.lnav--scrolled {
  background: var(--surface);
  border-bottom-color: var(--border);
}

.lnav-inner {
  max-width: 1120px;
  margin: 0 auto;
  padding: 1.1rem 2rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.lnav-logo {
  font-family: var(--ff-display);
  font-weight: 800;
  font-size: 1.15rem;
  letter-spacing: 0.06em;
  color: var(--text-primary);
}

.lnav-links {
  display: flex;
  gap: 1.75rem;
}
.lnav-link {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: var(--text-secondary);
  font-family: var(--ff-mono);
  font-size: 0.78rem;
  cursor: pointer;
  padding: 0 0 2px;
  transition: color 0.15s;
}
.lnav-link:hover { color: var(--text-primary); }

.lnav-actions {
  display: flex;
  align-items: center;
  gap: 0.6rem;
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

.icon-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 4px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color 0.15s, background 0.15s;
  text-decoration: none;
  flex-shrink: 0;
}
.icon-btn:hover { color: var(--text-primary); background: var(--surface2); }

/* Drawer móvil */
.ldrawer-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.6);
  backdrop-filter: blur(2px);
  z-index: 199;
}
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

.ldrawer {
  position: fixed;
  top: 0;
  right: 0;
  height: 100dvh;
  width: 280px;
  max-width: 85vw;
  background: var(--surface);
  border-left: 1px solid var(--border);
  z-index: 200;
  transform: translateX(100%);
  transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
  padding: 1.25rem 1rem;
  box-shadow: -8px 0 32px rgba(0,0,0,0.35);
}
.ldrawer--open { transform: translateX(0); }

.ldrawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
  padding: 0 0.25rem;
}

.ldrawer-nav {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  flex: 1;
}
.ldrawer-link {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: none;
  border: none;
  border-left: 2px solid transparent;
  text-align: left;
  padding: 0.7rem 0.6rem;
  padding-left: calc(0.6rem - 2px);
  border-radius: 6px;
  font-size: 0.85rem;
  color: var(--text-secondary);
  cursor: pointer;
  transition: background 0.15s, color 0.15s, border-color 0.15s;
}
.ldrawer-link svg { flex-shrink: 0; opacity: 0.8; }
.ldrawer-link:hover { background: var(--surface2); color: var(--text-primary); }

.ldrawer-auth {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  padding-top: 1rem;
  margin-top: 0.75rem;
  border-top: 1px solid var(--border);
}
.ldrawer-btn-full { width: 100%; justify-content: center; text-decoration: none; }

.lnav-link--active {
  color: var(--accent);
  font-weight: 600;
  border-bottom-color: var(--accent);
}

.ldrawer-link--active {
  color: var(--accent);
  background: rgba(99, 179, 255, 0.08);
  border-left-color: var(--accent);
  font-weight: 600;
}

@media (max-width: 800px) {
  .lnav-links, .lnav-actions { display: none; }
  .hamburger { display: flex; }
  .lnav-inner { padding: 1rem 1.25rem; }
}
</style>
