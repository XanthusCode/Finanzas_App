<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Planificación</div>
        <h1 class="page-title">Presupuesto</h1>
      </div>
      <MonthSelector :mes="mes" :anio="anio" @change="onCambioMes" />
    </div>

    <div v-if="hayPeligro" class="banner-danger">
      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
        <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
        <line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>
      </svg>
      Tienes categorías que superaron su presupuesto este mes
    </div>

    <div v-if="loadingInit || composableLoading" class="loading-list">
      <div v-for="i in 4" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <div v-for="tipo in (['FIJO', 'VARIABLE'] as const)" :key="tipo" class="seccion">
        <div class="seccion-header">
          <span class="seccion-titulo">{{ tipo === 'FIJO' ? 'Gastos fijos' : 'Gastos variables' }}</span>
          <span class="seccion-resumen">
            {{ itemsConLimite(tipo).length }} de {{ itemsPorTipo(tipo).length }} con límite
          </span>
        </div>

        <div v-if="itemsPorTipo(tipo).length === 0" class="empty">
          Sin categorías —
          <RouterLink to="/categorias" class="link">créalas aquí</RouterLink>
        </div>

        <div v-for="item in itemsPorTipo(tipo)" :key="item.categoria" class="budget-card">
          <!-- Vista normal -->
          <template v-if="editingCat !== item.categoria">
            <div class="bc-top">
              <span class="bc-cat">{{ item.categoria }}</span>
              <div class="bc-acciones">
                <span v-if="item.limite" class="bc-badge" :class="`badge-${item.estado}`">
                  {{ item.pct.toFixed(0) }}%
                </span>
                <button class="icon-btn" :title="item.limite ? 'Editar límite' : 'Asignar límite'" @click="startEdit(item.categoria, item.limite)">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                </button>
                <button v-if="item.limite" class="icon-btn danger" title="Quitar límite" @click="eliminar(item.categoria)">
                  <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                </button>
              </div>
            </div>

            <!-- Con límite -->
            <template v-if="item.limite">
              <div class="bc-bar-wrap">
                <div
                  class="bc-bar"
                  :class="`bar-${item.estado}`"
                  :style="{ width: Math.min(item.pct, 100) + '%' }"
                />
              </div>
              <div class="bc-nums">
                <span class="bc-gastado">{{ fmt(item.gastado) }} gastado</span>
                <span class="bc-resto" :class="item.estado === 'danger' ? 'resto-over' : 'resto-ok'">
                  {{ item.estado === 'danger'
                      ? `${fmt(item.gastado - item.limite)} excedido`
                      : `${fmt(item.limite - item.gastado)} disponible` }}
                </span>
                <span class="bc-limite">de {{ fmt(item.limite) }}</span>
              </div>
            </template>

            <!-- Sin límite -->
            <div v-else class="bc-sin-limite">
              <span class="bc-gastado-solo">{{ fmt(item.gastado) }} gastado</span>
              <button class="btn-asignar" @click="startEdit(item.categoria, null)">
                + Asignar límite
              </button>
            </div>
          </template>

          <!-- Modo edición -->
          <template v-else>
            <div class="bc-edit">
              <span class="bc-cat">{{ item.categoria }}</span>
              <div class="bc-edit-form">
                <span class="edit-label">Límite mensual</span>
                <div class="edit-row">
                  <CurrencyInput v-model="editLimite" class="input input-edit" />
                  <button class="btn" @click="editingCat = ''">Cancelar</button>
                  <button class="btn btn-primary" :disabled="saving || editLimite <= 0" @click="guardar(item.categoria)">
                    {{ saving ? '...' : 'Guardar' }}
                  </button>
                </div>
              </div>
            </div>
          </template>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { fmtCOP as fmt } from '@/utils'
import { usePresupuestoMensual } from '@/composables/usePresupuestoMensual'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import MonthSelector from '@/components/common/MonthSelector.vue'

const store = useFinanceStore()
const toast = useToast()

const loadingInit = ref(false)
const saving      = ref(false)
const editingCat  = ref('')
const editLimite  = ref(0)

const mes  = ref(new Date().getMonth() + 1)
const anio = ref(new Date().getFullYear())

const { items, loading: composableLoading, cargar } = usePresupuestoMensual(mes, anio)

const hayPeligro = computed(() => items.value.some(i => i.estado === 'danger'))

const itemsPorTipo    = (tipo: 'FIJO' | 'VARIABLE') => items.value.filter(i => i.tipo === tipo)
const itemsConLimite  = (tipo: 'FIJO' | 'VARIABLE') => items.value.filter(i => i.tipo === tipo && i.limite)

function onCambioMes(m: number, a: number) {
  mes.value = m
  anio.value = a
}

function startEdit(cat: string, limiteActual: number | null) {
  editingCat.value = cat
  editLimite.value = limiteActual ?? 0
}

async function guardar(cat: string) {
  if (editLimite.value <= 0) return
  saving.value = true
  try {
    await store.guardarPresupuesto(cat, editLimite.value)
    await cargar()
    toast.success('Presupuesto guardado')
    editingCat.value = ''
  } catch {
    toast.error('No se pudo guardar el presupuesto')
  } finally {
    saving.value = false
  }
}

