<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Planificación</div>
        <h1 class="page-title">Metas de ahorro</h1>
      </div>
      <button v-if="!showForm" class="btn btn-primary btn-new-meta" @click="showForm = !showForm">
        <svg  width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
       Nueva meta
      </button>
    </div>

    <!-- Formulario nueva meta -->
    <Transition name="form-slide">
      <div v-if="showForm" class="card form-card">
        <div class="meta-form">
          <div class="form-row">
            <div class="field" style="flex: 2">
              <div class="label-row">
                <label>Nombre de la meta</label>
                <span class="char-counter" :class="{ 'char-counter--limit': form.nombre.length >= 150 }">{{ form.nombre.length }}/150</span>
              </div>
              <input v-model="form.nombre" class="input" maxlength="150" :class="{ 'input-error': errors.nombre }" placeholder="ej. Vacaciones en Europa" />
              <span v-if="errors.nombre" class="field-error">{{ errors.nombre }}</span>
            </div>
            <div class="field">
              <label>Monto objetivo (COP)</label>
              <CurrencyInput v-model="form.montoObjetivo" :class="{ 'input-error': errors.montoObjetivo }" />
              <span v-if="errors.montoObjetivo" class="field-error">{{ errors.montoObjetivo }}</span>
            </div>
          </div>
          <div class="form-row">
            <div class="field field-fecha">
              <label>Fecha límite (opcional)</label>
              <input v-model="form.fechaLimite" type="date" class="input" />
            </div>
            <div v-if="store.cuentas.length > 0" class="field">
              <label>Cuenta donde guardas este dinero (opcional)</label>
              <select v-model="form.cuentaId" class="input">
                <option value="">Sin cuenta</option>
                <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
              </select>
            </div>
          </div>
          <div class="form-actions">
            <button class="btn" @click="showForm = false">Cancelar</button>
            <button class="btn btn-primary" :disabled="saving" @click="crearMeta">{{ saving ? 'Guardando...' : 'Crear meta' }}</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Modal gestionar saldo -->
    <Teleport to="body">
      <div v-if="abonarMeta" class="overlay" @click.self="abonarMeta = null">
        <div class="modal fade-up modal-saldo">
          <p class="modal-title">Gestionar saldo</p>
          <p class="modal-sub">{{ abonarMeta.nombre }}</p>

          <div class="saldo-card">
            <div class="saldo-card-row">
              <span class="saldo-label">Saldo actual</span>
              <span class="saldo-value">{{ fmt(abonarMeta.montoActual) }}</span>
            </div>
            <div class="meta-progress-bar-wrap">
              <div class="meta-progress-bar" :style="{ width: pctMeta(abonarMeta) + '%' }" />
            </div>
            <div class="saldo-card-row saldo-card-row--sub">
              <span>{{ pctMeta(abonarMeta) }}% de la meta</span>
              <span>Objetivo: {{ fmt(abonarMeta.montoObjetivo) }}</span>
            </div>
          </div>

          <div class="field" style="margin-top: 1rem">
            <label>Monto (COP)</label>
            <CurrencyInput v-model="montoAbono" />
          </div>

          <div class="field" style="margin-top: .75rem">
            <label>Tipo de movimiento</label>
            <div class="tipo-toggle">
              <button
                type="button"
                class="tipo-btn tipo-btn--abonar"
                :class="{ active: tipoMovimiento === 'abonar' }"
                @click="tipoMovimiento = 'abonar'"
              >
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                Abonar
              </button>
              <button
                type="button"
                class="tipo-btn tipo-btn--retirar"
                :class="{ active: tipoMovimiento === 'retirar' }"
                @click="tipoMovimiento = 'retirar'"
              >
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="5" y1="12" x2="19" y2="12"/></svg>
                Retirar
              </button>
            </div>
          </div>

          <!-- Abono: de qué cuenta sale -->
          <div v-if="tipoMovimiento === 'abonar' && store.cuentas.length > 0" class="field" style="margin-top: .75rem">
            <label>¿De qué cuenta sale? (opcional)</label>
            <select v-model="abonoCuentaOrigen" class="input">
              <option value="">Ya está en la cuenta del ahorro</option>
              <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
            </select>
          </div>

          <!-- Retiro: qué se hizo con la plata -->
          <template v-if="tipoMovimiento === 'retirar'">
            <div class="field" style="margin-top: .75rem">
              <label>¿Qué hiciste con esta plata?</label>
              <select v-model="retiroTipo" class="input">
                <option value="GASTO">La gasté (registrar como gasto)</option>
                <option value="TRANSFERENCIA">La pasé a otra cuenta</option>
                <option value="SIMPLE">Solo ajustar el ahorro</option>
              </select>
            </div>

            <div v-if="retiroTipo === 'GASTO'" class="field" style="margin-top: .6rem">
              <label>Categoría del gasto</label>
              <select v-model="retiroCategoria" class="input">
                <option value="" disabled>Seleccionar...</option>
                <option v-for="c in store.categoriasActivas" :key="c.id" :value="c.nombre">{{ c.nombre }}</option>
              </select>
            </div>
            <div v-if="retiroTipo === 'GASTO'" class="field" style="margin-top: .6rem">
              <label>Detalle (opcional)</label>
              <input v-model="retiroDetalle" class="input" maxlength="150" placeholder="ej. Mercado, arreglo del carro" />
            </div>

            <div v-if="retiroTipo === 'TRANSFERENCIA'" class="field" style="margin-top: .6rem">
              <label>Cuenta destino</label>
              <select v-model="retiroCuentaDestino" class="input">
                <option value="" disabled>Seleccionar...</option>
                <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
              </select>
            </div>
          </template>

          <div style="margin-bottom: 1.6rem" />

          <div v-if="montoAbono > 0" class="saldo-preview" :class="tipoMovimiento === 'retirar' ? 'saldo-preview--retirar' : 'saldo-preview--abonar'">
            <span>Nuevo saldo</span>
            <strong>{{ fmt(nuevoSaldoPreview) }}</strong>
          </div>
          <p v-if="excedeSaldo" class="field-error">No puedes retirar más dinero del disponible</p>

          <div class="modal-actions">
            <button class="btn" @click="abonarMeta = null">Cancelar</button>
            <button class="btn btn-primary" :disabled="saving || montoAbono <= 0 || excedeSaldo" @click="confirmarMovimiento">{{ saving ? '...' : 'Guardar' }}</button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Modal editar -->
    <Teleport to="body">
      <div v-if="editandoMeta" class="overlay" @click.self="editandoMeta = null">
        <div class="modal fade-up">
          <p class="modal-title">Editar meta</p>
          <div class="field" style="margin-top: 1rem">
            <div class="label-row">
              <label>Nombre</label>
              <span class="char-counter" :class="{ 'char-counter--limit': editForm.nombre.length >= 150 }">{{ editForm.nombre.length }}/150</span>
            </div>
            <input v-model="editForm.nombre" class="input" maxlength="150" />
          </div>
          <div class="field" style="margin-top: 0.75rem">
            <label>Monto objetivo (COP)</label>
            <CurrencyInput v-model="editForm.montoObjetivo" />
          </div>
          <div class="field" style="margin-top: 0.75rem">
            <label>Fecha límite (opcional)</label>
            <input v-model="editForm.fechaLimite" type="date" class="input" />
          </div>
          <div v-if="store.cuentas.length > 0" class="field" style="margin-top: 0.75rem">
            <label>Cuenta donde guardas este dinero (opcional)</label>
            <select v-model="editForm.cuentaId" class="input">
              <option value="">Sin cuenta</option>
              <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
            </select>
          </div>
          <div class="modal-actions" style="margin-top: 1.25rem">
            <button class="btn" @click="editandoMeta = null">Cancelar</button>
            <button class="btn btn-primary" :disabled="saving" @click="confirmarEdicion">{{ saving ? '...' : 'Guardar' }}</button>
          </div>
        </div>
      </div>
    </Teleport>

     <!-- Modal para ver el historial -->
     <Teleport to="body">
      <div v-if="verHistorialMeta" class="overlay" @click.self="verHistorialMeta = null">
        <div class="modal fade-up">
          <p class="modal-title">Historial de la meta {{ verHistorialMeta?.nombre }}</p>
            <p v-if="historial.length === 0" class="modal-sub">Sin movimientos todavía</p>
            <div v-for="item in historial" :key="item.id ?? item.fecha" class="historial-item">
              <span>{{ formatFecha(item.fecha) }}</span>
              <span>
                <span class="historial-tag" :class="item.monto >= 0 ? 'historial-tag--in' : 'historial-tag--out'">
                  {{ item.monto >= 0 ? 'Abono' : 'Retiro' }}
                </span>
                <span v-if="item.descripcion" class="historial-desc">· {{ item.descripcion }}</span>
              </span>
              <span>{{ fmt(Math.abs(item.monto)) }}</span>
          </div>

          <div class="modal-actions" style="margin-top: 1.25rem">
            <button class="btn" @click="verHistorialMeta = null">Cerrar</button>
          </div>
        </div>
      </div>
    </Teleport>

    <div v-if="loadingInit" class="loading-list">
      <div v-for="i in 2" :key="i" class="skeleton-row" style="height: 80px" />
    </div>

    <template v-else-if="store.metas.length > 0">
      <div class="metas-grid">
        <div v-for="meta in store.metas" :key="meta.id" class="meta-card" :class="{ 'meta-completada': meta.completada }">
          <div style="display: flex; align-items: center;" class="meta-header">
            <span  class="meta-nombre">{{ meta.nombre }}</span>
            <div style="display: flex; gap: 0.35rem; align-items: center;">
              <span v-if="meta.completada" class="badge-completada">✓ Lograda</span>
              <button v-if="isDesktop" class="btn btn-sm" @click="verHistorial(meta)">Ver historial</button>
              <button  v-if="isDesktop" class="btn btn-sm" @click="iniciarAbono(meta)">Gestionar saldo</button>
              <button  class="icon-btn" title="Editar" @click="iniciarEdicion(meta)">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button class="icon-btn danger" @click="borrarMeta(meta.id!)">
                <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
          </div>

          <div class="meta-progress-bar-wrap">
            <div class="meta-progress-bar" :style="{ width: pctMeta(meta) + '%' }" />
          </div>

          <div class="meta-footer">
            <span class="meta-actual">{{ fmt(meta.montoActual) }}</span>
            <span class="meta-pct">{{ pctMeta(meta) }}%</span>
            <span class="meta-objetivo">{{ fmt(meta.montoObjetivo) }}</span>
          </div>
          <div v-if="meta.fechaLimite" class="meta-fecha">
            <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
            {{ formatFecha(meta.fechaLimite) }}
          </div>
          <div v-if="cuentaNombre(meta.cuentaId)" class="meta-fecha">
            <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="6" width="20" height="12" rx="2"/><circle cx="12" cy="12" r="2"/></svg>
            {{ cuentaNombre(meta.cuentaId) }}
          </div>
          <template v-if="!meta.completada">
            <div
              v-if="calcularProyeccionMeta(meta).urgencia === 'danger' || calcularProyeccionMeta(meta).urgencia === 'warn'"
              class="meta-urgencia"
              :class="calcularProyeccionMeta(meta).urgencia!"
            >
              <svg width="9" height="9" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
              </svg>
              <span>
                {{ calcularProyeccionMeta(meta).diasRestantes! < 0 ? 'Vencida' : `${calcularProyeccionMeta(meta).diasRestantes} días` }}
              </span>
            </div>
            <div v-if="mesesParaMeta(meta) !== null" class="meta-ritmo">
              <svg width="9" height="9" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
              Al ritmo actual (~{{ fmt(promedioAhorro) }}/mes), en ~{{ mesesParaMeta(meta) }} mes{{ mesesParaMeta(meta) !== 1 ? 'es' : '' }}
            </div>
            <div v-else-if="calcularProyeccionMeta(meta).fechaEstimada" class="meta-estimada">
              ~{{ fmtFechaEstimada(calcularProyeccionMeta(meta).fechaEstimada!) }}
            </div>
          </template>

          <button v-if="isMobile" style="margin-top: 0.75rem;" class="btn btn-sm" @click="verHistorial(meta)">Ver historial</button>
          <button  v-if="isMobile" class="btn btn-sm" @click="iniciarAbono(meta)">Gestionar saldo</button>
        </div>
      </div>
    </template>

    <div v-else-if="!showForm" class="empty-state">
      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.3" style="color: var(--text-muted)"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
      <p class="empty-state-title">Sin metas de ahorro</p>
      <p class="empty-state-sub">Crea tu primera meta para empezar a ahorrar con un objetivo claro</p>
      <button class="btn btn-primary" style="margin-top: 0.75rem" @click="showForm = true">
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Nueva meta
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { fmtCOP as fmt } from '@/utils'
import { Teleport } from 'vue'
import { useDeviceType } from '@/composables/useDeviceType'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import type { Meta, MovimientoMeta, TipoRetiroMeta } from '@/types'
import { calcularProyeccionMeta } from '@/composables/useMetaProjection'


