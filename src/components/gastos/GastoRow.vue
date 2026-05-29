<template>
  <div class="gasto-row">
    <div class="row-left">
      <span class="tag" :class="gasto.tipo === 'FIJO' ? 'tag-red' : 'tag-amber'">
        {{ gasto.tipo === 'FIJO' ? 'Fijo' : 'Variable' }}
      </span>
      <div>
        <div class="row-name">{{ gasto.categoria }}</div>
        <div class="row-det">{{ gasto.detalle }}</div>
      </div>
    </div>
    <div class="row-right">
      <span class="row-monto">{{ fmt(gasto.monto) }}</span>
      <button class="icon-btn" title="Eliminar" @click="emit('delete', gasto.id!)">&#10005;</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Gasto } from '@/types'

defineProps<{ gasto: Gasto }>()
const emit = defineEmits<{ delete: [id: number] }>()

const fmt = (n: number) => '$' + Math.round(n).toLocaleString('es-CO')
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
.row-left { display: flex; align-items: center; gap: 0.75rem; }
.row-name { font-size: 0.78rem; color: var(--text-primary); }
.row-det  { font-size: 0.65rem; color: var(--text-muted); margin-top: 0.1rem; }
.row-right { display: flex; align-items: center; gap: 0.75rem; }
.row-monto { font-size: 0.78rem; color: var(--text-primary); }
.icon-btn {
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  font-size: 0.7rem;
  padding: 2px 4px;
  border-radius: 3px;
  transition: all 0.2s;
}
.icon-btn:hover { color: var(--red); background: rgba(248,113,113,0.08); }
</style>