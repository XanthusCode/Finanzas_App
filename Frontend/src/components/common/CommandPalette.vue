<template>
  <Teleport to="body">
    <Transition name="palette">
      <div v-if="open" class="palette-backdrop" @mousedown.self="cerrar">
        <div class="palette" role="dialog" aria-modal="true" aria-label="Búsqueda global">
          <div class="palette-search">
            <Search :size="17" :stroke-width="2" class="palette-search-icon" />
            <input
              ref="inputRef"
              v-model="query"
              class="palette-input"
              placeholder="Busca un movimiento (Netflix, $50.000) o una página…"
              role="combobox"
              aria-autocomplete="list"
              aria-controls="palette-list"
              :aria-activedescendant="items[activo] ? `palette-item-${activo}` : undefined"
              @keydown.down.prevent="mover(1)"
              @keydown.up.prevent="mover(-1)"
              @keydown.enter.prevent="seleccionar(items[activo])"
              @keydown.esc.prevent="cerrar"
            />
            <span v-if="buscando" class="palette-spinner" aria-hidden="true" />
            <kbd class="palette-kbd">Esc</kbd>
          </div>

          <div id="palette-list" ref="listRef" class="palette-body" role="listbox">
            <!-- Páginas -->
            <template v-if="paginasFiltradas.length > 0">
              <p class="palette-group">{{ query.trim() ? 'Páginas' : 'Ir a' }}</p>
              <button
                v-for="(p, i) in paginasFiltradas"
                :id="`palette-item-${i}`"
                :key="p.route"
                class="palette-item"
                :class="{ 'is-active': activo === i }"
                role="option"
                :aria-selected="activo === i"
                @mousemove="activo = i"
                @click="seleccionar(items[i])"
              >
                <span class="item-icon"><component :is="p.icon" :size="16" :stroke-width="1.9" /></span>
                <span class="item-main">
                  <span class="item-title">{{ p.title }}</span>
                  <span class="item-sub">{{ p.group }}</span>
                </span>
                <CornerDownLeft v-if="activo === i" :size="14" :stroke-width="2" class="item-enter" />
              </button>
            </template>

            <!-- Movimientos -->
            <template v-if="resultados.length > 0">
              <p class="palette-group">Movimientos</p>
              <button
                v-for="(r, j) in resultados"
                :id="`palette-item-${paginasFiltradas.length + j}`"
                :key="r.tipo + r.id"
                class="palette-item"
                :class="{ 'is-active': activo === paginasFiltradas.length + j }"
                role="option"
                :aria-selected="activo === paginasFiltradas.length + j"
                @mousemove="activo = paginasFiltradas.length + j"
                @click="seleccionar(items[paginasFiltradas.length + j])"
              >
                <span class="item-icon" :class="r.tipo === 'GASTO' ? 'item-icon--gasto' : 'item-icon--ingreso'">
                  <ArrowDownRight v-if="r.tipo === 'GASTO'" :size="16" :stroke-width="2" />
                  <ArrowUpRight v-else :size="16" :stroke-width="2" />
                </span>
                <span class="item-main">
                  <span class="item-title">{{ r.titulo }}<template v-if="r.detalle"> · <span class="item-detalle">{{ r.detalle }}</span></template></span>
                  <span class="item-sub">{{ r.tipo === 'GASTO' ? (r.tipoGasto === 'FIJO' ? 'Gasto fijo' : 'Gasto ocasional') : 'Ingreso' }} · {{ fechaLabel(r) }}</span>
                </span>
                <span class="item-monto" :class="r.tipo === 'INGRESO' ? 'is-pos' : ''">
                  {{ r.tipo === 'GASTO' ? '−' : '+' }}{{ fmtCOP(r.monto) }}
                </span>
              </button>
            </template>

            <!-- Sin resultados -->
            <div v-if="query.trim().length >= 2 && !buscando && items.length === 0" class="palette-empty">
              <SearchX :size="22" :stroke-width="1.8" />
              <p>No encontramos nada para “{{ query.trim() }}”</p>
              <span>Prueba con el nombre de la categoría, el detalle o el monto exacto.</span>
            </div>
            <p v-else-if="error" class="palette-error">No se pudo buscar. Revisa tu conexión e intenta de nuevo.</p>
          </div>

          <footer class="palette-footer">
            <span><kbd>↑</kbd><kbd>↓</kbd> navegar</span>
            <span><kbd>↵</kbd> abrir</span>
            <span class="palette-footer-hint">Busca en todos tus meses</span>
          </footer>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick, type Component } from 'vue'
