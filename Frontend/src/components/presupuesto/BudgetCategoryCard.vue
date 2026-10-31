<template>
  <!-- Una sola tarjeta contenedora; cada categoría es una fila separada por una línea sutil -->
  <div class="budget-list">
    <div
      v-for="(item, i) in items"
      :key="item.categoria"
      class="budget-row"
      :class="{
        'budget-row--editing': editingCat === item.categoria,
        'budget-row--sin-limite': !item.limite,
      }"
      :style="{ '--i': Math.min(i, 12), '--st': COLOR_ESTADO[item.estado] }"
    >
      <!-- 1. Ícono + nombre + gastado -->
      <div class="bc-main">
        <div class="bc-icon" :style="{ background: guessColor(item.categoria) + '22', color: guessColor(item.categoria) }">
          <component :is="guessIcono(item.categoria)" :size="18" :stroke-width="1.9" />
        </div>
        <div class="bc-info">
          <span class="bc-cat">{{ item.categoria }}</span>
          <span class="bc-sub">
            <strong>{{ fmt(item.gastado) }}</strong><template v-if="item.limite"> de {{ fmt(item.limite) }}</template><template v-else> gastado</template>
          </span>
        </div>
      </div>

      <!-- Edición del límite: ocupa barra + % + acciones -->
      <div v-if="editingCat === item.categoria" class="bc-edit">
        <label class="bc-edit-field">
          <span class="bc-edit-label">Límite mensual</span>
          <CurrencyInput
            v-model="editLimite"
            class="input input-edit"
            placeholder="$ 0"
            autofocus
            @keydown.enter.prevent="editLimite > 0 && emit('guardar', item.categoria)"
            @keydown.escape="emit('cancelar')"
          />
        </label>
        <div class="bc-edit-actions">
          <button class="btn btn-sm" @click="emit('cancelar')">Cancelar</button>
          <button class="btn btn-primary btn-sm" :disabled="saving || editLimite <= 0" @click="emit('guardar', item.categoria)">
            {{ saving ? '...' : 'Guardar' }}
          </button>
        </div>
      </div>

      <template v-else>
        <!-- 2. Barra (o aviso si no hay límite) -->
        <div class="bc-bar-col">
          <div v-if="item.limite" class="bc-bar-wrap" :title="`${item.pct}% usado`">
            <div class="bc-bar" :style="{ width: Math.max(Math.min(item.pct, 100), 1.5) + '%' }" />
          </div>
          <span v-else class="bc-sin-limite">Sin presupuesto asignado</span>
        </div>

        <!-- 3. Porcentaje + restante -->
        <div class="bc-pct">
          <template v-if="item.limite">
            <span class="bc-pct-value">{{ item.pct }}%</span>
            <span class="bc-resto" :class="{ 'bc-resto--over': item.gastado > item.limite }">
              {{ item.gastado > item.limite
                ? `${fmt(item.gastado - item.limite)} excedido`
                : `${fmt(item.limite - item.gastado)} restante` }}
            </span>
          </template>
        </div>

        <!-- 4. Acciones -->
        <div class="bc-actions">
          <button v-if="!item.limite" class="btn-asignar" @click="emit('edit', item.categoria, null)">
            <Plus :size="13" :stroke-width="2.4" /> Asignar límite
          </button>
          <template v-else>
            <button class="icon-btn" title="Editar límite" aria-label="Editar límite" @click="emit('edit', item.categoria, item.limite)">
              <Pencil :size="14" :stroke-width="1.9" />
            </button>
            <button class="icon-btn icon-btn--danger" title="Quitar límite" aria-label="Quitar límite" @click="emit('delete', item.categoria)">
              <Trash2 :size="14" :stroke-width="1.9" />
            </button>
          </template>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { fmtCOP as fmt } from '@/utils'
