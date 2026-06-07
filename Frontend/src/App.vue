<template>
  <template v-if="auth.isAuthenticated">
    <FullLayout />
  </template>
  <template v-else>
    <RouterView />
  </template>
</template>

<script setup lang="ts">
import FullLayout from '@/layouts/FullLayout/FullLayout.vue'
import { RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
</script>