const { isMobile, isTablet, isDesktop } = useDeviceType()
const store = useFinanceStore()
const toast = useToast()

const promedioAhorro = computed(() => {
  const positivos = store.tendencia.filter(r => r.ahorro > 0).slice(-3)
  if (positivos.length === 0) return 0
  return positivos.reduce((s, r) => s + r.ahorro, 0) / positivos.length
})

function mesesParaMeta(meta: Meta): number | null {
  if (promedioAhorro.value <= 0 || meta.completada) return null
  const falta = meta.montoObjetivo - meta.montoActual
  if (falta <= 0) return null
  return Math.ceil(falta / promedioAhorro.value)
}

const loadingInit  = ref(false)
const showForm     = ref(false)
const saving       = ref(false)
const abonarMeta   = ref<Meta | null>(null)
const montoAbono   = ref(0)
const editandoMeta = ref<Meta | null>(null)
const verHistorialMeta  = ref<Meta | null>(null)
const tipoMovimiento = ref<'abonar' | 'retirar'>('abonar')
const editForm     = reactive({ nombre: '', montoObjetivo: 0, fechaLimite: '', cuentaId: '' })
const historial = ref<MovimientoMeta[]>([])

// Detalle del movimiento cuando toca cuentas
const abonoCuentaOrigen   = ref('')
const retiroTipo          = ref<TipoRetiroMeta>('GASTO')
const retiroCategoria     = ref('')
const retiroDetalle       = ref('')
const retiroCuentaDestino = ref('')