async function eliminar(cat: string) {
  const p = store.presupuestos.find(x => x.categoria === cat)
  if (!p) return
  try {
    await store.eliminarPresupuesto(p.id!)
    await cargar()
    toast.success('Límite eliminado')
  } catch {
    toast.error('No se pudo eliminar el presupuesto')
  }
}

onMounted(async () => {
  loadingInit.value = true
  try {
    await Promise.all([store.cargarCategorias(), store.cargarPresupuestos()])
    await cargar()
  } finally {
    loadingInit.value = false
  }
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.empty       { font-size: 0.72rem; color: var(--text-muted); padding: 0.5rem 0; }
.link        { color: var(--accent); text-decoration: none; }
.link:hover  { text-decoration: underline; }

.banner-danger {
  display: flex; align-items: center; gap: 0.5rem;
  background: rgba(248,113,113,0.08); border: 1px solid rgba(248,113,113,0.2);
  border-radius: 8px; padding: 0.6rem 0.9rem;
  font-size: 0.78rem; color: var(--red); margin-bottom: 1rem;
}

/* Sección */
.seccion { margin-bottom: 1.25rem; }
.seccion-header {
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 0.6rem;
}
.seccion-titulo  { font-size: 0.65rem; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: var(--text-muted); }
.seccion-resumen { font-size: 0.62rem; color: var(--text-muted); }

/* Card por categoría */
.budget-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 0.85rem 1rem;
  margin-bottom: 0.5rem;
  transition: border-color 0.15s;
}
.budget-card:hover { border-color: rgba(99,179,255,0.25); }

/* Top row: nombre + acciones */
.bc-top {
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 0.5rem;
}
.bc-cat { font-size: 0.82rem; font-weight: 600; color: var(--text-primary); }

.bc-acciones { display: flex; align-items: center; gap: 0.3rem; }

/* Badge de porcentaje */
.bc-badge {
  font-size: 0.6rem; font-weight: 700; padding: 0.15rem 0.5rem;
  border-radius: 999px; letter-spacing: 0.03em;
}
.badge-ok     { background: rgba(52,211,153,0.12); color: #34d399; }
.badge-warn   { background: rgba(245,158,11,0.12); color: #f59e0b; }
.badge-danger { background: rgba(248,113,113,0.12); color: var(--red); }
.badge-sin-limite { display: none; }

/* Barra */
.bc-bar-wrap {
  height: 6px; background: var(--surface2); border-radius: 3px;
  overflow: hidden; margin-bottom: 0.45rem;
}
.bc-bar { height: 100%; border-radius: 3px; transition: width 0.5s ease; }
.bar-ok     { background: #34d399; }
.bar-warn   { background: #f59e0b; }
.bar-danger { background: var(--red); }

/* Números debajo de la barra */
.bc-nums {
  display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap;
}
.bc-gastado { font-size: 0.72rem; font-weight: 600; color: var(--text-primary); }
.bc-reste   { font-size: 0.65rem; }
.bc-limite  { font-size: 0.65rem; color: var(--text-muted); margin-left: auto; }
.bc-resto   { font-size: 0.65rem; font-weight: 600; }
.resto-ok   { color: #34d399; }
.resto-over { color: var(--red); }

/* Sin límite */
.bc-sin-limite {
  display: flex; align-items: center; justify-content: space-between; gap: 0.75rem;
}
.bc-gastado-solo { font-size: 0.72rem; color: var(--text-muted); }
.btn-asignar {
  font-size: 0.65rem; font-weight: 600; color: var(--accent);
  background: rgba(99,179,255,0.08); border: 1px solid rgba(99,179,255,0.2);
  border-radius: 5px; padding: 0.25rem 0.65rem; cursor: pointer;
  transition: background 0.15s; white-space: nowrap;
}
.btn-asignar:hover { background: rgba(99,179,255,0.16); }

/* Modo edición */
.bc-edit { display: flex; flex-direction: column; gap: 0.65rem; }
.bc-edit-form { display: flex; flex-direction: column; gap: 0.35rem; }
.edit-label { font-size: 0.62rem; color: var(--text-muted); letter-spacing: 0.06em; text-transform: uppercase; }
.edit-row { display: flex; align-items: center; gap: 0.5rem; }
.input-edit { height: 34px; font-size: 0.82rem; flex: 1; max-width: 200px; }

/* Icon btns */
.icon-btn {
  background: none; border: none; color: var(--text-muted); cursor: pointer;
  padding: 4px 5px; border-radius: 4px; transition: color 0.15s, background 0.15s;
  display: flex; align-items: center; justify-content: center;
}
.icon-btn:hover        { color: var(--accent); background: rgba(99,179,255,0.08); }
.icon-btn.danger:hover { color: var(--red);    background: rgba(248,113,113,0.08); }

/* Loading */
.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 80px; border-radius: 10px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }
</style>
