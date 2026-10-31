<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Finanzas</div>
        <h1 class="page-title">Deudas</h1>
      </div>
      <button class="btn btn-primary btn-new-deuda" @click="abrirForm()">
        <Plus :size="15" :stroke-width="2.4" />
        Nueva deuda
      </button>
    </div>

    <!-- Resumen: color solo cuando hay algo que destacar; $0 va neutro -->
    <div v-if="!loading" class="resumen-grid">
      <div class="resumen-card" :class="{ 'is-pos': totalMeDeben > 0 }">
        <div class="resumen-top">
          <span class="resumen-label">Me deben</span>
          <span class="resumen-icon resumen-icon--pos"><ArrowDownLeft :size="15" :stroke-width="2" /></span>
        </div>
        <span class="resumen-val">{{ fmt(totalMeDeben) }}</span>
        <span class="resumen-count">{{ pendientesMeDeben }} pendiente{{ pendientesMeDeben !== 1 ? 's' : '' }}</span>
      </div>
      <div class="resumen-card" :class="{ 'is-neg': totalLeDebo > 0 }">
        <div class="resumen-top">
          <span class="resumen-label">Le debo</span>
          <span class="resumen-icon resumen-icon--neg"><ArrowUpRight :size="15" :stroke-width="2" /></span>
        </div>
        <span class="resumen-val">{{ fmt(totalLeDebo) }}</span>
        <span class="resumen-count">{{ pendientesLeDebo }} pendiente{{ pendientesLeDebo !== 1 ? 's' : '' }}</span>
      </div>
      <div class="resumen-card" :class="{ 'is-pos': balance > 0, 'is-neg': balance < 0 }">
        <div class="resumen-top">
          <span class="resumen-label">Balance neto</span>
          <span class="resumen-icon"><Scale :size="15" :stroke-width="2" /></span>
        </div>
        <span class="resumen-val">{{ balance > 0 ? '+' : '' }}{{ fmt(balance) }}</span>
        <span class="resumen-count">{{ balance > 0 ? 'a tu favor' : balance < 0 ? 'en contra' : 'estás a paz y salvo' }}</span>
      </div>
    </div>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <section v-for="s in secciones" :key="s.tipo" class="deuda-section" :style="{ '--sc': s.color }">
        <header class="section-head">
          <div class="section-title">
            <component :is="s.icon" :size="15" :stroke-width="2.2" class="section-icon" />
            <h2>{{ s.titulo }}</h2>
            <span class="section-count">{{ s.lista.length }}</span>
          </div>
          <span v-if="s.pendiente > 0" class="section-total">{{ fmt(s.pendiente) }} pendiente</span>
        </header>

        <!-- Estado vacío -->
        <div v-if="s.lista.length === 0" class="empty-state" :class="{ 'empty-state--celebrate': s.tipo === 'LE_DEBO' }">
          <span class="empty-icon">
            <ShieldCheck v-if="s.tipo === 'LE_DEBO'" :size="26" :stroke-width="1.8" />
            <HandCoins v-else :size="24" :stroke-width="1.8" />
          </span>
          <p class="empty-title">{{ s.tipo === 'LE_DEBO' ? '¡Todo al día! No tienes deudas pendientes' : 'Nadie te debe dinero' }}</p>
          <p class="empty-sub">{{ s.tipo === 'LE_DEBO' ? 'Sigue así, tus cuentas están limpias.' : 'Cuando le prestes a alguien, regístralo aquí para no olvidarlo.' }}</p>
        </div>

        <div
          v-for="d in s.lista"
          :key="d.id"
          class="deuda-row"
          :class="{ pagada: d.pagada }"
        >
          <span class="deuda-avatar">{{ iniciales(d.persona) }}</span>

          <div class="deuda-info">
            <div class="deuda-top">
              <span class="deuda-persona">{{ d.persona }}</span>
              <span v-if="d.pagada" class="chip chip--pos"><Check :size="11" :stroke-width="2.6" /> {{ s.tipo === 'ME_DEBEN' ? 'Cobrada' : 'Pagada' }}</span>
              <span v-else-if="d.fechaLimite" class="chip" :class="{ 'chip--neg': estaVencida(d) }">
                <CalendarDays :size="11" :stroke-width="2.2" />
                {{ fmtFecha(d.fechaLimite!) }}{{ estaVencida(d) ? ' · vencida' : '' }}
              </span>
            </div>
            <span class="deuda-desc">
              {{ d.descripcion }}<template v-if="cuentaNombre(d.cuentaId)"> · {{ cuentaNombre(d.cuentaId) }}</template>
            </span>
            <div class="deuda-progress">
              <div class="deuda-progress-bar">
                <div class="deuda-progress-fill" :style="{ width: Math.max(pctPagado(d), d.montoPagado > 0 ? 2 : 0) + '%' }" />
              </div>
              <span class="deuda-progress-lbl"><strong>{{ pctPagado(d) }}%</strong> pagado · {{ fmt(d.montoPagado) }} de {{ fmt(d.monto) }}</span>
            </div>
          </div>

          <div class="deuda-right">
            <div class="deuda-monto-col">
              <span class="deuda-monto">{{ fmt(d.montoRestante) }}</span>
              <span class="deuda-monto-sub">{{ d.pagada ? 'saldada' : 'restante' }}</span>
            </div>
            <div class="deuda-actions">
              <template v-if="!d.pagada">
                <button class="btn btn-sm btn-ghost" @click="abrirAbonoModal(d)">
                  <Plus :size="13" :stroke-width="2.4" /> Abonar
                </button>
                <button class="icon-btn icon-btn--pos" :title="s.tipo === 'ME_DEBEN' ? 'Marcar como cobrada' : 'Marcar como pagada'" :aria-label="s.tipo === 'ME_DEBEN' ? 'Marcar como cobrada' : 'Marcar como pagada'" @click="togglePagada(d)">
                  <CheckCheck :size="15" :stroke-width="2" />
                </button>
              </template>
              <button v-else class="btn btn-sm btn-ghost" title="Volver a marcarla como pendiente" @click="togglePagada(d)">
                <RotateCcw :size="13" :stroke-width="2.2" /> Reabrir
              </button>
              <button class="icon-btn icon-btn--danger" title="Eliminar" aria-label="Eliminar deuda" @click="eliminar(d)">
                <Trash2 :size="14" :stroke-width="1.9" />
              </button>
            </div>
          </div>
        </div>
      </section>
    </template>

    <!-- Modal -->
    <div v-if="formVisible" class="modal-backdrop" @click.self="cerrarForm">
      <div class="modal">
        <p class="modal-title">Nueva deuda</p>

        <div class="field-group">
          <label class="field-label">Tipo</label>
          <div class="tipo-toggle">
            <button class="tipo-btn" :class="{ active: form.tipo === 'ME_DEBEN' }" @click="form.tipo = 'ME_DEBEN'">Me deben</button>
            <button class="tipo-btn" :class="{ active: form.tipo === 'LE_DEBO' }" @click="form.tipo = 'LE_DEBO'">Le debo</button>
          </div>
        </div>

        <div class="field-group">
          <label class="field-label">Persona</label>
          <input v-model="form.persona" class="input" placeholder="Nombre" />
        </div>
        <div class="field-group">
          <label class="field-label">Descripción</label>
          <input v-model="form.descripcion" class="input" placeholder="¿Por qué?" />
        </div>
        <div class="field-group">
          <label class="field-label">Monto</label>
          <CurrencyInput v-model="form.monto" class="input" />
        </div>
        <div class="field-group">
          <label class="field-label">Fecha límite (opcional)</label>
          <input v-model="form.fechaLimite" type="date" class="input" />
        </div>

        <div v-if="store.cuentas.length > 0" class="field-group">
          <label class="field-label">
            {{ form.tipo === 'ME_DEBEN' ? 'Cuenta de donde salió el préstamo' : 'Cuenta donde entró el préstamo' }}
            <span class="label-optional">(opcional)</span>
          </label>
          <select v-model="form.cuentaId" class="input">
            <option value="">Sin cuenta</option>
            <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
          </select>
          <span v-if="form.cuentaId" class="field-hint">
            {{ form.tipo === 'ME_DEBEN'
              ? 'Se registrará un retiro por este monto en esa cuenta.'
              : 'Se registrará un depósito por este monto en esa cuenta.' }}
          </span>
        </div>

        <div class="modal-actions">
          <button class="btn" @click="cerrarForm">Cancelar</button>
          <button class="btn btn-primary" :disabled="!formValido || guardando" @click="guardar">
            {{ guardando ? '…' : 'Guardar' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal de abono -->
    <div v-if="abonoModalVisible" class="modal-backdrop" @click.self="cerrarAbonoModal">
      <div class="modal">
        <p class="modal-title">Registrar abono</p>

        <div class="field-group">
          <label class="field-label">Monto del abono</label>
          <CurrencyInput v-model="abonoMonto" class="input" />
        </div>

        <div v-if="abonoHistorial.length > 0" class="abono-historial">
          <span class="field-label">Abonos anteriores</span>
          <div v-for="a in abonoHistorial" :key="a.id" class="abono-historial-row">
            <span>{{ fmtFecha(a.fecha) }}</span>
            <span>{{ fmt(a.monto) }}</span>
          </div>
        </div>

        <div class="modal-actions">
          <button class="btn" @click="cerrarAbonoModal">Cancelar</button>
          <button class="btn btn-primary" :disabled="abonoMonto <= 0 || abonoGuardando" @click="guardarAbono">
            {{ abonoGuardando ? '…' : 'Abonar' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import {
  Plus, ArrowDownLeft, ArrowUpRight, Scale, TrendingUp, TrendingDown, ShieldCheck, HandCoins,
  Check, CheckCheck, CalendarDays, RotateCcw, Trash2,
} from '@lucide/vue'
import type { Deuda, Abono } from '@/types'
import { deudasService } from '@/services/api'
import { fmtCOP as fmt } from '@/utils'
import { useToast } from '@/composables/useToast'
import { useFinanceStore } from '@/stores/finance'
import CurrencyInput from '@/components/common/CurrencyInput.vue'

const toast = useToast()
const store = useFinanceStore()

function cuentaNombre(id?: string) {
  if (!id) return ''
  return store.cuentas.find(c => c.id === id)?.nombre ?? ''
}

const deudas  = ref<Deuda[]>([])
const loading = ref(false)
const guardando = ref(false)
const formVisible = ref(false)

const abonoModalVisible = ref(false)
const abonoDeudaId      = ref<string | null>(null)
const abonoMonto        = ref(0)
const abonoGuardando    = ref(false)
const abonoHistorial    = ref<Abono[]>([])
const abonoDeudaTipo =  ref<'ME_DEBEN' | 'LE_DEBO' | null>(null)

const form = reactive({
  tipo: 'ME_DEBEN' as 'ME_DEBEN' | 'LE_DEBO',
  persona: '',
  descripcion: '',
  monto: 0,
  fechaLimite: '',
  cuentaId: '',
})

const formValido = computed(() =>
  form.persona.trim() && form.descripcion.trim() && form.monto > 0
)

const meDeben = computed(() => deudas.value.filter(d => d.tipo === 'ME_DEBEN'))
const leDebo  = computed(() => deudas.value.filter(d => d.tipo === 'LE_DEBO'))

const totalMeDeben = computed(() =>
  meDeben.value.filter(d => !d.pagada).reduce((s, d) => s + d.monto, 0)
)
const totalLeDebo = computed(() =>
  leDebo.value.filter(d => !d.pagada).reduce((s, d) => s + d.monto, 0)
)
const balance = computed(() => totalMeDeben.value - totalLeDebo.value)
const pendientesMeDeben = computed(() => meDeben.value.filter(d => !d.pagada).length)
const pendientesLeDebo  = computed(() => leDebo.value.filter(d => !d.pagada).length)

// Pendientes primero, saldadas al final
const porEstado = (lista: Deuda[]) => [...lista].sort((a, b) => Number(a.pagada) - Number(b.pagada))
const pendienteRestante = (lista: Deuda[]) => lista.filter(d => !d.pagada).reduce((s, d) => s + d.montoRestante, 0)

const secciones = computed(() => [
  { tipo: 'ME_DEBEN' as const, titulo: 'Me deben', icon: TrendingUp,   color: 'var(--c-pos)', lista: porEstado(meDeben.value), pendiente: pendienteRestante(meDeben.value) },
  { tipo: 'LE_DEBO'  as const, titulo: 'Le debo',  icon: TrendingDown, color: 'var(--c-neg)', lista: porEstado(leDebo.value),  pendiente: pendienteRestante(leDebo.value) },
])

function iniciales(nombre: string) {
  return nombre.trim().split(/\s+/).map(w => w[0]).slice(0, 2).join('').toUpperCase()
}

function estaVencida(d: Deuda) {
  return !d.pagada && !!d.fechaLimite && new Date(d.fechaLimite) < new Date()
}

function pctPagado(d: Deuda) {
  if (d.monto <= 0) return 0
  return Math.min(100, Math.round((d.montoPagado / d.monto) * 100))
}

async function abrirAbonoModal(d: Deuda) {
  abonoDeudaTipo.value = d.tipo
  abonoDeudaId.value = d.id!
  abonoMonto.value = 0
  abonoHistorial.value = []
  abonoModalVisible.value = true
  const { data } = await deudasService.getAbonos(d.id!)
  abonoHistorial.value = data
}

function cerrarAbonoModal() {
  abonoModalVisible.value = false
  abonoDeudaId.value = null
}

async function guardarAbono() {
  if (!abonoDeudaId.value || abonoMonto.value <= 0) return
  abonoGuardando.value = true
  try {
    const { data } = await deudasService.crearAbono(abonoDeudaId.value, abonoMonto.value)
    const idx = deudas.value.findIndex(d => d.id === abonoDeudaId.value)
    if (idx !== -1) deudas.value[idx] = data
    await refrescarFinanzas()
    cerrarAbonoModal()
    toast.success( abonoDeudaTipo.value === 'ME_DEBEN' ? 'Abono registrado como ingreso' : 'Abono registrado' )
  } catch {
    toast.error('No se pudo registrar el abono')
  } finally {
    abonoGuardando.value = false
  }
}

function fmtFecha(f: string) {
  return new Date(f).toLocaleDateString('es-CO', { day: 'numeric', month: 'short', year: 'numeric' })
}

async function cargar() {
  loading.value = true
  try {
    const { data } = await deudasService.getAll()
    deudas.value = data
  } finally {
    loading.value = false
  }
}

// Las deudas con cuenta generan movimientos/gastos/ingresos: refrescar cuentas y mes.
async function refrescarFinanzas() {
  await Promise.all([store.cargarCuentas(), store.cargarDatos()])
}

function abrirForm() {
  Object.assign(form, { tipo: 'ME_DEBEN', persona: '', descripcion: '', monto: 0, fechaLimite: '', cuentaId: '' })
  formVisible.value = true
}

function cerrarForm() { formVisible.value = false }

async function guardar() {
  guardando.value = true
  try {
    const payload = {
      tipo: form.tipo,
      persona: form.persona.trim(),
      descripcion: form.descripcion.trim(),
      monto: form.monto,
      ...(form.fechaLimite ? { fechaLimite: form.fechaLimite } : {}),
      ...(form.cuentaId ? { cuentaId: form.cuentaId } : {}),
    }
    const { data } = await deudasService.create(payload)
    deudas.value.unshift(data)
    if (form.cuentaId) await store.cargarCuentas()
    cerrarForm()
    toast.success('Deuda registrada')
  } catch {
    toast.error('No se pudo guardar')
  } finally {
    guardando.value = false
  }
}

async function togglePagada(d: Deuda) {
  try {
    const { data } = await deudasService.togglePagada(d.id!)
    const idx = deudas.value.findIndex(x => x.id === d.id)
    if (idx !== -1) deudas.value[idx] = data
    await refrescarFinanzas()
  } catch {
    toast.error('No se pudo actualizar')
  }
}

async function eliminar(d: Deuda) {
  try {
    await deudasService.delete(d.id!)
    deudas.value = deudas.value.filter(x => x.id !== d.id)
    if (d.cuentaId) await refrescarFinanzas()
    toast.success('Deuda eliminada')
  } catch {
    toast.error('No se pudo eliminar')
  }
}

onMounted(() => {
  cargar()
  if (store.cuentas.length === 0) store.cargarCuentas()
})
</script>

<style scoped>
/* ── Resumen ── */
.resumen-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.85rem;
  margin-bottom: 1.5rem;
}
.resumen-card {
  --rc: var(--text-primary);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  padding: 1.15rem 1.25rem 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  box-shadow: var(--shadow-card);
}
.resumen-card.is-pos { --rc: var(--c-pos); }
.resumen-card.is-neg { --rc: var(--c-neg); }
.resumen-top   { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.4rem; }
.resumen-label { font-size: 0.66rem; font-weight: 500; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.resumen-icon {
  --ic: var(--text-secondary);
  display: grid; place-items: center;
  width: 30px; height: 30px; border-radius: 9px;
  color: var(--ic);
  background: color-mix(in srgb, var(--ic) 13%, transparent);
}
.resumen-icon--pos { --ic: var(--c-pos); }
.resumen-icon--neg { --ic: var(--c-neg); }
.resumen-val {
  font-family: var(--ff-mono); font-variant-numeric: tabular-nums;
  font-size: 1.45rem; font-weight: 700; letter-spacing: -0.02em;
  color: var(--rc);
}
.resumen-count { font-size: 0.7rem; color: var(--text-muted); }

/* ── Secciones: una sola tarjeta por lista ── */
.deuda-section {
  margin-bottom: 1rem;
  padding: 0.35rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  box-shadow: var(--shadow-card);
}
.section-head {
  display: flex; justify-content: space-between; align-items: center; gap: 1rem;
  padding: 0.85rem 0.9rem 0.7rem;
}
.section-title { display: flex; align-items: center; gap: 0.5rem; }
.section-title h2 {
  font-family: var(--ff-body);
  font-size: 0.76rem; font-weight: 500; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--text-primary);
}
.section-icon { color: var(--sc); }
.section-count {
  font-size: 0.64rem; font-weight: 500; color: var(--text-secondary);
  padding: 0.1rem 0.5rem; border-radius: 999px; background: var(--surface2);
  font-variant-numeric: tabular-nums;
}
.section-total {
  font-family: var(--ff-mono); font-variant-numeric: tabular-nums;
  font-size: 0.74rem; font-weight: 700;
  padding: 0.25rem 0.65rem; border-radius: 999px;
  color: var(--sc); background: color-mix(in srgb, var(--sc) 12%, transparent);
}

/* ── Filas ── */
.deuda-row {
  position: relative;
  display: flex;
  align-items: center;
  gap: 0.9rem;
  padding: 0.95rem 0.9rem;
  border-radius: 12px;
  transition: background 0.12s ease, opacity 0.2s ease;
}
.deuda-row + .deuda-row::before,
.section-head + .deuda-row::before {
  content: '';
  position: absolute; top: 0; left: 0.9rem; right: 0.9rem;
  height: 1px; background: var(--border);
}
.deuda-row:hover,
.deuda-row:focus-within { background: var(--surface2); }
.deuda-row:hover::before,
.deuda-row:hover + .deuda-row::before { background: transparent; }
.deuda-row.pagada { opacity: 0.55; }
.deuda-row.pagada:hover { opacity: 0.9; }

.deuda-avatar {
  display: grid; place-items: center; flex-shrink: 0; align-self: flex-start;
  width: 40px; height: 40px; border-radius: 999px;
  font-size: 0.74rem; font-weight: 700; letter-spacing: 0.02em;
  color: var(--sc);
  background: color-mix(in srgb, var(--sc) 14%, transparent);
}

.deuda-info    { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 0.25rem; }
.deuda-top     { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
.deuda-persona { font-size: 0.95rem; font-weight: 700; color: var(--text-primary); }
.deuda-desc    {
  font-size: 0.74rem; color: var(--text-subtle);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}

.chip {
  display: inline-flex; align-items: center; gap: 0.3rem;
  font-size: 0.62rem; font-weight: 500; color: var(--text-subtle);
  padding: 0.15rem 0.5rem; border-radius: 999px; background: var(--surface2);
}
.deuda-row:hover .chip:not(.chip--pos):not(.chip--neg) { background: var(--surface); }
.chip--pos { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 13%, transparent); }
.chip--neg { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 13%, transparent); }

.deuda-progress { display: flex; align-items: center; gap: 0.7rem; margin-top: 0.45rem; }
.deuda-progress-bar {
  flex: 1; max-width: 260px; height: 7px; border-radius: 99px; overflow: hidden;
  background: color-mix(in srgb, var(--text-muted) 22%, var(--surface2));
}
.deuda-progress-fill { height: 100%; border-radius: 99px; background: var(--sc); transition: width 0.4s ease; }
.deuda-progress-lbl { font-size: 0.66rem; color: var(--text-muted); white-space: nowrap; font-variant-numeric: tabular-nums; }
.deuda-progress-lbl strong { color: var(--text-secondary); font-weight: 500; }

.deuda-right { display: flex; align-items: center; gap: 0.75rem; flex-shrink: 0; }
.deuda-monto-col { display: flex; flex-direction: column; align-items: flex-end; gap: 0.1rem; }
.deuda-monto {
  font-family: var(--ff-mono); font-variant-numeric: tabular-nums;
  font-size: 1rem; font-weight: 700; color: var(--text-primary);
}
.deuda-monto-sub { font-size: 0.64rem; color: var(--text-muted); }

/* Acciones: ocultas hasta hover/foco (en táctil siempre visibles) */
.deuda-actions {
  display: flex; align-items: center; gap: 0.15rem;
  opacity: 0; transform: translateX(4px);
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.deuda-row:hover .deuda-actions,
.deuda-row:focus-within .deuda-actions { opacity: 1; transform: none; }
@media (hover: none) { .deuda-actions { opacity: 1; transform: none; } }

.btn-ghost { background: transparent; border-color: transparent; color: var(--text-secondary); }
.btn-ghost:hover { background: var(--surface); border-color: var(--border); color: var(--text-primary); }

/* Estado vacío */
.empty-state {
  display: flex; flex-direction: column; align-items: center; text-align: center; gap: 0.35rem;
  padding: 1.5rem 1rem 1.75rem;
}
.empty-icon {
  display: grid; place-items: center;
  width: 52px; height: 52px; margin-bottom: 0.4rem; border-radius: 999px;
  color: var(--text-secondary); background: var(--surface2);
}
.empty-state--celebrate .empty-icon {
  color: var(--c-pos);
  background: color-mix(in srgb, var(--c-pos) 12%, transparent);
  box-shadow: 0 0 0 6px color-mix(in srgb, var(--c-pos) 6%, transparent);
}
.empty-title { font-size: 0.86rem; font-weight: 500; color: var(--text-primary); }
.empty-sub   { font-size: 0.72rem; color: var(--text-muted); max-width: 340px; }

.abono-historial { display: flex; flex-direction: column; gap: 0.4rem; max-height: 140px; overflow-y: auto; }
.abono-historial-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.78rem;
  color: var(--text-secondary);
  padding: 0.3rem 0;
  border-bottom: 1px solid var(--border);
}
.abono-historial-row:last-child { border-bottom: none; }

.icon-btn {
  display: grid; place-items: center;
  width: 32px; height: 32px; padding: 0;
  background: none; border: none; border-radius: 8px;
  color: var(--text-secondary); cursor: pointer;
  transition: color 0.15s, background 0.15s;
}
.icon-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }
.icon-btn--pos:hover    { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 12%, transparent); }
.icon-btn--danger:hover { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 12%, transparent); }

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
.label-optional { color: var(--text-muted); font-weight: 400; }
.field-hint { font-size: 0.65rem; color: var(--text-muted); }

.tipo-toggle { display: flex; gap: 0.5rem; }
.tipo-btn {
  flex: 1; padding: 0.45rem; border-radius: 6px; font-size: 0.78rem; cursor: pointer;
  background: var(--surface2); border: 1px solid var(--border); color: var(--text-secondary);
  transition: all 0.15s;
}
.tipo-btn.active { border-color: var(--accent); color: var(--accent-strong); background: color-mix(in srgb, var(--accent) 12%, transparent); }

.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 64px; border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }

@media (max-width: 768px) {
  .resumen-grid { grid-template-columns: 1fr 1fr; }
  .resumen-grid > :last-child { grid-column: 1 / -1; }
}

@media (max-width: 560px) {
  .resumen-grid { grid-template-columns: 1fr; }
  .btn-new-deuda { width: 100%; }
  .deuda-row { flex-wrap: wrap; }
  .deuda-info { flex-basis: calc(100% - 40px - 0.9rem); }
  .deuda-right { width: 100%; justify-content: space-between; padding-left: calc(40px + 0.9rem); }
  .deuda-monto-col { align-items: flex-start; }
  .deuda-desc { white-space: normal; }
}
</style>
