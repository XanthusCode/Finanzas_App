<template>
  <div class="fade-up">
    <div v-if="categorias.length === 0 && !showForm" class="empty">
      Sin categorías
    </div>

    <div v-else class="cat-grid">
      <!-- Card de creación: va primero para que se vea al darle "Nueva categoría" -->
      <div v-if="showForm && !editando" class="card card--editing">
        <div class="inline-form">
          <div class="inline-form-top">
            <div class="cat-icon-badge" :style="{ background: guessColor(nombre) + '26', color: guessColor(nombre) }">
              <component :is="guessIcono(nombre)" :size="20" :stroke-width="1.8" />
            </div>
            <div class="input-wrap">
              <input v-model="nombre" class="input-inline" maxlength="100" placeholder="Nombre de la categoría" @keydown.enter.prevent="emit('guardar')" @keydown.escape="emit('cancelar')" autofocus />
              <span class="char-counter" :class="{ 'char-counter--limit': nombre.length >= 100 }">{{ nombre.length }}/100</span>
            </div>
          </div>
          <div class="inline-form-actions">
            <button class="btn btn-sm" @click="emit('cancelar')">Cancelar</button>
            <button class="btn btn-primary btn-sm" :disabled="saving || !nombre.trim()" @click="emit('guardar')">{{ saving ? '...' : 'Guardar' }}</button>
          </div>
        </div>
      </div>

      <div
        v-for="cat in categorias"
        :key="cat.id"
        class="card"
        :class="{ 'card--editing': editando?.id === cat.id, 'card--clickable': editando?.id !== cat.id }"
        :role="editando?.id !== cat.id ? 'button' : undefined"
        :tabindex="editando?.id !== cat.id ? 0 : undefined"
        :title="editando?.id !== cat.id ? 'Ver gastos de esta categoría' : undefined"
        @click="editando?.id !== cat.id && emit('ver', cat)"
        @keydown.enter.self="editando?.id !== cat.id && emit('ver', cat)"
      >
        <div v-if="editando?.id === cat.id" class="inline-form">
          <div class="inline-form-top">
           <div class="cat-icon-badge" :style="{ background: guessColor(nombre) + '26', color: guessColor(nombre) }">
              <component :is="guessIcono(nombre)" :size="20" :stroke-width="1.8" />
            </div>
            <div class="input-wrap">
              <input v-model="nombre" class="input-inline" maxlength="100" @keydown.enter.prevent="emit('guardar')" @keydown.escape="emit('cancelar')" autofocus />
              <span class="char-counter" :class="{ 'char-counter--limit': nombre.length >= 100 }">{{ nombre.length }}/100</span>
            </div>
          </div>
          <div class="inline-form-actions">
            <button class="btn btn-sm" @click="emit('cancelar')">Cancelar</button>
            <button class="btn btn-primary btn-sm" :disabled="saving || !nombre.trim()" @click="emit('guardar')">{{ saving ? '...' : 'Guardar' }}</button>
          </div>
        </div>

        <div v-else class="cat-row">
          <div class="cat-info">
            <div class="cat-icon-badge" :style="{ background: guessColor(cat.nombre) + '26', color: guessColor(cat.nombre) }">
              <component :is="guessIcono(cat.nombre)" :size="20" :stroke-width="1.8" />
            </div>
            <span class="cat-name">{{ cat.nombre }}</span>
          </div>
          <div class="cat-actions">
            <button class="icon-btn" title="Editar" aria-label="Editar categoría" @click.stop="emit('abrir-form', cat)">
              <Pencil :size="14" :stroke-width="1.9" />
            </button>
            <button class="icon-btn danger" title="Eliminar" aria-label="Eliminar categoría" @click.stop="emit('delete', cat.id!)">
              <Trash2 :size="14" :stroke-width="1.9" />
            </button>
          </div>
          <ChevronRight :size="15" :stroke-width="2" class="cat-chevron" aria-hidden="true" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Pencil, Trash2, ChevronRight } from 'lucide-vue-next'
