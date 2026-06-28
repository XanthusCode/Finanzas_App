<template>
<div class="layout">
  <div v-if="sidebarOpen" class="sidebar-overlay" @click="sidebarOpen = false" />
  <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" />
  <div class="content-col">
    <Navbar @toggle-sidebar="sidebarOpen = !sidebarOpen" />
    <main class="main">
      <RouterView />
    </main>
  </div>
</div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { RouterView } from 'vue-router'
import Navbar from '@/layouts/FullLayout/Navbar/Navbar.vue'
import Sidebar from '@/layouts/FullLayout/Sidebar/Sidebar.vue'

const sidebarOpen = ref(false)
</script>

<style scoped>
.layout {
  display: grid;
  grid-template-columns: 220px 1fr;
  min-height: 100vh;
}

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

@media (max-width: 768px) {
  .layout {
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
