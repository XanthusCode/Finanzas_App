<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Configuración</div>
        <h1 class="page-title">Categorías</h1>
        <p class="page-sub">Organiza tus gastos a tu manera</p>
      </div>

      <button v-if="!showForm" class="btn btn-primary btn-new-cat" @click="abrirForm()">
        <Plus :size="15" :stroke-width="2.4" />
        Nueva categoría
      </button>
    </div>

    <section class="cat-grid">
      <div v-for="kpi in kpis" :key="kpi.label" class="kpi-card" :style="{ '--kc': kpi.color }">
        <div class="kpi-left">
          <div class="kpi-icon"><component :is="kpi.icon" :size="17" :stroke-width="1.9" /></div>
          <div class="kpi-label">{{ kpi.label }}</div>
        </div>
        <div class="kpi-value">{{ kpi.value ?? '—' }}</div>
      </div>
    </section>

    <!-- Barra de búsqueda -->
    <div v-if="!loadingCats && store.categoriasActivas.length > 0" class="filter-bar">
      <div class="search-wrap">
        <Search :size="15" :stroke-width="2" class="search-icon" />
        <input v-model="busqueda" class="input search-input" placeholder="Buscar categoría..." />
        <button v-if="busqueda" class="search-clear" title="Limpiar" aria-label="Limpiar búsqueda" @click="busqueda = ''">
          <X :size="12" :stroke-width="2.2" />
        </button>
      </div>
      <span class="filter-count">{{ categoriasFiltradas.length }} de {{ store.categoriasActivas.length }}</span>
    </div>

    <div v-if="loadingCats" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <CategoryCard
      v-else
      v-model:nombre="form.nombre"
      :categorias="categoriasFiltradas"
      :show-form="showForm"
      :saving="saving"
      :editando="editando"
      @abrir-form="abrirForm"
      @guardar="guardar"
      @cancelar="cancelar"
      @delete="onDelete"
      @ver="verMovimientos"
    />

    <ConfirmModal
      v-model="showConfirm"
      title="Eliminar categoría"
      message="Si eliminas esta categoría, los gastos que la usan quedarán sin categoría. ¿Continuar?"
      @confirm="confirmDelete"
    />

    <!-- Movimientos de la categoría -->
    <Teleport to="body">
      <div v-if="movCategoria" class="modal-backdrop" @click.self="cerrarMovimientos">
        <div class="modal fade-up">
          <div class="modal-head">
            <p class="modal-title">{{ movCategoria.nombre }}</p>
            <button class="icon-close" aria-label="Cerrar" @click="cerrarMovimientos"><X :size="16" :stroke-width="2" /></button>
          </div>

          <div v-if="loadingMov" class="loading-list">
            <div v-for="i in 3" :key="i" class="skeleton-row" />
          </div>

          <template v-else-if="movimientos">
            <div class="mov-total">
              <span>{{ movimientos.cantidad }} {{ movimientos.cantidad === 1 ? 'gasto' : 'gastos' }}</span>
              <strong>{{ fmtCOP(movimientos.total) }} {{ movimientos.moneda }}</strong>
            </div>

            <div v-if="movimientos.gastos.length === 0" class="empty">Esta categoría aún no tiene gastos</div>
            <ul v-else class="mov-list">
              <li v-for="g in movimientos.gastos" :key="g.id" class="mov-row">
                <div class="mov-info">
                  <span class="mov-detalle">{{ g.detalle || 'Sin detalle' }}</span>
                  <span class="mov-fecha">{{ fechaGasto(g) }} · {{ g.tipo === 'FIJO' ? 'Fijo' : 'Variable' }}</span>
                </div>
                <span class="mov-monto">{{ fmtCOP(g.monto) }}</span>
              </li>
            </ul>
          </template>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { categoriasService } from '@/services/api'
import { fmtCOP } from '@/utils'
import type { Categoria, CategoriaGastos, Gasto } from '@/types'
import { LayoutGrid, SquareActivity, CircleDashed, Plus, Search, X } from 'lucide-vue-next'
import CategoryCard from '@/components/Category/CategoryCard.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const store = useFinanceStore()
const toast = useToast()

