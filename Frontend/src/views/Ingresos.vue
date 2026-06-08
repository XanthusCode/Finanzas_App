<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Registro</div>
        <h1 class="page-title">Ingresos</h1>
      </div>
      <div style="display: flex; align-items: center; gap: 0.75rem;">
        <MonthSelector :mes="store.mesActual" :anio="store.anioActual" @change="store.cambiarMes" />
        <button class="btn btn-primary" @click="showForm = !showForm">
          {{ showForm ? '✕ Cancelar' : '+ Agregar' }}
        </button>
      </div>
    </div>

    <!-- Formulario -->
    <div v-if="showForm" class="card" style="margin-bottom: 1rem;">
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
        <div class="form-actions">
          <button type="button" class="btn" @click="cerrarForm">Cancelar</button>
          <button type="submit" class="btn btn-primary" :disabled="saving">
            {{ saving ? 'Guardando...' : 'Actualizar' }}
          </button>
        </div>
      </form>
    </div>

    <div v-if="store.loading" class="empty">Cargando...</div>

    <template v-else-if="store.ingresos.length > 0">
      <div class="card">
        <div class="card-header">
          <span class="card-title">Ingresos del mes</span>
          <span class="tag" style="border-color: rgba(99,179,255,0.3); color: var(--accent);">
            {{ fmt(total) }}
          </span>
        </div>
        <div v-for="i in store.ingresos" :key="i.id" class="ingreso-row">
          <div>
            <div class="row-name">{{ i.concepto }}</div>
          </div>
          <div style="display: flex; align-items: center; gap: 0.75rem;">
            <span class="row-monto">{{ fmt(i.monto) }}</span>
             <button class="icon-btn" title="Editar" @click="abrirForm(i)">✎</button>
            <button class="icon-btn danger" @click="onDelete(i.id!)">&#10005;</button>
          </div>
        </div>
      </div>
    </template>

    <template v-else-if="!showForm">
      <div class="empty">No hay ingresos registrados para este mes. ¡Agrega uno usando el botón de arriba!</div>
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
import MonthSelector from '@/components/common/MonthSelector.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import { ingresoSchema } from '@/schemas'
import type { Ingreso } from '@/types'

const store = useFinanceStore()
const showForm = ref(false)
const saving = ref(false)
const showConfirm = ref(false)
const pendingDeleteId = ref<string | null>(null)
  const editando    = ref<Ingreso | null>(null)

const form = reactive({ concepto: '', monto: 0 })
const errors = ref<Partial<Record<'concepto' | 'monto', string>>>({})
const total = computed(() => store.ingresos.reduce((s, i) => s + i.monto, 0))
const fmt = (n: number) => '$' + Math.round(n).toLocaleString('es-CO')

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
  const payload = { concepto: form.concepto, monto: form.monto }
  if (editando.value?.id) {
    await store.editarIngreso(editando.value.id, payload)
  } else {
    await store.agregarIngreso(payload)
  }
  saving.value = false
  cerrarForm()
}

function abrirForm(ingreso?: Ingreso) {
  editando.value = ingreso ?? null
  form.concepto    = ingreso?.concepto ?? ''
  form.monto      = ingreso?.monto   ?? 0
  showForm.value = true
}

function cerrarForm() {
  showForm.value = false
  form.concepto = ''
  form.monto = 0
  errors.value = {}
}

function onDelete(id: string) {
  pendingDeleteId.value = id
  showConfirm.value = true
}

async function confirmDelete() {
  if (pendingDeleteId.value != null) {
    await store.eliminarIngreso(pendingDeleteId.value)
    pendingDeleteId.value = null
  }
}

onMounted(() => store.cargarDatos())
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.ingreso-form { display: flex; flex-direction: column; gap: 0.75rem; }
.form-row { display: flex; gap: 0.75rem; }
.field { display: flex; flex-direction: column; flex: 1; }
.form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; }
.field-error { font-size: 0.65rem; color: var(--red); margin-top: 0.2rem; }
.input-error { border-color: var(--red) !important; }
.ingreso-row { display: flex; justify-content: space-between; align-items: center; padding: 0.65rem 0; border-bottom: 1px solid var(--border); }
.ingreso-row:last-child { border-bottom: none; }
.row-name  { font-size: 0.78rem; color: var(--text-primary); }
.row-monto { font-size: 0.78rem; color: var(--text-primary); }
.icon-btn  { background: none; border: none; color: var(--text-muted); cursor: pointer; font-size: 0.7rem; padding: 2px 4px; border-radius: 3px; transition: all 0.2s; }
.icon-btn:hover       { color: var(--accent); background: rgba(99,179,255,0.08); }
.icon-btn.danger:hover { color: var(--red);   background: rgba(248,113,113,0.08); }
.empty { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }
</style>
