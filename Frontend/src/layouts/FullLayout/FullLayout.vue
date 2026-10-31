<template>
<div class="layout" :class="{ 'layout--collapsed': collapsed }">
  <div v-if="sidebarOpen" class="sidebar-overlay" @click="sidebarOpen = false" />
  <Sidebar :open="sidebarOpen" :collapsed="collapsed" @close="sidebarOpen = false" />
  <div class="content-col">
    <Navbar :collapsed="collapsed" @toggle-sidebar="toggleSidebar" />
    <main class="main">
      <RouterView />
    </main>
  </div>

    <CopilotoFab />
</div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount } from 'vue'
import { RouterView } from 'vue-router'
import Navbar from '@/layouts/FullLayout/Navbar/Navbar.vue'
import Sidebar from '@/layouts/FullLayout/Sidebar/Sidebar.vue'
import CopilotoFab from '@/components/copiloto/CopilotoFab.vue'

const COLLAPSED_KEY = 'zaldo_sidebar_collapsed'

// Mismo breakpoint que el @media del layout: debajo de esto el sidebar es un drawer
const mobileQuery = window.matchMedia('(max-width: 768px), (max-width: 1024px) and (orientation: portrait)')

const sidebarOpen = ref(false)                 // drawer en móvil
const collapsed   = ref(leerColapsado())       // solo íconos en escritorio
const isMobile    = ref(mobileQuery.matches)

function leerColapsado() {
  try { return localStorage.getItem(COLLAPSED_KEY) === '1' } catch { return false }
}

watch(collapsed, v => {
  try { localStorage.setItem(COLLAPSED_KEY, v ? '1' : '0') } catch { /* sin storage: no se recuerda */ }
})

function toggleSidebar() {
  if (isMobile.value) sidebarOpen.value = !sidebarOpen.value
  else collapsed.value = !collapsed.value
}

function onMediaChange(e: MediaQueryListEvent) {
  isMobile.value = e.matches
  if (!e.matches) sidebarOpen.value = false
}

onMounted(() => mobileQuery.addEventListener('change', onMediaChange))
onBeforeUnmount(() => mobileQuery.removeEventListener('change', onMediaChange))
</script>

<style scoped>
.layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  min-height: 100vh;
  transition: grid-template-columns 0.22s cubic-bezier(0.4, 0, 0.2, 1);
}
.layout--collapsed { grid-template-columns: 84px 1fr; }

.content-col {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  min-width: 0;
}

.main {
  padding: 2.5rem;
  flex: 1;
}

.sidebar-overlay { display: none; }

@media (max-width: 768px), (max-width: 1024px) and (orientation: portrait) {
  .layout,
  .layout--collapsed {
    grid-template-columns: 1fr;
  }

  .main {
    padding: 1.25rem 1rem;
  }

  .sidebar-overlay {
    display: block;
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 99;
  }
}
</style>
