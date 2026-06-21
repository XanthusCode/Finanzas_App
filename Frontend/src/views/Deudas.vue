<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Finanzas</div>
        <h1 class="page-title">Deudas</h1>
      </div>
      <button class="btn btn-primary btn-sm" @click="abrirForm()">+ Nueva deuda</button>
    </div>

    <!-- Resumen -->
    <div v-if="!loading" class="resumen-grid">
      <div class="resumen-card resumen-me-deben">
        <span class="resumen-label">Me deben</span>
        <span class="resumen-val">{{ fmt(totalMeDeben) }}</span>
        <span class="resumen-count">{{ pendientesMeDeben }} pendiente{{ pendientesMeDeben !== 1 ? 's' : '' }}</span>
      </div>
      <div class="resumen-card resumen-le-debo">
        <span class="resumen-label">Le debo</span>
        <span class="resumen-val">{{ fmt(totalLeDebo) }}</span>
        <span class="resumen-count">{{ pendientesLeDebo }} pendiente{{ pendientesLeDebo !== 1 ? 's' : '' }}</span>
      </div>
      <div class="resumen-card resumen-balance" :class="balance >= 0 ? 'balance-pos' : 'balance-neg'">
        <span class="resumen-label">Balance neto</span>
        <span class="resumen-val">{{ balance >= 0 ? '+' : '' }}{{ fmt(balance) }}</span>
        <span class="resumen-count">{{ balance >= 0 ? 'a tu favor' : 'en contra' }}</span>
      </div>
    </div>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <!-- Me deben -->
      <div class="section-block">
        <p class="section-titulo me-deben-titulo">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/></svg>
          Me deben
        </p>
        <div v-if="meDeben.length === 0" class="empty">Sin deudas a favor</div>
        <div v-for="d in meDeben" :key="d.id" class="deuda-row" :class="{ pagada: d.pagada }">
          <div class="deuda-info">
            <div class="deuda-top">
              <span class="deuda-persona">{{ d.persona }}</span>
              <span v-if="d.fechaLimite" class="deuda-fecha" :class="estaVencida(d) ? 'vencida' : ''">
                {{ fmtFecha(d.fechaLimite!) }}{{ estaVencida(d) ? ' · vencida' : '' }}
              </span>
            </div>
            <span class="deuda-desc">{{ d.descripcion }}</span>
          </div>
          <span class="deuda-monto me-deben-color">{{ fmt(d.monto) }}</span>
          <div class="deuda-actions">
            <button class="btn btn-sm" :class="d.pagada ? '' : 'btn-primary'" @click="togglePagada(d)">
              {{ d.pagada ? 'Reabrir' : 'Cobrado' }}
            </button>
            <button class="icon-btn danger" @click="eliminar(d)">
              <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Le debo -->
      <div class="section-block">
        <p class="section-titulo le-debo-titulo">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="23 18 13.5 8.5 8.5 13.5 1 6"/></svg>
          Le debo
        </p>
        <div v-if="leDebo.length === 0" class="empty">Sin deudas en contra</div>
        <div v-for="d in leDebo" :key="d.id" class="deuda-row" :class="{ pagada: d.pagada }">
          <div class="deuda-info">
            <div class="deuda-top">
              <span class="deuda-persona">{{ d.persona }}</span>
              <span v-if="d.fechaLimite" class="deuda-fecha" :class="estaVencida(d) ? 'vencida' : ''">
                {{ fmtFecha(d.fechaLimite!) }}{{ estaVencida(d) ? ' · vencida' : '' }}
              </span>
            </div>
            <span class="deuda-desc">{{ d.descripcion }}</span>
          </div>
          <span class="deuda-monto le-debo-color">{{ fmt(d.monto) }}</span>
          <div class="deuda-actions">
            <button class="btn btn-sm" :class="d.pagada ? '' : 'btn-primary'" @click="togglePagada(d)">
              {{ d.pagada ? 'Reabrir' : 'Pagado' }}
            </button>
            <button class="icon-btn danger" @click="eliminar(d)">
              <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
        </div>
      </div>
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

        <div class="modal-actions">
          <button class="btn" @click="cerrarForm">Cancelar</button>
          <button class="btn btn-primary" :disabled="!formValido || guardando" @click="guardar">
            {{ guardando ? '…' : 'Guardar' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import type { Deuda } from '@/types'
import { deudasService } from '@/services/api'
import { fmtCOP as fmt } from '@/utils'
import { useToast } from '@/composables/useToast'
import CurrencyInput from '@/components/common/CurrencyInput.vue'

const toast = useToast()

const deudas  = ref<Deuda[]>([])
const loading = ref(false)
const guardando = ref(false)
const formVisible = ref(false)

const form = reactive({
  tipo: 'ME_DEBEN' as 'ME_DEBEN' | 'LE_DEBO',
  persona: '',
  descripcion: '',
  monto: 0,
  fechaLimite: '',
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

function estaVencida(d: Deuda) {
  return !d.pagada && !!d.fechaLimite && new Date(d.fechaLimite) < new Date()
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

function abrirForm() {
  Object.assign(form, { tipo: 'ME_DEBEN', persona: '', descripcion: '', monto: 0, fechaLimite: '' })
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
    }
    const { data } = await deudasService.create(payload)
    deudas.value.unshift(data)
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
  } catch {
    toast.error('No se pudo actualizar')
  }
}

async function eliminar(d: Deuda) {
  try {
    await deudasService.delete(d.id!)
    deudas.value = deudas.value.filter(x => x.id !== d.id)
    toast.success('Deuda eliminada')
  } catch {
    toast.error('No se pudo eliminar')
  }
}

onMounted(cargar)
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.btn-sm { padding: 0.3rem 0.7rem; font-size: 0.72rem; }
.empty  { font-size: 0.72rem; color: var(--text-muted); padding: 0.75rem 0; }

/* Resumen */
.resumen-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}
.resumen-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1rem 1.1rem;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}
.resumen-label { font-size: 0.62rem; color: var(--text-muted); letter-spacing: 0.1em; text-transform: uppercase; }
.resumen-val   { font-size: 1.3rem; font-weight: 700; }
.resumen-count { font-size: 0.68rem; color: var(--text-muted); }
.resumen-me-deben .resumen-val { color: var(--accent); }
.resumen-le-debo  .resumen-val { color: var(--red); }
.balance-pos .resumen-val { color: var(--accent); }
.balance-neg .resumen-val { color: var(--red); }

/* Secciones */
.section-block { margin-bottom: 1rem; }
.section-titulo {
  display: flex; align-items: center; gap: 0.4rem;
  font-size: 0.65rem; letter-spacing: 0.1em; text-transform: uppercase;
  font-weight: 600; margin-bottom: 0.5rem;
}
.me-deben-titulo { color: var(--accent); }
.le-debo-titulo  { color: var(--red); }
.me-deben-color  { color: var(--accent); }
.le-debo-color   { color: var(--red); }

/* Filas */
.deuda-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0.75rem 1rem;
  margin-bottom: 0.4rem;
  transition: opacity 0.2s;
}
.deuda-row.pagada { opacity: 0.45; }