const form = reactive({ nombre: '', montoObjetivo: 0, fechaLimite: '', cuentaId: '' })
const errors = ref<Partial<Record<'nombre' | 'montoObjetivo', string>>>({})

function cuentaNombre(id?: string) {
  if (!id) return ''
  return store.cuentas.find(c => c.id === id)?.nombre ?? ''
}

function pctMeta(meta: Meta) {
  if (meta.montoObjetivo === 0) return 0
  return Math.min(Math.round(meta.montoActual / meta.montoObjetivo * 100), 100)
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-CO', { day: 'numeric', month: 'short', year: 'numeric' })
}

function fmtFechaEstimada(fecha: Date): string {
  return fecha.toLocaleDateString('es-CO', { month: 'short', year: 'numeric' })
}

function iniciarAbono(meta: Meta) {
  abonarMeta.value = meta
  montoAbono.value = 0
  tipoMovimiento.value = 'abonar'
  abonoCuentaOrigen.value = ''
  retiroTipo.value = meta.cuentaId ? 'GASTO' : 'SIMPLE'
  retiroCategoria.value = ''
  retiroDetalle.value = ''
  retiroCuentaDestino.value = ''
}

const nuevoSaldoPreview = computed(() => {
  if (!abonarMeta.value) return 0
  return tipoMovimiento.value === 'retirar'
    ? abonarMeta.value.montoActual - montoAbono.value
    : abonarMeta.value.montoActual + montoAbono.value
})

