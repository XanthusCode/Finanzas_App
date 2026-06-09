<template>
  <div class="user-menu" ref="menuRef">
    <button class="avatar-btn" @click="toggleDropdown" :title="userFromToken(auth.token ?? '')?.nombre">
      <span class="avatar-initials">{{ initials }}</span>
    </button>

    <Transition name="dropdown">
      <div v-if="open" class="dropdown">
        <div class="dropdown-header">
          <span class="dropdown-name">{{ userFromToken(auth.token ?? '')?.nombre }}</span>
        </div>
        <div class="dropdown-divider" />
        <button class="dropdown-item" @click="openChangePassword">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
          Cambiar contraseña
        </button>
        <div class="dropdown-divider" />
        <button class="dropdown-item dropdown-item--danger" @click="handleLogout">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
          Cerrar sesión
        </button>
      </div>
    </Transition>
  </div>

  <!-- Modal cambiar contraseña -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
        <div class="modal-card">
          <div class="modal-header">
            <h2 class="modal-title">Cambiar contraseña</h2>
            <button class="modal-close" @click="closeModal">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>

          <form class="modal-form" @submit.prevent="onSubmit">
            <div class="field">
              <label class="field-label">Contraseña actual</label>
              <input
                v-model="form.current"
                type="password"
                class="field-input"
                :class="{ 'input-error': errors.current }"
                placeholder="••••••••"
                autocomplete="current-password"
              />
              <span v-if="errors.current" class="field-error">{{ errors.current }}</span>
            </div>

            <div class="field">
              <label class="field-label">Nueva contraseña</label>
              <input
                v-model="form.next"
                type="password"
                class="field-input"
                :class="{ 'input-error': errors.next }"
                placeholder="Mínimo 6 caracteres"
                autocomplete="new-password"
              />
              <span v-if="errors.next" class="field-error">{{ errors.next }}</span>
            </div>

            <div class="field">
              <label class="field-label">Confirmar nueva contraseña</label>
              <input
                v-model="form.confirm"
                type="password"
                class="field-input"
                :class="{ 'input-error': errors.confirm }"
                placeholder="••••••••"
                autocomplete="new-password"
              />
              <span v-if="errors.confirm" class="field-error">{{ errors.confirm }}</span>
            </div>

            <p v-if="serverError" class="server-error">{{ serverError }}</p>
            <p v-if="success" class="server-success">Contraseña actualizada correctamente.</p>

            <div class="modal-actions">
              <button type="button" class="btn-cancel" @click="closeModal">Cancelar</button>
              <button type="submit" class="btn-submit" :disabled="loading">
                <span v-if="loading" class="spinner" />
                {{ loading ? 'Guardando...' : 'Guardar' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { z } from 'zod'
import { userFromToken } from '@/utils/index'

const auth = useAuthStore()
const router = useRouter()
const menuRef = ref<HTMLElement | null>(null)
const open = ref(false)
const showModal = ref(false)
const loading = ref(false)
const serverError = ref('')
const success = ref(false)

const form = reactive({ current: '', next: '', confirm: '' })
const errors = reactive({ current: '', next: '', confirm: '' })

const initials = computed(() => {
  const name = userFromToken(auth.token ?? '')?.nombre ?? ''
  return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase()
})

const schema = z.object({
  current:  z.string().min(1, { message: 'Ingresa tu contraseña actual' }),
  next:     z.string().min(6, { message: 'Mínimo 6 caracteres' }),
  confirm:  z.string()
}).refine(d => d.next === d.confirm, {
  message: 'Las contraseñas no coinciden',
  path: ['confirm']
})

function toggleDropdown() { open.value = !open.value }

function handleClickOutside(e: MouseEvent) {
  if (menuRef.value && !menuRef.value.contains(e.target as Node)) {
    open.value = false
  }
}

onMounted(() => document.addEventListener('mousedown', handleClickOutside))
onUnmounted(() => document.removeEventListener('mousedown', handleClickOutside))

function openChangePassword() {
  open.value = false
  form.current = ''
  form.next = ''
  form.confirm = ''
  errors.current = ''
  errors.next = ''
  errors.confirm = ''
  serverError.value = ''
  success.value = false
  showModal.value = true
}

function closeModal() {
  showModal.value = false
}

function handleLogout() {
  auth.logout()
  router.push('/login')
}

async function onSubmit() {
  errors.current = ''
  errors.next = ''
  errors.confirm = ''
  serverError.value = ''
  success.value = false

  const result = schema.safeParse(form)
  if (!result.success) {
    result.error.issues.forEach(i => {
      const f = i.path[0] as keyof typeof errors
      if (f in errors) errors[f] = i.message
    })
    return
  }

  loading.value = true
  const err = await auth.changePassword(form.current, form.next)
  loading.value = false

  if (err) { serverError.value = err; return }
  success.value = true
  setTimeout(() => closeModal(), 1500)
}
</script>

<style scoped>
.user-menu {
  position: relative;
}

.avatar-btn {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: rgba(99,179,255,0.15);
  border: 1px solid rgba(99,179,255,0.3);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
}
.avatar-btn:hover { background: rgba(99,179,255,0.25); }

.avatar-initials {
  font-size: 0.68rem;
  font-weight: 700;
  color: var(--accent);
  letter-spacing: 0.04em;
}

/* Dropdown */
.dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  min-width: 200px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.25);
  z-index: 100;
  overflow: hidden;
}

