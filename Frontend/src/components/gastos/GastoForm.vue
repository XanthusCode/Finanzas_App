<template>
  <form class="gasto-form" @submit.prevent="submit">
    <div class="form-row">
      <div v-if="!tipoForzado" class="field">
        <label>Tipo</label>
        <div class="select-wrap">
          <select v-model="form.tipo" class="input" @change="form.categoria = ''">
            <option value="FIJO">Fijo</option>
            <option value="VARIABLE">Ocasional</option>
          </select>
          <ChevronDown class="select-icon" :size="14" :stroke-width="2" />
        </div>
      </div>
      <div class="field">
        <div class="label-row">
          <label>Categoría</label>
          <span v-if="creandoCategoria" class="char-counter" :class="{ 'char-counter--limit': nuevaCategoriaNombre.length >= 100 }">{{ nuevaCategoriaNombre.length }}/100</span>
        </div>
        <div v-if="!creandoCategoria" class="categoria-row">
          <div class="select-wrap">
            <select v-model="form.categoria" class="input" :class="{ 'input-error': errors.categoria }">
              <option value="" disabled>Seleccionar...</option>
              <option v-for="c in categorias" :key="c.id" :value="c.nombre">{{ c.nombre }}</option>
            </select>
            <ChevronDown class="select-icon" :size="14" :stroke-width="2" />
          </div>
          <button type="button" class="btn-add-cat" title="Nueva categoría" @click="creandoCategoria = true">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          </button>
        </div>
        <div v-else class="categoria-row">
          <input
            v-model="nuevaCategoriaNombre"
            class="input"
            maxlength="100"
            placeholder="Nombre de la categoría"
            autofocus
            @keydown.enter.prevent="crearCategoria"
            @keydown.escape="cancelarCategoria"
          />
          <button type="button" class="btn btn-sm" @click="cancelarCategoria">Cancelar</button>
          <button type="button" class="btn btn-primary btn-sm" :disabled="creandoCategoriaLoading" @click="crearCategoria">
            {{ creandoCategoriaLoading ? '...' : 'Crear' }}
          </button>
        </div>
        <span v-if="errors.categoria" class="field-error">{{ errors.categoria }}</span>
        <span v-else-if="categorias.length === 0 && !creandoCategoria" class="hint">
          Sin categorías — crea una con el botón "+"
        </span>
      </div>
    </div>

    <div class="form-row">
      <div class="field" style="flex: 2">
        <div class="label-row">
          <label>Detalle <span class="label-optional">(opcional)</span></label>
          <span class="char-counter" :class="{ 'char-counter--limit': form.detalle.length >= 150 }">{{ form.detalle.length }}/150</span>
        </div>
        <input v-model="form.detalle" class="input" maxlength="150" :class="{ 'input-error': errors.detalle }" placeholder="ej. Netflix mensual" />
        <span v-if="errors.detalle" class="field-error">{{ errors.detalle }}</span>
      </div>
      <div class="field">
        <label>Monto (COP)</label>
        <CurrencyInput v-model="form.monto" :class="{ 'input-error': errors.monto }" placeholder="$ 0" />
        <span v-if="errors.monto" class="field-error">{{ errors.monto }}</span>
      </div>
    </div>

    <div class="form-row">
      <div class="field">
      <label>Fecha <span class="label-optional">(opcional)</span></label>
      <input v-model="form.fecha" type="date" class="input" />
    </div>

    <div v-if="store.cuentas.length > 0" class="field">
      <label>Cuenta <span class="label-optional">(opcional)</span></label>
      <div class="select-wrap">
        <select v-model="form.cuentaId" class="input">
          <option value="">Ninguna</option>
          <option v-for="c in store.cuentas" :key="c.id" :value="c.id">{{ c.nombre }}</option>
        </select>
        <ChevronDown class="select-icon" :size="14" :stroke-width="2" />
      </div>
    </div>
    </div>
    

    <label class="recurrente-label">
      <input type="checkbox" v-model="form.esRecurrente" class="recurrente-check" />
      <span>Gasto recurrente (se copia automáticamente cada mes)</span>
    </label>

    <div v-if="!isEdit" class="cuotas-row">
      <label class="recurrente-label" style="flex-shrink:0">
        <input type="checkbox" v-model="form.esCuotas" class="recurrente-check" />
        <span>Pago en cuotas</span>
      </label>
      <div v-if="form.esCuotas" class="cuotas-input">
        <input
          v-model.number="form.numCuotas"
          type="number" min="2" max="60" class="input input-num"
          placeholder="Nº cuotas"
        />
        <span class="cuotas-hint">meses</span>
      </div>
    </div>

    <div class="form-actions">
      <button type="button" class="btn" @click="emit('cancel')">Cancelar</button>
      <button type="submit" class="btn btn-primary" :disabled="loading">
        {{ loading ? 'Guardando...' : (isEdit ? 'Guardar' : 'Agregar gasto') }}
      </button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ChevronDown } from '@lucide/vue'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import type { Gasto } from '@/types'
