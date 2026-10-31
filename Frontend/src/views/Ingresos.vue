<template>
  <div class="fade-up">
    <div class="page-header">
      <div class="header-left">
        <div>
          <div class="section-label">Registro</div>
          <h1 class="page-title">Ingresos</h1>
        </div>
      </div>
      <div class="header-actions">
        <button v-if="store.ingresos.length > 0" class="btn btn-ghost" title="Exportar a Excel" @click="descargarExcel">
          <FileSpreadsheet :size="15" :stroke-width="1.9" />
          Excel
        </button>
        <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
        <button class="btn btn-primary" :disabled="!!editando" @click="showForm ? cerrarForm() : abrirForm()">
          <Plus :size="15" :stroke-width="2.4" />
          Agregar ingreso
        </button>
      </div>
    </div>

    <!-- Formulario con transición -->
    <Transition name="form-slide">
      <div v-if="showForm && !editando" class="card form-card">
        <form class="ingreso-form" @submit.prevent="onSubmit">
          <div class="form-row">
            <div class="field" style="flex: 2">
              <div class="label-row">
                <label>Concepto</label>
                <span class="char-counter" :class="{ 'char-counter--limit': form.concepto.length >= 100 }">{{ form.concepto.length }}/100</span>
              </div>
              <input v-model="form.concepto" class="input" maxlength="100" :class="{ 'input-error': errors.concepto }" placeholder="ej. Salario básico" />
              <span v-if="errors.concepto" class="field-error">{{ errors.concepto }}</span>
            </div>
            <div class="field">
              <label>Monto (COP)</label>
              <CurrencyInput v-model="form.monto" :class="{ 'input-error': errors.monto }" placeholder="$ 0" />
              <span v-if="errors.monto" class="field-error">{{ errors.monto }}</span>
            </div>
          </div>
          <div v-if="store.cuentas.length > 0" class="field">
            <label>Cuenta <span class="label-optional">(opcional)</span></label>
            <select v-model="form.cuentaId" class="input">
              <option value="">Ninguna</option>
              <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
            </select>
          </div>
          <label class="check-label">
            <input type="checkbox" v-model="form.esRecurrente" />
            Repetir cada mes
          </label>
          <div v-if="form.esRecurrente" class="form-row">
            <div class="field">
              <label>Frecuencia</label>
              <select v-model="form.frecuencia" class="input">
                <option v-for="(label, val) in FRECUENCIA_LABEL" :key="val" :value="val">{{ label }}</option>
              </select>
            </div>
            <div v-if="mostrarDiaPago" class="field">
              <label>Día de pago <span class="label-optional">(opcional)</span></label>
              <input v-model.number="form.diaPago" type="number" min="1" max="31" class="input" placeholder="ej. 15" />
            </div>
          </div>
          <div class="form-actions">
            <button type="button" class="btn" @click="cerrarForm">Cancelar</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Guardando...' : 'Guardar' }}
            </button>
          </div>
        </form>
      </div>
    </Transition>

    <!-- Banner ingresos recurrentes pendientes -->
    <Transition name="banner-slide">
      <div v-if="store.promptIngresosRecurrentes && !store.loading" class="recurrentes-banner">
        <div class="banner-left">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>
          <span>Hay ingresos recurrentes del mes anterior — ¿los copiamos?</span>
        </div>
        <div class="banner-actions">
          <button class="btn btn-sm btn-primary" :disabled="copiando" @click="copiarRecurrentes">
            {{ copiando ? 'Copiando...' : 'Copiar' }}
          </button>
          <button class="banner-close" @click="store.descartarPromptIngresosRecurrentes">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>
      </div>
    </Transition>

    <div v-if="store.loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <div v-else-if="store.ingresos.length > 0" class="ingresos-layout">
      <div class="card list-card">
        <div class="card-header">
          <div>
            <h2 class="card-title">Ingresos del mes</h2>
            <p class="card-sub">{{ store.ingresos.length }} {{ store.ingresos.length === 1 ? 'fuente de ingreso' : 'fuentes de ingreso' }}</p>
          </div>
          <span class="total-badge">{{ fmt(total) }}</span>
        </div>
        <template v-for="i in store.ingresos" :key="i.id">
          <div v-if="editando?.id === i.id" class="inline-form-row">
            <form class="ingreso-form" @submit.prevent="onSubmit">
              <div class="form-row">
                <div class="field" style="flex: 2">
                  <div class="label-row">
                    <label>Concepto</label>
                    <span class="char-counter" :class="{ 'char-counter--limit': form.concepto.length >= 100 }">{{ form.concepto.length }}/100</span>
                  </div>
                  <input v-model="form.concepto" class="input" maxlength="100" :class="{ 'input-error': errors.concepto }" placeholder="ej. Salario básico" />
                  <span v-if="errors.concepto" class="field-error">{{ errors.concepto }}</span>
                </div>
                <div class="field">
                  <label>Monto (COP)</label>
                  <CurrencyInput v-model="form.monto" :class="{ 'input-error': errors.monto }" placeholder="$ 0" />
                  <span v-if="errors.monto" class="field-error">{{ errors.monto }}</span>
                </div>
              </div>
              <div v-if="store.cuentas.length > 0" class="field">
                <label>Cuenta <span class="label-optional">(opcional)</span></label>
                <select v-model="form.cuentaId" class="input">
                  <option value="">Ninguna</option>
                  <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
                </select>
              </div>
              <label class="check-label">
                <input type="checkbox" v-model="form.esRecurrente" />
                Repetir cada mes
              </label>
              <div v-if="form.esRecurrente" class="form-row">
                <div class="field">
                  <label>Frecuencia</label>
                  <select v-model="form.frecuencia" class="input">
                    <option v-for="(label, val) in FRECUENCIA_LABEL" :key="val" :value="val">{{ label }}</option>
                  </select>
                </div>
                <div v-if="mostrarDiaPago" class="field">
                  <label>Día de pago <span class="label-optional">(opcional)</span></label>
                  <input v-model.number="form.diaPago" type="number" min="1" max="31" class="input" placeholder="ej. 15" />
                </div>
              </div>
              <div class="form-actions">
                <button type="button" class="btn" @click="cerrarForm">Cancelar</button>
                <button type="submit" class="btn btn-primary" :disabled="saving">
                  {{ saving ? '...' : 'Guardar' }}
                </button>
              </div>
            </form>
          </div>
          <div v-else-if="!editando" class="ingreso-row" :class="{ 'mov-resaltado': resaltadoId === i.id }" :data-mov-id="i.id">
            <div class="row-left">
              <span class="row-icon">
                <component :is="iconoIngreso(i.concepto)" :size="18" :stroke-width="1.9" />
              </span>
              <div class="row-text">
                <div class="row-name">{{ i.concepto }}</div>
                <div class="row-meta">
                  <span v-if="i.esRecurrente" class="row-frecuencia">
                    <Repeat :size="11" :stroke-width="2.2" />
                    {{ i.frecuencia ? FRECUENCIA_LABEL[i.frecuencia] : 'Recurrente' }}<template v-if="i.diaPago"> · día {{ i.diaPago }}</template>
                  </span>
                  <span v-else>Puntual</span>
                  <template v-if="nombreCuenta(i.cuentaId)">
                    <span class="row-meta-sep">·</span>
                    <span>{{ nombreCuenta(i.cuentaId) }}</span>
                  </template>
                </div>
              </div>
            </div>
            <div class="row-right">
              <span class="row-monto">{{ fmt(i.monto) }}</span>
              <div class="row-actions">
                <button class="icon-btn" title="Editar" aria-label="Editar ingreso" @click="abrirForm(i)">
                  <Pencil :size="14" :stroke-width="1.9" />
                </button>
                <button class="icon-btn danger" title="Eliminar" aria-label="Eliminar ingreso" @click="onDelete(i.id!)">
                  <Trash2 :size="14" :stroke-width="1.9" />
                </button>
              </div>
            </div>
          </div>
        </template>

        <!-- Invitación a registrar más fuentes (llena el vacío cuando hay pocas) -->
        <div v-if="store.ingresos.length <= 2 && !editando && !showForm" class="add-more">
          <p>¿Tienes ingresos por freelance, arriendos o inversiones?</p>
          <button class="link-btn" @click="abrirForm()">
            <Plus :size="13" :stroke-width="2.4" /> Agrégalos aquí
          </button>
        </div>
      </div>

      <!-- Panel de resumen -->
      <aside class="card side-card">
        <p class="side-label">Total del mes</p>
        <div class="side-total-row">
          <span class="side-total">{{ fmt(total) }}</span>
          <span
            v-if="variacion != null"
            class="var-badge"
            :class="variacion >= 0 ? 'var-badge--good' : 'var-badge--bad'"
            title="Frente al mes anterior"
          >
            <ArrowUpRight v-if="variacion >= 0" :size="12" :stroke-width="2.5" />
            <ArrowDownRight v-else :size="12" :stroke-width="2.5" />
            {{ Math.abs(variacion) }}%
          </span>
        </div>
        <p v-if="store.resumenAnterior?.totalIngresos" class="side-hint">vs {{ fmt(store.resumenAnterior.totalIngresos) }} el mes anterior</p>

        <div class="side-metrics">
          <div class="side-metric">
            <span>Recurrentes</span>
            <strong>{{ fmt(totalRecurrente) }}</strong>
          </div>
          <div class="side-metric">
            <span>Puntuales</span>
            <strong>{{ fmt(total - totalRecurrente) }}</strong>
          </div>
          <div class="side-metric">
            <span>Promedio diario</span>
            <strong>{{ fmt(promedioDiario) }}</strong>
          </div>
          <div v-if="store.resumen" class="side-metric">
            <span>Te queda tras gastos</span>
            <strong :class="store.resumen.ahorro >= 0 ? 'pos' : 'neg'">{{ fmt(store.resumen.ahorro) }}</strong>
          </div>
        </div>

        <div class="side-compo">
          <p class="side-label">Composición</p>
          <div class="compo-bar">
            <span
              v-for="(c, idx) in composicion"
              :key="c.id ?? idx"
              class="compo-seg"
              :style="{ width: c.pct + '%', background: COMPO_COLORS[idx % COMPO_COLORS.length] }"
              :title="`${c.concepto}: ${c.pct}%`"
            />
          </div>
          <ul class="compo-legend">
            <li v-for="(c, idx) in composicion" :key="c.id ?? idx">
              <span class="compo-dot" :style="{ background: COMPO_COLORS[idx % COMPO_COLORS.length] }" />
              <span class="compo-name">{{ c.concepto }}</span>
              <span class="compo-pct">{{ c.pct }}%</span>
            </li>
          </ul>
        </div>
      </aside>
    </div>

    <template v-else-if="!showForm">
      <div class="empty-state">
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.3" style="color: var(--text-muted)"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/></svg>
        <p class="empty-state-title">Sin ingresos este mes</p>
        <p class="empty-state-sub">Registra tus ingresos para ver cómo se distribuye tu dinero</p>
        <button class="btn btn-primary" style="margin-top: 0.75rem" @click="abrirForm()">
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Agregar ingreso
        </button>
      </div>
    </template>

    <ConfirmModal
      v-model="showConfirm"
      title="Eliminar ingreso"
      message="¿Seguro que quieres eliminar este ingreso?"
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, type Component } from 'vue'
import {
  Plus, FileSpreadsheet, Pencil, Trash2, Repeat, ArrowUpRight, ArrowDownRight,
  Briefcase, Laptop, House, TrendingUp, Gift, Banknote,
} from '@lucide/vue'
import { useRoute, useRouter } from 'vue-router'
import { useFinanceStore } from '@/stores/finance'
import { useResaltarMovimiento } from '@/composables/useResaltarMovimiento'
import { useToast } from '@/composables/useToast'
import { fmtCOP as fmt, exportExcel } from '@/utils'
import MonthSelector from '@/components/common/MonthSelector.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import { ingresoSchema } from '@/schemas'
import type { Ingreso, FrecuenciaIngreso } from '@/types'

