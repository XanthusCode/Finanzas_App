<template>
  <section class="l-section shots">
    <div class="l-glow sg1" aria-hidden="true" />
    <div class="l-container">
      <div class="l-section-head reveal">
        <span class="l-eyebrow">En acción</span>
        <h2 class="l-h2">Así se ve tu dinero, <span class="l-grad">de un vistazo</span></h2>
      </div>

      <div class="tabs reveal" role="tablist" aria-label="Vistas de la app">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          role="tab"
          class="tab"
          :class="{ 'is-active': tab.id === active }"
          :aria-selected="tab.id === active"
          @click="active = tab.id"
        >
          <component :is="tab.icon" :size="15" :stroke-width="2" /> {{ tab.label }}
        </button>
      </div>

      <div class="window reveal" aria-hidden="true">
        <div class="win-bar">
          <span class="dot" /><span class="dot" /><span class="dot" />
          <span class="win-url">zaldo.app/{{ active }}</span>
        </div>

        <Transition name="swap" mode="out-in">
          <!-- Dashboard -->
          <div v-if="active === 'dashboard'" key="d" class="win-body">
            <div class="kpis">
              <div v-for="k in KPIS_DASH" :key="k.l" class="kpi" :style="{ '--kc': k.c }">
                <div class="kpi-top"><span>{{ k.l }}</span><i /></div>
                <strong :class="k.cls">{{ k.v }}</strong>
              </div>
            </div>
            <div class="dash-row">
              <div class="panel donut-panel">
                <div class="donut" />
                <ul class="legend">
                  <li><i style="background:#60a5fa" /> Fijos <b>31%</b></li>
                  <li><i style="background:#fb923c" /> Ocasionales <b>16%</b></li>
                  <li><i style="background:#34d399" /> Te queda <b>53%</b></li>
                </ul>
              </div>
              <div class="panel bars-panel">
                <div v-for="(m, i) in BARRAS" :key="i" class="col">
                  <span class="b b-in" :style="{ height: m[0] + '%' }" />
                  <span class="b b-out" :style="{ height: m[1] + '%' }" />
                </div>
              </div>
            </div>
          </div>

          <!-- Gastos -->
          <div v-else-if="active === 'gastos'" key="g" class="win-body">
            <div class="kpis kpis--3">
              <div v-for="k in KPIS_GASTOS" :key="k.l" class="kpi" :style="{ '--kc': k.c }">
                <div class="kpi-top"><span>{{ k.l }}</span><i /></div>
                <strong>{{ k.v }}</strong>
              </div>
            </div>
            <div class="panel list">
              <div v-for="row in GASTOS" :key="row.cat" class="list-row">
                <span class="list-ico" :style="{ '--rc': row.color }"><component :is="row.icon" :size="15" :stroke-width="2" /></span>
                <span class="list-main"><b>{{ row.cat }}</b><small>{{ row.det }}</small></span>
                <span class="list-tag" :class="row.tipo === 'Fijo' ? 'is-fijo' : ''">{{ row.tipo }}</span>
                <strong>{{ row.monto }}</strong>
              </div>
            </div>
          </div>

          <!-- Metas -->
          <div v-else key="m" class="win-body">
            <div class="metas">
              <div v-for="m in METAS" :key="m.nombre" class="panel meta">
                <div class="meta-top">
                  <span class="list-ico" style="--rc:#34d399"><PiggyBank :size="15" :stroke-width="2" /></span>
                  <b>{{ m.nombre }}</b>
                  <span class="meta-pct">{{ m.pct }}%</span>
                </div>
                <strong class="meta-amt">{{ m.actual }} <small>de {{ m.objetivo }}</small></strong>
                <div class="meta-track"><span :style="{ width: m.pct + '%' }" /></div>
                <span class="meta-ritmo"><Gauge :size="12" :stroke-width="2" /> A este ritmo, en ~{{ m.meses }} meses</span>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { LayoutDashboard, Receipt, PiggyBank, Gauge, House, ShoppingCart, Wifi, Car } from 'lucide-vue-next'

