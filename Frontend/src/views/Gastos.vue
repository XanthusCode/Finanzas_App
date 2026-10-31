<template>
  <div class="fade-up">
    <div class="page-header">
      <div class="header-left">
        <div>
          <div class="section-label">Registro</div>
          <h1 class="page-title">Gastos</h1>
        </div>
      </div>

      <div class="header-actions">
        <input ref="csvInput" type="file" accept=".csv" style="display:none" @change="onImportCSV" />
        <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
        <input ref="reciboInput" type="file" accept=".pdf,.jpg,.jpeg,.png" style="display:none" @change="onImportRecibo" />

        <!-- Acciones secundarias agrupadas -->
        <div ref="accionesRef" class="acciones">
          <button
            class="btn"
            :aria-expanded="accionesAbiertas"
            aria-haspopup="menu"
            @click="accionesAbiertas = !accionesAbiertas"
          >
            Acciones
            <ChevronDown :size="14" :stroke-width="2" class="acciones-chevron" :class="{ 'is-open': accionesAbiertas }" />
          </button>
          <Transition name="menu">
            <div v-if="accionesAbiertas" class="acciones-menu" role="menu">
              <button class="acciones-item" role="menuitem" @click="accion(() => reciboInput?.click())">
                <Camera :size="15" :stroke-width="1.9" />
                <span>Escanear recibo<small>Foto o PDF, se lee con IA</small></span>
              </button>
              <button class="acciones-item" role="menuitem" @click="accion(() => csvInput?.click())">
                <Upload :size="15" :stroke-width="1.9" />
                <span>Importar CSV<small>Varios gastos a la vez</small></span>
              </button>
              <div class="acciones-sep" />
              <button class="acciones-item" role="menuitem" :disabled="store.gastos.length === 0" @click="accion(descargarExcel)">
                <FileSpreadsheet :size="15" :stroke-width="1.9" />
                <span>Exportar a Excel<small>Gastos de este mes</small></span>
              </button>
            </div>
          </Transition>
        </div>

        <button class="btn btn-primary" @click="nuevoGastoPrincipal">
          <Plus :size="15" :stroke-width="2.4" />
          Nuevo gasto
        </button>
      </div>
    </div>

    <!-- KPIs del mes -->
    <div v-if="!store.loading && store.gastos.length > 0" class="kpi-grid">
      <div class="kpi-card" style="--ca: var(--c-need)">
        <div class="kpi-top">
          <span class="kpi-label"><span class="kpi-dot" />Gastos fijos</span>
          <span class="kpi-icon"><Lock :size="14" :stroke-width="2" /></span>
        </div>
        <div class="kpi-value">{{ fmt(totalFiltradoFijos) }}</div>
        <div class="kpi-sub">{{ filtradosFijos.length }} {{ filtradosFijos.length === 1 ? 'gasto' : 'gastos' }}</div>
      </div>
      <div class="kpi-card" style="--ca: var(--c-want)">
        <div class="kpi-top">
          <span class="kpi-label"><span class="kpi-dot" />Gastos ocasionales</span>
          <span class="kpi-icon"><Shuffle :size="14" :stroke-width="2" /></span>
        </div>
        <div class="kpi-value">{{ fmt(totalFiltradoVariables) }}</div>
        <div class="kpi-sub">{{ filtradosVariables.length }} {{ filtradosVariables.length === 1 ? 'gasto' : 'gastos' }}</div>
      </div>
      <div class="kpi-card" style="--ca: var(--accent-strong)">
        <div class="kpi-top">
          <span class="kpi-label"><span class="kpi-dot" />Total gastos</span>
          <span class="kpi-icon"><Wallet :size="14" :stroke-width="2" /></span>
        </div>
        <div class="kpi-value">{{ fmt(totalFiltradoFijos + totalFiltradoVariables) }}</div>
        <div class="kpi-sub">{{ busqueda || filtroCategoria ? 'Con los filtros aplicados' : 'Este mes' }}</div>
      </div>
    </div>

    <!-- Barra de búsqueda + filtros -->
    <div v-if="!store.loading && store.gastos.length > 0" class="filter-bar">
      <div class="search-wrap">
        <Search :size="14" :stroke-width="2" class="search-icon" />
          <input v-model="busqueda" class="input search-input" placeholder="Buscar Gasto..." />
        <button v-if="busqueda" class="search-clear" title="Limpiar" @click="busqueda = ''">
          <X :size="12" :stroke-width="2.2" />
        </button>
      </div>
      <div class="select-wrap filter-wrap">
        <ListFilter :size="15" :stroke-width="2" class="field-icon" />
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
      <div class="card list-card" style="--ca: var(--c-need); margin-bottom: 0.85rem;">
        <div class="card-header">
          <span class="card-title"><span class="kpi-dot" />Gastos fijos</span>
          <div class="card-header-right">
            <span class="total-badge">{{ fmt(totalFiltradoFijos) }}</span>
            <button v-if="!editando && tipoAbierto !== 'FIJO'" class="btn btn-sm btn-primary" @click="toggleFormPara('FIJO')">
              <Plus :size="13" :stroke-width="2.4" />
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
          <GastoRow v-for="g in filtradosFijos" :key="g.id" :gasto="g" :resaltado="resaltadoId === g.id" @edit="abrirEdicion" @delete="onDelete" />
        </template>
      </div>

      <!-- Gastos ocasionales -->
      <div ref="variablesCardRef" class="card list-card" style="--ca: var(--c-want)">
        <div class="card-header">
          <span class="card-title"><span class="kpi-dot" />Gastos ocasionales</span>
          <div class="card-header-right">
            <span class="total-badge">{{ fmt(totalFiltradoVariables) }}</span>
            <button v-if="!editando && tipoAbierto !== 'VARIABLE'" class="btn btn-sm btn-primary" @click="toggleFormPara('VARIABLE')">
              <Plus :size="13" :stroke-width="2.4" />
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
          <GastoRow v-for="g in filtradosVariables" :key="g.id" :gasto="g" :resaltado="resaltadoId === g.id" @edit="abrirEdicion" @delete="onDelete" />
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
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import {
  ChevronDown, Plus, Search, ListFilter, FileSpreadsheet, Upload, Camera, Lock, Shuffle, Wallet, X
} from '@lucide/vue'
import { useRoute, useRouter } from 'vue-router'
import { useFinanceStore } from '@/stores/finance'
import { useResaltarMovimiento } from '@/composables/useResaltarMovimiento'
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