const store = useFinanceStore()
const toast = useToast()

const FRECUENCIA_LABEL: Record<FrecuenciaIngreso, string> = {
  DIARIO: 'Diario',
  SEMANAL: 'Semanal',
  QUINCENAL: 'Quincenal',
  MENSUAL: 'Mensual',
  BIMESTRAL: 'Bimestral',
  TRIMESTRAL: 'Trimestral',
  ANUAL: 'Anual',
  PERSONALIZADO: 'Personalizado',
}

const showForm        = ref(false)
const saving          = ref(false)
const copiando        = ref(false)
const showConfirm     = ref(false)
const pendingDeleteId = ref<string | null>(null)
const editando        = ref<Ingreso | null>(null)

const form = reactive({
  concepto: '', monto: 0, esRecurrente: false, cuentaId: '',
  frecuencia: 'MENSUAL' as FrecuenciaIngreso,
  diaPago: undefined as number | undefined,
})
const errors = ref<Partial<Record<'concepto' | 'monto', string>>>({})
const total  = computed(() => store.ingresos.reduce((s, i) => s + i.monto, 0))

// ── Resumen lateral ──
const totalRecurrente = computed(() =>
  store.ingresos.filter(i => i.esRecurrente).reduce((s, i) => s + i.monto, 0)
)
const promedioDiario = computed(() => {
  const dias = new Date(store.anioActual, store.mesActual, 0).getDate()
  return total.value / dias
})
const variacion = computed(() => {
  const ant = store.resumenAnterior?.totalIngresos ?? 0
  return ant > 0 ? Math.round((total.value - ant) / ant * 100) : null
})
const COMPO_COLORS = ['#10b981', '#60a5fa', '#a78bfa', '#fb923c', '#f472b6', '#facc15']
const composicion = computed(() =>
  [...store.ingresos]
    .sort((a, b) => b.monto - a.monto)
    .map(i => ({ id: i.id, concepto: i.concepto, pct: total.value > 0 ? Math.round(i.monto / total.value * 100) : 0 }))
)

