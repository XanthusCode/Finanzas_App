<template>
  <footer id="footer" class="lfoot">
    <div class="lfoot-inner">
      <div class="lfoot-brand">
        <AppLogo class="lfoot-logo" />
        <p class="lfoot-tagline">Controla tus finanzas. Cumple tus metas.</p>
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
        <RouterLink to="/register" class="lfoot-link">Registrarse</RouterLink>
      </div>

      <div class="lfoot-col">
        <span class="lfoot-heading">Contacto</span>
        <a href="mailto:soporte@finanzas.app" class="lfoot-link">
          <Mail :size="13" :stroke-width="2" /> soporte@finanzas.app
        </a>
        <span class="lfoot-note">Muy pronto en redes sociales</span>
      </div>
    </div>

    <div class="lfoot-bottom">
      © {{ year }} Finanzas. Todos los derechos reservados.
    </div>
  </footer>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { Mail } from '@lucide/vue'
import { scrollToSection } from '@/composables/useLanding'
import AppLogo from '@/components/common/AppLogo.vue'

const year = new Date().getFullYear()
</script>

<style scoped>
.lfoot { padding: 3.5rem 2rem 0; border-top: 1px solid var(--border); scroll-margin-top: 5rem; }
.lfoot-inner {
  max-width: 1120px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1.4fr 1fr 1fr 1fr;
  gap: 2rem;
  padding-bottom: 2.5rem;
}

.lfoot-logo { font-family: var(--ff-display); font-weight: 800; font-size: 1.1rem; color: var(--text-primary); }
.lfoot-tagline { font-size: 0.78rem; color: var(--text-muted); margin-top: 0.5rem; max-width: 220px; }

.lfoot-col { display: flex; flex-direction: column; gap: 0.6rem; }
.lfoot-heading { font-size: 0.62rem; letter-spacing: 0.1em; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 0.2rem; }
.lfoot-link {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  background: none;
  border: none;
  padding: 0;
  font-family: var(--ff-body);
  font-size: 0.8rem;
  color: var(--text-secondary);
  text-decoration: none;
  cursor: pointer;
  text-align: left;
  transition: color 0.15s;
  width: fit-content;
}
.lfoot-link:hover { color: var(--accent); }
.lfoot-note { font-size: 0.72rem; color: var(--text-muted); }

.lfoot-bottom {
  border-top: 1px solid var(--border);
  padding: 1.25rem 0;
  text-align: center;
  font-size: 0.72rem;
  color: var(--text-muted);
}

@media (max-width: 760px) {
  .lfoot-inner { grid-template-columns: 1fr 1fr; }
}
@media (max-width: 480px) {
  .lfoot { padding: 2.5rem 1.25rem 0; scroll-margin-top: 5rem; }
  .lfoot-inner { grid-template-columns: 1fr; gap: 1.75rem; }
}
</style>
