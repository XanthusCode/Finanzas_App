<template>
  <div class="fade-up">
    <div class="page-header">
      <div class="header-left">
        <div>
          <div class="section-label">Planificación</div>
          <h1 class="page-title">Presupuesto</h1>
        </div>
      </div>
      <MonthSelector class="month-selector" :mes="mes" :anio="anio" @change="onCambioMes" />
    </div>

    <div v-if="hayPeligro" class="banner-warn">
      <div class="banner-icon-box">
        <TriangleAlert :size="16" :stroke-width="2" />
      </div>
      <div class="banner-text">
        <p class="banner-title">
          {{ sobrepasadas.length === 1
            ? 'Tienes 1 categoría que superó su presupuesto este mes'
            : `Tienes ${sobrepasadas.length} categorías que superaron su presupuesto este mes` }}
        </p>
        <p class="banner-sub">Revisa tus gastos y ajusta tus límites para mantener el control.</p>
      </div>
      <button class="banner-btn" @click="verSobrepasadas">
        {{ filtro === 'sobrepasadas' ? 'Ver todas' : 'Ver detalles' }}
        <ArrowRight :size="14" :stroke-width="2" />
      </button>
    </div>

    <div v-if="loadingInit || composableLoading" class="loading-list">
      <div v-for="i in 4" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <div class="seccion">
        <div class="seccion-header">
          <div class="seccion-left">
            <h2 class="seccion-titulo">Categorías</h2>
            <span class="seccion-resumen">{{ itemsConLimite.length }} de {{ items.length }} con límite</span>
          </div>

          <div v-if="items.length > 0" class="toolbar">
            <div class="search-wrap">
              <Search :size="14" :stroke-width="2" class="search-icon" />
              <input v-model="busqueda" class="input search-input" placeholder="Buscar categoría..." />
              <button v-if="busqueda" class="search-clear" title="Limpiar" @click="busqueda = ''">
                <X :size="12" :stroke-width="2.2" />
              </button>
            </div>

            <!-- Filtro -->
            <div class="dropdown">
              <button
                class="tool-btn tool-btn--icon"
                :class="{ 'tool-btn--active': filtro !== 'todas' }"
                title="Filtrar"
                @click="toggleMenu('filtro')"
                @blur="cerrarMenu"
              >
                <ListFilter :size="15" :stroke-width="2" />
                <span v-if="filtro !== 'todas'" class="tool-dot" />
              </button>
              <Transition name="menu">
                <div v-if="menuAbierto === 'filtro'" class="menu">
                  <button
                    v-for="op in FILTROS"
                    :key="op.value"
                    class="menu-item"
                    :class="{ 'menu-item--active': filtro === op.value }"
                    @mousedown.prevent
                    @click="filtro = op.value; menuAbierto = ''"
                  >
                    {{ op.label }}
                    <Check v-if="filtro === op.value" :size="13" :stroke-width="2.2" />
                  </button>
                </div>
              </Transition>
            </div>

            <!-- Orden -->
            <div class="dropdown">
              <button class="tool-btn" @click="toggleMenu('orden')" @blur="cerrarMenu">
                <ArrowDownWideNarrow :size="15" :stroke-width="2" />
                <span class="tool-label">Ordenar: {{ ordenLabel }}</span>
                <ChevronDown :size="14" :stroke-width="2" class="tool-chevron" :class="{ 'tool-chevron--open': menuAbierto === 'orden' }" />
              </button>
              <Transition name="menu">
                <div v-if="menuAbierto === 'orden'" class="menu">
                  <button
                    v-for="op in ORDENES"
                    :key="op.value"
                    class="menu-item"
                    :class="{ 'menu-item--active': orden === op.value }"
                    @mousedown.prevent
                    @click="orden = op.value; menuAbierto = ''"
                  >
                    {{ op.label }}
                    <Check v-if="orden === op.value" :size="13" :stroke-width="2.2" />
                  </button>
                </div>
              </Transition>
            </div>
          </div>
        </div>

        <div v-if="items.length === 0" class="empty">
          Sin categorías —
          <RouterLink to="/categorias" class="link">créalas aquí</RouterLink>
        </div>

        <div v-else-if="presupuestosFiltrados.length === 0" class="empty">
          Ninguna categoría coincide con la búsqueda o el filtro.
          <button class="link-btn" @click="limpiarFiltros">Limpiar filtros</button>
        </div>

        <BudgetCategoryCard
          v-else
          v-model:edit-limite="editLimite"
          :items="presupuestosFiltrados"
          :editing-cat="editingCat"
          :saving="saving"
          @edit="startEdit"
          @guardar="guardar"
          @cancelar="editingCat = ''"
          @delete="eliminar"
        />
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { TriangleAlert, ArrowRight, Search, X, ListFilter, ArrowDownWideNarrow, ChevronDown, Check } from 'lucide-vue-next'
import { usePresupuestoMensual } from '@/composables/usePresupuestoMensual'
import BudgetCategoryCard from '@/components/presupuesto/BudgetCategoryCard.vue'
import MonthSelector from '@/components/common/MonthSelector.vue'