import { gastoSchema } from '@/schemas'

const props = defineProps<{ gasto?: Gasto; loading?: boolean; tipoForzado?: 'FIJO' | 'VARIABLE' }>()
const emit  = defineEmits<{ submit: [gasto: Omit<Gasto, 'id'>]; cancel: [] }>()

const store = useFinanceStore()
const toast = useToast()
const isEdit = computed(() => !!props.gasto)

const creandoCategoria = ref(false)
const creandoCategoriaLoading = ref(false)
const nuevaCategoriaNombre = ref('')

const form = ref({
  tipo:         props.gasto?.tipo         ?? props.tipoForzado ?? 'FIJO' as 'FIJO' | 'VARIABLE',
  categoria:    props.gasto?.categoria    ?? '',
  detalle:      props.gasto?.detalle      ?? '',
  monto:        props.gasto?.monto        ?? 0,
  esRecurrente: props.gasto?.esRecurrente ?? false,
  cuentaId:     props.gasto?.cuentaId     ?? '',
  fecha:        props.gasto?.fecha?.slice(0, 10) ?? new Date().toISOString().slice(0, 10),
  esCuotas:     false,
  numCuotas:    2,
})

const errors = ref<Partial<Record<'categoria' | 'detalle' | 'monto', string>>>({})

const categorias = computed(() => store.categoriasActivas)

function cancelarCategoria() {
  creandoCategoria.value = false
  nuevaCategoriaNombre.value = ''
}

async function crearCategoria() {
  const nombre = nuevaCategoriaNombre.value.trim()
  if (!nombre) return
  creandoCategoriaLoading.value = true
  try {
    await store.agregarCategoria({ nombre, activa: true })
    form.value.categoria = nombre
    cancelarCategoria()
    toast.success('Categoría creada')
  } catch {
    toast.error('No se pudo crear la categoría')
  } finally {
    creandoCategoriaLoading.value = false
  }
}

function submit() {
  errors.value = {}
  const result = gastoSchema.safeParse(form.value)
  if (!result.success) {
    result.error.issues.forEach(e => {
      const field = e.path[0] as 'categoria' | 'detalle' | 'monto'
      errors.value[field] = e.message
    })
    return
  }
  const payload: Omit<Gasto, 'id'> & { numCuotas?: number } = {
    tipo: form.value.tipo,
    categoria: form.value.categoria,
    detalle: form.value.detalle,
    monto: form.value.monto,
    esRecurrente: form.value.esRecurrente,
    cuentaId: form.value.cuentaId || undefined,
    fecha: form.value.fecha || undefined,
    mes: 0,
    anio: 0,
    ...(form.value.esCuotas && form.value.numCuotas >= 2 ? { numCuotas: form.value.numCuotas } : {}),
  }
  emit('submit', payload)
}
</script>

<style scoped>
.gasto-form  { display: flex; flex-direction: column; gap: 0.75rem; }
.form-row    { display: flex; gap: 0.75rem; }
.field       { display: flex; flex-direction: column; flex: 1; }
.form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 0.25rem; }
.hint        { font-size: 0.65rem; color: var(--text-muted); margin-top: 0.25rem; }
.hint a      { color: var(--accent); text-decoration: none; }
.field-error { font-size: 0.65rem; color: var(--red); margin-top: 0.2rem; }
.input-error { border-color: var(--red) !important; }
.label-optional { color: var(--text-muted); font-weight: 400; }

.label-row { display: flex; justify-content: space-between; align-items: baseline; }
.char-counter { font-size: 0.62rem; color: var(--text-muted); }
.char-counter--limit { color: var(--red); }

.categoria-row { display: flex; align-items: center; gap: 0.4rem; }
.categoria-row .select-wrap { flex: 1; }
.btn-add-cat {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  background: none;
  border: 1px solid var(--border);
  border-radius: 6px;
  color: var(--text-secondary);
  cursor: pointer;
  transition: color 0.15s, background 0.15s, border-color 0.15s;
}
.btn-add-cat:hover { color: var(--accent); border-color: var(--accent); background: color-mix(in srgb, var(--accent) 8%, transparent); }

.recurrente-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.72rem;
  color: var(--text-secondary);
  cursor: pointer;
  user-select: none;
}
.recurrente-check { accent-color: var(--accent); width: 14px; height: 14px; cursor: pointer; }

.cuotas-row   { display: flex; align-items: center; gap: 0.75rem; }
.cuotas-input { display: flex; align-items: center; gap: 0.4rem; }
.input-num    { width: 80px; padding: 0.3rem 0.5rem; font-size: 0.8rem; }
.cuotas-hint  { font-size: 0.68rem; color: var(--text-muted); }

@media (max-width: 500px) {
  .form-row    { flex-direction: column; }
}
</style>