.deuda-info   { flex: 1; display: flex; flex-direction: column; gap: 0.2rem; }
.deuda-top    { display: flex; align-items: center; gap: 0.75rem; }
.deuda-persona { font-size: 0.82rem; font-weight: 600; color: var(--text-primary); }
.deuda-fecha  { font-size: 0.68rem; color: var(--text-muted); }
.deuda-fecha.vencida { color: var(--red); }
.deuda-desc   { font-size: 0.72rem; color: var(--text-muted); }
.deuda-monto  { font-size: 0.9rem; font-weight: 700; flex-shrink: 0; }
.deuda-actions { display: flex; align-items: center; gap: 0.35rem; flex-shrink: 0; }

.icon-btn {
  background: none; border: none; color: var(--text-muted); cursor: pointer;
  padding: 4px 5px; border-radius: 4px; transition: color 0.15s, background 0.15s;
  display: flex; align-items: center;
}
.icon-btn.danger:hover { color: var(--red); background: rgba(248,113,113,0.08); }

/* Modal */
.modal-backdrop {
  position: fixed; inset: 0; background: rgba(0,0,0,0.5);
  display: flex; align-items: center; justify-content: center; z-index: 100;
}
.modal {
  background: var(--surface); border: 1px solid var(--border); border-radius: 12px;
  padding: 1.5rem; width: 100%; max-width: 400px; display: flex; flex-direction: column; gap: 1rem;
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
.tipo-btn.active { border-color: var(--accent); color: var(--accent); background: rgba(99,179,255,0.08); }

.loading-list { display: flex; flex-direction: column; gap: 0.5rem; }
.skeleton-row {
  height: 64px; border-radius: 8px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%; animation: shimmer 1.4s infinite;
}
@keyframes shimmer { to { background-position: -200% 0; } }
</style>