import { Pencil, Trash2, Plus } from 'lucide-vue-next'
import { guessIcono, guessColor } from '@/composables/useCategoriaOptions'
import type { PresupuestoMensualItem } from '@/composables/usePresupuestoMensual'
import CurrencyInput from '@/components/common/CurrencyInput.vue'

defineProps<{
  items: PresupuestoMensualItem[]
  editingCat: string
  saving: boolean
}>()

const editLimite = defineModel<number>('editLimite', { required: true })

const emit = defineEmits<{
  edit: [categoria: string, limite: number | null]
  guardar: [categoria: string]
  cancelar: []
  delete: [categoria: string]
}>()

// Color semántico por estado: verde vas bien, naranja cerca del límite, rojo al pasarse
const COLOR_ESTADO: Record<PresupuestoMensualItem['estado'], string> = {
  ok: 'var(--c-pos)',
  warn: 'var(--c-warn)',
  danger: 'var(--c-neg)',
  'sin-limite': 'var(--text-muted)',
}
</script>

<style scoped>
.budget-list {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  box-shadow: var(--shadow-card);
  padding: 0.35rem;
  font-variant-numeric: tabular-nums;
}

/* ── Fila: 4 columnas fijas para que todo quede alineado entre filas ── */
.budget-row {
  display: grid;
  grid-template-columns: minmax(190px, 1.3fr) minmax(120px, 2fr) 130px 150px;
  align-items: center;
  column-gap: 1.75rem;
  padding: 0.85rem 0.9rem;
  border-radius: 12px;
  position: relative;
  transition: background 0.12s ease;
  animation: row-in 0.3s cubic-bezier(0.2, 0, 0, 1) both;
  animation-delay: calc(var(--i) * 30ms);
}
/* Divisor sutil entre filas (inset, no toca los bordes redondeados) */
.budget-row + .budget-row::before {
  content: '';
  position: absolute;
  top: 0; left: 0.9rem; right: 0.9rem;
  height: 1px;
  background: var(--border);
}
.budget-row:hover,
.budget-row:focus-within { background: var(--surface2); }
.budget-row:hover::before,
.budget-row:hover + .budget-row::before { background: transparent; }
.budget-row--editing,
.budget-row--editing:hover { background: color-mix(in srgb, var(--accent) 9%, var(--surface2)); }

@keyframes row-in {
  from { opacity: 0; transform: translateY(4px); }
}

/* ── 1. Ícono + nombre ── */
.bc-main { display: flex; align-items: center; gap: 0.85rem; min-width: 0; }
.bc-icon {
  width: 40px; height: 40px; border-radius: 10px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
}
.bc-info { display: flex; flex-direction: column; gap: 0.2rem; min-width: 0; }
.bc-cat  {
  font-size: 0.86rem; font-weight: 700;
  color: var(--text-primary);
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.bc-sub  { font-size: 0.7rem; color: var(--text-subtle); white-space: nowrap; }
.bc-sub strong { font-family: var(--ff-mono); font-weight: 700; color: var(--text-secondary); }

/* ── 2. Barra ── */
.bc-bar-col { min-width: 0; padding: 0 0.25rem; }
.bc-bar-wrap {
  height: 8px; border-radius: 99px; overflow: hidden;
  background: color-mix(in srgb, var(--text-muted) 22%, var(--surface2));
}
.bc-bar {
  height: 100%; border-radius: 99px;
  background: var(--st);
  transform-origin: left center;
  transition: width 0.6s cubic-bezier(0.2, 0, 0, 1);
  animation: bar-grow 0.8s cubic-bezier(0.2, 0, 0, 1) both;
  animation-delay: calc(var(--i) * 30ms + 120ms);
}
@keyframes bar-grow { from { transform: scaleX(0); } }
.bc-sin-limite { font-size: 0.72rem; color: var(--text-muted); font-style: italic; }

/* ── 3. Porcentaje grande + restante debajo ── */
.bc-pct { display: flex; flex-direction: column; align-items: flex-end; gap: 0.15rem; }
.bc-pct-value {
  font-family: var(--ff-mono);
  font-size: 1.05rem; font-weight: 700; letter-spacing: -0.01em;
  color: var(--st);
}
.bc-resto { font-family: var(--ff-mono); font-size: 0.68rem; color: var(--text-muted); white-space: nowrap; }
.bc-resto--over { color: var(--c-neg); font-weight: 500; }

/* ── 4. Acciones: ocultas hasta hover (en táctil siempre visibles) ── */
.bc-actions { display: flex; align-items: center; justify-content: flex-end; gap: 0.15rem; }
.icon-btn {
  display: grid; place-items: center;
  width: 32px; height: 32px; padding: 0;
  background: none; border: none; border-radius: 8px;
  color: var(--text-secondary); cursor: pointer;
  opacity: 0; transform: translateX(4px);
  transition: opacity 0.15s ease, transform 0.15s ease, color 0.15s, background 0.15s;
}
.budget-row:hover .icon-btn,
.budget-row:focus-within .icon-btn { opacity: 1; transform: none; }
@media (hover: none) { .icon-btn { opacity: 1; transform: none; } }
.icon-btn:hover         { color: var(--text-primary); background: var(--surface); }
.icon-btn--danger:hover { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 12%, transparent); }

