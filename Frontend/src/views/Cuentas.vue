<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Finanzas</div>
        <h1 class="page-title">Cuentas</h1>
      </div>
      <button class="btn btn-primary btn-new-account" @click="abrirForm()">
        <Plus :size="15" :stroke-width="2.4" />
        Nueva cuenta
      </button>
    </div>

    <!-- Hero: total en cuentas a todo el ancho -->
    <div v-if="!loading && cuentas.length > 0" class="hero">
      <div class="hero-main">
        <span class="hero-label">Total en cuentas</span>
        <span class="hero-val" :class="{ 'is-neg': totalGeneral < 0 }">{{ fmt(totalGeneral) }}</span>
        <span class="hero-sub">en {{ cuentas.length }} cuenta{{ cuentas.length !== 1 ? 's' : '' }}</span>
      </div>

      <div v-if="distribucion.length > 0" class="hero-dist">
        <div class="dist-bar">
          <span
            v-for="d in distribucion"
            :key="d.id"
            class="dist-seg"
            :style="{ width: d.pct + '%', background: d.color }"
            :title="`${d.nombre}: ${d.pct}%`"
          />
        </div>
        <ul class="dist-legend">
          <li v-for="d in distribucion" :key="d.id">
            <span class="dist-dot" :style="{ background: d.color }" />
            <span class="dist-name">{{ d.nombre }}</span>
            <span class="dist-pct">{{ d.pct }}%</span>
          </li>
        </ul>
      </div>
    </div>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="cuentas.length > 0">
      <div class="cuentas-grid">
        <article v-for="c in cuentas" :key="c.id" class="cuenta-card" :style="{ '--tc': TIPO_META[c.tipo].color }">
          <div class="cuenta-top">
            <div class="cuenta-tipo">
              <span class="cuenta-icon"><component :is="TIPO_META[c.tipo].icon" :size="17" :stroke-width="1.9" /></span>
              <span class="tipo-badge">{{ TIPO_LABEL[c.tipo] }}</span>
            </div>
            <div class="cuenta-actions">
              <button class="icon-btn" title="Editar" aria-label="Editar cuenta" @click="abrirForm(c)">
                <Pencil :size="14" :stroke-width="1.9" />
              </button>
              <button class="icon-btn icon-btn--danger" title="Eliminar" aria-label="Eliminar cuenta" @click="eliminar(c)">
                <Trash2 :size="14" :stroke-width="1.9" />
              </button>
            </div>
          </div>

          <div class="cuenta-body">
            <p class="cuenta-nombre">{{ c.nombre }}</p>
            <p class="cuenta-saldo" :class="{ 'is-neg': c.saldo < 0 }">{{ fmt(c.saldo) }}</p>
            <p v-if="totalGeneral > 0 && c.saldo > 0" class="cuenta-share">{{ Math.round(c.saldo / totalGeneral * 100) }}% de tu dinero</p>
          </div>

          <button class="btn-mov" @click="abrirMovimientoModal(c)">
            <Plus :size="14" :stroke-width="2.4" />
            Nuevo movimiento
          </button>
        </article>
      </div>
    </template>

    <template v-else>
      <div class="empty-state">
        <span class="empty-icon"><Landmark :size="24" :stroke-width="1.8" /></span>
        <p class="empty-state-title">Sin cuentas registradas</p>
        <p class="empty-state-sub">Crea tu efectivo, cuentas bancarias o alcancía para saber cuánto tienes en cada una</p>
        <button class="btn btn-primary" style="margin-top: 0.75rem" @click="abrirForm()">
          <Plus :size="15" :stroke-width="2.4" /> Nueva cuenta
        </button>
      </div>
    </template>

    <!-- Modal cuenta -->
    <div v-if="formVisible" class="modal-backdrop" @click.self="cerrarForm">
      <div class="modal">
        <p class="modal-title">{{ editando ? 'Editar cuenta' : 'Nueva cuenta' }}</p>

        <div class="field-group">
          <div class="label-row">
            <label class="field-label">Nombre</label>
            <span class="char-counter" :class="{ 'char-counter--limit': form.nombre.length >= 100 }">{{ form.nombre.length }}/100</span>
          </div>
          <input v-model="form.nombre" class="input" maxlength="100" placeholder="ej. Bancolombia, Efectivo, Alcancía viaje" />
        </div>

        <div class="field-group">
          <label class="field-label">Tipo</label>
          <div class="select-wrap">
            <select v-model="form.tipo" class="input">
              <option v-for="(label, tipo) in TIPO_LABEL" :key="tipo" :value="tipo">{{ label }}</option>
            </select>
          </div>
        </div>

        <div v-if="!editando" class="field-group">
          <label class="field-label">Saldo inicial (opcional)</label>
          <CurrencyInput v-model="form.saldoInicial" class="input" />
        </div>

        <div class="modal-actions">
          <button class="btn" @click="cerrarForm">Cancelar</button>
          <button class="btn btn-primary" :disabled="!form.nombre.trim() || guardando" @click="guardar">
            {{ guardando ? '…' : 'Guardar' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal movimiento -->
    <div v-if="movModalVisible" class="modal-backdrop" @click.self="cerrarMovimientoModal">
      <div class="modal">
        <p class="modal-title">Registrar movimiento</p>

        <div class="field-group">
          <label class="field-label">Tipo</label>
          <div class="tipo-toggle">
            <button class="tipo-btn" :class="{ active: movTipo === 'DEPOSITO' }" @click="movTipo = 'DEPOSITO'">Depósito</button>
            <button class="tipo-btn" :class="{ active: movTipo === 'RETIRO' }" @click="movTipo = 'RETIRO'">Retiro</button>
          </div>
        </div>

        <div class="field-group">
          <label class="field-label">Monto</label>
          <CurrencyInput v-model="movMonto" class="input" />
        </div>

        <div class="field-group">
          <label class="field-label">Descripción (opcional)</label>
          <input v-model="movDescripcion" class="input" placeholder="ej. Ahorro del mes" />
        </div>

        <div v-if="historialConSaldo.length > 0" class="mov-historial">
          <div class="mov-historial-head">
            <span class="field-label">Movimientos de la cuenta</span>
            <span v-if="movCuenta" class="mov-saldo-actual" :class="movCuenta.saldo >= 0 ? 'total-pos' : 'total-neg'">
              Saldo: {{ fmt(movCuenta.saldo) }}
            </span>
          </div>
          <p class="mov-hint">Incluye ingresos y gastos que asignaste a esta cuenta. Saldo = suma de todo.</p>
          <div class="mov-historial-list">
            <div v-for="(m, i) in historialConSaldo" :key="i" class="mov-historial-row">
              <span class="mov-row-main">
                <span class="mov-tag" :class="`mov-tag--${m.tipo.toLowerCase()}`">{{ LINEA_LABEL[m.tipo] }}</span>
                {{ fmtFecha(m.fecha) }}<span v-if="m.descripcion"> · {{ m.descripcion }}</span>
              </span>
              <span class="mov-row-nums">
                <span :class="m.monto >= 0 ? 'total-pos' : 'total-neg'">{{ m.monto >= 0 ? '+' : '' }}{{ fmt(m.monto) }}</span>
                <span class="mov-row-saldo">{{ fmt(m.saldoTras) }}</span>
              </span>
            </div>
          </div>
        </div>

        <div class="modal-actions">
          <button class="btn" @click="cerrarMovimientoModal">Cancelar</button>
          <button class="btn btn-primary" :disabled="movMonto <= 0 || movGuardando" @click="guardarMovimiento">
            {{ movGuardando ? '…' : 'Guardar' }}
          </button>
        </div>
      </div>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Eliminar cuenta"
      message="Se eliminará la cuenta y dejará de contarse en tu total. Esta acción no se puede deshacer."
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, type Component } from 'vue'
import {
  Plus, Pencil, Trash2, Landmark, PiggyBank, Wallet, Smartphone, CreditCard, Bitcoin, CircleDollarSign,
} from '@lucide/vue'
import type { Cuenta, LineaCuenta, TipoCuenta } from '@/types'
import { cuentasService } from '@/services/api'
import { fmtCOP as fmt } from '@/utils'
import { useToast } from '@/composables/useToast'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const toast = useToast()

const TIPO_LABEL: Record<TipoCuenta, string> = {
  EFECTIVO: 'Efectivo',
  BANCO: 'Banco',
  BILLETERA_DIGITAL: 'Billetera digital',
  TARJETA_CREDITO: 'Tarjeta de crédito',
  ALCANCIA: 'Alcancía',
  CRIPTO: 'Cripto',
  OTRO: 'Otro',
}

// Ícono + color (hex para poder usarlo también en la barra de distribución)
const TIPO_META: Record<TipoCuenta, { icon: Component; color: string }> = {
  EFECTIVO:          { icon: Wallet,           color: '#10b981' },
  BANCO:             { icon: Landmark,         color: '#60a5fa' },
  BILLETERA_DIGITAL: { icon: Smartphone,       color: '#a78bfa' },
  TARJETA_CREDITO:   { icon: CreditCard,       color: '#fb923c' },
  ALCANCIA:          { icon: PiggyBank,        color: '#f472b6' },
  CRIPTO:            { icon: Bitcoin,          color: '#facc15' },
  OTRO:              { icon: CircleDollarSign, color: '#94a3b8' },
}

// Reparto del dinero entre cuentas con saldo positivo
const distribucion = computed(() => {
  const positivas = cuentas.value.filter(c => c.saldo > 0)
  const total = positivas.reduce((s, c) => s + c.saldo, 0)
  if (total <= 0) return []
  return positivas
    .sort((a, b) => b.saldo - a.saldo)
    .map((c, i) => ({ id: c.id, nombre: c.nombre, color: DIST_COLORS[i % DIST_COLORS.length], pct: Math.round(c.saldo / total * 100) }))
})
const DIST_COLORS = ['#60a5fa', '#10b981', '#a78bfa', '#fb923c', '#f472b6', '#facc15']

const LINEA_LABEL: Record<LineaCuenta['tipo'], string> = {
  MOVIMIENTO: 'Manual',
  INGRESO: 'Ingreso',
  GASTO: 'Gasto',
}

const cuentas = ref<Cuenta[]>([])
const loading = ref(false)
const guardando = ref(false)
const formVisible = ref(false)
const editando = ref<Cuenta | null>(null)
const showConfirm = ref(false)
const pendingId = ref<string | null>(null)

const form = reactive({
  nombre: '',
  tipo: 'EFECTIVO' as TipoCuenta,
  saldoInicial: 0,
})

const totalGeneral = computed(() => cuentas.value.reduce((s, c) => s + c.saldo, 0))

function fmtFecha(f: string) {
  return new Date(f).toLocaleDateString('es-CO', { day: 'numeric', month: 'short', year: 'numeric' })
}

async function cargar() {
  loading.value = true
  try {
    const { data } = await cuentasService.getAll()
    cuentas.value = data
  } finally {
    loading.value = false
  }
}

function abrirForm(c?: Cuenta) {
  editando.value = c ?? null
  form.nombre = c?.nombre ?? ''
  form.tipo = c?.tipo ?? 'EFECTIVO'
  form.saldoInicial = 0
  formVisible.value = true
}

function cerrarForm() { formVisible.value = false }

async function guardar() {
  guardando.value = true
  try {
    if (editando.value?.id) {
      const { data } = await cuentasService.update(editando.value.id, { nombre: form.nombre.trim(), tipo: form.tipo })
      const idx = cuentas.value.findIndex(c => c.id === data.id)
      if (idx !== -1) cuentas.value[idx] = data
      toast.success('Cuenta actualizada')
    } else {
      const { data } = await cuentasService.create({ nombre: form.nombre.trim(), tipo: form.tipo, saldoInicial: form.saldoInicial })
      cuentas.value.push(data)
      toast.success('Cuenta creada')
    }
    cerrarForm()
  } catch {
    toast.error('No se pudo guardar la cuenta')
  } finally {
    guardando.value = false
  }
}

function eliminar(c: Cuenta) {
  pendingId.value = c.id ?? null
  showConfirm.value = true
}

async function confirmDelete() {
  if (!pendingId.value) return
  try {
    await cuentasService.delete(pendingId.value)
    cuentas.value = cuentas.value.filter(c => c.id !== pendingId.value)
    toast.success('Cuenta eliminada')
  } catch {
    toast.error('No se pudo eliminar la cuenta')
  } finally {
    pendingId.value = null
  }
}

// Movimientos
const movModalVisible = ref(false)
const movCuentaId = ref<string | null>(null)
const movCuenta = ref<Cuenta | null>(null)
const movTipo = ref<'DEPOSITO' | 'RETIRO'>('DEPOSITO')
const movMonto = ref(0)
const movDescripcion = ref('')
const movGuardando = ref(false)
const movHistorial = ref<LineaCuenta[]>([])

// Saldo corriente por línea: el historial viene del más nuevo al más viejo,
// así que el saldo tras la línea i = saldo actual − suma de las líneas más nuevas.
const historialConSaldo = computed(() => {
  const saldo = movCuenta.value?.saldo ?? 0
  let acum = 0
  return movHistorial.value.map(m => {
    const saldoTras = saldo - acum
    acum += m.monto
    return { ...m, saldoTras }
  })
})

async function abrirMovimientoModal(c: Cuenta) {
  movCuentaId.value = c.id!
  movCuenta.value = c
  movTipo.value = 'DEPOSITO'
  movMonto.value = 0
  movDescripcion.value = ''
  movHistorial.value = []
  movModalVisible.value = true
  const { data } = await cuentasService.getMovimientos(c.id!)
  movHistorial.value = data
}

function cerrarMovimientoModal() {
  movModalVisible.value = false
  movCuentaId.value = null
  movCuenta.value = null
}

async function guardarMovimiento() {
  if (!movCuentaId.value || movMonto.value <= 0) return
  movGuardando.value = true
  try {
    const montoFirmado = movTipo.value === 'RETIRO' ? -movMonto.value : movMonto.value
    const { data } = await cuentasService.crearMovimiento(movCuentaId.value, montoFirmado, movDescripcion.value.trim() || undefined)
    const idx = cuentas.value.findIndex(c => c.id === movCuentaId.value)
    if (idx !== -1) cuentas.value[idx] = data
    cerrarMovimientoModal()
    toast.success('Movimiento registrado')
  } catch {
    toast.error('No se pudo registrar el movimiento')
  } finally {
    movGuardando.value = false
  }
}

onMounted(cargar)
</script>

<style scoped>

.fade-up { font-variant-numeric: tabular-nums; }

/* ── Hero: total en cuentas ── */
.hero {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  align-items: center;
  gap: 2.5rem;
  margin-bottom: 1.25rem;
  padding: 1.6rem 1.75rem;
  border: 1px solid color-mix(in srgb, var(--c-need) 18%, var(--border));
  border-radius: var(--radius-card);
  background:
    radial-gradient(140% 160% at 0% 0%, color-mix(in srgb, var(--c-need) 12%, transparent), transparent 60%),
    linear-gradient(135deg, var(--surface) 0%, color-mix(in srgb, #1e3a8a 14%, var(--surface)) 100%);
  box-shadow: var(--shadow-card);
}
.hero-main  { display: flex; flex-direction: column; gap: 0.45rem; }
.hero-label { font-size: 0.66rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.14em; text-transform: uppercase; }
.hero-val {
  font-family: var(--ff-mono);
  font-size: 2.4rem; font-weight: 700; letter-spacing: -0.03em; line-height: 1;
  color: var(--text-primary);
}
.hero-val.is-neg { color: var(--c-neg); }
.hero-sub { font-size: 0.72rem; color: var(--text-subtle); }

.hero-dist { min-width: 0; max-width: 520px; justify-self: end; width: 100%; }
.dist-bar { display: flex; gap: 3px; height: 10px; border-radius: 99px; overflow: hidden; background: var(--surface2); }
.dist-seg { height: 100%; min-width: 4px; }
.dist-legend { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem 1.25rem; margin-top: 0.8rem; }
.dist-legend li { display: flex; align-items: center; gap: 0.45rem; font-size: 0.72rem; color: var(--text-secondary); }
.dist-dot  { width: 8px; height: 8px; border-radius: 99px; flex-shrink: 0; }
.dist-name { max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dist-pct  { font-family: var(--ff-mono); color: var(--text-primary); }

/* Colores del historial (modal) */
.total-pos { color: var(--c-pos); }
.total-neg { color: var(--c-neg); }

/* ── Tarjetas de cuenta: se expanden para llenar el ancho ── */
.cuentas-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1rem;
}
.cuenta-card {
  display: flex; flex-direction: column; gap: 1.1rem;
  padding: 1.2rem 1.25rem 1.25rem;
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card);
  box-shadow: var(--shadow-card);
  transition: border-color 0.2s ease;
}
.cuenta-card:hover { border-color: var(--border-hover); }

.cuenta-top  { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
.cuenta-tipo { display: flex; align-items: center; gap: 0.6rem; }
.cuenta-icon {
  display: grid; place-items: center;
  width: 36px; height: 36px; border-radius: 10px;
  color: var(--tc); background: color-mix(in srgb, var(--tc) 14%, transparent);
}
.tipo-badge {
  font-size: 0.62rem; font-weight: 500; letter-spacing: 0.08em; text-transform: uppercase;
  padding: 0.2rem 0.6rem; border-radius: 999px;
  color: var(--tc); background: color-mix(in srgb, var(--tc) 12%, transparent);
}

.cuenta-actions {
  display: flex; gap: 0.1rem;
  opacity: 0; transition: opacity 0.15s ease;
}
.cuenta-card:hover .cuenta-actions,
.cuenta-card:focus-within .cuenta-actions { opacity: 1; }
@media (hover: none) { .cuenta-actions { opacity: 1; } }

.cuenta-body   { display: flex; flex-direction: column; gap: 0.3rem; }
.cuenta-nombre { font-size: 0.86rem; font-weight: 500; color: var(--text-secondary); }
.cuenta-saldo  {
  font-family: var(--ff-mono);
  font-size: 1.6rem; font-weight: 700; letter-spacing: -0.02em;
  color: var(--text-primary);
}
.cuenta-saldo.is-neg { color: var(--c-neg); }
.cuenta-share { font-size: 0.68rem; color: var(--text-muted); }

.btn-mov {
  display: inline-flex; align-items: center; justify-content: center; gap: 0.4rem;
  width: 100%; min-height: 38px; margin-top: auto;
  font: inherit; font-size: 0.76rem; font-weight: 500;
  color: var(--text-primary);
  background: var(--surface2);
  border: 1px solid var(--border-hover);
  border-radius: var(--radius);
  cursor: pointer;
  transition: background 0.15s ease, border-color 0.15s ease, transform 0.1s ease;
}
.btn-mov:hover  { background: color-mix(in srgb, var(--accent) 14%, var(--surface2)); border-color: var(--accent); }
.btn-mov:active { transform: scale(0.98); }
.btn-mov:focus-visible { outline: none; box-shadow: var(--focus-ring); }

.icon-btn {
  display: grid; place-items: center;
  width: 30px; height: 30px; padding: 0;
  background: none; border: none; border-radius: 8px;
  color: var(--text-secondary); cursor: pointer;
  transition: color 0.15s, background 0.15s;
}
.icon-btn:hover         { color: var(--text-primary); background: var(--surface2); }
.icon-btn--danger:hover { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 12%, transparent); }
.icon-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; opacity: 1; }

.empty-icon {
  display: grid; place-items: center;
  width: 52px; height: 52px; border-radius: 999px;
  color: var(--text-secondary); background: var(--surface2);
}
.empty-state { display: flex; flex-direction: column; align-items: center; text-align: center; padding: 3rem 1rem; gap: 0.35rem; }
.empty-state-title { font-size: 0.95rem; font-weight: 600; color: var(--text-primary); margin-top: 0.5rem; }
.empty-state-sub   { font-size: 0.78rem; color: var(--text-muted); max-width: 320px; }

/* Modal */
.modal-backdrop {
  position: fixed; inset: 0; background: rgba(0,0,0,0.5);
  display: flex; align-items: flex-start; justify-content: center; z-index: 100; padding: 3rem 1rem;
  
}


.modal {
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-modal);
  padding: 1.5rem; width: 100%; max-width: 400px; display: flex; flex-direction: column; gap: 1rem;
  max-height: calc(100vh - 4rem); overflow-y: auto;
  box-shadow: var(--shadow-pop);
}
.modal-title  { font-size: 1rem; font-weight: 700; margin: 0; }
.modal-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 0.25rem; }