// Menú "Acciones" (escanear / importar / exportar)
const accionesRef      = ref<HTMLElement>()
const accionesAbiertas = ref(false)

function accion(fn: () => void) {
  accionesAbiertas.value = false
  fn()
}

function onClickFuera(e: MouseEvent) {
  if (accionesRef.value && !accionesRef.value.contains(e.target as Node)) accionesAbiertas.value = false
}
function onEsc(e: KeyboardEvent) {
  if (e.key === 'Escape') accionesAbiertas.value = false
}

// Botón principal: los ocasionales son el registro más frecuente
const variablesCardRef = ref<HTMLElement>()
async function nuevoGastoPrincipal() {
  editando.value    = null
  tipoAbierto.value = 'VARIABLE'
  await nextTick()
  variablesCardRef.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

// Búsqueda global → resaltar el gasto (limpiando filtros/formularios que lo ocultarían)
const { resaltadoId } = useResaltarMovimiento(
  () => store.gastos.map(g => g.id),
  () => store.loading,
  () => { busqueda.value = ''; filtroCategoria.value = ''; cerrarForm() },
)

// Estado vacío del Dashboard → /gastos?nuevo=1 abre el formulario directamente
const route  = useRoute()
const router = useRouter()

onMounted(() => {
  store.cargarDatos(); store.cargarCategorias(); store.cargarCuentas()
  if (route.query.nuevo) {
    const { nuevo: _quitar, ...resto } = route.query
    router.replace({ query: resto })
    nuevoGastoPrincipal()
  }
  document.addEventListener('mousedown', onClickFuera)
  document.addEventListener('keydown', onEsc)
})
onBeforeUnmount(() => {
  document.removeEventListener('mousedown', onClickFuera)
  document.removeEventListener('keydown', onEsc)
})
</script>

<style scoped>
/* ── Encabezado: título + mes a la izquierda, acciones a la derecha ── */
.header-left { display: flex; align-items: flex-end; gap: 1.25rem; flex-wrap: wrap; }
.header-actions { display: flex; align-items: center; gap: 0.6rem; }

.acciones { position: relative; }
.acciones-chevron { transition: transform 0.18s ease; }
.acciones-chevron.is-open { transform: rotate(180deg); }
.acciones-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  z-index: 40;
  min-width: 240px;
  padding: 0.35rem;
  background: var(--surface);
  border: 1px solid var(--border-hover);
  border-radius: 12px;
  box-shadow: var(--shadow-pop);
}
.acciones-item {
  display: flex;
  align-items: flex-start;
  gap: 0.7rem;
  width: 100%;
  padding: 0.6rem 0.7rem;
  border: none;
  border-radius: 8px;
  background: none;
  color: var(--text-secondary);
  font: inherit;
  font-size: 0.78rem;
  text-align: left;
  cursor: pointer;
  transition: background 0.12s ease, color 0.12s ease;
}
.acciones-item svg { margin-top: 1px; flex-shrink: 0; }
.acciones-item span { display: flex; flex-direction: column; gap: 2px; color: var(--text-primary); }
.acciones-item small { font-size: 0.66rem; color: var(--text-muted); }
.acciones-item:hover:not(:disabled) { background: var(--surface2); color: var(--text-primary); }
.acciones-item:disabled { opacity: 0.45; cursor: not-allowed; }
.acciones-sep { height: 1px; background: var(--border); margin: 0.3rem 0.4rem; }

