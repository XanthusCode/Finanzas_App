<template>
  <section class="shots">
    <div class="shots-inner">
      <div class="section-label" style="justify-content:center">En acción</div>
      <h2 class="shots-title">Así se ve tu dinero, de un vistazo</h2>

      <div class="shots-tabs">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          class="shots-tab"
          :class="{ 'shots-tab--active': tab.id === active }"
          @click="active = tab.id"
        >
          {{ tab.label }}
        </button>
      </div>

      <div class="shots-window">
        <div class="mock-titlebar">
          <span class="mock-dot" style="background:#f87171" />
          <span class="mock-dot" style="background:#fbbf24" />
          <span class="mock-dot" style="background:#34d399" />
          <span class="mock-titlebar-label">finanzas.app/{{ active }}</span>
        </div>

        <!-- Dashboard -->
        <div v-if="active === 'dashboard'" class="mock-body">
          <div class="mock-kpis">
            <div class="mock-kpi" style="--ca: var(--green)"><span class="mock-kpi-lbl">Ingresos</span><span class="mock-kpi-val" style="color:var(--green)">$ 4.200.000</span></div>
            <div class="mock-kpi" style="--ca: var(--red)"><span class="mock-kpi-lbl">Gastos</span><span class="mock-kpi-val" style="color:var(--red)">$ 2.850.000</span></div>
            <div class="mock-kpi" style="--ca: var(--accent)"><span class="mock-kpi-lbl">Ahorro</span><span class="mock-kpi-val" style="color:var(--accent)">32%</span></div>
          </div>
          <div class="mock-donut-row">
            <div class="mock-donut" />
            <div class="mock-bars-v">
              <div v-for="(h, i) in [40,65,50,85,60,95]" :key="i" class="mock-bar" :style="{ height: h + '%', background: h > 70 ? 'var(--green)' : 'var(--amber)' }" />
            </div>
          </div>
        </div>

        <!-- Gastos -->
        <div v-else-if="active === 'gastos'" class="mock-body">
          <div class="mock-kpis">
            <div class="mock-kpi" style="--ca: var(--red)"><span class="mock-kpi-lbl">Fijos</span><span class="mock-kpi-val" style="color:var(--red)">$ 1.295.000</span></div>
            <div class="mock-kpi" style="--ca: var(--amber)"><span class="mock-kpi-lbl">Ocasionales</span><span class="mock-kpi-val" style="color:var(--amber)">$ 690.000</span></div>
            <div class="mock-kpi" style="--ca: var(--accent)"><span class="mock-kpi-lbl">Total</span><span class="mock-kpi-val" style="color:var(--accent)">$ 1.985.000</span></div>
          </div>
          <div class="mock-list">
            <div v-for="row in mockGastos" :key="row.cat" class="mock-list-row">
              <span class="tag" :class="row.tipo === 'Fijo' ? 'tag-red' : 'tag-amber'">{{ row.tipo }}</span>
              <span class="mock-list-cat">{{ row.cat }}</span>
              <span class="mock-list-val">{{ row.monto }}</span>
            </div>
          </div>
        </div>

        <!-- Historial -->
        <div v-else class="mock-body">
          <div class="mock-kpis">
            <div class="mock-kpi" style="--ca: var(--green)"><span class="mock-kpi-lbl">Ahorro total</span><span class="mock-kpi-val" style="color:var(--green)">$ 8.100.000</span></div>
            <div class="mock-kpi" style="--ca: var(--green)"><span class="mock-kpi-lbl">% Ahorro prom.</span><span class="mock-kpi-val" style="color:var(--green)">27%</span></div>
          </div>
          <div class="mock-stat-grid">
            <div v-for="m in mockMeses" :key="m.mes" class="mock-stat-col">
              <div class="mock-stat-bars">
                <div class="mock-stat-bar" :style="{ height: m.ingresos + '%', background: 'rgba(99,179,255,0.7)' }" />
                <div class="mock-stat-bar" :style="{ height: m.gastos + '%', background: 'rgba(248,113,113,0.7)' }" />
              </div>
              <span class="mock-stat-lbl">{{ m.mes }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const tabs = [
  { id: 'dashboard', label: 'Dashboard' },
  { id: 'gastos',    label: 'Gastos' },
  { id: 'historial', label: 'Historial' },
] as const

const active = ref<typeof tabs[number]['id']>('dashboard')

const mockGastos = [
  { tipo: 'Fijo',      cat: 'Arriendo',       monto: '$ 1.200.000' },
  { tipo: 'Ocasional', cat: 'Mercado',        monto: '$ 480.000' },
  { tipo: 'Fijo',      cat: 'Internet',       monto: '$ 95.000' },
  { tipo: 'Ocasional', cat: 'Transporte',     monto: '$ 210.000' },
]

const mockMeses = [
  { mes: 'Mar', ingresos: 70, gastos: 55 },
  { mes: 'Abr', ingresos: 80, gastos: 70 },
  { mes: 'May', ingresos: 60, gastos: 45 },
  { mes: 'Jun', ingresos: 95, gastos: 85 },
  { mes: 'Jul', ingresos: 75, gastos: 65 },
]
</script>

<style scoped>
.shots { padding: 5rem 2rem; }
.shots-inner { max-width: 900px; margin: 0 auto; text-align: center; }
.shots-title { font-size: 1.9rem; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0.5rem 0 2rem; }

.shots-tabs { display: flex; flex-wrap: wrap; justify-content: center; gap: 0.5rem; margin-bottom: 1.75rem; }
.shots-tab {
  font-family: var(--ff-mono);
  font-size: 0.72rem;
  letter-spacing: 0.06em;
  padding: 0.5rem 1.1rem;
  border-radius: 999px;
  border: 1px solid var(--border);
  background: transparent;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.15s;
}
.shots-tab:hover { border-color: var(--border-hover); color: var(--text-primary); }
.shots-tab--active { border-color: var(--accent); color: var(--accent); background: rgba(99,179,255,0.08); }

.shots-window {
  text-align: left;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 30px 60px -20px rgba(0,0,0,0.5);
}
.mock-titlebar { display: flex; align-items: center; gap: 0.4rem; padding: 0.75rem 1rem; border-bottom: 1px solid var(--border); }
.mock-dot { width: 9px; height: 9px; border-radius: 50%; }
.mock-titlebar-label { margin-left: 0.75rem; font-size: 0.68rem; color: var(--text-muted); }

.mock-body { padding: 1.75rem; min-height: 220px; }
.mock-kpis { display: flex; gap: 0.75rem; margin-bottom: 1.5rem; }
.mock-kpi {
  position: relative;
  flex: 1;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0.65rem 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  overflow: hidden;
}
.mock-kpi::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 2px;
  background: var(--ca, var(--border));
}
.mock-kpi-lbl { font-size: 0.58rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.06em; }
.mock-kpi-val { font-size: 0.85rem; font-weight: 700; }