const excedeSaldo = computed(() =>
  tipoMovimiento.value === 'retirar' && !!abonarMeta.value && montoAbono.value > abonarMeta.value.montoActual
)

function iniciarEdicion(meta: Meta) {
  editandoMeta.value        = meta
  editForm.nombre           = meta.nombre
  editForm.montoObjetivo    = meta.montoObjetivo
  editForm.fechaLimite      = meta.fechaLimite ? meta.fechaLimite.substring(0, 10) : ''
  editForm.cuentaId         = meta.cuentaId ?? ''
}

async function confirmarEdicion() {
  if (!editandoMeta.value || !editForm.nombre.trim() || editForm.montoObjetivo <= 0) return
  saving.value = true
  try {
    await store.editarMeta(editandoMeta.value.id!, {
      nombre:        editForm.nombre.trim(),
      montoObjetivo: editForm.montoObjetivo,
      fechaLimite:   editForm.fechaLimite || undefined,
      cuentaId:      editForm.cuentaId || undefined
    })
    toast.success('Meta actualizada')
    editandoMeta.value = null
  } catch {
    toast.error('No se pudo actualizar la meta')
  } finally {
    saving.value = false
  }
}

async function crearMeta() {
  errors.value = {}
  if (!form.nombre.trim()) { errors.value.nombre = 'Ingresa un nombre'; return }
  if (form.montoObjetivo <= 0) { errors.value.montoObjetivo = 'Ingresa un monto mayor a 0'; return }

  saving.value = true
  try {
    await store.crearMeta({
      nombre: form.nombre.trim(),
      montoObjetivo: form.montoObjetivo,
      fechaLimite: form.fechaLimite || undefined,
      cuentaId: form.cuentaId || undefined
    })
    toast.success('Meta creada')
    showForm.value = false
    form.nombre = ''
    form.montoObjetivo = 0
    form.fechaLimite = ''
    form.cuentaId = ''
  } catch {
    toast.error('No se pudo crear la meta')
  } finally {
    saving.value = false
  }
}

