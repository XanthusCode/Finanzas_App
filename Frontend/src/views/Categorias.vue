<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Configuración</div>
        <h1 class="page-title">Categorías</h1>
      </div>
    </div>

    <div v-if="loadingCats" class="loading-list">
      <div v-for="i in 3" :key="i" class="skeleton-row" />
    </div>

    <template v-else>
      <!-- Fijas -->
      <div class="card" style="margin-bottom: 0.75rem;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
          <p class="card-title">Gastos fijos</p>

          <button v-if="!(showForm && form.tipo === 'FIJO')" class="btn btn-sm" @click="abrirForm(undefined, 'FIJO')"><svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            Nueva categoría
          </button>
        </div>

        <div v-if="store.categoriasFijas.length === 0 && !(showForm && form.tipo === 'FIJO')" class="empty">
          Sin categorías fijas
        </div>

        <template v-for="cat in store.categoriasFijas" :key="cat.id">
          <div v-if="editando?.id === cat.id" class="inline-form-row">
            <span class="tag tag-red">Fijo</span>
            <input v-model="form.nombre" class="input input-inline" @keydown.enter.prevent="guardar" @keydown.escape="cancelar" autofocus />
            <button class="btn btn-sm" @click="cancelar">Cancelar</button>
            <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar">{{ saving ? '...' : 'Guardar' }}</button>
          </div>
          <div v-else class="cat-row">
            <div class="cat-info">
              <span class="tag tag-red">Fijo</span>
              <span class="cat-name">{{ cat.nombre }}</span>
            </div>
            <div class="cat-actions">
              <button class="icon-btn" title="Editar" @click="abrirForm(cat)">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button class="icon-btn danger" title="Eliminar" @click="onDelete(cat.id!)">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
          </div>
        </template>

        <div v-if="showForm && form.tipo === 'FIJO' && !editando" class="inline-form-row">
          <span class="tag tag-red">Fijo</span>
          <input v-model="form.nombre" class="input input-inline" placeholder="Nombre de la categoría" @keydown.enter.prevent="guardar" @keydown.escape="cancelar" autofocus />
          <button class="btn btn-sm" @click="cancelar">Cancelar</button>
          <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar">{{ saving ? '...' : 'Guardar' }}</button>
        </div>
      </div>

      <!-- Variables -->
      <div class="card">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
          <p class="card-title">Gastos variables</p>
          <button v-if="!(showForm && form.tipo === 'VARIABLE')" class="btn btn-sm" @click="abrirForm(undefined, 'VARIABLE')">+ Nueva categoría</button>
        </div>

        <div v-if="store.categoriasVariables.length === 0 && !(showForm && form.tipo === 'VARIABLE')" class="empty">
          Sin categorías variables
        </div>

        <template v-for="cat in store.categoriasVariables" :key="cat.id">
          <div v-if="editando?.id === cat.id" class="inline-form-row">
            <span class="tag tag-amber">Variable</span>
            <input v-model="form.nombre" class="input input-inline" @keydown.enter.prevent="guardar" @keydown.escape="cancelar" autofocus />
            <button class="btn btn-sm" @click="cancelar">Cancelar</button>
            <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar">{{ saving ? '...' : 'Guardar' }}</button>
          </div>
          <div v-else class="cat-row">
            <div class="cat-info">
              <span class="tag tag-amber">Variable</span>
              <span class="cat-name">{{ cat.nombre }}</span>
            </div>
            <div class="cat-actions">
              <button class="icon-btn" title="Editar" @click="abrirForm(cat)">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button class="icon-btn danger" title="Eliminar" @click="onDelete(cat.id!)">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
          </div>
        </template>

        <div v-if="showForm && form.tipo === 'VARIABLE' && !editando" class="inline-form-row">
          <span class="tag tag-amber">Variable</span>
          <input v-model="form.nombre" class="input input-inline" placeholder="Nombre de la categoría" @keydown.enter.prevent="guardar" @keydown.escape="cancelar" autofocus />
          <button class="btn btn-sm" @click="cancelar">Cancelar</button>
          <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar">{{ saving ? '...' : 'Guardar' }}</button>
        </div> 
      </div>
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
import { useToast } from '@/composables/useToast'
import type { Categoria } from '@/types'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const store = useFinanceStore()
const toast = useToast()

const showForm    = ref(false)
const saving      = ref(false)
const loadingCats = ref(false)
const editando    = ref<Categoria | null>(null)
const showConfirm = ref(false)
const pendingId   = ref<string | null>(null)

const form = reactive({ nombre: '', tipo: 'FIJO' as 'FIJO' | 'VARIABLE' })

function abrirForm(cat?: Categoria, tipo?: 'FIJO' | 'VARIABLE') {
  editando.value = cat ?? null
  form.nombre    = cat?.nombre ?? ''
  form.tipo      = cat?.tipo ?? tipo ?? 'FIJO'
  showForm.value = true
}

function cancelar() {
  showForm.value = false
  editando.value = null
  form.nombre    = ''
}

async function guardar() {
  if (!form.nombre.trim()) return
  saving.value = true
  const payload = { nombre: form.nombre.trim(), tipo: form.tipo, activa: true }
  try {
    if (editando.value?.id) {
      await store.actualizarCategoria(editando.value.id, payload)
      toast.success('Categoría actualizada')
    } else {
      await store.agregarCategoria(payload)
      toast.success('Categoría creada')
    }
    showForm.value = false
    editando.value = null
    form.nombre    = ''
  } catch {
    toast.error('No se pudo guardar la categoría')
  } finally {
    saving.value = false
  }
}

function onDelete(id: string) {
  pendingId.value   = id
  showConfirm.value = true
}

async function confirmDelete() {
  if (pendingId.value != null) {
    try {
      await store.eliminarCategoria(pendingId.value)
      toast.success('Categoría eliminada')
    } catch (error: any) {
      const msg = error?.response?.status === 409
        ? error.response.data
        : 'No se pudo eliminar la categoría'
      toast.error(msg)
    } finally {
      pendingId.value = null
    }
  }
}

onMounted(async () => {
  loadingCats.value = true
  try {
    await store.cargarCategorias()
  } finally {
    loadingCats.value = false
  }
})
</script>

<style scoped>
.page-header  { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title   { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }
.card-title   { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 0.75rem; }

.cat-row      { display: flex; justify-content: space-between; align-items: center; padding: 0.65rem 0; border-bottom: 1px solid var(--border); }
.cat-row:last-of-type { border-bottom: none; }
.cat-info     { display: flex; align-items: center; gap: 0.75rem; }
.cat-name     { font-size: 0.82rem; color: var(--text-primary); }
.cat-actions  { display: flex; gap: 0.35rem; }

.inline-form-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--border);
}
.input-inline {
  flex: 1;
  height: 30px;
  font-size: 0.78rem;
  padding: 0 0.6rem;
}
.btn-sm { padding: 0.3rem 0.7rem; font-size: 0.72rem; }

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
.empty { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }

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
</style>