type Filtro = 'todas' | 'con-limite' | 'sin-limite' | 'sobrepasadas'
type Orden  = 'nombre' | 'uso' | 'gastado'

const FILTROS: { value: Filtro; label: string }[] = [
  { value: 'todas',        label: 'Todas' },
  { value: 'con-limite',   label: 'Con límite' },
  { value: 'sin-limite',   label: 'Sin límite' },
  { value: 'sobrepasadas', label: 'Sobrepasadas' },
]

const ORDENES: { value: Orden; label: string }[] = [
  { value: 'nombre',  label: 'Nombre' },
  { value: 'uso',     label: '% usado' },
  { value: 'gastado', label: 'Gastado' },
]

const store = useFinanceStore()
const toast = useToast()

const loadingInit = ref(false)
const saving      = ref(false)
const editingCat  = ref('')
const editLimite  = ref(0)
const busqueda    = ref('')
const filtro      = ref<Filtro>('todas')
const orden       = ref<Orden>('nombre')
const menuAbierto = ref<'' | 'filtro' | 'orden'>('')
const mes  = ref(new Date().getMonth() + 1)
const anio = ref(new Date().getFullYear())
const { items, loading: composableLoading, cargar } = usePresupuestoMensual(mes, anio)

const sobrepasadas   = computed(() => items.value.filter(i => i.estado === 'danger'))
const hayPeligro     = computed(() => sobrepasadas.value.length > 0)
const itemsConLimite = computed(() => items.value.filter(i => i.limite))
const ordenLabel     = computed(() => ORDENES.find(o => o.value === orden.value)!.label)

const presupuestosFiltrados = computed(() => {
  const q = busqueda.value.trim().toLowerCase()
  const lista = items.value.filter(i => {
    if (q && !i.categoria.toLowerCase().includes(q)) return false
    switch (filtro.value) {
      case 'con-limite':   return !!i.limite
      case 'sin-limite':   return !i.limite
      case 'sobrepasadas': return i.estado === 'danger'
      default:             return true
    }
  })

  return [...lista].sort((a, b) => {
    if (orden.value === 'uso')     return b.pct - a.pct
    if (orden.value === 'gastado') return b.gastado - a.gastado
    return a.categoria.localeCompare(b.categoria, 'es')
  })
})

function toggleMenu(menu: 'filtro' | 'orden') {
  menuAbierto.value = menuAbierto.value === menu ? '' : menu
}

function cerrarMenu() {
  menuAbierto.value = ''
}

function verSobrepasadas() {
  filtro.value = filtro.value === 'sobrepasadas' ? 'todas' : 'sobrepasadas'
  busqueda.value = ''
}

function limpiarFiltros() {
  busqueda.value = ''
  filtro.value = 'todas'
}

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
.header-left { display: flex; align-items: flex-end; gap: 1.25rem; flex-wrap: wrap; }
.empty       { font-size: 0.78rem; color: var(--text-muted); padding: 1.5rem 0; text-align: center; }
.link        { color: var(--accent-strong); text-decoration: none; }
.link:hover  { text-decoration: underline; }
.link-btn    { background: none; border: none; padding: 0; margin-left: 0.25rem; color: var(--accent-strong); font: inherit; cursor: pointer; }
.link-btn:hover { text-decoration: underline; }

/* ── Banner de alerta: tinte transparente, sin borde ── */
.banner-warn {
  --w: var(--c-warn);
  display: flex; align-items: center; gap: 0.9rem;
  padding: 0.85rem 0.9rem 0.85rem 1rem;
  margin-bottom: 1.75rem;
  border-radius: 12px;
  background: color-mix(in srgb, var(--w) 10%, transparent);
}
.banner-icon-box {
  width: 34px; height: 34px; border-radius: 9px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  color: var(--w);
  background: color-mix(in srgb, var(--w) 16%, transparent);
}
.banner-text  { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 0.2rem; }
.banner-title { margin: 0; font-size: 0.84rem; font-weight: 500; color: var(--w); }
.banner-sub   { margin: 0; font-size: 0.72rem; color: var(--text-secondary); }
.banner-btn {
  display: inline-flex; align-items: center; gap: 0.4rem; flex-shrink: 0;
  padding: 0.45rem 0.7rem; border-radius: 8px;
  font: inherit; font-size: 0.76rem; font-weight: 500; white-space: nowrap;
  color: var(--w); cursor: pointer;
  background: none; border: none;
  transition: background 0.15s;
}
.banner-btn:hover  { background: color-mix(in srgb, var(--w) 12%, transparent); }
.banner-btn svg    { transition: transform 0.15s; }
.banner-btn:hover svg { transform: translateX(3px); }
.banner-btn:focus-visible { outline: 2px solid var(--w); outline-offset: 2px; }

