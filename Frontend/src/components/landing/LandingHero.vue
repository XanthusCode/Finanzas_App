<template>
  <section id="hero" class="hero">
    <div class="hero-bg" />
    <div class="hero-inner">
      <div class="hero-copy">
        <div class="section-label">Finanzas personales</div>
        <h1 class="hero-title">Controla tus finanzas.<br>Cumple tus metas.</h1>
        <p class="hero-sub">
          Registra tus gastos, ingresos y deudas en un solo lugar, entiende en qué se va tu dinero
          y toma mejores decisiones cada mes.
        </p>
        <div class="hero-actions">
          <RouterLink to="/register" class="btn btn-primary btn-lg" style="text-decoration: none;">Empezar gratis</RouterLink>
          <button class="btn btn-lg" @click="scrollToSection('features')">Ver funciones</button>
        </div>
      </div>

      <div class="hero-visual">
        <div class="mock-window">
          <div class="mock-titlebar">
            <span class="mock-dot" style="background:#c17165" />
            <span class="mock-dot" style="background:#c2a06b" />
            <span class="mock-dot" style="background:#5c9678" />
          </div>
          <div class="mock-body">
            <div class="mock-kpis">
              <div class="mock-kpi"><span class="mock-kpi-lbl">Ingresos</span><span class="mock-kpi-val" style="color:var(--green)">$ 4.200.000</span></div>
              <div class="mock-kpi"><span class="mock-kpi-lbl">Gastos</span><span class="mock-kpi-val" style="color:var(--red)">$ 2.850.000</span></div>
              <div class="mock-kpi"><span class="mock-kpi-lbl">Ahorro</span><span class="mock-kpi-val" style="color:var(--accent)">32%</span></div>
            </div>
            <div class="mock-bars">
              <div class="mock-bar" style="height:40%; background:var(--red)" />
              <div class="mock-bar" style="height:65%; background:var(--amber)" />
              <div class="mock-bar" style="height:50%; background:var(--red)" />
              <div class="mock-bar" style="height:85%; background:var(--green)" />
              <div class="mock-bar" style="height:60%; background:var(--amber)" />
              <div class="mock-bar" style="height:95%; background:var(--green)" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { scrollToSection } from '@/composables/useLanding'
</script>

<style scoped>
.hero { position: relative; overflow: hidden; padding: 4rem 2rem 3rem; scroll-margin-top: 5rem; }
.hero-bg {
  position: absolute;
  inset: 0;
  z-index: -1;
  background-image:
    radial-gradient(ellipse 70% 55% at 25% 20%, rgba(99,179,255,0.10) 0%, transparent 70%),
    radial-gradient(rgba(99,179,255,0.05) 1px, transparent 1px);
  background-size: auto, 28px 28px;
}

.hero-inner {
  max-width: 1120px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1.05fr 0.95fr;
  gap: 3rem;
  align-items: center;
}

.hero-title {
  font-size: 2.6rem;
  line-height: 1.15;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--text-primary);
  margin: 0.5rem 0 1rem;
}

.hero-sub {
  font-size: 0.95rem;
  color: var(--text-secondary);
  line-height: 1.6;
  max-width: 480px;
  margin-bottom: 1.75rem;
}

.hero-actions { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.btn-lg { padding: 0.75rem 1.5rem; font-size: 0.78rem; }

.hero-visual { display: flex; justify-content: center; }
.mock-window {
  width: 100%;
  max-width: 420px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 30px 60px -20px rgba(0,0,0,0.5);
}
.mock-titlebar { display: flex; gap: 0.4rem; padding: 0.75rem 1rem; border-bottom: 1px solid var(--border); }
.mock-dot { width: 9px; height: 9px; border-radius: 50%; }

.mock-body { padding: 1.5rem; }
.mock-kpis { display: flex; gap: 0.75rem; margin-bottom: 1.75rem; }
.mock-kpi {
  flex: 1;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0.65rem 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}
.mock-kpi-lbl { font-size: 0.58rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.06em; }
.mock-kpi-val { font-size: 0.85rem; font-weight: 700; }

.mock-bars {
  display: flex;
  align-items: flex-end;
  gap: 0.55rem;
  height: 120px;
}
.mock-bar { flex: 1; border-radius: 4px 4px 0 0; opacity: 0.85; }

@media (max-width: 860px) {
  .hero-inner { grid-template-columns: 1fr; }
  .hero-title { font-size: 2.1rem; }
  .hero-copy { text-align: center; }
  .hero-sub { margin-left: auto; margin-right: auto; }
  .hero-actions { justify-content: center; }
}

@media (max-width: 600px) {
  .hero { padding: 2.5rem 1.25rem 2rem; }
  .hero-title { font-size: 1.75rem; }
  .hero-actions { flex-direction: column; align-items: stretch; }
  .hero-actions .btn { justify-content: center; }
  .mock-body { padding: 1.1rem; }
  .mock-kpis { gap: 0.5rem; margin-bottom: 1.25rem; }
  .mock-kpi { padding: 0.5rem 0.55rem; }
  .mock-kpi-lbl { font-size: 0.54rem; }
  .mock-kpi-val { font-size: 0.75rem; }
  .mock-bars { height: 90px; gap: 0.4rem; }
}
</style>