const showForm    = ref(false)
const saving      = ref(false)
const loadingCats = ref(false)
const editando    = ref<Categoria | null>(null)
const showConfirm = ref(false)
const pendingId   = ref<string | null>(null)
const busqueda    = ref('')

const totalCategorias = ref<number | null>(null)
const conGastos       = ref<number | null>(null)

const movCategoria = ref<Categoria | null>(null)
const movimientos  = ref<CategoriaGastos | null>(null)
const loadingMov   = ref(false)

const form = reactive({ nombre: '' })

const categoriasFiltradas = computed(() => {
  const q = busqueda.value.trim().toLowerCase()
  if (!q) return store.categoriasActivas
  return store.categoriasActivas.filter(c => c.nombre.toLowerCase().includes(q))
})

const kpis = computed(() => [
  { label: 'Categorías', value: totalCategorias.value, icon: LayoutGrid,     color: 'var(--accent-strong)' },
  { label: 'Con gastos', value: conGastos.value,       icon: SquareActivity, color: 'var(--c-pos)' },
  {
    label: 'Sin gastos',
    value: totalCategorias.value != null && conGastos.value != null ? totalCategorias.value - conGastos.value : null,
    icon: CircleDashed,
    color: 'var(--text-secondary)',
  },
])

async function cargarConteos() {
  try {
    const [total, usadas] = await Promise.all([
      categoriasService.getTotal(),
      categoriasService.getTotalConGastos(),
    ])
    totalCategorias.value = total.data.total
    conGastos.value       = usadas.data.total
  } catch {
    // Los KPIs son informativos; si fallan se muestran como "—"
  }
}

function abrirForm(cat?: Categoria) {
  editando.value = cat ?? null
  form.nombre    = cat?.nombre ?? ''
  showForm.value = true
}

function cancelar() {
  showForm.value = false
  editando.value = null
  form.nombre    = ''
}

async function guardar() {
  if (!form.nombre.trim()) return
  saving.value = true
  const payload = { nombre: form.nombre.trim(), activa: true }
  try {
    if (editando.value?.id) {
      await store.actualizarCategoria(editando.value.id, payload)
      toast.success('Categoría actualizada')
    } else {
      await store.agregarCategoria(payload)
      toast.success('Categoría creada')
    }
    cancelar()
    cargarConteos()
  } catch {
    toast.error('No se pudo guardar la categoría')
  } finally {
    saving.value = false
  }
}

function onDelete(id: string) {
  pendingId.value   = id
  showConfirm.value = true
}

async function confirmDelete() {
  if (pendingId.value != null) {
    try {
      await store.eliminarCategoria(pendingId.value)
      toast.success('Categoría eliminada')
      cargarConteos()
    } catch (error: any) {
      const msg = error?.response?.status === 409
        ? error.response.data
        : 'No se pudo eliminar la categoría'
      toast.error(msg)
    } finally {
      pendingId.value = null
    }
  }
}

async function verMovimientos(cat: Categoria) {
  movCategoria.value = cat
  movimientos.value  = null
  loadingMov.value   = true
  try {
    const { data } = await categoriasService.getGastos(cat.id!)
    movimientos.value = data
  } catch {
    toast.error('No se pudieron cargar los movimientos')
    movCategoria.value = null
  } finally {
    loadingMov.value = false
  }
}

function cerrarMovimientos() {
  movCategoria.value = null
  movimientos.value  = null
}

const MESES = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic']

// Las cuotas 2..N y copias recurrentes no tienen fecha: se muestra el mes/año
function fechaGasto(g: Gasto) {
  if (g.fecha) {
    const [y, m, d] = g.fecha.slice(0, 10).split('-')
    return `${Number(d)} ${MESES[Number(m) - 1]} ${y}`
  }
  return `${MESES[g.mes - 1]} ${g.anio}`
}

onMounted(async () => {
  loadingCats.value = true
  try {
    await Promise.all([store.cargarCategorias(), cargarConteos()])
  } finally {
    loadingCats.value = false
  }
})
</script>

<style scoped>
.page-sub { font-size: 0.82rem; color: var(--text-subtle); margin-top: 0.4rem; }