/* ── Encabezado de sección ── */
.seccion { margin-bottom: 1.25rem; }
.seccion-header {
  display: flex; align-items: center; justify-content: space-between; gap: 1rem;
  margin-bottom: 1rem; flex-wrap: wrap;
}
.seccion-left    { display: flex; align-items: center; gap: 0.6rem; }
.seccion-titulo  { margin: 0; font-family: var(--ff-body); font-size: 0.78rem; font-weight: 500; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text-primary); }
.seccion-resumen {
  font-size: 0.68rem; font-weight: 500; color: var(--text-secondary);
  padding: 0.2rem 0.6rem; border-radius: 999px;
  background: var(--surface2); border: 1px solid var(--border);
  font-variant-numeric: tabular-nums;
}

/* ── Toolbar ── */
.toolbar { display: flex; align-items: center; gap: 0.5rem; }

.search-wrap { position: relative; width: 280px; }
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

.tool-btn {
  position: relative;
  height: 38px; padding: 0 0.8rem; border-radius: 10px;
  display: inline-flex; align-items: center; gap: 0.5rem;
  font-size: 0.78rem; color: var(--text-secondary); white-space: nowrap; cursor: pointer;
  background: var(--surface); border: 1px solid var(--border-hover);
  transition: color 0.15s, border-color 0.15s, background 0.15s;
}
.tool-btn:hover { color: var(--text-primary); border-color: color-mix(in srgb, var(--accent) 35%, var(--border)); }
.tool-btn--icon { width: 38px; padding: 0; justify-content: center; }
.tool-btn--active { color: var(--accent-strong); border-color: color-mix(in srgb, var(--accent) 55%, transparent); }
.tool-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 2px; }
.tool-dot {
  position: absolute; top: 6px; right: 6px;
  width: 6px; height: 6px; border-radius: 50%; background: var(--accent-strong);
}
.tool-chevron { transition: transform 0.15s; color: var(--text-muted); }
.tool-chevron--open { transform: rotate(180deg); }

/* ── Menús desplegables ── */
.dropdown { position: relative; }
.menu {
  position: absolute; right: 0; top: calc(100% + 6px); z-index: 30;
  min-width: 170px; padding: 0.3rem;
  background: var(--surface); border: 1px solid var(--border-hover); border-radius: 12px;
  box-shadow: var(--shadow-pop);
  transform-origin: top right;
}
.menu-item {
  width: 100%; display: flex; align-items: center; justify-content: space-between; gap: 0.5rem;
  padding: 0.5rem 0.65rem; border-radius: 7px;
  font-size: 0.78rem; text-align: left; color: var(--text-primary);
  background: none; border: none; cursor: pointer;
  transition: background 0.12s;
}
.menu-item:hover { background: var(--surface2); }
.menu-item--active { color: var(--accent-strong); font-weight: 500; }

.menu-enter-active { transition: opacity 0.14s, transform 0.14s cubic-bezier(0.2, 0, 0, 1); }
.menu-leave-active { transition: opacity 0.1s, transform 0.1s ease-in; }
.menu-enter-from,
.menu-leave-to { opacity: 0; transform: scale(0.95) translateY(-4px); }

/* ── Loading ── */
.loading-list { display: flex; flex-direction: column; gap: 0.6rem; }
.skeleton-row {
  height: 70px; border-radius: 12px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 900px) {
  .toolbar { width: 100%; }
  .search-wrap { flex: 1; width: auto; }
}

@media (max-width: 640px) {
  .banner-warn { flex-wrap: wrap; }
  .banner-btn  { margin-left: calc(34px + 0.9rem); }
  .tool-label  { display: none; }
  .tool-btn:not(.tool-btn--icon) { width: 38px; padding: 0; justify-content: center; }
  .tool-chevron { display: none; }
}

@media (max-width: 500px) {
  .header-left { width: 100%; }
  .month-selector { width: 100%; justify-content: space-between; }
  .month-selector :deep(.label) { flex: 1; }
}
</style>