async function verHistorial(meta: Meta) {
  verHistorialMeta.value = meta
  try {
    historial.value = await store.cargarHistorialMeta(meta.id!)
  } catch {
    toast.error('No se pudo cargar el historial')
    verHistorialMeta.value = null
  }
}

async function confirmarMovimiento() {
  if (!abonarMeta.value || montoAbono.value <= 0 || excedeSaldo.value) return

  const esRetiro = tipoMovimiento.value === 'retirar'

  if (esRetiro && retiroTipo.value === 'GASTO' && !retiroCategoria.value) {
    toast.error('Elige la categoría del gasto')
    return
  }
  if (esRetiro && retiroTipo.value === 'TRANSFERENCIA' && !retiroCuentaDestino.value) {
    toast.error('Elige la cuenta destino')
    return
  }

  const payload = {
    monto: esRetiro ? -montoAbono.value : montoAbono.value,
    ...(esRetiro
      ? {
          tipoRetiro: retiroTipo.value,
          categoria: retiroTipo.value === 'GASTO' ? retiroCategoria.value : undefined,
          detalle: retiroTipo.value === 'GASTO' ? retiroDetalle.value.trim() || undefined : undefined,
          cuentaDestinoId: retiroTipo.value === 'TRANSFERENCIA' ? retiroCuentaDestino.value : undefined,
        }
      : { cuentaOrigenId: abonoCuentaOrigen.value || undefined }),
  }

  saving.value = true
  try {
    await store.registrarMovimientoMeta(abonarMeta.value.id!, payload)
    toast.success('Movimiento registrado')
    abonarMeta.value = null
    montoAbono.value = 0
  } catch (err: any) {
    toast.error(err?.response?.data?.message || 'No se pudo registrar el movimiento')
  } finally {
    saving.value = false
  }
}

