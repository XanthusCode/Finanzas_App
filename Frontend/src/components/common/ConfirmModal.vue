<template>
  <Teleport to="body">
    <div v-if="modelValue" class="overlay" @click.self="emit('update:modelValue', false)">
      <div class="modal fade-up">
        <p class="modal-title">{{ title }}</p>
        <p class="modal-msg">{{ message }}</p>
        <div class="modal-actions">
          <button class="btn" @click="emit('update:modelValue', false)">Cancelar</button>
          <button class="btn btn-danger" @click="confirm">{{ confirmText }}</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
// El botón nombra la acción ("Eliminar", "Quitar límite") en vez de un genérico "Confirmar"
withDefaults(defineProps<{ modelValue: boolean; title: string; message: string; confirmText?: string }>(), {
  confirmText: 'Eliminar',
})
const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
}>()

function confirm() {
  emit('confirm')
  emit('update:modelValue', false)
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
  width: 320px;
  box-shadow: var(--shadow-pop);
}
.modal-title {
  font-family: var(--ff-display);
  font-weight: 600;
  font-size: 1rem;
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}
.modal-msg {
  font-size: 0.78rem;
  color: var(--text-secondary);
  line-height: 1.7;
  margin-bottom: 1.25rem;
}
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}
</style>