import { guessIcono, guessColor } from '@/composables/useCategoriaOptions'
import type { Categoria } from '@/types'

defineProps<{
  categorias: Categoria[]
  showForm: boolean
  saving: boolean
  editando: Categoria | null
}>()

const nombre = defineModel<string>('nombre', { required: true })

const emit = defineEmits<{
  'abrir-form': [cat?: Categoria]
  guardar: []
  cancelar: []
  delete: [id: string]
  ver: [cat: Categoria]
}>()
</script>

<style scoped>
.cat-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.75rem;
}

/* Sin ruido: borde muy sutil, se ilumina al pasar el mouse */
.card {
  position: relative;
  background: var(--surface);
  border: 1px solid color-mix(in srgb, var(--border) 75%, transparent);
  border-radius: 12px;
  padding: 0.85rem 0.95rem;
  transition: background 0.15s ease, border-color 0.15s ease;
}
.card--clickable { cursor: pointer; }
.card--clickable:hover,
.card--clickable:focus-visible {
  background: var(--surface2);
  border-color: color-mix(in srgb, var(--primary) 45%, var(--border));
  outline: none;
}
.card--editing {
  border-color: color-mix(in srgb, var(--accent) 55%, transparent);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--accent) 12%, transparent);
}

.cat-row     { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
.cat-info    { display: flex; align-items: center; gap: 0.8rem; min-width: 0; }
.cat-name    { font-size: 0.88rem; font-weight: 500; color: var(--text-primary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* Acciones solo en hover/foco (en táctil siempre visibles); la flecha indica que abre los gastos */
.cat-actions {
  display: flex; gap: 0.1rem;
  opacity: 0; transform: translateX(4px);
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.cat-chevron { flex-shrink: 0; color: var(--text-muted); transition: opacity 0.15s ease; }
.card:hover .cat-actions,
.card:focus-within .cat-actions { opacity: 1; transform: none; }
.card:hover .cat-chevron,
.card:focus-within .cat-chevron { display: none; }
@media (hover: none) {
  .cat-actions { opacity: 1; transform: none; }
  .cat-chevron { display: none; }
}

.cat-icon-badge {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.inline-form         { display: flex; flex-direction: column; gap: 0.6rem; }
.inline-form-top     { display: flex; align-items: center; gap: 0.75rem; }
.inline-form-actions { display: flex; justify-content: flex-end; gap: 0.4rem; }

.input-wrap { position: relative; flex: 1; min-width: 0; }
.input-inline {
  width: 100%;
  height: 36px;
  padding: 0 3.2rem 0 0.7rem;
  font-size: 0.88rem;
  font-weight: 600;
  color: var(--text-primary);
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 8px;
  outline: none;
  transition: border-color 0.15s;
}
.input-inline:focus { border-color: var(--accent); box-shadow: var(--focus-ring); }
.input-inline::placeholder { color: var(--text-muted); font-weight: 400; }

.char-counter {
  position: absolute;
  right: 0.6rem;
  top: 50%;
  transform: translateY(-50%);
  font-size: 0.62rem;
  color: var(--text-muted);
  pointer-events: none;
}
.char-counter--limit { color: var(--red); }


.icon-btn {
  display: grid; place-items: center;
  width: 30px; height: 30px; padding: 0;
  background: none; border: none; border-radius: 8px;
  color: var(--text-secondary); cursor: pointer;
  transition: color 0.15s, background 0.15s;
}
.icon-btn:hover        { color: var(--text-primary); background: var(--surface); }
.icon-btn.danger:hover { color: var(--c-neg); background: color-mix(in srgb, var(--c-neg) 12%, transparent); }
.icon-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }
.empty { font-size: 0.72rem; color: var(--text-muted); padding: 1rem 0; text-align: center; }

@media (max-width: 900px) {
  .cat-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 560px) {
  .cat-grid { grid-template-columns: 1fr; }
}
</style>