async function borrarMeta(id: string) {
  try {
    await store.eliminarMeta(id)
    toast.success('Meta eliminada')
  } catch {
    toast.error('No se pudo eliminar la meta')
  }
}

onMounted(async () => {
  loadingInit.value = true
  if (store.tendencia.length === 0) store.cargarTendencia()
  if (store.cuentas.length === 0) store.cargarCuentas()
  if (store.categorias.length === 0) store.cargarCategorias()
  try { await store.cargarMetas() }
  finally { loadingInit.value = false }
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.form-card   { margin-bottom: 1rem; }

.meta-form   { display: flex; flex-direction: column; gap: 0.75rem; }
.form-row    { display: flex; gap: 0.75rem; }
.field       { display: flex; flex-direction: column; flex: 1; }
.field-fecha { max-width: 220px; }
.form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; }
.field-error { font-size: 0.65rem; color: var(--red); margin-top: 0.2rem; }
.input-error { border-color: var(--red) !important; }

.label-row { display: flex; justify-content: space-between; align-items: baseline; }
.char-counter { font-size: 0.62rem; color: var(--text-muted); }
.char-counter--limit { color: var(--red); }

.form-slide-enter-active { transition: opacity 0.22s ease, transform 0.22s ease; }
.form-slide-leave-active { transition: opacity 0.18s ease, transform 0.18s ease; }
.form-slide-enter-from   { opacity: 0; transform: translateY(-8px); }
.form-slide-leave-to     { opacity: 0; transform: translateY(-8px); }

.metas-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(500px, 1fr)); gap: 0.75rem; }

.meta-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.1rem;
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}
.meta-card.meta-completada { border-color: color-mix(in srgb, var(--accent) 30%, transparent); }

.meta-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 0.5rem; }
.meta-nombre { font-size: 0.82rem; font-weight: 600; color: var(--text-primary); line-height: 1.3; }

.badge-completada { font-size: 0.62rem; background: color-mix(in srgb, var(--accent) 12%, transparent); color: var(--accent); padding: 0.2rem 0.5rem; border-radius: 4px; white-space: nowrap; }

.meta-progress-bar-wrap { height: 5px; border-radius: 3px; background: var(--surface2); overflow: hidden; }
.meta-progress-bar { height: 100%; background: var(--accent); border-radius: 3px; transition: width 0.5s ease; }

.meta-footer { display: flex; justify-content: space-between; align-items: center; }
.meta-actual  { font-size: 0.72rem; color: var(--text-secondary); }
.meta-pct     { font-size: 0.65rem; color: var(--text-muted); }
.meta-objetivo { font-size: 0.72rem; color: var(--text-muted); }

.meta-fecha { display: flex; align-items: center; gap: 0.3rem; font-size: 0.65rem; color: var(--text-muted); }

.btn-sm { padding: 0.3rem 0.7rem; font-size: 0.72rem; }
.icon-btn {
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px 5px;
  border-radius: 4px;
  transition: color 0.15s, background 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.icon-btn:hover        { color: var(--accent); background: color-mix(in srgb, var(--accent) 8%, transparent); }
.icon-btn.danger:hover { color: var(--red);   background: color-mix(in srgb, var(--red) 8%, transparent); }

/* Empty state */
.empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 4rem 0 3rem; text-align: center; gap: 0.4rem; }
.empty-state-title { font-size: 0.85rem; color: var(--text-secondary); font-weight: 600; margin-top: 0.5rem; }
.empty-state-sub   { font-size: 0.72rem; color: var(--text-muted); max-width: 280px; }

