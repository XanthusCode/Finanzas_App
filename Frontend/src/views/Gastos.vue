<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Registro</div>
        <h1 class="page-title">Gastos</h1>
      </div>
      <div class="header-actions">
        <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
        <button v-if="store.gastos.length > 0" class="btn btn-primary" @click="descargarExcel">
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          Excel
        </button>
        <button class="btn" @click="csvInput?.click()">
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
          Importar
        </button>
        <input ref="csvInput" type="file" accept=".csv" style="display:none" @change="onImportCSV" />
        <input ref="reciboInput" type="file" accept=".pdf,.jpg,.jpeg,.png" style="display:none" @change="onImportRecibo" />

        <button class="btn " @click="reciboInput?.click()">
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.5 4h-5L7 7H4a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-3l-2.5-3z"/><circle cx="12" cy="13" r="3"/></svg>
          Escanear Recibo
        </button>
      </div>
    </div>

    <!-- KPIs del mes -->
    <div v-if="!store.loading && store.gastos.length > 0" class="kpi-grid">
      <div class="kpi-card" style="--ca: var(--red)">
        <div class="kpi-top">
          <span class="kpi-label">Gastos fijos</span>
          <svg class="kpi-icon" style="color:var(--red)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
        </div>
        <div class="kpi-value" style="color: var(--red)">{{ fmt(totalFiltradoFijos) }}</div>
      </div>
      <div class="kpi-card" style="--ca: var(--amber)">
        <div class="kpi-top">
          <span class="kpi-label">Gastos ocasionales</span>
          <svg class="kpi-icon" style="color:var(--amber)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3h18v18H3z"/><path d="M3 9h18M9 21V9"/></svg>
        </div>
        <div class="kpi-value" style="color: var(--amber)">{{ fmt(totalFiltradoVariables) }}</div>
      </div>
      <div class="kpi-card" style="--ca: var(--accent)">
        <div class="kpi-top">
          <span class="kpi-label">Total gastos</span>
          <svg class="kpi-icon" style="color:var(--accent)" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
        </div>
        <div class="kpi-value" style="color: var(--accent)">{{ fmt(totalFiltradoFijos + totalFiltradoVariables) }}</div>
      </div>
    </div>

    <!-- Barra de búsqueda + filtros -->
    <div v-if="!store.loading && store.gastos.length > 0" class="filter-bar">
      <div class="search-wrap">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="search-icon"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
        <input v-model="busqueda" class="input search-input" placeholder="Buscar por detalle o categoría..." />
      </div>
      <div class="select-wrap">
        <select v-model="filtroCategoria" class="input filter-select">
          <option value="">Todas las categorías</option>
          <option v-for="cat in todasCategorias" :key="cat" :value="cat">{{ cat }}</option>
        </select>
        <ChevronDown class="select-icon" :size="14" :stroke-width="2" />
      </div>
    </div>

    <!-- Banner recurrentes pendientes -->
    <Transition name="banner-slide">
      <div v-if="store.promptRecurrentes && !store.loading" class="recurrentes-banner">
        <div class="banner-left">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>
          <span>Hay gastos recurrentes del mes anterior — ¿los copiamos?</span>
        </div>
        <div class="banner-actions">
          <button class="btn btn-sm btn-primary" :disabled="copiando" @click="copiarRecurrentes">
            {{ copiando ? 'Copiando...' : 'Copiar' }}
          </button>
          <button class="banner-close" @click="store.descartarPromptRecurrentes">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>
      </div>
    </Transition>

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
            <button v-if="!editando && tipoAbierto !== 'FIJO'" class="btn btn-sm btn-primary" @click="toggleFormPara('FIJO')">
              <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              Nuevo gasto
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

      <!-- Gastos ocasionales -->
      <div class="card">
        <div class="card-header">
          <span class="card-title">Gastos ocasionales</span>
          <div style="display: flex; align-items: center; gap: 0.5rem;">
            <span class="tag tag-amber">{{ fmt(totalFiltradoVariables) }}</span>
            <button v-if="!editando && tipoAbierto !== 'VARIABLE'" class="btn btn-sm btn-primary" @click="toggleFormPara('VARIABLE')">
              <svg  width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                Nuevo gasto
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
          <div v-if="filtradosVariables.length === 0" class="empty">Sin gastos ocasionales este mes</div>
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
            Gasto ocasional
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

    <EscanearReciboModal v-model="showReciboModal" :datos="datosRecibo" @submit="onSubmitRecibo" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ChevronDown } from '@lucide/vue'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { fmtCOP as fmt, exportExcel } from '@/utils'
import type { Gasto, ReciboExtraido } from '@/types'
import MonthSelector from '@/components/common/MonthSelector.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import GastoForm from '@/components/gastos/GastoForm.vue'
import GastoRow from '@/components/gastos/GastoRow.vue'
import EscanearReciboModal from '@/components/gastos/EscanearReciboModal.vue'

