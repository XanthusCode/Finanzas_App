<template>
  <footer id="footer" class="lfoot">
    <div class="l-container">
      <div class="lfoot-top">
        <div class="lfoot-brand">
          <AppLogo class="lfoot-logo" />
          <p class="lfoot-tagline">Tu dinero, claro por fin. Finanzas personales hechas en Colombia.</p>
        </div>

        <div class="lfoot-col">
          <span class="lfoot-heading">Producto</span>
          <button class="lfoot-link" @click="scrollToSection('features')">Funciones</button>
          <button class="lfoot-link" @click="scrollToSection('how-it-works')">Cómo funciona</button>
          <button class="lfoot-link" @click="scrollToSection('faq')">Preguntas frecuentes</button>
        </div>

        <div class="lfoot-col">
          <span class="lfoot-heading">Cuenta</span>
          <RouterLink to="/login" class="lfoot-link">Iniciar sesión</RouterLink>
          <RouterLink to="/register" class="lfoot-link">Crear cuenta</RouterLink>
        </div>

        <div class="lfoot-col">
          <span class="lfoot-heading">Contacto</span>
          <a href="mailto:soporte@zaldo.app" class="lfoot-link"><Mail :size="14" :stroke-width="2" /> soporte@zaldo.app</a>
          <span class="lfoot-note">Muy pronto en redes sociales</span>
        </div>
      </div>

      <div class="lfoot-bottom">
        <span>© {{ year }} Zaldo. Todos los derechos reservados.</span>
        <span class="lfoot-status"><span class="lfoot-dot" /> Beta abierta</span>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { Mail } from 'lucide-vue-next'
import { scrollToSection } from '@/composables/useLanding'
import AppLogo from '@/components/common/AppLogo.vue'

const year = new Date().getFullYear()
</script>

<style scoped>
.lfoot { position: relative; padding: 4rem 0 2rem; border-top: 1px solid var(--l-line); }
.lfoot-top { display: grid; grid-template-columns: 1.6fr repeat(3, 1fr); gap: 2rem; }
.lfoot-logo { font-size: 1.1rem; }
.lfoot-tagline { margin-top: 0.85rem; max-width: 280px; font-size: 0.84rem; line-height: 1.6; color: var(--l-sub); }
.lfoot-col { display: flex; flex-direction: column; align-items: flex-start; gap: 0.6rem; }
.lfoot-heading { margin-bottom: 0.3rem; font-size: 0.72rem; font-weight: 600; letter-spacing: 0.1em; text-transform: uppercase; color: var(--l-text); }
.lfoot-link {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0; background: none; border: none;
  font-size: 0.86rem; color: var(--l-sub); text-decoration: none; cursor: pointer;
  transition: color 0.15s ease;
}
.lfoot-link:hover { color: var(--l-text); }
.lfoot-note { font-size: 0.76rem; color: var(--l-muted); }
.lfoot-bottom {
  display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap;
  margin-top: 3rem; padding-top: 1.5rem; border-top: 1px solid var(--l-line);
  font-size: 0.78rem; color: var(--l-muted);
}
.lfoot-status { display: inline-flex; align-items: center; gap: 0.45rem; }
.lfoot-dot { width: 7px; height: 7px; border-radius: 99px; background: var(--l-green); box-shadow: 0 0 8px var(--l-green); }

@media (max-width: 820px) {
  .lfoot-top { grid-template-columns: 1fr 1fr; }
  .lfoot-brand { grid-column: 1 / -1; }
}
</style>
