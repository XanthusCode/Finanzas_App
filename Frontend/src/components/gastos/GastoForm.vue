<template>
  <form class="gasto-form" @submit.prevent="submit">
    <div class="form-row">
      <div v-if="!tipoForzado" class="field">
        <label>Tipo</label>
        <select v-model="form.tipo" class="input" @change="form.categoria = ''">
          <option value="FIJO">Fijo</option>
          <option value="VARIABLE">Variable</option>
        </select>
      </div>
      <div class="field">
        <label>Categoría</label>
        <select v-model="form.categoria" class="input" :class="{ 'input-error': errors.categoria }">
          <option value="" disabled>Seleccionar...</option>
          <option v-for="c in categorias" :key="c.id" :value="c.nombre">{{ c.nombre }}</option>
        </select>
        <span v-if="errors.categoria" class="field-error">{{ errors.categoria }}</span>
        <span v-else-if="categorias.length === 0" class="hint">
          Sin categorías — <a href="/categorias">créalas aquí</a>
        </span>
      </div>
    </div>

    <div class="form-row">
      <div class="field" style="flex: 2">
        <label>Detalle</label>
        <input v-model="form.detalle" class="input" :class="{ 'input-error': errors.detalle }" placeholder="ej. Netflix mensual" />
        <span v-if="errors.detalle" class="field-error">{{ errors.detalle }}</span>
      </div>
      <div class="field">
        <label>Monto (COP)</label>
        <CurrencyInput v-model="form.monto" :class="{ 'input-error': errors.monto }" placeholder="$ 0" />
        <span v-if="errors.monto" class="field-error">{{ errors.monto }}</span>
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
import { ref, computed, watch } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import CurrencyInput from '@/components/common/CurrencyInput.vue'
import type { Gasto } from '@/types'
import { gastoSchema } from '@/schemas'

const props = defineProps<{ gasto?: Gasto; loading?: boolean; tipoForzado?: 'FIJO' | 'VARIABLE' }>()
const emit  = defineEmits<{ submit: [gasto: Omit<Gasto, 'id'>]; cancel: [] }>()

const store = useFinanceStore()
const isEdit = computed(() => !!props.gasto)

const form = ref({
  tipo:         props.gasto?.tipo         ?? props.tipoForzado ?? 'FIJO' as 'FIJO' | 'VARIABLE',
  categoria:    props.gasto?.categoria    ?? '',
  detalle:      props.gasto?.detalle      ?? '',
  monto:        props.gasto?.monto        ?? 0,
  esRecurrente: props.gasto?.esRecurrente ?? false,
  esCuotas:     false,
  numCuotas:    2,
})

const errors = ref<Partial<Record<'categoria' | 'detalle' | 'monto', string>>>({})

const categorias = computed(() =>
  form.value.tipo === 'FIJO' ? store.categoriasFijas : store.categoriasVariables
)

watch(() => form.value.tipo, (_, prev) => { if (prev !== undefined) form.value.categoria = '' })

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
</style>
