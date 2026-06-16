<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Registro</div>
        <h1 class="page-title">Ingresos</h1>
      </div>
      <div style="display: flex; align-items: center; gap: 0.75rem;">
        <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
        <button v-if="store.ingresos.length > 0" class="btn" @click="descargarCSV">
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          CSV
        </button>
        <button class="btn btn-primary" :disabled="!!editando" @click="showForm ? cerrarForm() : abrirForm()">
          <svg v-if="showForm" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          <svg v-else width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          {{ showForm ? 'Cancelar' : 'Agregar' }}
        </button>
      </div>
    </div>

    <!-- Formulario con transición -->
    <Transition name="form-slide">
      <div v-if="showForm && !editando" class="card form-card">
        <form class="ingreso-form" @submit.prevent="onSubmit">
          <div class="form-row">
            <div class="field" style="flex: 2">
              <label>Concepto</label>
              <input v-model="form.concepto" class="input" :class="{ 'input-error': errors.concepto }" placeholder="ej. Salario básico" />
              <span v-if="errors.concepto" class="field-error">{{ errors.concepto }}</span>
            </div>
            <div class="field">
              <label>Monto (COP)</label>
              <CurrencyInput v-model="form.monto" :class="{ 'input-error': errors.monto }" placeholder="$ 0" />
              <span v-if="errors.monto" class="field-error">{{ errors.monto }}</span>
            </div>
          </div>
          <label class="check-label">
            <input type="checkbox" v-model="form.esRecurrente" />
            Repetir cada mes
          </label>
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

    <template v-else-if="store.ingresos.length > 0">
      <div class="card">
        <div class="card-header">
          <span class="card-title">Ingresos del mes</span>
          <span class="tag" style="border-color: rgba(99,179,255,0.3); color: var(--accent);">
            {{ fmt(total) }}
          </span>
        </div>
        <template v-for="i in store.ingresos" :key="i.id">
          <div v-if="editando?.id === i.id" class="inline-form-row">
            <input v-model="form.concepto" class="input input-inline" @keydown.enter.prevent="onSubmit" @keydown.escape="cerrarForm" autofocus />
            <CurrencyInput v-model="form.monto" class="input input-inline" style="max-width: 140px" />
            <button class="btn btn-sm" @click="cerrarForm">Cancelar</button>
            <button class="btn btn-primary btn-sm" :disabled="saving" @click="onSubmit">{{ saving ? '...' : 'Guardar' }}</button>
          </div>
          <div v-else class="ingreso-row">
            <div>
              <div class="row-name">{{ i.concepto }}</div>
            </div>
            <div style="display: flex; align-items: center; gap: 0.5rem;">
              <span class="row-monto">{{ fmt(i.monto) }}</span>
              <button class="icon-btn" title="Editar" @click="abrirForm(i)">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button class="icon-btn danger" title="Eliminar" @click="onDelete(i.id!)">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
          </div>
        </template>
      </div>
    </template>

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
import { ref, computed, reactive, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import { fmtCOP as fmt, exportCSV } from '@/utils'
import MonthSelector from '@/components/common/MonthSelector.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import { ingresoSchema } from '@/schemas'
import type { Ingreso } from '@/types'

const store = useFinanceStore()
const toast = useToast()

const showForm        = ref(false)
const saving          = ref(false)
const copiando        = ref(false)
const showConfirm     = ref(false)
const pendingDeleteId = ref<string | null>(null)
const editando        = ref<Ingreso | null>(null)

const form   = reactive({ concepto: '', monto: 0, esRecurrente: false })
const errors = ref<Partial<Record<'concepto' | 'monto', string>>>({})
const total  = computed(() => store.ingresos.reduce((s, i) => s + i.monto, 0))

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
  const payload = { concepto: form.concepto, monto: form.monto, esRecurrente: form.esRecurrente }
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
  showForm.value       = ingreso == null
}

function cerrarForm() {
  showForm.value    = false
  editando.value    = null
  form.concepto     = ''
  form.monto        = 0
  form.esRecurrente = false
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

function descargarCSV() {
  exportCSV(
    store.ingresos.map(i => ({ Concepto: i.concepto, Monto: i.monto })),
    `ingresos-${store.mesActual}-${store.anioActual}.csv`
  )
}

onMounted(() => store.cargarDatos())
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }

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

.ingreso-form { display: flex; flex-direction: column; gap: 0.75rem; }
.form-row  { display: flex; gap: 0.75rem; }
.field     { display: flex; flex-direction: column; flex: 1; }
.form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; }
.field-error  { font-size: 0.65rem; color: var(--red); margin-top: 0.2rem; }
.input-error  { border-color: var(--red) !important; }

.ingreso-row { display: flex; justify-content: space-between; align-items: center; padding: 0.65rem 0; border-bottom: 1px solid var(--border); }
.ingreso-row:last-child { border-bottom: none; }
.row-name  { font-size: 0.78rem; color: var(--text-primary); }
.row-monto { font-size: 0.78rem; color: var(--text-primary); }

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

.inline-form-row { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0; border-bottom: 1px solid var(--border); }
.input-inline    { flex: 1; height: 30px; font-size: 0.78rem; padding: 0 0.6rem; }
.btn-sm          { padding: 0.3rem 0.7rem; font-size: 0.72rem; }

.check-label { display: flex; align-items: center; gap: 0.5rem; font-size: 0.75rem; color: var(--text-secondary); cursor: pointer; user-select: none; }
.check-label input { accent-color: var(--accent); cursor: pointer; }

.recurrentes-banner {
  display: flex; justify-content: space-between; align-items: center; gap: 1rem;
  background: var(--surface); border: 1px solid rgba(99,179,255,0.2);
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
</style>