.dropdown-header {
  padding: 0.85rem 1rem 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.dropdown-name {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-primary);
}

.dropdown-email {
  font-size: 0.7rem;
  color: var(--text-muted);
}

.dropdown-divider {
  height: 1px;
  background: var(--border);
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  width: 100%;
  padding: 0.65rem 1rem;
  font-size: 0.78rem;
  color: var(--text-secondary);
  background: none;
  border: none;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
  text-align: left;
}
.dropdown-item:hover { background: var(--surface2); color: var(--text-primary); }
.dropdown-item--danger:hover { background: rgba(245,101,101,0.08); color: #f56565; }

/* Dropdown animation */
.dropdown-enter-active, .dropdown-leave-active { transition: opacity 0.15s, transform 0.15s; }
.dropdown-enter-from, .dropdown-leave-to { opacity: 0; transform: translateY(-6px); }

/* Modal overlay */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
  padding: 1rem;
}

.modal-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.75rem;
  width: 100%;
  max-width: 380px;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.modal-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: color 0.2s;
}
.modal-close:hover { color: var(--text-primary); }

.modal-form { display: flex; flex-direction: column; gap: 1rem; }

.field { display: flex; flex-direction: column; gap: 0.3rem; }
.field-label { font-size: 0.73rem; color: var(--text-secondary); font-weight: 500; }
.field-input {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 6px;
  padding: 0.55rem 0.75rem;
  font-size: 0.83rem;
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s;
}
.field-input:focus { border-color: var(--accent); }
.field-input.input-error { border-color: #f56565; }
.field-error { font-size: 0.7rem; color: #f56565; }

.server-error {
  font-size: 0.78rem;
  color: #f56565;
  background: rgba(245,101,101,0.08);
  border: 1px solid rgba(245,101,101,0.2);
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  margin: 0;
}
.server-success {
  font-size: 0.78rem;
  color: #68d391;
  background: rgba(104,211,145,0.08);
  border: 1px solid rgba(104,211,145,0.2);
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  margin: 0;
}

.modal-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 0.25rem;
}

.btn-cancel {
  padding: 0.55rem 1rem;
  border-radius: 6px;
  font-size: 0.82rem;
  background: none;
  border: 1px solid var(--border);
  color: var(--text-secondary);
  cursor: pointer;
  transition: border-color 0.2s, color 0.2s;
}
.btn-cancel:hover { border-color: var(--text-muted); color: var(--text-primary); }

.btn-submit {
  padding: 0.55rem 1.25rem;
  border-radius: 6px;
  font-size: 0.82rem;
  font-weight: 600;
  background: var(--accent);
  color: #fff;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.4rem;
  transition: opacity 0.2s;
}
.btn-submit:hover:not(:disabled) { opacity: 0.88; }
.btn-submit:disabled { opacity: 0.6; cursor: not-allowed; }

.spinner {
  width: 12px;
  height: 12px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* Modal animation */
.modal-enter-active, .modal-leave-active { transition: opacity 0.2s; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.modal-enter-active .modal-card, .modal-leave-active .modal-card { transition: transform 0.2s; }
.modal-enter-from .modal-card, .modal-leave-to .modal-card { transform: scale(0.95); }
</style>
