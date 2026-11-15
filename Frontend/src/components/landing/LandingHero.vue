<template>
  <section id="hero" class="hero">
    <div class="l-container hero-inner">
      <div class="hero-split">
        <div class="hero-copy">
          <h1 class="hero-title">Sabe cuánto te queda antes de que se acabe el mes</h1>
          <p class="hero-sub">
            Escríbele a Zaldo «25000 almuerzo» por WhatsApp o anótalo en la app.
            Ves en qué se va tu plata y cuánto te queda, sin conectar tu banco.
          </p>
          <div class="hero-actions">
            <RouterLink to="/register" class="l-btn l-btn--primary l-btn--lg">
              Crear mi cuenta gratis <ArrowRight :size="16" :stroke-width="1.75" class="l-arrow" />
            </RouterLink>
            <button class="l-btn l-btn--ghost l-btn--lg" @click="scrollToSection('how-it-works')">
              Ver cómo funciona
            </button>
          </div>
        </div>

        <!-- Momento: el mensaje de WhatsApp se vuelve un gasto registrado -->
        <div class="hero-moment" aria-hidden="true">
          <div class="chat">
            <span class="chat-head"><MessageCircle :size="14" :stroke-width="1.5" /> WhatsApp · Zaldo</span>
            <span class="bubble bubble--out">25000 almuerzo</span>
            <span class="bubble bubble--in">Listo, $25.000 en Comida</span>
          </div>
          <div class="receipt">
            <div class="receipt-head">
              <span>Gasto registrado</span>
              <span>13 nov · 12:41</span>
            </div>
            <dl class="receipt-rows">
              <div><dt>Detalle</dt><dd>Almuerzo</dd></div>
              <div><dt>Categoría</dt><dd>Comida</dd></div>
              <div><dt>Cuenta</dt><dd>Bancolombia</dd></div>
            </dl>
            <div class="receipt-total"><span>Total</span><strong>$25.000</strong></div>
            <div class="receipt-left"><span>Te queda este mes</span><strong>$1.460.000</strong></div>
          </div>
        </div>
      </div>

      <!-- Maqueta del resumen -->
      <div class="hero-visual" aria-hidden="true">
        <div class="mock">
          <div class="mock-bar-top">
            <span class="dot" /><span class="dot" /><span class="dot" />
            <span class="mock-url">zaldo.app/dashboard</span>
          </div>
          <div class="mock-app">
            <aside class="mock-side">
              <span class="mock-logo" />
              <span v-for="i in 6" :key="i" class="mock-nav" :class="{ 'is-active': i === 1 }" />
            </aside>
            <div class="mock-main">
              <div class="mock-kpis">
                <div v-for="k in KPIS" :key="k.label" class="mock-kpi" :class="{ 'is-lead': k.lead }">
                  <span class="mock-lbl">{{ k.label }}</span>
                  <strong>{{ k.value }}</strong>
                </div>
              </div>
              <div class="mock-banner">
                <div>
                  <span class="mock-lbl">Total en cuentas</span>
                  <strong class="mock-big">$4.820.000</strong>
                </div>
                <div class="mock-progress">
                  <div class="mock-progress-head"><span class="mock-lbl">Sin gastar 2026</span><span class="mock-lbl mock-good">Muy bien</span></div>
                  <div class="mock-progress-track"><span /></div>
                </div>
              </div>
              <div class="mock-chart">
                <div v-for="(m, i) in BARRAS" :key="i" class="mock-col" :class="{ 'is-now': i === BARRAS.length - 1 }">
                  <span class="b b-in" :style="{ height: m[0] + '%' }" />
                  <span class="b b-out" :style="{ height: m[1] + '%' }" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { ArrowRight, MessageCircle } from '@lucide/vue'
import { scrollToSection } from '@/composables/useLanding'

// Datos ilustrativos de la maqueta
const KPIS = [
  { label: 'Te queda',     value: '$2.215.000', lead: true },
  { label: 'Ingresos',     value: '$4.200.000' },
  { label: 'Gastos fijos', value: '$1.295.000' },
  { label: 'Ocasionales',  value: '$690.000' },
]
const BARRAS: [number, number][] = [[62, 48], [70, 55], [58, 40], [80, 62], [74, 50], [90, 58], [84, 60], [95, 66]]
</script>