function nombreCuenta(id?: string) {
  return id ? store.cuentas.find(c => c.id === id)?.nombre : undefined
}

// Ícono según el concepto; por defecto un billete
const ICONOS_INGRESO: [RegExp, Component][] = [
  [/salario|sueldo|n[oó]mina|quincena/i, Briefcase],
  [/freelance|proyecto|cliente|consultor/i, Laptop],
  [/arriendo|alquiler|renta/i, House],
  [/inversi|dividend|inter[eé]s|cdt|acciones/i, TrendingUp],
  [/regalo|bono|prima/i, Gift],
]
function iconoIngreso(concepto: string) {
  return ICONOS_INGRESO.find(([re]) => re.test(concepto))?.[1] ?? Banknote
}

const mostrarDiaPago = computed(() =>
  !['DIARIO', 'SEMANAL', 'PERSONALIZADO'].includes(form.frecuencia)
)

async function onSubmit() {
  errors.value = {}
  const result = ingresoSchema.safeParse(form)
  if (!result.success) {
    result.error.issues.forEach(e => {
      const field = e.path[0] as 'concepto' | 'monto'
      errors.value[field] = e.message
    })
    return
  }
  saving.value = true
  const payload = {
    concepto: form.concepto,
    monto: form.monto,
    esRecurrente: form.esRecurrente,
    cuentaId: form.cuentaId || undefined,
    frecuencia: form.esRecurrente ? form.frecuencia : undefined,
    diaPago: form.esRecurrente && mostrarDiaPago.value ? form.diaPago : undefined,
  }
  try {
    if (editando.value?.id) {
      await store.editarIngreso(editando.value.id, payload)
      toast.success('Ingreso actualizado')
    } else {
      await store.agregarIngreso(payload)
      toast.success('Ingreso agregado')
    }
    cerrarForm()
  } catch {
    toast.error('No se pudo guardar el ingreso')
  } finally {
    saving.value = false
  }
}

