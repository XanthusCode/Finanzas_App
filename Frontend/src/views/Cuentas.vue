<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Finanzas</div>
        <h1 class="page-title">Cuentas</h1>
      </div>
      <button class="btn btn-primary btn-sm" @click="abrirForm()">+ Nueva cuenta</button>
    </div>

    <!-- Resumen -->
    <div v-if="!loading && cuentas.length > 0" class="resumen-grid">
      <div class="resumen-card">
        <span class="resumen-label">Total en cuentas</span>
        <span class="resumen-val" :class="totalGeneral >= 0 ? 'total-pos' : 'total-neg'">{{ fmt(totalGeneral) }}</span>
        <span class="resumen-count">{{ cuentas.length }} cuenta{{ cuentas.length !== 1 ? 's' : '' }}</span>
      </div>
    </div>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else-if="cuentas.length > 0">
      <div class="cuentas-grid">
        <div v-for="c in cuentas" :key="c.id" class="cuenta-card">
          <div class="cuenta-top">
            <span class="tag" :class="tagClass(c.tipo)">{{ TIPO_LABEL[c.tipo] }}</span>
            <div class="cuenta-actions">
              <button class="icon-btn" title="Editar" @click="abrirForm(c)">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button class="icon-btn danger" title="Eliminar" @click="eliminar(c)">
                <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
          </div>
          <p class="cuenta-nombre">{{ c.nombre }}</p>
          <p class="cuenta-saldo" :class="c.saldo >= 0 ? 'total-pos' : 'total-neg'">{{ fmt(c.saldo) }}</p>
          <button class="btn btn-sm" @click="abrirMovimientoModal(c)">+ Movimiento</button>
        </div>
      </div>
    </template>

    <template v-else>
      <div class="empty-state">
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.3" style="color: var(--text-muted)"><rect x="2" y="6" width="20" height="12" rx="2"/><circle cx="12" cy="12" r="2"/></svg>
        <p class="empty-state-title">Sin cuentas registradas</p>
        <p class="empty-state-sub">Crea tu efectivo, cuentas bancarias o alcancía para saber cuánto tienes en cada una</p>
        <button class="btn btn-primary" style="margin-top: 0.75rem" @click="abrirForm()">+ Nueva cuenta</button>
      </div>
    </template>

    <!-- Modal cuenta -->
    <div v-if="formVisible" class="modal-backdrop" @click.self="cerrarForm">
      <div class="modal">
        <p class="modal-title">{{ editando ? 'Editar cuenta' : 'Nueva cuenta' }}</p>

        <div class="field-group">
          <label class="field-label">Nombre</label>
          <input v-model="form.nombre" class="input" placeholder="ej. Bancolombia, Efectivo, Alcancía viaje" />
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

        <div v-if="movHistorial.length > 0" class="mov-historial">
          <span class="field-label">Movimientos anteriores</span>
          <div v-for="m in movHistorial" :key="m.id" class="mov-historial-row">
            <span>{{ fmtFecha(m.fecha) }}<span v-if="m.descripcion"> · {{ m.descripcion }}</span></span>
            <span :class="m.monto >= 0 ? 'total-pos' : 'total-neg'">{{ m.monto >= 0 ? '+' : '' }}{{ fmt(m.monto) }}</span>
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
import { ref, computed, reactive, onMounted } from 'vue'
import type { Cuenta, MovimientoCuenta, TipoCuenta } from '@/types'
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

const TIPO_TAG: Record<TipoCuenta, string> = {
  EFECTIVO: 'tag-green',
  BANCO: 'tag-red',
  BILLETERA_DIGITAL: 'tag-amber',
  TARJETA_CREDITO: 'tag-amber',
  ALCANCIA: 'tag-green',
  CRIPTO: 'tag-red',
  OTRO: 'tag-amber',
}

function tagClass(tipo: TipoCuenta) { return TIPO_TAG[tipo] }

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
const movTipo = ref<'DEPOSITO' | 'RETIRO'>('DEPOSITO')
const movMonto = ref(0)
const movDescripcion = ref('')
const movGuardando = ref(false)
const movHistorial = ref<MovimientoCuenta[]>([])

