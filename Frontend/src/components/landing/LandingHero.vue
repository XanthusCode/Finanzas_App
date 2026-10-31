<template>
  <section id="hero" class="hero">
    <div class="l-glow g1" aria-hidden="true" />
    <div class="l-glow g2" aria-hidden="true" />
    <div class="l-glow g3" aria-hidden="true" />
    <div class="hero-grid" aria-hidden="true" />

    <div class="l-container hero-inner">
      <div class="hero-copy">
        <span class="l-eyebrow"><span class="l-eyebrow-dot" /> Nuevo · Registra gastos por WhatsApp</span>
        <h1 class="hero-title">Tu dinero, <span class="l-grad">claro</span> por fin.</h1>
        <p class="hero-sub">
          Gastos, ingresos, metas, cuentas y presupuestos en un solo lugar.
          Entiende en qué se va tu plata y cierra cada mes con más ahorro.
        </p>
        <div class="hero-actions">
          <RouterLink to="/register" class="l-btn l-btn--white l-btn--lg">
            Empezar gratis <ArrowRight :size="17" :stroke-width="2.3" class="l-arrow" />
          </RouterLink>
          <button class="l-btn l-btn--ghost l-btn--lg" @click="scrollToSection('how-it-works')">
            <PlayCircle :size="17" :stroke-width="2" /> Ver cómo funciona
          </button>
        </div>
        <ul class="hero-trust">
          <li><Check :size="14" :stroke-width="2.6" /> Gratis en beta</li>
          <li><Check :size="14" :stroke-width="2.6" /> Sin conectar tu banco</li>
          <li><Check :size="14" :stroke-width="2.6" /> Celular y escritorio</li>
        </ul>
      </div>

      <!-- Maqueta del dashboard -->
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
                <div v-for="k in KPIS" :key="k.label" class="mock-kpi" :style="{ '--kc': k.color }">
                  <div class="mock-kpi-top"><span>{{ k.label }}</span><i /></div>
                  <strong>{{ k.value }}</strong>
                  <div class="mock-kpi-track"><span :style="{ width: k.pct + '%' }" /></div>
                </div>
              </div>
              <div class="mock-banner">
                <div>
                  <span class="mock-lbl">Total en cuentas</span>
                  <strong class="mock-big">$4.820.000</strong>
                </div>
                <div class="mock-progress">
                  <div class="mock-progress-head"><span class="mock-lbl">Tasa de ahorro</span><span class="mock-pill">Excelente</span></div>
                  <div class="mock-progress-track"><span /></div>
                </div>
              </div>
              <div class="mock-chart">
                <div v-for="(m, i) in BARRAS" :key="i" class="mock-col">
                  <span class="b b-in" :style="{ height: m[0] + '%' }" />
                  <span class="b b-out" :style="{ height: m[1] + '%' }" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Widgets flotando -->
        <div class="float f-chat l-glass">
          <span class="f-head"><MessageCircle :size="12" :stroke-width="2.2" /> WhatsApp</span>
          <span class="bubble bubble--out">25000 almuerzo</span>
          <span class="bubble bubble--in"><Check :size="12" :stroke-width="3" /> Registrado en Comida</span>
        </div>
        <div class="float f-ring l-glass">
          <div class="ring">
            <svg viewBox="0 0 64 64"><circle cx="32" cy="32" r="26" class="ring-track" /><circle cx="32" cy="32" r="26" class="ring-fill" /></svg>
            <span>64%</span>
          </div>
          <div class="f-ring-text"><strong>Viaje a Cartagena</strong><span>Faltan $720.000</span></div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { ArrowRight, PlayCircle, Check, MessageCircle } from 'lucide-vue-next'
import { scrollToSection } from '@/composables/useLanding'

// Datos ilustrativos de la maqueta
const KPIS = [
  { label: 'Ingresos',    value: '$4.200.000', pct: 100, color: '#60a5fa' },
  { label: 'Gastos fijos', value: '$1.295.000', pct: 31,  color: '#818cf8' },
  { label: 'Ocasionales', value: '$690.000',   pct: 16,  color: '#d4a85a' },
  { label: 'Te queda',    value: '$2.215.000', pct: 53,  color: '#34d399' },
]
const BARRAS: [number, number][] = [[62, 48], [70, 55], [58, 40], [80, 62], [74, 50], [90, 58], [84, 60], [95, 66]]
</script>