function abrirForm(ingreso?: Ingreso) {
  editando.value       = ingreso ?? null
  form.concepto        = ingreso?.concepto     ?? ''
  form.monto           = ingreso?.monto        ?? 0
  form.esRecurrente    = ingreso?.esRecurrente ?? false
  form.cuentaId        = ingreso?.cuentaId     ?? ''
  form.frecuencia      = ingreso?.frecuencia   ?? 'MENSUAL'
  form.diaPago         = ingreso?.diaPago      ?? undefined
  showForm.value       = ingreso == null
}

function cerrarForm() {
  showForm.value    = false
  editando.value    = null
  form.concepto     = ''
  form.monto        = 0
  form.esRecurrente = false
  form.cuentaId     = ''
  form.frecuencia   = 'MENSUAL'
  form.diaPago      = undefined
  errors.value      = {}
}

async function copiarRecurrentes() {
  copiando.value = true
  try {
    const n = await store.copiarIngresosRecurrentes()
    if (n > 0) toast.success(`${n} ingreso${n > 1 ? 's' : ''} recurrente${n > 1 ? 's' : ''} copiado${n > 1 ? 's' : ''}`)
    else toast.info('No hay ingresos recurrentes del mes anterior')
  } catch {
    toast.error('No se pudieron copiar los ingresos recurrentes')
  } finally {
    copiando.value = false
  }
}

