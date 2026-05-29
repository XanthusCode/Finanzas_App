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
        <select v-model="form.categoria" class="input" required>
          <option value="" disabled>Seleccionar...</option>
          <option v-for="c in categorias" :key="c.id" :value="c.nombre">{{ c.nombre }}</option>
        </select>
        <span v-if="categorias.length === 0" class="hint">
          Sin categorías — <a href="/categorias">créalas aquí</a>
        </span>
      </div>
    </div>

    <div class="form-row">
      <div class="field" style="flex: 2">
        <label>Detalle</label>
        <input v-model="form.detalle" class="input" placeholder="ej. Netflix mensual" required />
      </div>
      <div class="field">
        <label>Monto (COP)</label>
        <input v-model.number="form.monto" class="input" type="number" min="0" placeholder="0" required />
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
import type { Gasto } from '@/types'

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

const categorias = computed(() =>
  form.value.tipo === 'FIJO' ? store.categoriasFijas : store.categoriasVariables
)

watch(() => form.value.tipo, () => { form.value.categoria = '' })

function submit() {
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
</style>