<style scoped>
.hero { position: relative; padding: 9.5rem 0 5.5rem; overflow: hidden; }
.g1 { width: 720px; height: 720px; top: -320px; left: 50%; transform: translateX(-60%); background: rgba(37, 99, 235, 0.4); }
.g2 { width: 520px; height: 520px; top: 34%; right: -180px; background: rgba(16, 185, 129, 0.24); }
.g3 { width: 420px; height: 420px; top: 46%; left: -160px; background: rgba(139, 92, 246, 0.2); }
.hero-grid {
  position: absolute; inset: 0; pointer-events: none;
  background-image:
    linear-gradient(rgba(255, 255, 255, 0.035) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.035) 1px, transparent 1px);
  background-size: 64px 64px;
  mask-image: radial-gradient(ellipse 70% 55% at 50% 30%, #000 30%, transparent 75%);
}

.hero-inner { position: relative; display: flex; flex-direction: column; align-items: center; gap: 4rem; }
.hero-copy { display: flex; flex-direction: column; align-items: center; text-align: center; max-width: 760px; }
.hero-title {
  font-size: clamp(2.6rem, 6.4vw, 4.6rem);
  font-weight: 800; line-height: 1.02; letter-spacing: -0.045em;
  color: var(--l-text);
}
.hero-sub { margin-top: 1.4rem; max-width: 580px; font-size: clamp(1rem, 1.5vw, 1.12rem); line-height: 1.65; color: var(--l-sub); }
.hero-actions { display: flex; flex-wrap: wrap; justify-content: center; gap: 0.7rem; margin-top: 2.2rem; }
.hero-trust { list-style: none; display: flex; flex-wrap: wrap; justify-content: center; gap: 0.5rem 1.4rem; margin-top: 1.6rem; padding: 0; }
.hero-trust li { display: inline-flex; align-items: center; gap: 0.4rem; font-size: 0.8rem; color: var(--l-sub); }
.hero-trust svg { color: var(--l-green); }

/* ── Maqueta ── */
.hero-visual { position: relative; width: 100%; max-width: 1000px; }
.mock {
  position: relative;
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: linear-gradient(180deg, rgba(23, 26, 31, 0.92), rgba(15, 17, 20, 0.96));
  box-shadow: 0 50px 120px -40px rgba(37, 99, 235, 0.45), 0 30px 60px -30px rgba(0, 0, 0, 0.9), inset 0 1px 0 rgba(255, 255, 255, 0.06);
  overflow: hidden;
  transform: perspective(1600px) rotateX(6deg);
  transform-origin: center top;
}
.mock::after {
  content: ''; position: absolute; inset: auto 0 0 0; height: 35%;
  background: linear-gradient(to top, var(--l-bg), transparent);
  pointer-events: none;
}
.mock-bar-top { display: flex; align-items: center; gap: 6px; height: 38px; padding: 0 14px; border-bottom: 1px solid rgba(255, 255, 255, 0.06); }
.dot { width: 10px; height: 10px; border-radius: 99px; background: rgba(255, 255, 255, 0.14); }
.mock-url { margin: 0 auto; padding: 3px 14px; border-radius: 8px; background: rgba(255, 255, 255, 0.05); font-size: 0.68rem; color: var(--l-muted); }