function onDelete(id: string) {
  pendingDeleteId.value = id
  showConfirm.value = true
}

async function confirmDelete() {
  if (pendingDeleteId.value != null) {
    try {
      await store.eliminarIngreso(pendingDeleteId.value)
      toast.success('Ingreso eliminado')
    } catch {
      toast.error('No se pudo eliminar el ingreso')
    } finally {
      pendingDeleteId.value = null
    }
  }
}

function descargarExcel() {
  exportExcel(
    store.ingresos.map(i => ({
      Concepto: i.concepto,
      Monto: i.monto,
      Recurrente: i.esRecurrente ? 'Sí' : 'No',
      Frecuencia: i.frecuencia ? FRECUENCIA_LABEL[i.frecuencia] : '',
      'Día de pago': i.diaPago ?? '',
    })),
    `ingresos-${store.mesActual}-${store.anioActual}.xlsx`,
    'Ingresos'
  )
}

// Búsqueda global → resaltar el ingreso
const { resaltadoId } = useResaltarMovimiento(
  () => store.ingresos.map(i => i.id),
  () => store.loading,
  () => cerrarForm(),
)

// Estado vacío del Dashboard → /ingresos?nuevo=1 abre el formulario directamente
const route  = useRoute()
const router = useRouter()

onMounted(() => {
  store.cargarDatos(); store.cargarCuentas()
  if (route.query.nuevo) {
    const { nuevo: _quitar, ...resto } = route.query
    router.replace({ query: resto })
    abrirForm()
  }
})
</script>

<style scoped>
/* ── Encabezado ── */
.header-left { display: flex; align-items: flex-end; gap: 1.25rem; flex-wrap: wrap; }
.header-actions { display: flex; align-items: center; gap: 0.6rem; }
.btn-ghost { background: transparent; border-color: transparent; }
.btn-ghost:hover { background: var(--surface2); border-color: var(--border); }

/* ── Layout: lista + panel de resumen ── */
.ingresos-layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 320px;
  gap: 1rem;
  align-items: start;
}
.list-card { padding: 1.25rem 1.35rem; }
.list-card:hover { border-color: var(--border); }

.card-header { display: flex; justify-content: space-between; align-items: center; gap: 1rem; margin-bottom: 0.85rem; }
.card-title  {
  font-family: var(--ff-body);
  font-size: 0.78rem; font-weight: 500; color: var(--text-primary);
  letter-spacing: 0.08em; text-transform: uppercase;
}
.card-sub { font-size: 0.7rem; color: var(--text-muted); margin-top: 0.2rem; }
.total-badge {
  font-family: var(--ff-mono);
  font-variant-numeric: tabular-nums;
  font-size: 0.8rem; font-weight: 700;
  padding: 0.3rem 0.75rem;
  border-radius: 999px;
  color: var(--c-pos);
  background: color-mix(in srgb, var(--c-pos) 12%, transparent);
}