import { useRouter } from 'vue-router'
import { Search, SearchX, ArrowDownRight, ArrowUpRight, CornerDownLeft, UserRound } from 'lucide-vue-next'
import { menu } from '@/layouts/FullLayout/Sidebar/SidebarItems'
import { busquedaService } from '@/services/api'
import { useFinanceStore } from '@/stores/finance'
import { fmtCOP } from '@/utils'
import type { ResultadoBusqueda } from '@/types'

const open = defineModel<boolean>({ required: true })

const router = useRouter()
const store  = useFinanceStore()

const inputRef   = ref<HTMLInputElement>()
const listRef    = ref<HTMLElement>()
const query      = ref('')
const activo     = ref(0)
const buscando   = ref(false)
const error      = ref(false)
const resultados = ref<ResultadoBusqueda[]>([])

const MESES = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic']

// ── Páginas (búsqueda local, sin acentos) ──
type Pagina = { title: string; route: string; group: string; icon: Component }
const PAGINAS: Pagina[] = [
  ...menu.flatMap(s => s.items.map(i => ({ title: i.title, route: i.route, group: s.label, icon: i.icon }))),
  { title: 'Perfil', route: '/perfil', group: 'Cuenta', icon: UserRound },
]
const normalizar = (s: string) => s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase()

const paginasFiltradas = computed(() => {
  const q = normalizar(query.value.trim())
  if (!q) return PAGINAS.slice(0, 6)
  return PAGINAS.filter(p => normalizar(p.title).includes(q) || normalizar(p.group).includes(q)).slice(0, 4)
})

type Item = { kind: 'pagina'; pagina: Pagina } | { kind: 'mov'; mov: ResultadoBusqueda }
const items = computed<Item[]>(() => [
  ...paginasFiltradas.value.map(p => ({ kind: 'pagina' as const, pagina: p })),
  ...resultados.value.map(r => ({ kind: 'mov' as const, mov: r })),
])

// ── Movimientos (backend, con debounce y descarte de respuestas viejas) ──
let timer: ReturnType<typeof setTimeout> | undefined
let ultimaPeticion = 0

watch(query, q => {
  activo.value = 0
  error.value = false
  clearTimeout(timer)
  const texto = q.trim()
  if (texto.length < 2) {
    resultados.value = []
    buscando.value = false
    return
  }
  buscando.value = true
  timer = setTimeout(() => buscar(texto), 250)
})

async function buscar(texto: string) {
  const id = ++ultimaPeticion
  try {
    const { data } = await busquedaService.buscar(texto)
    if (id === ultimaPeticion) resultados.value = data
  } catch {
    if (id === ultimaPeticion) { resultados.value = []; error.value = true }
  } finally {
    if (id === ultimaPeticion) buscando.value = false
  }
}

// ── Teclado ──
function mover(delta: number) {
  const n = items.value.length
  if (n === 0) return
  activo.value = (activo.value + delta + n) % n
  nextTick(() => {
    listRef.value?.querySelector(`#palette-item-${activo.value}`)?.scrollIntoView({ block: 'nearest' })
  })
}

function seleccionar(item?: Item) {
  if (!item) return
  if (item.kind === 'pagina') {
    router.push(item.pagina.route)
  } else {
    const r = item.mov
    if (r.mes !== store.mesActual || r.anio !== store.anioActual) store.cambiarMes(r.mes, r.anio)
    router.push({ path: r.tipo === 'GASTO' ? '/gastos' : '/ingresos', query: { resaltar: r.id } })
  }
  cerrar()
}

function cerrar() { open.value = false }

function fechaLabel(r: ResultadoBusqueda) {
  if (r.fecha) {
    const [y, m, d] = r.fecha.slice(0, 10).split('-')
    return `${Number(d)} ${MESES[Number(m) - 1]} ${y}`
  }
  return `${MESES[r.mes - 1]} ${r.anio}`
}

// Al abrir: limpiar y enfocar
watch(open, abierto => {
  if (!abierto) return
  query.value = ''
  resultados.value = []
  activo.value = 0
  nextTick(() => inputRef.value?.focus())
})
</script>

