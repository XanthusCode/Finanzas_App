<template>
  <form class="gasto-form" @submit.prevent="submit">
    <div class="form-row">
      <div class="field">
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

    <div class="form-actions">
      <button type="button" class="btn" @click="emit('cancel')">Cancelar</button>
      <button type="submit" class="btn btn-primary" :disabled="loading">
        {{ loading ? 'Guardando...' : (isEdit ? 'Guardar cambios' : 'Agregar gasto') }}
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

const props = defineProps<{ gasto?: Gasto; loading?: boolean }>()
const emit  = defineEmits<{ submit: [gasto: Omit<Gasto, 'id'>]; cancel: [] }>()

const store = useFinanceStore()
const isEdit = computed(() => !!props.gasto)

const form = ref({
  tipo:      props.gasto?.tipo      ?? 'FIJO' as 'FIJO' | 'VARIABLE',
  categoria: props.gasto?.categoria ?? '',
  detalle:   props.gasto?.detalle   ?? '',
  monto:     props.gasto?.monto     ?? 0,
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
  emit('submit', { ...form.value, mes: 0, anio: 0 })
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
</style>
