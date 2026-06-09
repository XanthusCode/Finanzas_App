<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <div class="section-label">Configuracion</div>
        <h1 class="page-title">Categorías</h1>
      </div>
    </div>

    <div v-if="store.loading" class="empty">Cargando...</div>

    <template v-else>
      <!-- Fijas -->
      <div class="card" style="margin-bottom: 0.75rem;">
        <p class="card-title">Gastos fijos</p>

        <div v-if="store.categoriasFijas.length === 0 && !(showForm && form.tipo === 'FIJO')" class="empty">
          Sin categorías fijas
        </div>

        <template v-for="cat in store.categoriasFijas" :key="cat.id">
          <div v-if="editando?.id === cat.id" class="inline-form-row">
            <span class="tag tag-red">Fijo</span>
            <input v-model="form.nombre" class="input input-inline" @keydown.enter.prevent="guardar" @keydown.escape="cancelar" autofocus />
            <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar">{{ saving ? '...' : 'Guardar' }}</button>
            <button class="btn btn-sm" @click="cancelar">Cancelar</button>
          </div>
          <div v-else class="cat-row">
            <div class="cat-info">
              <span class="tag tag-red">Fijo</span>
              <span class="cat-name">{{ cat.nombre }}</span>
            </div>
            <div class="cat-actions">
              <button class="icon-btn" title="Editar" @click="abrirForm(cat)">✎</button>
              <button class="icon-btn danger" title="Eliminar" @click="onDelete(cat.id!)">✕</button>
            </div>
          </div>
        </template>

        <div v-if="showForm && form.tipo === 'FIJO' && !editando" class="inline-form-row">
          <span class="tag tag-red">Fijo</span>
          <input v-model="form.nombre" class="input input-inline" placeholder="Nombre de la categoría" @keydown.enter.prevent="guardar" @keydown.escape="cancelar" autofocus />
          <button class="btn btn-sm" @click="cancelar">Cancelar</button>
          <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar">{{ saving ? '...' : 'Guardar' }}</button>
        </div>

        <button v-if="!(showForm && form.tipo === 'FIJO')" class="btn-add" @click="abrirForm(undefined, 'FIJO')">+ Nueva categoría</button>
      </div>

      <!-- Variables -->
      <div class="card">
        <p class="card-title">Gastos variables</p>

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
              <button class="icon-btn" title="Editar" @click="abrirForm(cat)">✎</button>
              <button class="icon-btn danger" title="Eliminar" @click="onDelete(cat.id!)">✕</button>
            </div>
          </div>
        </template>

        <div v-if="showForm && form.tipo === 'VARIABLE' && !editando" class="inline-form-row">
          <span class="tag tag-amber">Variable</span>
          <input v-model="form.nombre" class="input input-inline" placeholder="Nombre de la categoría" @keydown.enter.prevent="guardar" @keydown.escape="cancelar" autofocus />
          <button class="btn btn-sm" @click="cancelar">Cancelar</button>
          <button class="btn btn-primary btn-sm" :disabled="saving" @click="guardar">{{ saving ? '...' : 'Guardar' }}</button>
        </div>

        <button v-if="!(showForm && form.tipo === 'VARIABLE')" class="btn-add" @click="abrirForm(undefined, 'VARIABLE')">+ Nueva categoría</button>
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
import type { Categoria } from '@/types'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const store = useFinanceStore()

const showForm    = ref(false)
const saving      = ref(false)
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
  if (editando.value?.id) {
    await store.actualizarCategoria(editando.value.id, payload)
  } else {
    await store.agregarCategoria(payload)
  }
  saving.value   = false
  showForm.value = false
  editando.value = null
  form.nombre    = ''
}

function onDelete(id: string) {
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

.btn-add {
  display: block;
  width: 100%;
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  font-size: 0.72rem;
  text-align: right;
  padding: 1rem 0 0.1rem;
  transition: color 0.2s;
}
.btn-add:hover { color: var(--accent); }

.icon-btn     { background: none; border: none; color: var(--text-muted); cursor: pointer; font-size: 0.85rem; padding: 3px 6px; border-radius: 3px; transition: all 0.2s; }
.icon-btn:hover       { color: var(--accent); background: rgba(99,179,255,0.08); }
.icon-btn.danger:hover { color: var(--red);   background: rgba(248,113,113,0.08); }
.empty { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }
</style>
