<template>
  <section class="l-section cta">
    <div class="l-container">
      <div class="cta-panel reveal">
        <div class="l-glow cg1" aria-hidden="true" />
        <div class="l-glow cg2" aria-hidden="true" />
        <div class="cta-content">
          <h2 class="l-h2">Empieza a controlar tu dinero <span class="l-grad">hoy</span></h2>
          <p class="l-lead">Crea tu cuenta gratis, sin conectar tu banco ni tarjeta. Tu primer gasto en menos de un minuto.</p>
          <div class="cta-actions">
            <RouterLink to="/register" class="l-btn l-btn--white l-btn--lg">
              Crear cuenta gratis <ArrowRight :size="17" :stroke-width="2.3" class="l-arrow" />
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
import { ArrowRight } from 'lucide-vue-next'
</script>

<style scoped>
.cta { padding-top: 2rem; }
.cta-panel {
  position: relative; overflow: hidden;
  padding: clamp(3rem, 6vw, 5rem) 1.5rem;
  border-radius: 28px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: linear-gradient(180deg, rgba(23, 29, 41, 0.9), rgba(12, 15, 21, 0.95));
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.07), 0 40px 80px -40px rgba(37, 99, 235, 0.45);
  text-align: center;
}
.cg1 { width: 520px; height: 320px; top: -160px; left: 50%; transform: translateX(-50%); background: rgba(37, 99, 235, 0.45); }
.cg2 { width: 360px; height: 260px; bottom: -160px; right: 10%; background: rgba(16, 185, 129, 0.3); }
.cta-content { position: relative; max-width: 620px; margin: 0 auto; }
.cta-actions { display: flex; flex-wrap: wrap; justify-content: center; gap: 0.7rem; margin-top: 2rem; }

@media (max-width: 560px) {
  .cta-actions .l-btn { width: 100%; }
}
</style>
