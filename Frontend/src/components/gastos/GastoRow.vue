<template>
  <div class="gasto-row">
    <div class="row-left">
      <span class="tag" :class="gasto.tipo === 'FIJO' ? 'tag-red' : 'tag-amber'">
        {{ gasto.tipo === 'FIJO' ? 'Fijo' : 'Variable' }}
      </span>
      <div>
        <div class="row-name">
          {{ gasto.categoria }}
          <svg v-if="gasto.esRecurrente" class="recurrente-icon" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" title="Recurrente"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>
        </div>
        <div class="row-det">{{ gasto.detalle }}</div>
      </div>
    </div>
    <div class="row-right">
      <span class="row-monto">{{ fmt(gasto.monto) }}</span>
      <button class="icon-btn" title="Editar" @click="emit('edit', gasto)">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
      </button>
      <button class="icon-btn danger" title="Eliminar" @click="emit('delete', gasto.id!)">
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Gasto } from '@/types'
import { fmtCOP as fmt } from '@/utils'

defineProps<{ gasto: Gasto }>()
const emit = defineEmits<{ delete: [id: string], edit: [gasto: Gasto], create: [gasto: Gasto] }>()
</script>

<style scoped>
.gasto-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.65rem 0;
  border-bottom: 1px solid var(--border);
}
.gasto-row:last-child { border-bottom: none; }
.row-left  { display: flex; align-items: center; gap: 0.75rem; }
.row-name  { font-size: 0.78rem; color: var(--text-primary); display: flex; align-items: center; gap: 0.35rem; }
.row-det   { font-size: 0.65rem; color: var(--text-muted); margin-top: 0.1rem; }
.row-right { display: flex; align-items: center; gap: 0.5rem; }
.row-monto { font-size: 0.78rem; color: var(--text-primary); margin-right: 0.25rem; }

.recurrente-icon { color: var(--accent); flex-shrink: 0; }

.icon-btn {
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px 5px;
  border-radius: 4px;
  transition: color 0.15s, background 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.icon-btn:hover       { color: var(--accent); background: rgba(99, 179, 255, 0.08); }
.icon-btn.danger:hover { color: var(--red);  background: rgba(248, 113, 113, 0.08); }
</style>