.cat-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

/* KPIs: ícono + etiqueta a la izquierda, número a la derecha */
.kpi-card {
  display: flex; align-items: center; justify-content: space-between; gap: 0.75rem;
  padding: 0.9rem 1.1rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card); box-shadow: var(--shadow-card);
}
.kpi-left { display: flex; align-items: center; gap: 0.75rem; min-width: 0; }
.kpi-icon {
  width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  color: var(--kc); background: color-mix(in srgb, var(--kc) 13%, transparent);
}
.kpi-label { font-size: 0.68rem; font-weight: 500; color: var(--text-subtle); letter-spacing: 0.08em; text-transform: uppercase; }
.kpi-value { font-family: var(--ff-mono); font-variant-numeric: tabular-nums; font-size: 1.45rem; font-weight: 700; color: var(--text-primary); }

/* Modal de movimientos */
.modal-backdrop {
  position: fixed; inset: 0; background: rgba(0,0,0,0.5);
  display: flex; align-items: flex-start; justify-content: center; z-index: 100; padding: 3rem 1rem;
}
.modal {
  background: var(--surface); border: 1px solid var(--border-hover); border-radius: var(--radius-modal);
  padding: 1.5rem; width: 100%; max-width: 460px; display: flex; flex-direction: column; gap: 1rem;
  max-height: calc(100vh - 4rem); overflow-y: auto;
  box-shadow: var(--shadow-pop);
}
.icon-close {
  display: grid; place-items: center; width: 32px; height: 32px; padding: 0;
  background: none; border: none; border-radius: 8px; color: var(--text-secondary); cursor: pointer;
}
.icon-close:hover { color: var(--text-primary); background: var(--surface2); }
.modal-head  { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
.modal-title { font-size: 1rem; font-weight: 700; margin: 0; }
.mov-total {
  display: flex; justify-content: space-between; align-items: baseline;
  font-size: 0.78rem; color: var(--text-secondary);
  padding-bottom: 0.6rem; border-bottom: 1px solid var(--border);
}
.mov-total strong { font-family: var(--ff-mono); font-size: 1.05rem; color: var(--text-primary); }
.mov-list    { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; }
.mov-row     { display: flex; justify-content: space-between; align-items: center; gap: 0.75rem; padding: 0.55rem 0; border-bottom: 1px solid var(--border); }
.mov-row:last-child { border-bottom: none; }
.mov-info    { display: flex; flex-direction: column; min-width: 0; }
.mov-detalle { font-size: 0.85rem; font-weight: 600; color: var(--text-primary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.mov-fecha   { font-size: 0.68rem; color: var(--text-subtle); }
.mov-monto   { font-family: var(--ff-mono); font-variant-numeric: tabular-nums; font-size: 0.85rem; font-weight: 700; color: var(--text-primary); flex-shrink: 0; }
.empty       { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }

/* Filter bar */
.filter-bar {
  display: flex;
  gap: 0.6rem;
  align-items: center;
  margin-bottom: 1.25rem;
}
.search-wrap {  position: relative;  flex: 1 1 auto; width: auto; max-width: none; }
.search-icon  { position: absolute; left: 0.75rem; top: 50%; transform: translateY(-50%); }
.search-input { width: 100%; height: 38px; padding-left: 2.2rem; padding-right: 2rem; border-radius: 10px; background: var(--surface); border-color: var(--border-hover); }
.search-clear {
  position: absolute; right: 0.5rem; top: 50%; transform: translateY(-50%);
  width: 20px; height: 20px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  background: var(--surface2); border: none; color: var(--text-muted); cursor: pointer;
}
.search-clear:hover { color: var(--text-primary); }
.filter-count {  font-family: var(--ff-mono);
  font-size: 0.72rem;
  color: var(--text-muted);
  white-space: nowrap;
  flex-shrink: 0; /* Para que no se aplaste */ }

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

@media (max-width: 900px) {
  .cat-grid { gap: 0.5rem; }
  .kpi-card { flex-direction: column; align-items: flex-start; }
}

@media (max-width: 640px) {
  .search-wrap { max-width: none; }
  .btn-new-cat { width: 100%; }
}
</style>
