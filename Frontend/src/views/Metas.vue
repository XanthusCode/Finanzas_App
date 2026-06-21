<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Planificación</div>
        <h1 class="page-title">Metas de ahorro</h1>
      </div>
      <button class="btn btn-primary" @click="showForm = !showForm">
        <svg v-if="showForm" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
        <svg v-else width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        {{ showForm ? 'Cancelar' : 'Nueva meta' }}
      </button>
    </div>

    <!-- Formulario nueva meta -->
    <Transition name="form-slide">
      <div v-if="showForm" class="card form-card">
        <div class="meta-form">
          <div class="form-row">
            <div class="field" style="flex: 2">
              <label>Nombre de la meta</label>
              <input v-model="form.nombre" class="input" :class="{ 'input-error': errors.nombre }" placeholder="ej. Vacaciones en Europa" />
              <span v-if="errors.nombre" class="field-error">{{ errors.nombre }}</span>
            </div>
            <div class="field">
              <label>Monto objetivo (COP)</label>
              <CurrencyInput v-model="form.montoObjetivo" :class="{ 'input-error': errors.montoObjetivo }" />
              <span v-if="errors.montoObjetivo" class="field-error">{{ errors.montoObjetivo }}</span>
            </div>
          </div>
          <div class="field" style="max-width: 220px">
            <label>Fecha límite (opcional)</label>
            <input v-model="form.fechaLimite" type="date" class="input" />
          </div>
          <div class="form-actions">
            <button class="btn" @click="showForm = false">Cancelar</button>
            <button class="btn btn-primary" :disabled="saving" @click="crearMeta">{{ saving ? 'Guardando...' : 'Crear meta' }}</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Modal abonar -->
    <Teleport to="body">
      <div v-if="abonarMeta" class="overlay" @click.self="abonarMeta = null">
        <div class="modal fade-up">
          <p class="modal-title">Abonar a "{{ abonarMeta.nombre }}"</p>
          <p class="modal-sub">Actual: {{ fmt(abonarMeta.montoActual) }} / {{ fmt(abonarMeta.montoObjetivo) }}</p>
          <div class="field" style="margin: 1rem 0">
            <label>Monto a abonar (COP)</label>
            <CurrencyInput v-model="montoAbono" />
          </div>
          <div class="modal-actions">
            <button class="btn" @click="abonarMeta = null">Cancelar</button>
            <button class="btn btn-primary" :disabled="saving" @click="confirmarAbono">{{ saving ? '...' : 'Abonar' }}</button>
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
            <label>Nombre</label>
            <input v-model="editForm.nombre" class="input" />
          </div>
          <div class="field" style="margin-top: 0.75rem">
            <label>Monto objetivo (COP)</label>
            <CurrencyInput v-model="editForm.montoObjetivo" />
          </div>
          <div class="field" style="margin-top: 0.75rem">
            <label>Fecha límite (opcional)</label>
            <input v-model="editForm.fechaLimite" type="date" class="input" />
          </div>
          <div class="modal-actions" style="margin-top: 1.25rem">
            <button class="btn" @click="editandoMeta = null">Cancelar</button>
            <button class="btn btn-primary" :disabled="saving" @click="confirmarEdicion">{{ saving ? '...' : 'Guardar' }}</button>
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
          <div class="meta-header">
            <span class="meta-nombre">{{ meta.nombre }}</span>
            <div style="display: flex; gap: 0.35rem; align-items: center;">
              <span v-if="meta.completada" class="badge-completada">✓ Lograda</span>
              <button v-if="!meta.completada" class="btn btn-sm" @click="iniciarAbono(meta)">Abonar</button>
              <button class="icon-btn" title="Editar" @click="iniciarEdicion(meta)">
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
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import type { Meta } from '@/types'
import { calcularProyeccionMeta } from '@/composables/useMetaProjection'

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
const editForm     = reactive({ nombre: '', montoObjetivo: 0, fechaLimite: '' })

const form = reactive({ nombre: '', montoObjetivo: 0, fechaLimite: '' })
const errors = ref<Partial<Record<'nombre' | 'montoObjetivo', string>>>({})

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
}

function iniciarEdicion(meta: Meta) {
  editandoMeta.value        = meta
  editForm.nombre           = meta.nombre
  editForm.montoObjetivo    = meta.montoObjetivo
  editForm.fechaLimite      = meta.fechaLimite ? meta.fechaLimite.substring(0, 10) : ''
}

async function confirmarEdicion() {
  if (!editandoMeta.value || !editForm.nombre.trim() || editForm.montoObjetivo <= 0) return
  saving.value = true
  try {
    await store.editarMeta(editandoMeta.value.id!, {
      nombre:        editForm.nombre.trim(),
      montoObjetivo: editForm.montoObjetivo,
      fechaLimite:   editForm.fechaLimite || undefined
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
      fechaLimite: form.fechaLimite || undefined
    })
    toast.success('Meta creada')
    showForm.value = false
    form.nombre = ''
    form.montoObjetivo = 0
    form.fechaLimite = ''
  } catch {
    toast.error('No se pudo crear la meta')
  } finally {
    saving.value = false
  }
}

async function confirmarAbono() {
  if (!abonarMeta.value || montoAbono.value <= 0) return
  saving.value = true
  try {
    await store.abonarMeta(abonarMeta.value.id!, montoAbono.value)
    toast.success('Abono registrado')
    abonarMeta.value = null
  } catch {
    toast.error('No se pudo registrar el abono')
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
  try { await store.cargarMetas() }
  finally { loadingInit.value = false }
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.form-card   { margin-bottom: 1rem; }

.meta-form   { display: flex; flex-direction: column; gap: 0.75rem; }
.form-row    { display: flex; gap: 0.75rem; }
.field       { display: flex; flex-direction: column; flex: 1; }
.form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; }
.field-error { font-size: 0.65rem; color: var(--red); margin-top: 0.2rem; }
.input-error { border-color: var(--red) !important; }

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
.meta-card.meta-completada { border-color: rgba(99,179,255,0.3); }

.meta-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 0.5rem; }
.meta-nombre { font-size: 0.82rem; font-weight: 600; color: var(--text-primary); line-height: 1.3; }

.badge-completada { font-size: 0.62rem; background: rgba(99,179,255,0.12); color: var(--accent); padding: 0.2rem 0.5rem; border-radius: 4px; white-space: nowrap; }

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
.icon-btn:hover        { color: var(--accent); background: rgba(99, 179, 255, 0.08); }
.icon-btn.danger:hover { color: var(--red);   background: rgba(248, 113, 113, 0.08); }

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
.modal { background: var(--surface); border: 1px solid var(--border); border-radius: 12px; padding: 1.5rem; width: 320px; }
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
.meta-urgencia.warn   { background: rgba(245,158,11,0.1); color: #f59e0b; }
.meta-urgencia.danger { background: rgba(248,113,113,0.1); color: var(--red); }

.meta-estimada {
  font-size: 0.62rem;
  color: var(--text-muted);
}

.meta-ritmo {
  display: flex; align-items: center; gap: 0.3rem;
  font-size: 0.62rem; color: var(--accent);
}
</style>
