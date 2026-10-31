<template>
  <header class="lnav" :class="{ 'lnav--scrolled': scrolled }">
    <div class="lnav-inner">
      <RouterLink to="/" class="lnav-brand" aria-label="Zaldo, inicio" @click="goToSection('hero')">
        <AppLogo />
      </RouterLink>

      <nav class="lnav-links" aria-label="Secciones">
        <button
          v-for="item in items"
          :key="item.id"
          class="lnav-link"
          :class="{ 'is-active': activeSection === item.id }"
          @click="goToSection(item.id)"
        >
          {{ item.label }}
        </button>
      </nav>

      <div class="lnav-actions">
        <RouterLink to="/login" class="l-btn l-btn--sm lnav-login">Iniciar sesión</RouterLink>
        <RouterLink to="/register" class="l-btn l-btn--sm l-btn--white">
          Crear cuenta <ArrowRight :size="14" :stroke-width="2.4" class="l-arrow" />
        </RouterLink>
      </div>

      <button class="hamburger" aria-label="Abrir menú" @click="drawerOpen = true">
        <Menu :size="20" :stroke-width="2" />
      </button>
    </div>
  </header>

  <!-- Menú móvil -->
  <Teleport to="body">
    <Transition name="ldrawer-fade">
      <div v-if="drawerOpen" class="ldrawer-overlay" @click="drawerOpen = false" />
    </Transition>
    <aside class="ldrawer" :class="{ 'ldrawer--open': drawerOpen }" :aria-hidden="!drawerOpen">
      <div class="ldrawer-header">
        <AppLogo class="ldrawer-logo" />
        <button class="ldrawer-close" aria-label="Cerrar menú" @click="drawerOpen = false">
          <X :size="18" :stroke-width="2" />
        </button>
      </div>

      <nav class="ldrawer-nav">
        <button
          v-for="item in items"
          :key="item.id"
          class="ldrawer-link"
          :class="{ 'is-active': activeSection === item.id }"
          @click="goToSection(item.id)"
        >
          <component :is="item.icon" :size="17" :stroke-width="1.9" />
          {{ item.label }}
        </button>
      </nav>

      <div class="ldrawer-auth">
        <RouterLink to="/register" class="ldrawer-btn ldrawer-btn--white" @click="drawerOpen = false">
          <UserPlus :size="15" :stroke-width="2" /> Crear cuenta gratis
        </RouterLink>
        <RouterLink to="/login" class="ldrawer-btn" @click="drawerOpen = false">
          <LogIn :size="15" :stroke-width="2" /> Iniciar sesión
        </RouterLink>
      </div>
    </aside>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { Menu, X, LayoutGrid, ListChecks, HelpCircle, Mail, LogIn, UserPlus, Home, ArrowRight } from 'lucide-vue-next'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { scrollToSection } from '@/composables/useLanding'
import AppLogo from '@/components/common/AppLogo.vue'

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
  drawerOpen.value = false
  if (route.path !== '/') await router.push('/')
  activeSection.value = id
  scrollToSection(id)
}

const handleScroll = () => {
  scrolled.value = window.scrollY > 12
  const scrollPosition = window.scrollY + 140
  for (const id of sectionIds) {
    const section = document.getElementById(id)
    if (section && scrollPosition >= section.offsetTop && scrollPosition < section.offsetTop + section.offsetHeight) {
      activeSection.value = id
    }
  }
}

onMounted(() => { window.addEventListener('scroll', handleScroll, { passive: true }); handleScroll() })
onUnmounted(() => window.removeEventListener('scroll', handleScroll))
</script>

<style scoped>
.lnav {
  position: fixed; top: 0; left: 0; right: 0; z-index: 50;
  padding: 14px 1.25rem 0;
  pointer-events: none;
}
.lnav-inner {
  pointer-events: auto;
  display: flex; align-items: center; gap: 1.5rem;
  max-width: 1160px; height: 60px; margin: 0 auto;
  padding: 0 0.6rem 0 1.1rem;
  border-radius: 16px;
  border: 1px solid transparent;
  transition: background 0.25s ease, border-color 0.25s ease, box-shadow 0.25s ease, backdrop-filter 0.25s ease;
}
.lnav--scrolled .lnav-inner {
  background: rgba(13, 17, 24, 0.66);
  border-color: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(16px) saturate(150%);
  -webkit-backdrop-filter: blur(16px) saturate(150%);
  box-shadow: 0 16px 40px -20px rgba(0, 0, 0, 0.8);
}
.lnav-brand { font-size: 1.1rem; color: var(--l-text); text-decoration: none; }

