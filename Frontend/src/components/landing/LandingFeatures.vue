<template>
  <section id="features" class="l-section features">
    <div class="l-glow fg1" aria-hidden="true" />
    <div class="l-container">
      <div class="l-section-head reveal">
        <span class="l-eyebrow">Funciones</span>
        <h2 class="l-h2">Todo lo que necesitas para <span class="l-grad">controlar tu dinero</span></h2>
        <p class="l-lead">Desde el café de todos los días hasta la meta de fin de año.</p>
      </div>

      <div class="bento">
        <!-- Dashboard (grande) -->
        <article class="tile tile--dash l-glass reveal">
          <div class="tile-copy">
            <span class="l-icon" style="--ic: #60a5fa"><LayoutDashboard :size="19" :stroke-width="1.9" /></span>
            <h3>Dashboard que se entiende</h3>
            <p>Ingresos, gastos, lo que te queda y tu tasa de ahorro. La regla 50/30/20 y tus tendencias del año, sin hojas de cálculo.</p>
          </div>
          <div class="viz-dash" aria-hidden="true">
            <div class="vd-row">
              <div class="vd-kpi"><span>Te queda</span><strong>$2.215.000</strong></div>
              <div class="vd-kpi"><span>Ahorro</span><strong class="pos">29%</strong></div>
            </div>
            <div class="vd-regla">
              <div v-for="r in REGLA" :key="r.label" class="vd-regla-row">
                <span class="vd-regla-lbl">{{ r.label }}</span>
                <div class="vd-track"><span :style="{ width: r.pct + '%', background: r.color }" /><i :style="{ left: r.meta + '%' }" /></div>
                <span class="vd-regla-pct">{{ r.pct }}%</span>
              </div>
            </div>
          </div>
        </article>

        <!-- WhatsApp (ancha) -->
        <article class="tile tile--wa l-glass reveal">
          <div class="tile-copy">
            <span class="l-icon" style="--ic: #25d366"><MessageCircle :size="19" :stroke-width="1.9" /></span>
            <h3>Registra gastos por WhatsApp</h3>
            <p>Escríbele al bot como a un amigo. Zaldo entiende el monto y la categoría.</p>
          </div>
          <div class="viz-chat" aria-hidden="true">
            <span class="bubble bubble--out">25k mercado</span>
            <span class="bubble bubble--in"><Check :size="12" :stroke-width="3" /> $25.000 en Mercado</span>
            <span class="bubble bubble--out">12.000 taxi</span>
          </div>
        </article>

        <!-- Pequeñas -->
        <article v-for="f in pequenas" :key="f.title" class="tile tile--sm l-glass reveal">
          <span class="l-icon" :style="{ '--ic': f.color }"><component :is="f.icon" :size="18" :stroke-width="1.9" /></span>
          <h3>{{ f.title }}</h3>
          <p>{{ f.desc }}</p>
        </article>

        <!-- IA (ancha) -->
        <article class="tile tile--wide l-glass tile--ai reveal">
          <div class="tile-copy">
            <span class="l-icon" style="--ic: #a78bfa"><Sparkles :size="19" :stroke-width="1.9" /></span>
            <h3>Análisis con IA</h3>
            <p>Detecta patrones y te sugiere dónde ajustar, en un clic.</p>
          </div>
          <div class="viz-ai" aria-hidden="true">
            <span class="ai-line"><Lightbulb :size="13" :stroke-width="2" /> Gastaste 18% más en domicilios que el mes pasado.</span>
            <span class="ai-line"><Lightbulb :size="13" :stroke-width="2" /> Si bajas "Ocio" a $200.000, llegas a tu meta en 6 meses.</span>
          </div>
        </article>

        <!-- Búsqueda (ancha) -->
        <article class="tile tile--wide l-glass reveal">
          <div class="tile-copy">
            <span class="l-icon" style="--ic: #94a3b8"><Search :size="19" :stroke-width="1.9" /></span>
            <h3>Encuéntralo todo</h3>
            <p>Ctrl + K y escribe "Netflix" o "$50.000": aparece en cualquier mes.</p>
          </div>
          <div class="viz-search" aria-hidden="true">
            <div class="vs-input"><Search :size="14" :stroke-width="2" /> netflix<span class="vs-caret" /><kbd>⌘K</kbd></div>
            <div class="vs-result"><span class="vs-ico"><ArrowDownRight :size="13" :stroke-width="2.2" /></span><span>Suscripciones · Netflix<small>Gasto fijo · Oct 2026</small></span><strong>−$44.900</strong></div>
          </div>
        </article>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { LayoutDashboard, MessageCircle, Sparkles, Search, Check, Lightbulb, ArrowDownRight } from 'lucide-vue-next'
import { features } from '@/composables/useLanding'

// Las de IA y búsqueda tienen tarjeta ancha propia
const pequenas = computed(() => features.filter(f => !['Análisis con IA', 'Búsqueda global'].includes(f.title)))

const REGLA = [
  { label: 'Necesidades', pct: 46, meta: 50, color: '#60a5fa' },
  { label: 'Gustos',      pct: 25, meta: 30, color: '#fb923c' },
  { label: 'Ahorro',      pct: 29, meta: 20, color: '#34d399' },
]
</script>

<style scoped>
.features { overflow: hidden; }
.fg1 { width: 600px; height: 600px; top: 10%; left: 50%; transform: translateX(-50%); background: rgba(37, 99, 235, 0.16); }