const tabs = [
  { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { id: 'gastos',    label: 'Gastos',    icon: Receipt },
  { id: 'metas',     label: 'Metas',     icon: PiggyBank },
] as const
const active = ref<typeof tabs[number]['id']>('dashboard')

// Datos ilustrativos
const KPIS_DASH = [
  { l: 'Ingresos', v: '$4.200.000', c: '#60a5fa', cls: '' },
  { l: 'Gastos',   v: '$1.985.000', c: '#f87171', cls: '' },
  { l: 'Ahorro',   v: '53%',        c: '#34d399', cls: 'pos' },
]
const KPIS_GASTOS = [
  { l: 'Fijos',       v: '$1.295.000', c: '#60a5fa' },
  { l: 'Ocasionales', v: '$690.000',   c: '#fb923c' },
  { l: 'Total',       v: '$1.985.000', c: '#94a3b8' },
]
const GASTOS = [
  { tipo: 'Fijo',      cat: 'Arriendo',   det: 'Apartamento',     monto: '$1.200.000', icon: House,        color: '#34d399' },
  { tipo: 'Ocasional', cat: 'Mercado',    det: 'D1 y plaza',      monto: '$480.000',   icon: ShoppingCart, color: '#fb923c' },
  { tipo: 'Fijo',      cat: 'Internet',   det: 'Fibra 300 MB',    monto: '$95.000',    icon: Wifi,         color: '#60a5fa' },
  { tipo: 'Ocasional', cat: 'Transporte', det: 'Taxis y MIO',     monto: '$210.000',   icon: Car,          color: '#a78bfa' },
]
const METAS = [
  { nombre: 'Viaje a Cartagena', actual: '$1.280.000', objetivo: '$2.000.000', pct: 64, meses: 3 },
  { nombre: 'Fondo de emergencia', actual: '$3.100.000', objetivo: '$10.000.000', pct: 31, meses: 11 },
]
const BARRAS: [number, number][] = [[62, 48], [70, 55], [58, 40], [80, 62], [74, 50], [90, 58]]
</script>

<style scoped>
.shots { overflow: hidden; }
.sg1 { width: 700px; height: 500px; bottom: -120px; left: 50%; transform: translateX(-50%); background: rgba(16, 185, 129, 0.14); }

.tabs {
  display: flex; width: max-content; margin: 0 auto 1.75rem; gap: 2px;
  padding: 4px; border-radius: 14px;
  background: rgba(255, 255, 255, 0.04); border: 1px solid var(--l-line);
}
.tab {
  display: inline-flex; align-items: center; gap: 0.45rem;
  height: 38px; padding: 0 1rem;
  background: none; border: none; border-radius: 10px;
  font-size: 0.84rem; font-weight: 500; color: var(--l-sub); cursor: pointer;
  transition: color 0.15s ease, background 0.15s ease;
}
.tab:hover:not(.is-active) { color: var(--l-text); }
.tab.is-active { color: var(--l-text); background: rgba(255, 255, 255, 0.08); box-shadow: inset 0 0 0 1px var(--l-line-2); }
.tab:focus-visible { outline: 2px solid var(--l-blue); outline-offset: 1px; }

.window {
  position: relative;
  max-width: 920px; margin: 0 auto;
  border-radius: 20px; overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: linear-gradient(180deg, rgba(23, 26, 31, 0.92), rgba(15, 17, 20, 0.96));
  box-shadow: 0 40px 100px -40px rgba(16, 185, 129, 0.3), 0 30px 60px -30px rgba(0, 0, 0, 0.9);
}
.win-bar { display: flex; align-items: center; gap: 6px; height: 38px; padding: 0 14px; border-bottom: 1px solid rgba(255, 255, 255, 0.06); }
.dot { width: 10px; height: 10px; border-radius: 99px; background: rgba(255, 255, 255, 0.14); }
.win-url { margin: 0 auto; padding: 3px 14px; border-radius: 8px; background: rgba(255, 255, 255, 0.05); font-size: 0.68rem; color: var(--l-muted); }
.win-body { display: flex; flex-direction: column; gap: 12px; padding: 16px; min-height: 330px; }

.panel { padding: 14px; border-radius: 14px; background: rgba(255, 255, 255, 0.035); border: 1px solid rgba(255, 255, 255, 0.06); }
.kpis { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
.kpi { display: flex; flex-direction: column; gap: 8px; padding: 14px; border-radius: 14px; background: rgba(255, 255, 255, 0.035); border: 1px solid rgba(255, 255, 255, 0.06); }
.kpi-top { display: flex; justify-content: space-between; align-items: center; font-size: 0.6rem; letter-spacing: 0.08em; text-transform: uppercase; color: var(--l-sub); }
.kpi-top i { width: 20px; height: 20px; border-radius: 6px; background: color-mix(in srgb, var(--kc) 22%, transparent); }
.kpi strong { font-size: 1.15rem; font-weight: 800; letter-spacing: -0.02em; }
.kpi strong.pos { color: var(--l-green); }

.dash-row { display: grid; grid-template-columns: 1fr 1.4fr; gap: 12px; flex: 1; }
.donut-panel { display: flex; align-items: center; gap: 18px; }
.donut {
  width: 104px; height: 104px; flex-shrink: 0; border-radius: 50%;
  background: conic-gradient(#60a5fa 0 31%, #fb923c 31% 47%, #34d399 47% 100%);
  -webkit-mask: radial-gradient(circle, transparent 54%, #000 55%);
  mask: radial-gradient(circle, transparent 54%, #000 55%);
}
.legend { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 8px; font-size: 0.74rem; color: var(--l-sub); }
.legend li { display: flex; align-items: center; gap: 7px; }
.legend i { width: 8px; height: 8px; border-radius: 99px; }
.legend b { margin-left: auto; padding-left: 10px; color: var(--l-text); font-weight: 700; }
.bars-panel { display: flex; align-items: flex-end; gap: 5%; padding-bottom: 0; }
.col { flex: 1; display: flex; align-items: flex-end; justify-content: center; gap: 3px; height: 100%; }
.b { width: 40%; max-width: 16px; border-radius: 4px 4px 0 0; }
.b-in  { background: linear-gradient(to top, rgba(59, 130, 246, 0.45), #3b82f6); }
.b-out { background: linear-gradient(to top, rgba(244, 63, 94, 0.4), #f43f5e); }

.list { display: flex; flex-direction: column; gap: 2px; padding: 6px; }
.list-row { display: flex; align-items: center; gap: 12px; padding: 10px; border-radius: 10px; }
.list-row:hover { background: rgba(255, 255, 255, 0.04); }
.list-ico { display: grid; place-items: center; width: 32px; height: 32px; border-radius: 9px; flex-shrink: 0; color: var(--rc); background: color-mix(in srgb, var(--rc) 15%, transparent); }
.list-main { flex: 1; display: flex; flex-direction: column; gap: 1px; min-width: 0; }
.list-main b { font-size: 0.82rem; font-weight: 600; }
.list-main small { font-size: 0.68rem; color: var(--l-sub); }
.list-tag { padding: 2px 8px; border-radius: 99px; font-size: 0.62rem; color: #fb923c; background: rgba(251, 146, 60, 0.13); }
.list-tag.is-fijo { color: var(--l-blue); background: rgba(96, 165, 250, 0.13); }
.list-row strong { font-size: 0.84rem; font-weight: 700; min-width: 92px; text-align: right; }

.metas { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.meta { display: flex; flex-direction: column; gap: 10px; }
.meta-top { display: flex; align-items: center; gap: 10px; }
.meta-top b { flex: 1; font-size: 0.86rem; font-weight: 700; }
.meta-pct { padding: 2px 8px; border-radius: 99px; font-size: 0.66rem; font-weight: 700; color: var(--l-green); background: rgba(52, 211, 153, 0.13); }
.meta-amt { font-size: 1.4rem; font-weight: 800; letter-spacing: -0.03em; }
.meta-amt small { font-size: 0.74rem; font-weight: 400; color: var(--l-sub); letter-spacing: 0; }
.meta-track { height: 10px; border-radius: 99px; background: rgba(255, 255, 255, 0.07); overflow: hidden; }
.meta-track span { display: block; height: 100%; border-radius: 99px; background: linear-gradient(90deg, #2dd4bf, #34d399); box-shadow: 0 0 12px rgba(52, 211, 153, 0.5); }
.meta-ritmo { display: inline-flex; align-items: center; gap: 6px; font-size: 0.72rem; color: var(--l-sub); }
.meta-ritmo svg { color: var(--l-blue); }

.swap-enter-active, .swap-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.swap-enter-from { opacity: 0; transform: translateY(6px); }
.swap-leave-to   { opacity: 0; }

@media (max-width: 720px) {
  .kpis, .kpis--3 { grid-template-columns: 1fr 1fr; }
  .dash-row, .metas { grid-template-columns: 1fr; }
  .bars-panel { min-height: 140px; }
  .list-tag { display: none; }
  .tab { padding: 0 0.75rem; }
}
</style>
