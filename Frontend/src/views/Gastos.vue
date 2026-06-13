<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Registro</div>
        <h1 class="page-title">Gastos</h1>
      </div>
      <div style="display: flex; align-items: center; gap: 0.75rem;">
        <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
        <button v-if="store.gastos.length > 0" class="btn btn-primary" @click="descargarCSV">
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          CSV
        </button>
      </div>
    </div>

    <!-- Barra de búsqueda + filtros -->
    <div v-if="!store.loading && store.gastos.length > 0" class="filter-bar">
      <div class="search-wrap">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="search-icon"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
        <input v-model="busqueda" class="input search-input" placeholder="Buscar por detalle o categoría..." />
      </div>
      <select v-model="filtroCategoria" class="input filter-select">
        <option value="">Todas las categorías</option>
        <option v-for="cat in todasCategorias" :key="cat" :value="cat">{{ cat }}</option>
      </select>
      <button v-if="busqueda || filtroCategoria" class="btn btn-primary" @click="busqueda = ''; filtroCategoria = ''">
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
        Limpiar
      </button>
    </div>

    <div v-if="store.loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="store.gastosFijos.length + store.gastosVariables.length > 0 || tipoAbierto">

      <!-- Gastos fijos -->
      <div class="card" style="margin-bottom: 0.75rem;">
        <div class="card-header">
          <span class="card-title">Gastos fijos</span>
          <div style="display: flex; align-items: center; gap: 0.5rem;">
            <span class="tag tag-red">{{ fmt(totalFiltradoFijos) }}</span>
            <button v-if="!editando && tipoAbierto !== 'FIJO'" class="btn btn-sm" @click="toggleFormPara('FIJO')">
              <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              Agregar
            </button>
          </div>
        </div>

        <Transition name="form-slide">
          <div v-if="tipoAbierto === 'FIJO'" class="form-wrap">
            <GastoForm
              :key="editando?.id ?? 'new-fijo'"
              :gasto="editando ?? undefined"
              :tipo-forzado="editando ? undefined : 'FIJO'"
              :loading="saving"
              @submit="onSubmit"
              @cancel="cerrarForm"
            />
          </div>
        </Transition>

        <template v-if="tipoAbierto !== 'FIJO'">
          <div v-if="filtradosFijos.length === 0" class="empty">Sin gastos fijos este mes</div>
          <GastoRow v-for="g in filtradosFijos" :key="g.id" :gasto="g" @edit="abrirEdicion" @delete="onDelete" />
        </template>
      </div>

      <!-- Gastos variables -->
      <div class="card">
        <div class="card-header">
          <span class="card-title">Gastos variables</span>
          <div style="display: flex; align-items: center; gap: 0.5rem;">
            <span class="tag tag-amber">{{ fmt(totalFiltradoVariables) }}</span>
            <button v-if="!editando && tipoAbierto !== 'VARIABLE'" class="btn btn-sm" @click="toggleFormPara('VARIABLE')">
              <svg  width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                Agregar
            </button>
          </div>
        </div>

        <Transition name="form-slide">
          <div v-if="tipoAbierto === 'VARIABLE'" class="form-wrap">
            <GastoForm
              :key="editando?.id ?? 'new-variable'"
              :gasto="editando ?? undefined"
              :tipo-forzado="editando ? undefined : 'VARIABLE'"
              :loading="saving"
              @submit="onSubmit"
              @cancel="cerrarForm"
            />
          </div>
        </Transition>

        <template v-if="tipoAbierto !== 'VARIABLE'">
          <div v-if="filtradosVariables.length === 0" class="empty">Sin gastos variables este mes</div>
          <GastoRow v-for="g in filtradosVariables" :key="g.id" :gasto="g" @edit="abrirEdicion" @delete="onDelete" />
        </template>
      </div>
    </template>

    <template v-else>
      <div class="empty-state">
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.3" style="color: var(--text-muted)"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
        <p class="empty-state-title">Sin gastos este mes</p>
        <p class="empty-state-sub">Registra tu primer gasto para empezar a hacer seguimiento</p>
        <div style="display: flex; gap: 0.5rem; margin-top: 0.75rem; flex-wrap: wrap; justify-content: center;">
          <button class="btn btn-primary" @click="toggleFormPara('FIJO')">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            Gasto fijo
          </button>
          <button class="btn btn-primary" @click="toggleFormPara('VARIABLE')">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            Gasto variable
          </button>
          <button class="btn" :disabled="copiando" @click="copiarRecurrentes">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>
            {{ copiando ? 'Copiando...' : 'Copiar recurrentes' }}
          </button>
        </div>
      </div>
    </template>

    <ConfirmModal
      v-model="showConfirm"
      title="Eliminar gasto"
      message="¿Seguro que quieres eliminar este gasto? Esta acción no se puede deshacer."
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { fmtCOP as fmt, exportCSV } from '@/utils'
import type { Gasto } from '@/types'
import MonthSelector from '@/components/common/MonthSelector.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import GastoForm from '@/components/gastos/GastoForm.vue'
import GastoRow from '@/components/gastos/GastoRow.vue'

