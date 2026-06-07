<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Registro</div>
        <h1 class="page-title">Gastos</h1>
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
      <GastoForm :key="editando?.id ?? 'new'" :gasto="editando ?? undefined" :loading="saving" @submit="onSubmit" @cancel="cerrarForm" />
    </div>

    <div v-if="store.loading" class="empty">Cargando...</div>

    <template v-else-if="store.gastosFijos.length + store.gastosVariables.length > 0">
      <!-- Fijos -->
      <div class="card" style="margin-bottom: 0.75rem;">
        <div class="card-header">
          <span class="card-title">Gastos fijos</span>
          <span class="tag tag-red">{{ fmt(totalFijos) }}</span>
        </div>
        <div v-if="store.gastosFijos.length === 0" class="empty">Sin gastos fijos este mes</div>
        <GastoRow
          v-for="g in store.gastosFijos"
          :key="g.id"
          :gasto="g"
          @edit="abrirForm"
          @delete="onDelete"
        />
      </div>

      <!-- Variables -->
      <div class="card">
        <div class="card-header">
          <span class="card-title">Gastos variables</span>
          <span class="tag tag-amber">{{ fmt(totalVariables) }}</span>
        </div>
        <div v-if="store.gastosVariables.length === 0" class="empty">Sin gastos variables este mes</div>
        <GastoRow
          v-for="g in store.gastosVariables"
          :key="g.id"
          :gasto="g"
          @edit="abrirForm"
          @delete="onDelete"
        />
      </div>
    </template>

     <template v-else-if="!showForm && store.gastosVariables.length === 0">
        <div class="empty">No hay gastos registrados para este mes. ¡Agrega uno usando el botón de arriba!
        </div>
    </template>

    <ConfirmModal
      v-model="showConfirm"
      title="Eliminar gasto"
      message="¿Seguro que quieres eliminar este gasto? Esta acción no se puede deshacer."
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import type { Gasto } from '@/types'
import MonthSelector from '@/components/common/MonthSelector.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import GastoForm from '@/components/gastos/GastoForm.vue'
import GastoRow from '@/components/gastos/GastoRow.vue'

const store = useFinanceStore()
const showForm = ref(false)
const saving = ref(false)
const showConfirm = ref(false)
const pendingDeleteId = ref<number | null>(null)
const editando = ref<Gasto | null>(null)

const totalFijos = computed(() => store.gastosFijos.reduce((s, g) => s + g.monto, 0))
const totalVariables = computed(() => store.gastosVariables.reduce((s, g) => s + g.monto, 0))
const fmt = (n: number) => '$' + Math.round(n).toLocaleString('es-CO')

async function onSubmit(gasto: Omit<Gasto, 'id'>) {
  saving.value = true
  if (editando.value?.id != null) {
    await store.editarGasto(editando.value.id, gasto)
  } else {
    await store.agregarGasto(gasto)
  }
  saving.value = false
  cerrarForm()
}

function cerrarForm() {
  showForm.value = false
  editando.value = null
}

function onDelete(id: number) {
  pendingDeleteId.value = id
  showConfirm.value = true
}

function abrirForm(gasto: Gasto) {
  editando.value = gasto
  showForm.value = true
}

async function confirmDelete() {
  if (pendingDeleteId.value != null) {
    await store.eliminarGasto(pendingDeleteId.value)
    pendingDeleteId.value = null
  }
}

onMounted(() => { store.cargarDatos(); store.cargarCategorias() })
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; }
.empty { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center;  }
</style>