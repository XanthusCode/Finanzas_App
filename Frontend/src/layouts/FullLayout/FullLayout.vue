<template>
<div class="layout">
  <Sidebar />
  <div class="content-col">
    <Navbar />
    <main class="main">
      <RouterView />
    </main>
  </div>
</div>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import Navbar from '@/layouts/FullLayout/Navbar/Navbar.vue'
import Sidebar from '@/layouts/FullLayout/Sidebar/Sidebar.vue'
</script>

<style scoped>
.layout {
  display: grid;
  grid-template-columns: 220px 1fr;
  min-height: 100vh;
}

/* ── Content column ── */
.content-col {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.main {
  padding: 2.5rem;
  flex: 1;
}
</style>