.lnav-links { display: flex; align-items: center; gap: 0.15rem; margin: 0 auto; }
.lnav-link {
  height: 36px; padding: 0 0.85rem;
  background: none; border: none; border-radius: 10px;
  font-size: 0.84rem; font-weight: 500; color: var(--l-sub);
  cursor: pointer; transition: color 0.15s ease, background 0.15s ease;
}
.lnav-link:hover { color: var(--l-text); background: rgba(255, 255, 255, 0.05); }
.lnav-link.is-active { color: var(--l-text); }
.lnav-link:focus-visible { outline: 2px solid var(--l-blue); outline-offset: 1px; }

.lnav-actions { display: flex; align-items: center; gap: 0.4rem; }
.lnav-login { color: var(--l-sub); background: none; }
.lnav-login:hover { color: var(--l-text); background: rgba(255, 255, 255, 0.05); }

.hamburger {
  display: none; margin-left: auto;
  width: 42px; height: 42px; place-items: center;
  background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 12px;
  color: var(--l-text); cursor: pointer;
}

/* ── Drawer (teleportado: usa colores fijos) ── */
.ldrawer-overlay { position: fixed; inset: 0; z-index: 90; background: rgba(3, 5, 8, 0.6); backdrop-filter: blur(4px); }
.ldrawer {
  position: fixed; top: 10px; right: 10px; bottom: 10px; z-index: 100;
  width: min(320px, calc(100vw - 20px));
  display: flex; flex-direction: column; gap: 1.25rem;
  padding: 1.1rem;
  background: rgba(13, 17, 24, 0.92);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  backdrop-filter: blur(20px);
  font-family: 'Inter', system-ui, sans-serif;
  color: #f1f5f9;
  transform: translateX(calc(100% + 20px));
  transition: transform 0.3s cubic-bezier(0.32, 0.72, 0, 1);
}
.ldrawer--open { transform: none; }
.ldrawer-header { display: flex; justify-content: space-between; align-items: center; }
.ldrawer-logo { font-family: var(--ff-display); font-weight: 800; letter-spacing: 0.06em; font-size: 1.05rem; }
.ldrawer-logo :deep(.app-logo-mark) { color: #60a5fa; }
.ldrawer-close {
  display: grid; place-items: center; width: 36px; height: 36px;
  background: rgba(255, 255, 255, 0.05); border: none; border-radius: 10px; color: #f1f5f9; cursor: pointer;
}
.ldrawer-nav { display: flex; flex-direction: column; gap: 2px; }
.ldrawer-link {
  display: flex; align-items: center; gap: 0.75rem;
  height: 46px; padding: 0 0.85rem;
  background: none; border: none; border-radius: 12px;
  font: inherit; font-size: 0.9rem; font-weight: 500; color: #94a3b8; text-align: left;
  cursor: pointer;
}
.ldrawer-link:hover, .ldrawer-link.is-active { color: #f1f5f9; background: rgba(255, 255, 255, 0.06); }
.ldrawer-auth { margin-top: auto; display: flex; flex-direction: column; gap: 0.5rem; }
.ldrawer-btn {
  display: flex; align-items: center; justify-content: center; gap: 0.5rem;
  height: 48px; border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.14); background: rgba(255, 255, 255, 0.04);
  font-size: 0.9rem; font-weight: 600; color: #f1f5f9; text-decoration: none;
}
.ldrawer-btn--white { background: #f8fafc; border-color: transparent; color: #0b0f17; }
.ldrawer-fade-enter-active, .ldrawer-fade-leave-active { transition: opacity 0.2s ease; }
.ldrawer-fade-enter-from, .ldrawer-fade-leave-to { opacity: 0; }

@media (max-width: 900px) {
  .lnav-links, .lnav-actions { display: none; }
  .hamburger { display: grid; }
  .lnav { padding: 10px 0.75rem 0; }
  .lnav-inner { height: 56px; padding: 0 0.5rem 0 1rem; }
}
</style>