<style scoped>
.palette-backdrop {
  position: fixed; inset: 0; z-index: 600;
  display: flex; justify-content: center; align-items: flex-start;
  padding: 12vh 1rem 1rem;
  background: rgba(5, 7, 10, 0.6);
  backdrop-filter: blur(6px);
  -webkit-backdrop-filter: blur(6px);
}
.palette {
  width: 100%; max-width: 620px;
  display: flex; flex-direction: column;
  max-height: 70vh;
  background: var(--surface);
  border: 1px solid var(--border-hover);
  border-radius: 16px;
  box-shadow: var(--shadow-pop);
  overflow: hidden;
}

/* Campo de búsqueda */
.palette-search {
  display: flex; align-items: center; gap: 0.7rem;
  padding: 0 1rem;
  border-bottom: 1px solid var(--border);
}
.palette-search-icon { color: var(--text-muted); flex-shrink: 0; }
.palette-input {
  flex: 1; min-width: 0; height: 56px;
  background: none; border: none; outline: none;
  font: inherit; font-size: 0.92rem; color: var(--text-primary);
}
.palette-input::placeholder { color: var(--text-muted); }
.palette-spinner {
  width: 15px; height: 15px; border-radius: 50%; flex-shrink: 0;
  border: 2px solid var(--border-hover); border-top-color: var(--accent-strong);
  animation: spin 0.7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

kbd, .palette-kbd {
  display: inline-grid; place-items: center;
  min-width: 22px; height: 20px; padding: 0 0.35rem;
  font-family: var(--ff-mono); font-size: 0.62rem; color: var(--text-secondary);
  background: var(--surface2); border: 1px solid var(--border-hover); border-radius: 5px;
}

/* Resultados */
.palette-body { flex: 1; overflow-y: auto; padding: 0.4rem; }
.palette-group {
  padding: 0.65rem 0.65rem 0.35rem;
  font-size: 0.62rem; font-weight: 500; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--text-muted);
}
.palette-item {
  display: flex; align-items: center; gap: 0.75rem; width: 100%;
  padding: 0.55rem 0.65rem; border: none; border-radius: 10px;
  background: none; font: inherit; text-align: left; cursor: pointer;
  color: var(--text-primary);
}
.palette-item.is-active { background: var(--surface2); }
.item-icon {
  display: grid; place-items: center; flex-shrink: 0;
  width: 32px; height: 32px; border-radius: 9px;
  color: var(--text-secondary); background: var(--surface2);
}
.palette-item.is-active .item-icon { background: var(--surface); }
.item-icon--gasto   { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 13%, transparent) !important; }
.item-icon--ingreso { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 13%, transparent) !important; }
.item-main  { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 0.1rem; }
.item-title { font-size: 0.82rem; font-weight: 500; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.item-detalle { color: var(--text-subtle); font-weight: 400; }
.item-sub   { font-size: 0.68rem; color: var(--text-muted); }
.item-monto {
  flex-shrink: 0;
  font-family: var(--ff-mono); font-variant-numeric: tabular-nums;
  font-size: 0.82rem; font-weight: 700; color: var(--text-primary);
}
.item-monto.is-pos { color: var(--c-pos); }
.item-enter { color: var(--text-muted); flex-shrink: 0; }

.palette-empty {
  display: flex; flex-direction: column; align-items: center; gap: 0.35rem;
  padding: 2rem 1rem; text-align: center; color: var(--text-muted);
}
.palette-empty p { font-size: 0.84rem; color: var(--text-primary); }
.palette-empty span { font-size: 0.72rem; }
.palette-error { padding: 1.25rem; text-align: center; font-size: 0.76rem; color: var(--c-neg); }

.palette-footer {
  display: flex; align-items: center; gap: 1rem;
  padding: 0.6rem 1rem;
  border-top: 1px solid var(--border);
  font-size: 0.66rem; color: var(--text-muted);
}
.palette-footer span { display: inline-flex; align-items: center; gap: 0.25rem; }
.palette-footer-hint { margin-left: auto; }

/* Animación */
.palette-enter-active { transition: opacity 0.15s ease; }
.palette-leave-active { transition: opacity 0.1s ease; }
.palette-enter-from, .palette-leave-to { opacity: 0; }
.palette-enter-active .palette { transition: transform 0.18s var(--ease-out); }
.palette-enter-from .palette { transform: translateY(-8px) scale(0.98); }

@media (max-width: 640px) {
  .palette-backdrop { padding-top: 1rem; }
  .palette { max-height: calc(100dvh - 2rem); }
  .palette-footer { display: none; }
}
</style>
