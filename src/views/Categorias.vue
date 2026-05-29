<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Configuracion</div>
        <h1 class="page-title">Categorías</h1>
      </div>
      <button class="btn btn-primary" @click="abrirForm()">+ Nueva categoría</button>
    </div>

    <!-- Formulario inline -->
    <div v-if="showForm" class="card form-card">
      <p class="card-title">{{ editando ? 'Editar categoría' : 'Nueva categoría' }}</p>
      <form class="cat-form" @submit.prevent="guardar">
        <div class="form-row">
          <div class="field" style="flex: 2">
            <label>Nombre</label>
            <input v-model="form.nombre" class="input" placeholder="ej. Servicios" required />
          </div>
          <div class="field">
            <label>Tipo</label>
            <select v-model="form.tipo" class="input">
              <option value="FIJO">Fijo</option>
              <option value="VARIABLE">Variable</option>
            </select>
          </div>
          <div class="field" style="justify-content: flex-end; padding-bottom: 0;">
            <label style="opacity: 0">.</label>
            <div style="display: flex; gap: 0.5rem;">
              <button type="button" class="btn" @click="cancelar">Cancelar</button>
              <button type="submit" class="btn btn-primary" :disabled="saving">
                {{ saving ? 'Guardando...' : 'Guardar' }}
              </button>
            </div>
          </div>
        </div>
      </form>
    </div>

    <div v-if="store.loading" class="empty">Cargando...</div>

    <template v-else-if="store.categoriasFijas.length + store.categoriasVariables.length > 0">
      <!-- Fijas -->
      <div class="card" style="margin-bottom: 0.75rem;">
        <p class="card-title">Gastos fijos</p>
        <div v-if="store.categoriasFijas.length === 0" class="empty">
          Sin categorías fijas — crea una arriba
        </div>
        <div v-for="cat in store.categoriasFijas" :key="cat.id" class="cat-row">
          <div class="cat-info">
            <span class="tag tag-red">Fijo</span>
            <span class="cat-name">{{ cat.nombre }}</span>
          </div>
          <div class="cat-actions">
            <button class="icon-btn" title="Editar" @click="abrirForm(cat)">✎</button>
            <button class="icon-btn danger" title="Eliminar" @click="onDelete(cat.id!)">✕</button>
          </div>
        </div>
      </div>

      <!-- Variables -->
      <div class="card">
        <p class="card-title">Gastos variables</p>
        <div v-if="store.categoriasVariables.length === 0" class="empty">
          Sin categorías variables — crea una arriba
        </div>
        <div v-for="cat in store.categoriasVariables" :key="cat.id" class="cat-row">
          <div class="cat-info">
            <span class="tag tag-amber">Variable</span>
            <span class="cat-name">{{ cat.nombre }}</span>
          </div>
          <div class="cat-actions">
            <button class="icon-btn" title="Editar" @click="abrirForm(cat)">✎</button>
            <button class="icon-btn danger" title="Eliminar" @click="onDelete(cat.id!)">✕</button>
          </div>
        </div>
      </div>
    </template>

    <template v-else>
        <div class="empty">No hay categorías creadas aún. ¡Crea una usando el botón de arriba!</div>
    </template>

    <ConfirmModal
      v-model="showConfirm"
      title="Eliminar categoría"
      message="Si eliminas esta categoría, los gastos que la usan quedarán sin categoría. ¿Continuar?"
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import type { Categoria } from '@/types'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const store = useFinanceStore()

const showForm    = ref(false)
const saving      = ref(false)
const editando    = ref<Categoria | null>(null)
const showConfirm = ref(false)
const pendingId   = ref<number | null>(null)

const form = reactive({ nombre: '', tipo: 'FIJO' as 'FIJO' | 'VARIABLE' })

function abrirForm(cat?: Categoria) {
  editando.value = cat ?? null
  form.nombre    = cat?.nombre ?? ''
  form.tipo      = cat?.tipo   ?? 'FIJO'
  showForm.value = true
}

function cancelar() {
  showForm.value = false
  editando.value = null
}

async function guardar() {
  saving.value = true
  const payload = { nombre: form.nombre, tipo: form.tipo, activa: true }
  if (editando.value?.id) {
    await store.actualizarCategoria(editando.value.id, payload)
  } else {
    await store.agregarCategoria(payload)
  }
  saving.value  = false
  showForm.value = false
  editando.value = null
}

function onDelete(id: number) {
  pendingId.value   = id
  showConfirm.value = true
}

async function confirmDelete() {
  if (pendingId.value != null) {
    await store.eliminarCategoria(pendingId.value)
    pendingId.value = null
  }
}

onMounted(() => store.cargarCategorias())
</script>

<style scoped>
.page-header  { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title   { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-title   { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 0.75rem; }
.form-card    { margin-bottom: 1rem; }
.cat-form     { display: flex; flex-direction: column; gap: 0.5rem; }
.form-row     { display: flex; gap: 0.75rem; align-items: flex-start; }
.field        { display: flex; flex-direction: column; flex: 1; }
.cat-row      { display: flex; justify-content: space-between; align-items: center; padding: 0.65rem 0; border-bottom: 1px solid var(--border); }
.cat-row:last-child { border-bottom: none; }
.cat-info     { display: flex; align-items: center; gap: 0.75rem; }
.cat-name     { font-size: 0.82rem; color: var(--text-primary); }
.cat-actions  { display: flex; gap: 0.35rem; }
.icon-btn     { background: none; border: none; color: var(--text-muted); cursor: pointer; font-size: 0.85rem; padding: 3px 6px; border-radius: 3px; transition: all 0.2s; }
.icon-btn:hover       { color: var(--accent); background: rgba(99,179,255,0.08); }
.icon-btn.danger:hover { color: var(--red);   background: rgba(248,113,113,0.08); }
.empty        { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; align-items: center; gap: 0.25rem; }
</style>