.mock-donut-row { display: flex; align-items: flex-end; gap: 1.5rem; }
.mock-donut {
  width: 90px; height: 90px; border-radius: 50%; flex-shrink: 0;
  background: conic-gradient(var(--red) 0% 35%, var(--amber) 35% 60%, var(--green) 60% 100%);
  mask: radial-gradient(circle, transparent 55%, black 56%);
  -webkit-mask: radial-gradient(circle, transparent 55%, black 56%);
}
.mock-bars-v { flex: 1; display: flex; align-items: flex-end; gap: 0.5rem; height: 90px; }
.mock-bar { flex: 1; border-radius: 4px 4px 0 0; opacity: 0.85; }

.mock-list { display: flex; flex-direction: column; gap: 0.6rem; }
.mock-list-row { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0; border-bottom: 1px solid var(--border); }
.mock-list-row:last-child { border-bottom: none; }
.mock-list-cat { flex: 1; font-size: 0.8rem; color: var(--text-primary); }
.mock-list-val { font-size: 0.8rem; color: var(--text-primary); font-weight: 600; }

.mock-stat-grid { display: flex; align-items: flex-end; justify-content: space-around; height: 140px; gap: 1rem; }
.mock-stat-col { display: flex; flex-direction: column; align-items: center; justify-content: flex-end; gap: 0.5rem; height: 100%; flex: 1; }
.mock-stat-bars { flex: 1; display: flex; align-items: flex-end; gap: 3px; width: 100%; max-width: 34px; }
.mock-stat-bar { flex: 1; border-radius: 3px 3px 0 0; opacity: 0.9; }
.mock-stat-lbl { font-size: 0.65rem; color: var(--text-muted); }

@media (max-width: 600px) {
  .shots { padding: 3rem 1.25rem; }
  .shots-title { font-size: 1.5rem; }
  .mock-body { padding: 1.1rem; }
  .mock-kpis { flex-wrap: wrap; gap: 0.5rem; }
  .mock-kpi { min-width: calc(50% - 0.25rem); }
  .mock-donut-row { flex-direction: column; align-items: center; gap: 1rem; }
  .mock-bars-v { width: 100%; }
  .mock-stat-grid { gap: 0.4rem; height: 120px; }
}
</style>
