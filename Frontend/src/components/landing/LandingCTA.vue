<template>
  <section class="l-section cta">
    <div class="l-container">
      <div class="cta-panel reveal">
        <div class="cta-content">
          <h2 class="l-h2">Registra tu primer gasto <span class="l-accent">hoy</span></h2>
          <p class="l-lead">Crea tu cuenta gratis, sin conectar tu banco ni tarjeta. Tu primer gasto en menos de un minuto.</p>
          <div class="cta-actions">
            <RouterLink to="/register" class="l-btn l-btn--primary l-btn--lg">
              Crear mi cuenta gratis <ArrowRight :size="17" :stroke-width="2.3" class="l-arrow" />
            </RouterLink>
            <RouterLink to="/login" class="l-btn l-btn--ghost l-btn--lg">Ya tengo cuenta</RouterLink>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { ArrowRight } from '@lucide/vue'
</script>

<style scoped>
.cta { padding-top: 2rem; }
.cta-panel {
  position: relative; overflow: hidden;
  padding: clamp(3rem, 6vw, 5rem) 1.5rem;
  border-radius: 28px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: var(--l-surface);
  text-align: center;
}
.cta-content { position: relative; max-width: 620px; margin: 0 auto; }
.cta-actions { display: flex; flex-wrap: wrap; justify-content: center; gap: 0.7rem; margin-top: 2rem; }

@media (max-width: 560px) {
  .cta-actions .l-btn { width: 100%; }
}
</style>
