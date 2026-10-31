<!-- src/components/gastos/EscanearReciboModal.vue -->
<template>
  <Teleport to="body">
    <div v-if="modelValue" class="overlay" @click.self="cerrar">
      <div class="modal fade-up">
        <p class="modal-title">Revisa los datos del recibo</p>
        <p class="modal-msg">Confirma o corrige antes de guardar.</p>
        <GastoForm
          v-if="gastoPreLlenado"
          :gasto="gastoPreLlenado"
          :loading="loading"
          @submit="onSubmit"
          @cancel="cerrar"
        />
        <p v-else class="modal-msg">Cargando…</p>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import GastoForm from './GastoForm.vue'
import { useFinanceStore } from '@/stores/finance'
import type { Gasto, ReciboExtraido } from '@/types'

const props = defineProps<{ modelValue: boolean; datos: ReciboExtraido | null; loading?: boolean }>()
const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  submit: [gasto: Omit<Gasto, 'id'>]
}>()

const store = useFinanceStore()
const gastoPreLlenado = ref<Gasto>()

async function resolverCategoria(sugerida: string | null | undefined): Promise<string> {
  if (!sugerida) return ''
  const existente = store.categorias.find(c => c.nombre.toLowerCase() === sugerida.toLowerCase())
  if (existente) return existente.nombre

  await store.agregarCategoria({ nombre: sugerida, activa: true })
  return sugerida
}

watch(() => props.datos, async (datos) => {
  if (!datos) { gastoPreLlenado.value = undefined; return }

  const tipo: 'FIJO' | 'VARIABLE' = 'VARIABLE'
  const categoria = await resolverCategoria(datos.categoriaSugerida)

  gastoPreLlenado.value = {
    categoria,
    detalle: datos.descripcion ?? datos.comercio ?? '',
    monto: datos.monto,
    tipo,
    esRecurrente: false,
    fecha: datos.fecha ?? undefined,
    mes: 0,
    anio: 0,
  } as Gasto
}, { immediate: true })

function cerrar() {
  emit('update:modelValue', false)
}

function onSubmit(gasto: Omit<Gasto, 'id'>) {
  emit('submit', gasto)
  cerrar()
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(8, 12, 16, 0.75);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 500;
}
.modal {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-modal);
  padding: 1.5rem;
  width: 420px;
  max-width: 90vw;
  box-shadow: var(--shadow-pop);
}
.modal-title { font-family: var(--ff-display); font-weight: 700; font-size: 1rem; color: var(--text-primary); margin-bottom: 0.35rem; }
.modal-msg { font-size: 0.78rem; color: var(--text-secondary); margin-bottom: 1rem; }
</style>