.bento {
  position: relative;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  grid-auto-rows: minmax(180px, auto);
  gap: 1rem;
}
.tile {
  display: flex; flex-direction: column; gap: 0.6rem;
  padding: 1.4rem;
  overflow: hidden;
  transition: translate 0.3s var(--ease-out), border-color 0.3s ease, opacity 0.7s var(--ease-out), transform 0.7s var(--ease-out);
}
.tile:hover { translate: 0 -4px; border-color: var(--l-line-2); }
.tile h3 { font-size: 1rem; font-weight: 700; letter-spacing: -0.015em; color: var(--l-text); margin-top: 0.35rem; }
.tile p  { font-size: 0.84rem; line-height: 1.55; color: var(--l-sub); }
.tile-copy { display: flex; flex-direction: column; gap: 0.6rem; }

.tile--dash { grid-column: span 2; grid-row: span 2; justify-content: space-between; gap: 1.5rem; }
.tile--wa   { grid-column: span 2; flex-direction: row; align-items: center; gap: 1.5rem; }
.tile--wa .tile-copy { flex: 1; }
.tile--wide { grid-column: span 2; flex-direction: row; align-items: center; gap: 1.5rem; }
.tile--wide .tile-copy { flex: 1; }
.tile--ai { background: radial-gradient(120% 140% at 0% 0%, rgba(139, 92, 246, 0.12), transparent 55%), var(--l-surface); }

/* Visual dashboard */
.viz-dash { display: flex; flex-direction: column; gap: 0.75rem; }
.vd-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.6rem; }
.vd-kpi { display: flex; flex-direction: column; gap: 0.3rem; padding: 0.85rem; border-radius: 14px; background: rgba(255, 255, 255, 0.04); border: 1px solid var(--l-line); }
.vd-kpi span { font-size: 0.62rem; letter-spacing: 0.08em; text-transform: uppercase; color: var(--l-sub); }
.vd-kpi strong { font-size: 1.3rem; font-weight: 800; letter-spacing: -0.03em; }
.vd-kpi strong.pos { color: var(--l-green); }
.vd-regla { display: flex; flex-direction: column; gap: 0.65rem; padding: 0.95rem; border-radius: 14px; background: rgba(255, 255, 255, 0.04); border: 1px solid var(--l-line); }
.vd-regla-row { display: grid; grid-template-columns: 88px 1fr 40px; align-items: center; gap: 0.7rem; }
.vd-regla-lbl { font-size: 0.74rem; color: var(--l-text); }
.vd-regla-pct { font-size: 0.78rem; font-weight: 700; text-align: right; }
.vd-track { position: relative; height: 6px; border-radius: 99px; background: rgba(255, 255, 255, 0.07); }
.vd-track span { display: block; height: 100%; border-radius: 99px; }
.vd-track i { position: absolute; top: -3px; bottom: -3px; width: 2px; border-radius: 2px; background: rgba(255, 255, 255, 0.55); }

/* Visual chat */
.viz-chat { display: flex; flex-direction: column; gap: 6px; width: 210px; flex-shrink: 0; }
.bubble { max-width: 92%; padding: 6px 10px; border-radius: 12px; font-size: 0.76rem; line-height: 1.3; }
.bubble--out { align-self: flex-end; background: #1d4ed8; color: #fff; border-bottom-right-radius: 4px; }
.bubble--in  { align-self: flex-start; display: inline-flex; align-items: center; gap: 5px; background: rgba(255, 255, 255, 0.08); border-bottom-left-radius: 4px; }
.bubble--in svg { color: var(--l-green); }

/* Visual IA */
.viz-ai { display: flex; flex-direction: column; gap: 6px; width: 250px; flex-shrink: 0; }
.ai-line { display: flex; align-items: flex-start; gap: 6px; padding: 8px 10px; border-radius: 12px; font-size: 0.72rem; line-height: 1.45; color: var(--l-text); background: rgba(255, 255, 255, 0.05); border: 1px solid var(--l-line); }
.ai-line svg { flex-shrink: 0; margin-top: 1px; color: var(--l-violet); }

/* Visual búsqueda */
.viz-search { display: flex; flex-direction: column; gap: 6px; width: 250px; flex-shrink: 0; }
.vs-input { display: flex; align-items: center; gap: 7px; height: 38px; padding: 0 10px; border-radius: 11px; background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(96, 165, 250, 0.45); box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.14); font-size: 0.78rem; color: var(--l-text); }
.vs-input svg { color: var(--l-muted); }
.vs-caret { width: 1.5px; height: 15px; margin-left: -4px; background: var(--l-blue); animation: caret 1s steps(1) infinite; }
.vs-input kbd { margin-left: auto; padding: 1px 6px; border-radius: 5px; font-size: 0.62rem; color: var(--l-sub); background: rgba(255, 255, 255, 0.06); border: 1px solid var(--l-line-2); }
@keyframes caret { 50% { opacity: 0; } }
.vs-result { display: flex; align-items: center; gap: 8px; padding: 8px 10px; border-radius: 11px; background: rgba(255, 255, 255, 0.06); }
.vs-ico { display: grid; place-items: center; width: 24px; height: 24px; border-radius: 7px; color: var(--l-rose); background: rgba(248, 113, 113, 0.14); }
.vs-result > span:nth-child(2) { display: flex; flex-direction: column; flex: 1; min-width: 0; font-size: 0.74rem; }
.vs-result small { font-size: 0.62rem; color: var(--l-muted); }
.vs-result strong { font-size: 0.74rem; }

@media (max-width: 1024px) {
  .bento { grid-template-columns: repeat(2, 1fr); }
  .tile--dash { grid-row: auto; }
}
@media (max-width: 680px) {
  .bento { grid-template-columns: 1fr; }
  .tile--dash, .tile--wa, .tile--wide { grid-column: auto; }
  .tile--wa, .tile--wide { flex-direction: column; align-items: stretch; }
  .viz-chat, .viz-ai, .viz-search { width: 100%; }
}
</style>