/* Skeleton */
.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row { border-radius: 8px; background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
@keyframes shimmer { to { background-position: -200% 0; } }

/* Modal abonar */
.overlay { position: fixed; inset: 0; background: rgba(8,12,16,0.75); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 500; }
.modal { background: var(--surface); border: 1px solid var(--border); border-radius: 12px; padding: 1.5rem; width: 400px; }
.modal-title { font-family: var(--ff-display); font-weight: 700; font-size: 1rem; color: var(--text-primary); margin-bottom: 0.25rem; }
.modal-sub   { font-size: 0.72rem; color: var(--text-muted); margin-bottom: 0.5rem; }
.modal-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 0.5rem; }

.meta-urgencia {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.62rem;
  font-weight: 600;
  padding: 0.15rem 0.4rem;
  border-radius: 4px;
  width: fit-content;
}
.meta-urgencia.warn   { background: color-mix(in srgb, var(--amber) 10%, transparent); color: var(--amber); }
.meta-urgencia.danger { background: color-mix(in srgb, var(--red) 10%, transparent); color: var(--red); }

.meta-estimada {
  font-size: 0.62rem;
  color: var(--text-muted);
}

.meta-ritmo {
  display: flex; align-items: center; gap: 0.3rem;
  font-size: 0.62rem; color: var(--accent);
}

.modal-saldo { width: 380px; }

.saldo-card {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 0.85rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
}
.saldo-card-row { display: flex; justify-content: space-between; align-items: baseline; }
.saldo-card-row--sub { font-size: 0.65rem; color: var(--text-muted); }
.saldo-label { font-size: 0.65rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.06em; }
.saldo-value { font-family: var(--ff-display); font-size: 1.35rem; color: var(--text-primary); }

.tipo-toggle { display: flex; gap: 0.5rem; margin-top: 0.4rem; }
.tipo-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  padding: 0.5rem;
  border-radius: 8px;
  font-size: 0.78rem;
  font-weight: 600;
  cursor: pointer;
  background: var(--surface2);
  border: 1px solid var(--border);
  color: var(--text-secondary);
  transition: border-color 0.15s ease, color 0.15s ease, background 0.15s ease;
}
.tipo-btn:hover { border-color: var(--border-hover); }
.tipo-btn--abonar.active  { border-color: var(--green); color: var(--green); background: color-mix(in srgb, var(--green) 10%, transparent); }
.tipo-btn--retirar.active { border-color: var(--red);   color: var(--red);   background: color-mix(in srgb, var(--red) 10%, transparent); }

.saldo-preview {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 0.9rem;
  padding: 0.55rem 0.75rem;
  border-radius: 8px;
  font-size: 0.75rem;
  color: var(--text-secondary);
}
.saldo-preview strong { font-family: var(--ff-display); font-size: 0.95rem; }
.saldo-preview--abonar  { background: color-mix(in srgb, var(--green) 8%, transparent); border: 1px solid color-mix(in srgb, var(--green) 25%, transparent); }
.saldo-preview--abonar strong  { color: var(--green); }
.saldo-preview--retirar { background: color-mix(in srgb, var(--red) 8%, transparent); border: 1px solid color-mix(in srgb, var(--red) 25%, transparent); }
.saldo-preview--retirar strong { color: var(--red); }

.historial-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0;
  font-size: 0.75rem;
  color: var(--text-secondary);
  border-bottom: 1px solid var(--border);
}
.historial-item:last-child { border-bottom: none; }
.historial-desc { color: var(--text-muted); font-size: 0.7rem; }
.historial-tag {
  display: inline-block; font-size: 0.6rem; font-weight: 600;
  padding: 0.05rem 0.35rem; border-radius: 4px;
}
.historial-tag--in  { background: color-mix(in srgb, var(--green) 12%, transparent); color: var(--green); }
.historial-tag--out { background: color-mix(in srgb, var(--red) 12%, transparent); color: var(--red); }

@media (max-width: 500px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 1.25rem; margin-bottom: 1.25rem; }
  .page-title { font-size: 2rem;  }
  .section-label { font-size: 0.75rem; }
  .btn-new-meta { width: 100%; justify-content: center; }

  .form-row    { flex-direction: column; }
  .field       { margin-bottom: 0.74rem; }
  .field-fecha { max-width: none; }


  .metas-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));}
}
</style>