.field-group { display: flex; flex-direction: column; gap: 0.35rem; }
.field-label { font-size: 0.72rem; color: var(--text-secondary); }

.label-row { display: flex; justify-content: space-between; align-items: baseline; }
.char-counter { font-size: 0.62rem; color: var(--text-muted); }
.char-counter--limit { color: var(--red); }

.tipo-toggle { display: flex; gap: 0.5rem; }
.tipo-btn {
  flex: 1; padding: 0.45rem; border-radius: 6px; font-size: 0.78rem; cursor: pointer;
  background: var(--surface2); border: 1px solid var(--border); color: var(--text-secondary);
  transition: all 0.15s;
}
.tipo-btn.active { border-color: var(--accent); color: var(--accent); background: color-mix(in srgb, var(--accent) 8%, transparent); }

.mov-historial { display: flex; flex-direction: column; gap: 0.3rem; }
.mov-historial-head { display: flex; justify-content: space-between; align-items: baseline; }
.mov-saldo-actual { font-size: 0.8rem; font-weight: 700; }
.mov-hint { font-size: 0.68rem; color: var(--text-muted); margin: -0.1rem 0 0.2rem; }
.mov-historial-list { display: flex; flex-direction: column; max-height: 320px; overflow-y: auto; }
.mov-historial-row {
  display: flex; justify-content: space-between; gap: 0.5rem; font-size: 0.78rem; color: var(--text-secondary);
  padding: 0.5rem; border-bottom: 1px solid var(--border);
}
.mov-historial-row:last-child { border-bottom: none; }
.mov-row-main { min-width: 0; }
.mov-row-nums { display: flex; flex-direction: column; align-items: flex-end; white-space: nowrap; }
.mov-row-saldo { font-size: 0.62rem; color: var(--text-muted); }
.mov-tag {
  display: inline-block; font-size: 0.6rem; font-weight: 600; padding: 0.05rem 0.3rem;
  border-radius: 4px; margin-right: 0.35rem; vertical-align: middle;
  background: var(--surface2); color: var(--text-muted);
}
.mov-tag--ingreso { background: color-mix(in srgb, var(--accent) 12%, transparent); color: var(--accent); }
.mov-tag--gasto   { background: color-mix(in srgb, var(--red) 12%, transparent); color: var(--red); }

.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 64px; border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 768px) {
  .hero { grid-template-columns: 1fr; gap: 1.25rem; padding: 1.3rem 1.25rem; }
  .hero-dist { justify-self: stretch; max-width: none; }
  .hero-val { font-size: 2rem; }
}

@media (max-width: 500px) {
  .btn-new-account { width: 100%; }
  .cuentas-grid { grid-template-columns: 1fr; }
}
</style>