.form-card { margin-bottom: 1rem; }

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

.ingreso-form { display: flex; flex-direction: column; gap: 0.75rem; width: 100%; }
.form-row  { display: flex; gap: 0.75rem; }
.field     { display: flex; flex-direction: column; flex: 1; }
.form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; }
.field-error  { font-size: 0.65rem; color: var(--red); margin-top: 0.2rem; }
.input-error  { border-color: var(--red) !important; }

.label-row { display: flex; justify-content: space-between; align-items: baseline; }
.char-counter { font-size: 0.62rem; color: var(--text-muted); }
.char-counter--limit { color: var(--red); }

/* ── Filas ── */
.ingreso-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  margin: 0 -0.6rem;
  padding: 0.85rem 0.6rem;
  border-radius: 10px;
  border-bottom: 1px solid var(--border);
  transition: background 0.12s ease;
}
.ingreso-row:last-child,
.ingreso-row:has(+ .add-more) { border-bottom-color: transparent; }
.ingreso-row:hover,
.ingreso-row:focus-within { background: var(--surface2); border-bottom-color: transparent; }

.row-left { display: flex; align-items: center; gap: 0.9rem; min-width: 0; }
.row-icon {
  display: grid; place-items: center;
  width: 40px; height: 40px; border-radius: 10px; flex-shrink: 0;
  color: var(--c-pos);
  background: var(--surface2);
  border: 1px solid var(--border);
}
.ingreso-row:hover .row-icon { background: var(--surface); }
.row-text { min-width: 0; }
.row-name  {
  font-size: 0.86rem; font-weight: 700; color: var(--text-primary);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.row-meta { display: flex; align-items: center; gap: 0.35rem; margin-top: 0.25rem; font-size: 0.7rem; color: var(--text-subtle); }
.row-meta-sep { color: var(--text-muted); }
.row-frecuencia { display: inline-flex; align-items: center; gap: 0.3rem; }
.row-frecuencia svg { color: var(--accent-strong); }

.row-right { display: flex; align-items: center; gap: 0.75rem; flex-shrink: 0; }
.row-monto {
  font-family: var(--ff-mono);
  font-variant-numeric: tabular-nums;
  font-size: 0.9rem; font-weight: 700; color: var(--text-primary);
}
.row-actions {
  display: flex; gap: 0.15rem;
  opacity: 0; transform: translateX(4px);
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.ingreso-row:hover .row-actions,
.ingreso-row:focus-within .row-actions { opacity: 1; transform: none; }
@media (hover: none) { .row-actions { opacity: 1; transform: none; } }

.icon-btn {
  display: grid; place-items: center;
  width: 30px; height: 30px; padding: 0;
  background: none; border: none; border-radius: 8px;
  color: var(--text-secondary); cursor: pointer;
  transition: color 0.15s, background 0.15s;
}
.icon-btn:hover        { color: var(--text-primary); background: var(--surface); }
.icon-btn.danger:hover { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 12%, transparent); }
.icon-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }

/* Invitación a agregar más */
.add-more {
  display: flex; flex-direction: column; align-items: center; gap: 0.45rem;
  margin-top: 0.85rem; padding: 1.25rem 1rem;
  border: 1px dashed var(--border-hover); border-radius: 12px;
  text-align: center;
}
.add-more p { font-size: 0.76rem; color: var(--text-secondary); }
.link-btn {
  display: inline-flex; align-items: center; gap: 0.3rem;
  background: none; border: none; padding: 0.2rem 0.4rem; border-radius: 6px;
  font: inherit; font-size: 0.76rem; font-weight: 500;
  color: var(--accent-strong); cursor: pointer;
}
.link-btn:hover { background: color-mix(in srgb, var(--accent) 12%, transparent); }