<style scoped>
.hero { position: relative; padding: 128px 0 64px; overflow: hidden; }
.hero-inner { position: relative; display: flex; flex-direction: column; gap: 64px; }

.hero-split { display: grid; grid-template-columns: minmax(0, 1.15fr) minmax(0, 0.85fr); align-items: center; gap: 64px; }
.hero-copy { display: flex; flex-direction: column; align-items: flex-start; }
.hero-title {
  font-size: clamp(2.5rem, 5vw, 4rem);
  font-weight: 500; line-height: 1.05; letter-spacing: -0.02em;
  color: var(--l-text);
  max-width: 13em;
}
.hero-sub { margin-top: 24px; max-width: 36rem; font-size: 1.125rem; line-height: 1.6; color: var(--l-sub); }
.hero-actions { display: flex; flex-wrap: wrap; gap: 12px; margin-top: 32px; }

/* ── Momento: chat → recibo ── */
.hero-moment { position: relative; display: flex; flex-direction: column; align-items: flex-end; gap: 16px; }
.chat {
  display: flex; flex-direction: column; gap: 8px;
  width: min(100%, 300px);
  padding: 16px;
  border-radius: 16px;
  background: var(--l-surface);
  border: 1px solid var(--l-line);
}
.chat-head { display: inline-flex; align-items: center; gap: 8px; font-size: 0.75rem; color: var(--l-muted); margin-bottom: 4px; }
.bubble { max-width: 90%; padding: 8px 12px; border-radius: 12px; font-size: 0.875rem; line-height: 1.3; }
.bubble--out { align-self: flex-end; font-family: var(--ff-mono); background: var(--l-primary); color: #fff; border-bottom-right-radius: 4px; }
.bubble--in  { align-self: flex-start; background: var(--l-surface-2); color: var(--l-text); border-bottom-left-radius: 4px; }

/* El recibo: papel claro, cifras en mono, borde inferior troquelado */
.receipt {
  --paper: #f3efe6;
  --ink: #1d1b17;
  --ink-soft: #6b665c;
  width: min(100%, 320px);
  margin-right: 24px;
  padding: 20px 20px 28px;
  background: var(--paper);
  color: var(--ink);
  font-family: var(--ff-mono);
  font-size: 0.8125rem;
  rotate: -1.5deg;
  box-shadow: 0 24px 48px -24px rgba(0, 0, 0, 0.6);
  -webkit-mask: radial-gradient(circle 6px at 8px 100%, transparent 98%, #000) 0 0 / 16px 100% repeat-x;
          mask: radial-gradient(circle 6px at 8px 100%, transparent 98%, #000) 0 0 / 16px 100% repeat-x;
}
.receipt-head { display: flex; justify-content: space-between; gap: 12px; padding-bottom: 12px; border-bottom: 1px dashed color-mix(in srgb, var(--ink) 30%, transparent); font-family: var(--ff-body); font-weight: 500; }
.receipt-head span:last-child { font-family: var(--ff-mono); font-weight: 400; color: var(--ink-soft); }
.receipt-rows { display: flex; flex-direction: column; gap: 8px; margin: 12px 0; }
.receipt-rows div { display: flex; justify-content: space-between; gap: 12px; }
.receipt-rows dt { color: var(--ink-soft); }
.receipt-total, .receipt-left { display: flex; justify-content: space-between; align-items: baseline; gap: 12px; }
.receipt-total { padding-top: 12px; border-top: 1px dashed color-mix(in srgb, var(--ink) 30%, transparent); }
.receipt-total strong { font-size: 1.25rem; font-weight: 500; }
.receipt-left { margin-top: 8px; color: var(--ink-soft); }
.receipt-left strong { font-weight: 500; color: #18794e; }

/* ── Maqueta ── */
.hero-visual { position: relative; width: 100%; }
.mock {
  position: relative;
  border-radius: 20px;
  border: 1px solid var(--l-line-2);
  background: var(--l-surface);
  overflow: hidden;
}
.mock::after {
  content: ''; position: absolute; inset: auto 0 0 0; height: 35%;
  background: linear-gradient(to top, var(--l-bg), transparent);
  pointer-events: none;
}
.mock-bar-top { display: flex; align-items: center; gap: 6px; height: 36px; padding: 0 12px; border-bottom: 1px solid var(--l-line); }
.dot { width: 10px; height: 10px; border-radius: 99px; background: rgba(255, 255, 255, 0.14); }
.mock-url { margin: 0 auto; padding: 2px 12px; border-radius: 6px; background: rgba(255, 255, 255, 0.05); font-family: var(--ff-mono); font-size: 0.6875rem; color: var(--l-muted); }

.mock-app  { display: grid; grid-template-columns: 64px 1fr; gap: 12px; padding: 12px; }
.mock-side { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 12px 0; border-radius: 12px; background: rgba(255, 255, 255, 0.03); border: 1px solid var(--l-line); }
.mock-logo { width: 24px; height: 24px; border-radius: 6px; background: var(--l-primary); margin-bottom: 8px; }
.mock-nav  { width: 28px; height: 28px; border-radius: 8px; background: rgba(255, 255, 255, 0.05); }
.mock-nav.is-active { background: rgba(255, 255, 255, 0.12); }

.mock-main { display: flex; flex-direction: column; gap: 12px; min-width: 0; }
.mock-kpis { display: grid; grid-template-columns: 2fr 1fr 1fr 1fr; gap: 8px; }
.mock-kpi  { display: flex; flex-direction: column; gap: 8px; padding: 12px; border-radius: 12px; background: rgba(255, 255, 255, 0.035); border: 1px solid var(--l-line); }
.mock-kpi strong { font-family: var(--ff-mono); font-size: 1rem; font-weight: 500; color: var(--l-text); }
.mock-kpi.is-lead strong { font-size: 1.5rem; color: var(--l-green); }
.mock-lbl { font-size: 0.6875rem; color: var(--l-sub); }
.mock-good { color: var(--l-green); }

.mock-banner { display: grid; grid-template-columns: auto 1fr; align-items: center; gap: 24px; padding: 12px 16px; border-radius: 12px; background: rgba(255, 255, 255, 0.035); border: 1px solid var(--l-line); }
.mock-big { display: block; margin-top: 4px; font-family: var(--ff-mono); font-size: 1.25rem; font-weight: 500; }
.mock-progress { display: flex; flex-direction: column; gap: 8px; }
.mock-progress-head { display: flex; justify-content: space-between; align-items: center; }
.mock-progress-track { height: 6px; border-radius: 99px; background: rgba(255, 255, 255, 0.07); overflow: hidden; }
.mock-progress-track span { display: block; width: 34%; height: 100%; border-radius: 99px; background: var(--l-green); }

/* Un acento por gráfico: el mes actual */
.mock-chart { display: flex; align-items: flex-end; gap: 4%; height: 148px; padding: 12px 16px 0; border-radius: 12px; background: rgba(255, 255, 255, 0.035); border: 1px solid var(--l-line); }
.mock-col { flex: 1; display: flex; align-items: flex-end; justify-content: center; gap: 2px; height: 100%; }
.b { width: 42%; max-width: 16px; border-radius: 2px 2px 0 0; }
.b-in  { background: rgba(255, 255, 255, 0.26); }
.b-out { background: rgba(255, 255, 255, 0.1); }
.is-now .b-in  { background: var(--l-blue); }
.is-now .b-out { background: color-mix(in srgb, var(--l-blue) 40%, transparent); }

@media (max-width: 900px) {
  .hero { padding: 104px 0 48px; }
  .hero-split { grid-template-columns: 1fr; gap: 48px; }
  .hero-moment { align-items: flex-start; }
  .mock-kpis { grid-template-columns: 1fr 1fr; }
  .mock-kpi.is-lead { grid-column: 1 / -1; }
}
@media (max-width: 560px) {
  .hero-title { font-size: 2.375rem; }
  .mock-app { grid-template-columns: 1fr; }
  .mock-side { display: none; }
  .mock-banner { grid-template-columns: 1fr; gap: 12px; }
  .hero-actions { width: 100%; }
  .hero-actions .l-btn { width: 100%; }
  .receipt { margin-right: 0; }
}
</style>