.mock-app  { display: grid; grid-template-columns: 64px 1fr; gap: 14px; padding: 14px; }
.mock-side { display: flex; flex-direction: column; align-items: center; gap: 10px; padding: 12px 0; border-radius: 14px; background: rgba(255, 255, 255, 0.03); border: 1px solid rgba(255, 255, 255, 0.05); }
.mock-logo { width: 26px; height: 26px; border-radius: 8px; background: linear-gradient(135deg, #3b82f6, #8b5cf6); margin-bottom: 8px; }
.mock-nav  { width: 30px; height: 30px; border-radius: 9px; background: rgba(255, 255, 255, 0.05); }
.mock-nav.is-active { background: rgba(96, 165, 250, 0.2); box-shadow: inset 2px 0 0 #60a5fa; }

.mock-main { display: flex; flex-direction: column; gap: 12px; min-width: 0; }
.mock-kpis { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; }
.mock-kpi  { display: flex; flex-direction: column; gap: 8px; padding: 12px; border-radius: 12px; background: rgba(255, 255, 255, 0.035); border: 1px solid rgba(255, 255, 255, 0.06); }
.mock-kpi-top { display: flex; justify-content: space-between; align-items: center; font-size: 0.58rem; letter-spacing: 0.08em; text-transform: uppercase; color: var(--l-sub); }
.mock-kpi-top i { width: 18px; height: 18px; border-radius: 6px; background: color-mix(in srgb, var(--kc) 22%, transparent); }
.mock-kpi strong { font-size: 0.98rem; font-weight: 700; color: var(--l-text); letter-spacing: -0.02em; }
.mock-kpi-track { height: 4px; border-radius: 99px; background: rgba(255, 255, 255, 0.07); overflow: hidden; }
.mock-kpi-track span { display: block; height: 100%; border-radius: 99px; background: var(--kc); }

.mock-banner { display: grid; grid-template-columns: auto 1fr; align-items: center; gap: 24px; padding: 14px 16px; border-radius: 12px; background: rgba(255, 255, 255, 0.035); border: 1px solid rgba(255, 255, 255, 0.06); }
.mock-lbl { font-size: 0.58rem; letter-spacing: 0.08em; text-transform: uppercase; color: var(--l-sub); }
.mock-big { display: block; margin-top: 4px; font-size: 1.35rem; font-weight: 800; letter-spacing: -0.03em; }
.mock-progress { display: flex; flex-direction: column; gap: 8px; }
.mock-progress-head { display: flex; justify-content: space-between; align-items: center; }
.mock-pill { padding: 2px 8px; border-radius: 99px; font-size: 0.56rem; color: var(--l-green); background: rgba(52, 211, 153, 0.13); }
.mock-progress-track { height: 7px; border-radius: 99px; background: rgba(255, 255, 255, 0.07); overflow: hidden; }
.mock-progress-track span { display: block; width: 34%; height: 100%; border-radius: 99px; background: linear-gradient(90deg, rgba(52, 211, 153, 0.6), #34d399); }

.mock-chart { display: flex; align-items: flex-end; gap: 4%; height: 150px; padding: 14px 16px 0; border-radius: 12px; background: rgba(255, 255, 255, 0.035); border: 1px solid rgba(255, 255, 255, 0.06); }
.mock-col { flex: 1; display: flex; align-items: flex-end; justify-content: center; gap: 3px; height: 100%; }
.b { width: 42%; max-width: 16px; border-radius: 4px 4px 0 0; }
.b-in  { background: linear-gradient(to top, rgba(59, 130, 246, 0.45), #3b82f6); }
.b-out { background: linear-gradient(to top, rgba(244, 63, 94, 0.4), #f43f5e); }

/* ── Widgets flotantes ── */
.float { position: absolute; z-index: 2; padding: 12px 14px; border-radius: 16px; background: rgba(17, 22, 31, 0.72); animation: float 7s ease-in-out infinite; }
.f-chat { left: -28px; bottom: 22%; display: flex; flex-direction: column; gap: 7px; width: 220px; }
.f-head { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.62rem; letter-spacing: 0.08em; text-transform: uppercase; color: var(--l-sub); }
.f-head svg { color: #25d366; }
.bubble { max-width: 92%; padding: 6px 10px; border-radius: 12px; font-size: 0.74rem; line-height: 1.3; }
.bubble--out { align-self: flex-end; background: #1d4ed8; color: #fff; border-bottom-right-radius: 4px; }
.bubble--in  { align-self: flex-start; display: inline-flex; align-items: center; gap: 5px; background: rgba(255, 255, 255, 0.08); border-bottom-left-radius: 4px; }
.bubble--in svg { color: var(--l-green); }

.f-ring { right: -24px; top: 30%; display: flex; align-items: center; gap: 12px; animation-delay: -3s; }
.ring { position: relative; width: 52px; height: 52px; }
.ring svg { width: 100%; height: 100%; transform: rotate(-90deg); }
.ring-track { fill: none; stroke: rgba(255, 255, 255, 0.08); stroke-width: 7; }
.ring-fill { fill: none; stroke: #34d399; stroke-width: 7; stroke-linecap: round; stroke-dasharray: 163.4; stroke-dashoffset: 58.8; filter: drop-shadow(0 0 6px rgba(52, 211, 153, 0.5)); }
.ring span { position: absolute; inset: 0; display: grid; place-items: center; font-size: 0.72rem; font-weight: 700; }
.f-ring-text { display: flex; flex-direction: column; gap: 2px; }
.f-ring-text strong { font-size: 0.78rem; font-weight: 600; }
.f-ring-text span { font-size: 0.68rem; color: var(--l-sub); }

@keyframes float {
  0%, 100% { translate: 0 0; }
  50%      { translate: 0 -8px; }
}

@media (max-width: 900px) {
  .hero { padding: 7.5rem 0 3.5rem; }
  .mock-kpis { grid-template-columns: repeat(2, 1fr); }
  .float { display: none; }
  .mock { transform: none; }
}
@media (max-width: 560px) {
  .mock-app { grid-template-columns: 1fr; }
  .mock-side { display: none; }
  .mock-banner { grid-template-columns: 1fr; gap: 12px; }
  .hero-actions .l-btn { width: 100%; }
}
</style>