const store   = useFinanceStore()
const toast   = useToast()

const tipoAbierto     = ref<'FIJO' | 'VARIABLE' | null>(null)
const editando        = ref<Gasto | null>(null)
const saving          = ref(false)
const copiando        = ref(false)
const showConfirm     = ref(false)
const pendingDeleteId = ref<string | null>(null)
const busqueda        = ref('')
const filtroCategoria = ref('')

const todasCategorias = computed(() => {
  const cats = new Set(store.gastos.map(g => g.categoria))
  return [...cats].sort()
})

function filtrar(lista: Gasto[]) {
  return lista.filter(g => {
    const q = busqueda.value.toLowerCase()
    const matchQ   = !q || g.detalle.toLowerCase().includes(q) || g.categoria.toLowerCase().includes(q)
    const matchCat = !filtroCategoria.value || g.categoria === filtroCategoria.value
    return matchQ && matchCat
  })
}

const filtradosFijos     = computed(() => filtrar(store.gastosFijos))
const filtradosVariables = computed(() => filtrar(store.gastosVariables))
const totalFiltradoFijos     = computed(() => filtradosFijos.value.reduce((s, g) => s + g.monto, 0))
const totalFiltradoVariables = computed(() => filtradosVariables.value.reduce((s, g) => s + g.monto, 0))

function toggleFormPara(tipo: 'FIJO' | 'VARIABLE') {
  if (tipoAbierto.value === tipo && !editando.value) {
    cerrarForm()
  } else {
    editando.value   = null
    tipoAbierto.value = tipo
  }
}

function abrirEdicion(gasto: Gasto) {
  editando.value    = gasto
  tipoAbierto.value = gasto.tipo
}

function cerrarForm() {
  tipoAbierto.value = null
  editando.value    = null
}

async function onSubmit(gasto: Omit<Gasto, 'id'>) {
  saving.value = true
  try {
    if (editando.value?.id != null) {
      await store.editarGasto(editando.value.id, gasto)
      toast.success('Gasto actualizado')
    } else {
      await store.agregarGasto(gasto)
      toast.success('Gasto agregado')
    }
    cerrarForm()
  } catch {
    toast.error('No se pudo guardar el gasto')
  } finally {
    saving.value = false
  }
}

function onDelete(id: string) {
  pendingDeleteId.value = id
  showConfirm.value = true
}

async function confirmDelete() {
  if (pendingDeleteId.value != null) {
    try {
      await store.eliminarGasto(pendingDeleteId.value)
      toast.success('Gasto eliminado')
    } catch {
      toast.error('No se pudo eliminar el gasto')
    } finally {
      pendingDeleteId.value = null
    }
  }
}

function descargarCSV() {
  exportCSV(
    store.gastos.map(g => ({ Tipo: g.tipo, Categoria: g.categoria, Detalle: g.detalle, Monto: g.monto, Recurrente: g.esRecurrente ? 'Sí' : 'No' })),
    `gastos-${store.mesActual}-${store.anioActual}.csv`
  )
}

async function copiarRecurrentes() {
  copiando.value = true
  try {
    const n = await store.copiarRecurrentes()
    if (n > 0) toast.success(`${n} gasto${n > 1 ? 's' : ''} recurrente${n > 1 ? 's' : ''} copiado${n > 1 ? 's' : ''}`)
    else toast.info('No hay gastos recurrentes del mes anterior')
  } catch {
    toast.error('No se pudieron copiar los gastos recurrentes')
  } finally {
    copiando.value = false
  }
}

onMounted(() => { store.cargarDatos(); store.cargarCategorias() })
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.empty       { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }

.form-wrap { margin-bottom: 1rem; }

/* Filter bar */
.filter-bar {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}
.search-wrap {
  position: relative;
  flex: 1;
  min-width: 180px;
}
.search-icon {
  position: absolute;
  left: 0.65rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-muted);
  pointer-events: none;
}
.search-input  { padding-left: 2rem; }
.filter-select {  cursor: pointer; width: auto; min-width: 160px; }

.btn-sm { padding: 0.3rem 0.65rem; font-size: 0.72rem; }

/* Slide-down transition */
.form-slide-enter-active { transition: opacity 0.22s ease, transform 0.22s ease; }
.form-slide-leave-active { transition: opacity 0.18s ease, transform 0.18s ease; }
.form-slide-enter-from   { opacity: 0; transform: translateY(-8px); }
.form-slide-leave-to     { opacity: 0; transform: translateY(-8px); }

/* Skeleton loading */
.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 52px;
  border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

/* Empty state */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 0 3rem;
  text-align: center;
  gap: 0.4rem;
}
.empty-state-title { font-size: 0.85rem; color: var(--text-secondary); font-weight: 600; margin-top: 0.5rem; }
.empty-state-sub   { font-size: 0.72rem; color: var(--text-muted); max-width: 280px; }
</style>