const store    = useFinanceStore()
const toast    = useToast()
const csvInput = ref<HTMLInputElement>()
const reciboInput = ref<HTMLInputElement>()
const showReciboModal = ref(false)
const datosRecibo = ref<ReciboExtraido | null>(null)

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

async function onImportCSV(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  const text = await file.text()
  const lines = text.trim().split('\n').slice(1) // skip header
  const filas = lines.flatMap(line => {
    const cols = line.match(/(".*?"|[^,]+)(?=,|$)/g)?.map(c => c.replace(/^"|"$/g, '').trim()) ?? []
    if (cols.length < 4) return []
    const [tipo, categoria, detalle, montoStr, recurrente] = cols
    const monto = parseFloat(montoStr ?? '0')
    if (!tipo || !categoria || isNaN(monto)) return []
    return [{ tipo: tipo as 'FIJO' | 'VARIABLE', categoria, detalle: detalle ?? '', monto, esRecurrente: recurrente === 'Sí' }]
  })
  if (csvInput.value) csvInput.value.value = ''
  if (filas.length === 0) { toast.error('No se encontraron filas válidas en el CSV'); return }
  try {
    const n = await store.importarGastos(filas)
    toast.success(`${n} gasto${n !== 1 ? 's' : ''} importado${n !== 1 ? 's' : ''}`)
  } catch {
    toast.error('Error al importar el CSV')
  }
}

async function onImportRecibo(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  try {
    datosRecibo.value = await store.importarRecibo(file)
    showReciboModal.value = true
  } catch {
    toast.error('Error al escanear el recibo')
  } finally {
    if (reciboInput.value) reciboInput.value.value = ''
  }
}

async function onSubmitRecibo(gasto: Omit<Gasto, 'id'>) {
  try {
    await store.agregarGasto(gasto)
    toast.success('Gasto agregado')
  } catch {
    toast.error('No se pudo guardar el gasto')
  }
}

function descargarExcel() {
  exportExcel(
    store.gastos.map(g => ({ Tipo: g.tipo, Categoria: g.categoria, Detalle: g.detalle, Monto: g.monto, Recurrente: g.esRecurrente ? 'Sí' : 'No' })),
    `gastos-${store.mesActual}-${store.anioActual}.xlsx`,
    'Gastos'
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

onMounted(() => { store.cargarDatos(); store.cargarCategorias(); store.cargarCuentas() })
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.header-actions { display: flex; align-items: center; gap: 0.75rem; flex-wrap: nowrap; overflow-x: auto; }
.header-actions > * { flex-shrink: 0; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.empty       { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }

.form-wrap { margin-bottom: 1rem; }

/* KPIs — mismo estilo que Dashboard.vue */
.kpi-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem; margin-bottom: 1rem; }
.kpi-card {
  position: relative;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1rem 1.1rem;
  overflow: hidden;
}
.kpi-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 2px;
  background: var(--ca, var(--border));
  border-radius: 10px 10px 0 0;
}
.kpi-top   { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
.kpi-label { font-size: 0.62rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.kpi-icon  { opacity: 0.7; flex-shrink: 0; }
.kpi-value { font-size: 1.25rem; font-family: var(--ff-display); font-weight: 700; }

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
.select-wrap { min-width: 160px; flex-shrink: 0; }
.filter-select { cursor: pointer; }

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

/* Banner recurrentes */
.recurrentes-banner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  background: var(--surface);
  border: 1px solid color-mix(in srgb, var(--accent) 20%, transparent);
  border-left: 3px solid var(--accent);
  border-radius: 8px;
  padding: 0.6rem 0.85rem;
  margin-bottom: 1rem;
  font-size: 0.75rem;
  color: var(--text-secondary);
}
.banner-left  { display: flex; align-items: center; gap: 0.5rem; }
.banner-actions { display: flex; align-items: center; gap: 0.35rem; flex-shrink: 0; }
.banner-close {
  background: none; border: none; cursor: pointer; color: var(--text-muted);
  padding: 3px 5px; border-radius: 4px; display: flex; align-items: center;
  transition: color 0.15s;
}
.banner-close:hover { color: var(--text-primary); }

.banner-slide-enter-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.banner-slide-leave-active { transition: opacity 0.15s ease, transform 0.15s ease; }
.banner-slide-enter-from   { opacity: 0; transform: translateY(-6px); }
.banner-slide-leave-to     { opacity: 0; transform: translateY(-6px); }

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

@media (max-width: 500px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 1.25rem; margin-bottom: 1.25rem; }
  .page-title { font-size: 2rem;  }
  .section-label { font-size: 0.75rem; }
  .header-actions { flex-direction: column; gap: 0.4rem; font-size: 0.9rem; align-items: center; display: block; justify-content: space-between; margin-bottom: 0.5rem; }

  .btn { margin-top: 3%; margin-left: 5px; }

  .filter-bar { display: block; }
  .search-wrap { margin-bottom: 0.5rem; }

  .kpi-grid { grid-template-columns: 1fr; }
}
</style>