/* ── Panel de resumen ── */
.side-card { position: sticky; top: 88px; display: flex; flex-direction: column; padding: 1.25rem 1.35rem; }
.side-card:hover { border-color: var(--border); }
.side-label { font-size: 0.66rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.side-total-row { display: flex; align-items: center; flex-wrap: wrap; gap: 0.5rem; margin-top: 0.5rem; }
.side-total {
  font-family: var(--ff-mono); font-variant-numeric: tabular-nums;
  font-size: 1.6rem; font-weight: 700; letter-spacing: -0.02em; color: var(--text-primary);
}
.side-hint { font-size: 0.68rem; color: var(--text-muted); margin-top: 0.3rem; }
.var-badge {
  display: inline-flex; align-items: center; gap: 2px;
  font-size: 0.66rem; font-weight: 500;
  padding: 0.15rem 0.45rem 0.15rem 0.3rem; border-radius: 999px;
}
.var-badge--good { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 13%, transparent); }
.var-badge--bad  { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 13%, transparent); }

.side-metrics { display: flex; flex-direction: column; margin: 1.1rem 0; border-top: 1px solid var(--border); }
.side-metric {
  display: flex; justify-content: space-between; align-items: center;
  padding: 0.65rem 0; border-bottom: 1px solid var(--border);
  font-size: 0.74rem; color: var(--text-secondary);
}
.side-metric strong {
  font-family: var(--ff-mono); font-variant-numeric: tabular-nums;
  font-weight: 700; color: var(--text-primary);
}
.side-metric strong.pos { color: var(--c-pos); }
.side-metric strong.neg { color: var(--c-neg); }

.compo-bar { display: flex; gap: 3px; height: 8px; margin: 0.65rem 0 0.75rem; border-radius: 99px; overflow: hidden; background: var(--surface2); }
.compo-seg { height: 100%; min-width: 4px; }
.compo-legend { list-style: none; display: flex; flex-direction: column; gap: 0.45rem; }
.compo-legend li { display: flex; align-items: center; gap: 0.5rem; font-size: 0.72rem; color: var(--text-secondary); }
.compo-dot { width: 8px; height: 8px; border-radius: 99px; flex-shrink: 0; }
.compo-name { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.compo-pct { font-family: var(--ff-mono); font-variant-numeric: tabular-nums; color: var(--text-primary); }

.inline-form-row { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0; }
.input-inline    { flex: 1; height: 30px; font-size: 0.78rem; padding: 0 0.6rem; }

.check-label { display: flex; align-items: center; gap: 0.5rem; font-size: 0.75rem; color: var(--text-secondary); cursor: pointer; user-select: none; }
.check-label input { accent-color: var(--accent); cursor: pointer; }

.recurrentes-banner {
  display: flex; justify-content: space-between; align-items: center; gap: 1rem;
  background: var(--surface); border: 1px solid color-mix(in srgb, var(--accent) 20%, transparent);
  border-left: 3px solid var(--accent); border-radius: 8px;
  padding: 0.6rem 0.85rem; margin-bottom: 1rem;
  font-size: 0.75rem; color: var(--text-secondary);
}
.banner-left    { display: flex; align-items: center; gap: 0.5rem; }
.banner-actions { display: flex; align-items: center; gap: 0.35rem; flex-shrink: 0; }
.banner-close {
  background: none; border: none; cursor: pointer; color: var(--text-muted);
  padding: 3px 5px; border-radius: 4px; display: flex; align-items: center; transition: color 0.15s;
}
.banner-close:hover { color: var(--text-primary); }
.banner-slide-enter-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.banner-slide-leave-active { transition: opacity 0.15s ease, transform 0.15s ease; }
.banner-slide-enter-from   { opacity: 0; transform: translateY(-6px); }
.banner-slide-leave-to     { opacity: 0; transform: translateY(-6px); }


@media (max-width: 1024px) {
  .ingresos-layout { grid-template-columns: 1fr; }
  .side-card { position: static; }
}

@media (max-width: 500px) {
  .header-left { width: 100%; justify-content: space-between; }
  .header-actions { width: 100%; }
  .header-actions > .btn-primary { flex: 1; }

  .form-row { flex-direction: column;}
  .inline-form-row { flex-direction: column; align-items: stretch; gap: 0.5rem; }

  .input-inline    { flex: 1; height: 30px; font-size: 0.78rem; padding: 0.5rem; }
}
</style>
