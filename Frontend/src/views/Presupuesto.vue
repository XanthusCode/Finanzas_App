<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Planificación</div>
        <h1 class="page-title">Presupuesto</h1>
      </div>
    </div>

    <div v-if="loadingInit" class="loading-list">
      <div v-for="i in 4" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <div v-for="tipo in (['FIJO', 'VARIABLE'] as const)" :key="tipo" class="card" :style="tipo === 'FIJO' ? 'margin-bottom: 0.75rem' : ''">
        <p class="card-title">{{ tipo === 'FIJO' ? 'Gastos fijos' : 'Gastos variables' }}</p>

        <div
          v-for="cat in categoriasPorTipo(tipo)"
          :key="cat.id ?? cat.nombre"
          class="budget-row"
        >
          <div class="budget-info">
            <span class="budget-cat">{{ cat.nombre }}</span>
            <span class="budget-nums">
              <span class="spent">{{ fmt(gastosPorCategoria[cat.nombre] ?? 0) }}</span>
              <span class="sep">/</span>
              <span class="limit" v-if="presupuestoPorCategoria[cat.nombre]">{{ fmt(presupuestoPorCategoria[cat.nombre]!.limite) }}</span>
              <span class="no-limit" v-else>sin límite</span>
            </span>
          </div>

          <div class="budget-bar-wrap" v-if="presupuestoPorCategoria[cat.nombre]">
            <div
              class="budget-bar"
              :class="pctGasto(cat.nombre) >= 100 ? 'bar-danger' : pctGasto(cat.nombre) >= 80 ? 'bar-warn' : 'bar-ok'"
              :style="{ width: Math.min(pctGasto(cat.nombre), 100) + '%' }"
            />
          </div>

          <div class="budget-actions">
            <template v-if="editingCat === cat.nombre">
              <CurrencyInput v-model="editLimite" class="input input-inline" style="max-width: 130px;" />
              <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar(cat.nombre)">{{ saving ? '...' : 'OK' }}</button>
              <button class="btn btn-sm" @click="editingCat = ''">✕</button>
            </template>
            <template v-else>
              <button class="btn btn-sm" @click="startEdit(cat.nombre)">
                {{ presupuestoPorCategoria[cat.nombre] ? 'Editar' : 'Asignar límite' }}
              </button>
              <button v-if="presupuestoPorCategoria[cat.nombre]" class="icon-btn danger" @click="eliminar(cat.nombre)">
                <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </template>
          </div>
        </div>

        <div v-if="categoriasPorTipo(tipo).length === 0" class="empty">
          Sin categorías — <a href="/categorias" style="color: var(--accent)">créalas aquí</a>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { fmtCOP as fmt } from '@/utils'
import CurrencyInput from '@/components/common/CurrencyInput.vue'

const store = useFinanceStore()
const toast = useToast()

const loadingInit = ref(false)
const saving      = ref(false)
const editingCat  = ref('')
const editLimite  = ref(0)

const categoriasPorTipo = (tipo: 'FIJO' | 'VARIABLE') =>
  store.categorias.filter(c => c.tipo === tipo && c.activa)

const presupuestoPorCategoria = computed(() => {
  const m: Record<string, { id: string; limite: number }> = {}
  for (const p of store.presupuestos) {
    m[p.categoria] = { id: p.id!, limite: p.limite }
  }
  return m
})

const gastosPorCategoria = computed(() => {
  const m: Record<string, number> = {}
  for (const g of store.gastos) {
    m[g.categoria] = (m[g.categoria] ?? 0) + g.monto
  }
  return m
})

function pctGasto(cat: string) {
  const p = presupuestoPorCategoria.value[cat]
  if (!p || p.limite === 0) return 0
  return Math.round((gastosPorCategoria.value[cat] ?? 0) / p.limite * 100)
}

function startEdit(cat: string) {
  editingCat.value = cat
  editLimite.value = presupuestoPorCategoria.value[cat]?.limite ?? 0
}

async function guardar(cat: string) {
  if (editLimite.value <= 0) return
  saving.value = true
  try {
    await store.guardarPresupuesto(cat, editLimite.value)
    toast.success('Presupuesto guardado')
    editingCat.value = ''
  } catch {
    toast.error('No se pudo guardar el presupuesto')
  } finally {
    saving.value = false
  }
}

async function eliminar(cat: string) {
  const p = presupuestoPorCategoria.value[cat]
  if (!p) return
  try {
    await store.eliminarPresupuesto(p.id)
    toast.success('Presupuesto eliminado')
  } catch {
    toast.error('No se pudo eliminar el presupuesto')
  }
}

onMounted(async () => {
  loadingInit.value = true
  try {
    await Promise.all([store.cargarCategorias(), store.cargarPresupuestos(), store.cargarDatos()])
  } finally {
    loadingInit.value = false
  }
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 0.75rem; }
.empty       { font-size: 0.72rem; color: var(--text-muted); padding: 0.75rem 0; }

.budget-row {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  padding: 0.65rem 0;
  border-bottom: 1px solid var(--border);
}
.budget-row:last-of-type { border-bottom: none; }

.budget-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.budget-cat  { font-size: 0.78rem; color: var(--text-primary); }
.budget-nums { font-size: 0.72rem; color: var(--text-muted); display: flex; align-items: center; gap: 0.25rem; }
.spent       { color: var(--text-primary); }
.sep         { color: var(--border); }
.no-limit    { color: var(--text-muted); font-style: italic; }

.budget-bar-wrap {
  height: 4px;
  border-radius: 2px;
  background: var(--surface2);
  overflow: hidden;
}
.budget-bar {
  height: 100%;
  border-radius: 2px;
  transition: width 0.4s ease;
}
.bar-ok     { background: var(--accent); }
.bar-warn   { background: #f59e0b; }
.bar-danger { background: var(--red); }

.budget-actions { display: flex; align-items: center; gap: 0.35rem; }

.btn-sm      { padding: 0.3rem 0.7rem; font-size: 0.72rem; }
.input-inline { height: 30px; font-size: 0.78rem; padding: 0 0.6rem; }

.icon-btn {
  background: none; border: none; color: var(--text-muted); cursor: pointer;
  padding: 4px 5px; border-radius: 4px; transition: color 0.15s, background 0.15s;
  display: flex; align-items: center; justify-content: center;
}
.icon-btn.danger:hover { color: var(--red); background: rgba(248,113,113,0.08); }

.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 52px; border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }
</style>