.menu-enter-active { transition: opacity 0.14s ease, transform 0.14s var(--ease-out); }
.menu-leave-active { transition: opacity 0.1s ease; }
.menu-enter-from   { opacity: 0; transform: translateY(-4px) scale(0.98); }
.menu-leave-to     { opacity: 0; }

/* ── Tarjetas de lista ── */
.card-header { display: flex; justify-content: space-between; align-items: center; gap: 0.75rem; margin-bottom: 0.6rem; }
.card-header-right { display: flex; align-items: center; gap: 0.6rem; }
.card-title  {
  display: inline-flex; align-items: center; gap: 0.5rem;
  font-size: 0.68rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase;
}
.total-badge {
  font-family: var(--ff-mono);
  font-variant-numeric: tabular-nums;
  font-size: 0.74rem;
  font-weight: 700;
  padding: 0.25rem 0.65rem;
  border-radius: 999px;
  color: var(--ca);
  background: color-mix(in srgb, var(--ca) 13%, transparent);
}
.list-card:hover { border-color: var(--border); }
.empty       { font-size: 0.74rem; color: var(--text-muted); padding: 1.25rem 0; text-align: center; }

.form-wrap { margin-bottom: 1rem; }

/* ── KPIs: borde neutro, color solo en el punto y el ícono ── */
.kpi-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.85rem; margin-bottom: 1.25rem; }
.kpi-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  padding: 1.1rem 1.25rem;
  box-shadow: var(--shadow-card);
}
.kpi-top   { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.kpi-label {
  display: inline-flex; align-items: center; gap: 0.5rem;
  font-size: 0.66rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase;
}
.kpi-dot { width: 7px; height: 7px; border-radius: 99px; background: var(--ca); flex-shrink: 0; }
.kpi-icon {
  display: grid; place-items: center;
  width: 30px; height: 30px; border-radius: 9px; flex-shrink: 0;
  color: var(--ca);
  background: color-mix(in srgb, var(--ca) 13%, transparent);
}
.kpi-value {
  font-family: var(--ff-mono);
  font-variant-numeric: tabular-nums;
  font-size: 1.45rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--text-primary);
}
.kpi-sub { font-size: 0.68rem; color: var(--text-muted); margin-top: 0.3rem; }

/* ── Búsqueda + filtro ── */
.filter-bar {
  display: flex;
  gap: 0.6rem;
  align-items: center;
  margin-bottom: 1.25rem;
}
.search-wrap { flex: 1 1 auto; width: auto; max-width: none; }
.search-icon {
  position: absolute; left: 0.75rem; top: 50%; transform: translateY(-50%);
  color: var(--text-muted); pointer-events: none;
}
.search-input { width: 100%; height: 38px; padding-left: 2.2rem; padding-right: 2rem; border-radius: 10px; background: var(--surface); border-color: var(--border-hover); }
.search-clear {
  position: absolute; right: 0.5rem; top: 50%; transform: translateY(-50%);
  width: 20px; height: 20px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  background: var(--surface2); border: none; color: var(--text-muted); cursor: pointer;
}
.search-clear:hover { color: var(--text-primary); }
.field-icon {
  position: absolute;
  left: 0.8rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-muted);
  pointer-events: none;
  z-index: 1;
}
.search-wrap:focus-within .field-icon,
.filter-wrap:focus-within .field-icon { color: var(--accent-strong); }
.search-input,
.filter-select { padding-left: 2.3rem; background: var(--surface); border-color: var(--border-hover); }
.filter-wrap { position: relative; min-width: 210px; }

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

@media (max-width: 768px) {
  .kpi-grid { grid-template-columns: 1fr 1fr; }
  .kpi-grid > :last-child { grid-column: 1 / -1; }
}

@media (max-width: 500px) {
  .header-left { width: 100%; justify-content: space-between; }
  .header-actions { width: 100%; }
  .header-actions > .btn-primary { flex: 1; }
  .acciones-menu { right: auto; left: 0; }

  .search-wrap { max-width: none; flex-basis: 100%; }
  .filter-wrap { flex: 1; min-width: 0; }

  .kpi-grid { grid-template-columns: 1fr; }
  .card-header { flex-wrap: wrap; }
}
</style>