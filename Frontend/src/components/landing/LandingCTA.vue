<template>
  <section id="footer" class="cta">
    <div class="cta-inner">
      <h2 class="cta-title">Empieza a controlar tu dinero hoy</h2>
      <p class="cta-sub">Crea tu cuenta gratis, sin conectar tu banco ni tarjeta.</p>
      <RouterLink to="/register" class="btn btn-primary btn-lg"style="text-decoration: none;">Crear cuenta gratis</RouterLink>
    </div>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<style scoped>
.cta {
  padding: 4.5rem 2rem;
  text-align: center;
  background-image: radial-gradient(ellipse 60% 100% at 50% 0%, rgba(99,179,255,0.1) 0%, transparent 70%);
  border-top: 1px solid var(--border);
}
.cta-title { font-size: 1.7rem; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin-bottom: 0.6rem; }
.cta-sub { font-size: 0.85rem; color: var(--text-secondary); margin-bottom: 1.75rem; }
.btn-lg { padding: 0.75rem 1.75rem; font-size: 0.78rem; }

@media (max-width: 600px) {
  .cta { padding: 3rem 1.25rem; scroll-margin-top: 5rem; }
  .cta-title { font-size: 1.35rem; }
}
</style>