.btn-asignar {
  display: inline-flex; align-items: center; gap: 0.35rem;
  font: inherit; font-size: 0.72rem; font-weight: 500; white-space: nowrap;
  color: var(--text-secondary);
  background: var(--surface2);
  border: 1px solid transparent;
  border-radius: 8px; padding: 0.42rem 0.75rem; cursor: pointer;
  transition: color 0.15s, background 0.15s, border-color 0.15s;
}
.budget-row:hover .btn-asignar { background: var(--surface); }
.btn-asignar:hover { color: var(--text-primary); border-color: var(--border-hover); }

.icon-btn:focus-visible,
.btn-asignar:focus-visible { outline: 2px solid var(--accent); outline-offset: 2px; opacity: 1; }

/* ── Edición ── */
.bc-edit { grid-column: 2 / -1; display: flex; align-items: center; gap: 0.75rem; min-width: 0; }
.bc-edit-field { flex: 1; min-width: 0; display: flex; align-items: center; gap: 0.75rem; margin: 0; }
.bc-edit-label {
  font-size: 0.66rem; font-weight: 500; letter-spacing: 0.08em; text-transform: uppercase;
  color: var(--text-muted); white-space: nowrap;
}
.input-edit { flex: 1; min-width: 0; height: 38px; font-size: 0.88rem; font-weight: 700; }
.bc-edit-actions { display: flex; gap: 0.5rem; flex-shrink: 0; }

@media (prefers-reduced-motion: reduce) {
  .budget-row, .bc-bar { animation: none; }
}

/* ── Tablet ── */
@media (max-width: 1100px) {
  .budget-row { grid-template-columns: minmax(160px, 1.2fr) minmax(100px, 1.5fr) 115px 130px; column-gap: 1.1rem; }
}

/* ── Móvil: nombre + acciones arriba, barra y números abajo ── */
@media (max-width: 640px) {
  .budget-row {
    grid-template-columns: 1fr auto;
    grid-template-areas:
      "main actions"
      "bar  bar"
      "pct  pct";
    row-gap: 0.6rem;
    padding: 0.85rem 0.75rem;
  }
  .bc-main    { grid-area: main; }
  .bc-actions { grid-area: actions; }
  .bc-bar-col { grid-area: bar; padding: 0; }
  .bc-pct     { grid-area: pct; flex-direction: row; justify-content: space-between; align-items: baseline; }
  .budget-row--sin-limite .bc-pct { display: none; }
  .bc-edit    { grid-area: bar; grid-column: 1 / -1; flex-direction: column; align-items: stretch; gap: 0.6rem; }
  .bc-edit-field { flex-direction: column; align-items: stretch; gap: 0.35rem; }
  .bc-edit .btn-sm { flex: 1; }
}
</style>