async function abrirMovimientoModal(c: Cuenta) {
  movCuentaId.value = c.id!
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
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }

.resumen-grid { margin-bottom: 1.25rem; }
.resumen-card {
  background: var(--surface); border: 1px solid var(--border); border-radius: 10px;
  padding: 1rem 1.1rem; display: flex; flex-direction: column; gap: 0.2rem; max-width: 280px;
}
.resumen-label { font-size: 0.62rem; color: var(--text-muted); letter-spacing: 0.1em; text-transform: uppercase; }
.resumen-val   { font-size: 1.3rem; font-weight: 700; }
.resumen-count { font-size: 0.68rem; color: var(--text-muted); }

.total-pos { color: var(--accent); }
.total-neg { color: var(--red); }

.cuentas-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 0.85rem;
}
.cuenta-card {
  background: var(--surface); border: 1px solid var(--border); border-radius: 10px;
  padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem;
}
.cuenta-top { display: flex; justify-content: space-between; align-items: center; }
.cuenta-actions { display: flex; gap: 0.2rem; }
.cuenta-nombre { font-size: 0.85rem; font-weight: 600; color: var(--text-primary); }
.cuenta-saldo { font-size: 1.15rem; font-weight: 700; }

.icon-btn {
  background: none; border: none; color: var(--text-muted); cursor: pointer;
  padding: 4px 5px; border-radius: 4px; transition: color 0.15s, background 0.15s;
  display: flex; align-items: center;
}
.icon-btn:hover        { color: var(--accent); background: color-mix(in srgb, var(--accent) 8%, transparent); }
.icon-btn.danger:hover { color: var(--red);   background: color-mix(in srgb, var(--red) 8%, transparent); }

.empty-state { display: flex; flex-direction: column; align-items: center; text-align: center; padding: 3rem 1rem; gap: 0.35rem; }
.empty-state-title { font-size: 0.95rem; font-weight: 600; color: var(--text-primary); margin-top: 0.5rem; }
.empty-state-sub   { font-size: 0.78rem; color: var(--text-muted); max-width: 320px; }

/* Modal */
.modal-backdrop {
  position: fixed; inset: 0; background: rgba(0,0,0,0.5);
  display: flex; align-items: flex-start; justify-content: center; z-index: 100; padding: 3rem 1rem;
}
.modal {
  background: var(--surface); border: 1px solid var(--border); border-radius: 12px;
  padding: 1.5rem; width: 100%; max-width: 400px; display: flex; flex-direction: column; gap: 1rem;
  max-height: calc(100vh - 4rem); overflow-y: auto;
}
.modal-title  { font-size: 1rem; font-weight: 700; margin: 0; }
.modal-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 0.25rem; }

.field-group { display: flex; flex-direction: column; gap: 0.35rem; }
.field-label { font-size: 0.72rem; color: var(--text-secondary); }

.tipo-toggle { display: flex; gap: 0.5rem; }
.tipo-btn {
  flex: 1; padding: 0.45rem; border-radius: 6px; font-size: 0.78rem; cursor: pointer;
  background: var(--surface2); border: 1px solid var(--border); color: var(--text-secondary);
  transition: all 0.15s;
}
.tipo-btn.active { border-color: var(--accent); color: var(--accent); background: color-mix(in srgb, var(--accent) 8%, transparent); }

.mov-historial { display: flex; flex-direction: column; gap: 0.4rem; max-height: 140px; overflow-y: auto; }
.mov-historial-row {
  display: flex; justify-content: space-between; font-size: 0.78rem; color: var(--text-secondary);
  padding: 0.3rem 0; border-bottom: 1px solid var(--border);
}
.mov-historial-row:last-child { border-bottom: none; }

.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 64px; border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 500px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 1.25rem; margin-bottom: 1.25rem; }
  .page-title { font-size: 2rem; }
}
</style>
