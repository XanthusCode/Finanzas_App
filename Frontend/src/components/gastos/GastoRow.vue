<template>
  <div class="gasto-row" :class="{ 'mov-resaltado': resaltado }" :data-mov-id="gasto.id">
    <div class="row-left">
      <div class="cat-icon-badge" :style="{ background: guessColor(gasto.categoria) + '26', color: guessColor(gasto.categoria) }">
        <component :is="guessIcono(gasto.categoria)" :size="16" :stroke-width="2" />
      </div>
      <div class="row-text">
        <div class="row-name">
          <span class="row-name-text">{{ gasto.categoria }}</span>
          <Repeat v-if="gasto.esRecurrente" class="recurrente-icon" :size="11" :stroke-width="2.2" aria-label="Recurrente" />
          <span v-if="gasto.numCuotas" class="cuota-badge">{{ gasto.cuotaActual }}/{{ gasto.numCuotas }}</span>
        </div>
        <div v-if="gasto.detalle" class="row-det">{{ gasto.detalle }}</div>
      </div>
    </div>
    <div class="row-right">
      <span class="row-monto">{{ fmt(gasto.monto) }}</span>
      <div class="row-actions">
        <button class="icon-btn" title="Editar" aria-label="Editar gasto" @click="emit('edit', gasto)">
          <Pencil :size="14" :stroke-width="1.9" />
        </button>
        <button class="icon-btn danger" title="Eliminar" aria-label="Eliminar gasto" @click="emit('delete', gasto.id!)">
          <Trash2 :size="14" :stroke-width="1.9" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Pencil, Trash2, Repeat } from '@lucide/vue'
import type { Gasto } from '@/types'
import { fmtCOP as fmt } from '@/utils'
import { guessIcono, guessColor } from '@/composables/useCategoriaOptions'

defineProps<{ gasto: Gasto; resaltado?: boolean }>()
const emit = defineEmits<{ delete: [id: string], edit: [gasto: Gasto], create: [gasto: Gasto] }>()
</script>

<style scoped>
/* Sin separadores: la fila se marca con el hover. Se extiende hasta el borde de la tarjeta. */
.gasto-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin: 0 -0.6rem;
  padding: 0.8rem 0.6rem;
  border-radius: 10px;
  transition: background 0.12s ease;
}
.gasto-row:hover,
.gasto-row:focus-within { background: var(--surface2); }

.row-left  { display: flex; align-items: center; gap: 0.85rem; min-width: 0; }
.row-text  { min-width: 0; }
.row-name  { display: flex; align-items: center; gap: 0.4rem; font-size: 0.82rem; font-weight: 500; color: var(--text-primary); }
.row-name-text { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.row-det   {
  font-size: 0.7rem; color: var(--text-subtle); margin-top: 0.2rem;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}

.row-right { display: flex; align-items: center; gap: 0.75rem; flex-shrink: 0; }
.row-monto {
  font-family: var(--ff-mono);
  font-variant-numeric: tabular-nums;
  font-size: 0.84rem;
  font-weight: 700;
  color: var(--text-primary);
  text-align: right;
}

.cat-icon-badge {
  width: 34px;
  height: 34px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.recurrente-icon { color: var(--accent-strong); flex-shrink: 0; }
.cuota-badge {
  font-size: 0.6rem; font-weight: 500; letter-spacing: 0.04em;
  background: color-mix(in srgb, var(--c-ai) 14%, transparent); color: var(--c-ai);
  border-radius: 999px; padding: 1px 7px;
}

/* Acciones: ocultas hasta hover/foco en la fila (en táctil siempre visibles) */
.row-actions {
  display: flex;
  gap: 0.15rem;
  opacity: 0;
  transform: translateX(4px);
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.gasto-row:hover .row-actions,
.gasto-row:focus-within .row-actions { opacity: 1; transform: none; }
@media (hover: none) {
  .row-actions { opacity: 1; transform: none; }
}

.icon-btn {
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;
  padding: 0;
  background: none;
  border: none;
  border-radius: 8px;
  color: var(--text-secondary);
  cursor: pointer;
  transition: color 0.15s, background 0.15s;
}
.icon-btn:hover        { color: var(--text-primary); background: var(--surface); }
.icon-btn.danger:hover { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 12%, transparent); }
.icon-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }
</style>
