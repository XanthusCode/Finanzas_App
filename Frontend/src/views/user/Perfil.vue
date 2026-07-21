<template>
  <div class="fade-up">
    <div class="page-header">
      <div>
        <button class="back-btn" title="Volver" @click="router.back()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
          Volver
        </button>
        <div class="section-label">Cuenta</div>
        <h1 class="page-title">Mi perfil</h1>
      </div>
    </div>

    <!-- Datos del perfil -->
    <div class="card" style="margin-bottom: 0.75rem;">
      <p class="card-title">Información personal</p>

      <div class="field-group">
        <div class="field">
          <label>Nombre</label>
          <div v-if="!editNombre" class="display-row">
            <span class="display-val">{{ user?.nombre ?? '—' }}</span>
            <button class="btn btn-sm" @click="startEditNombre">Editar</button>
          </div>
          <div v-else class="edit-row">
            <input v-model="nuevoNombre" class="input" @keydown.enter="guardarNombre" @keydown.escape="editNombre = false" autofocus />
            <button class="btn btn-sm" @click="editNombre = false">Cancelar</button>
            <button class="btn btn-primary btn-sm" :disabled="savingNombre" @click="guardarNombre">{{ savingNombre ? '...' : 'Guardar' }}</button>
          </div>
        </div>

        <div class="field">
          <label>Correo electrónico</label>
          <span class="display-val muted">{{ user?.email ?? '—' }}</span>
        </div>
      </div>
    </div>

    <!-- Apariencia -->
    <div class="card" style="margin-bottom: 0.75rem;">
      <p class="card-title">Apariencia</p>

      <div class="field-group">
        <div class="field">
          <label>Tema</label>
          <div class="theme-options">
            <button class="theme-option" :class="{ 'theme-option--active': theme === 'dark' }" @click="theme = 'dark'">
              <Moon :size="14" :stroke-width="2" /> Oscuro
            </button>
            <button class="theme-option" :class="{ 'theme-option--active': theme === 'light' }" @click="theme = 'light'">
              <Sun :size="14" :stroke-width="2" /> Claro
            </button>
          </div>
        </div>

        <div class="field">
          <label>Color de acento</label>
          <div class="accent-swatches">
            <button
              v-for="preset in ACCENT_PRESETS" :key="preset.name" type="button"
              class="accent-swatch"
              :class="{ 'accent-swatch--active': accent === preset.value }"
              :style="{ background: preset.value ?? '#63b3ff' }"
              :title="preset.name"
              @click="setAccent(preset.value)"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Cambiar contraseña -->
    <div class="card">
      <p class="card-title">Cambiar contraseña</p>

      <div class="field-group">
        <div class="field">
          <label>Contraseña actual</label>
          <input v-model="pwForm.actual" type="password" class="input" :class="{ 'input-error': pwErrors.actual }" />
          <span v-if="pwErrors.actual" class="field-error">{{ pwErrors.actual }}</span>
        </div>
        <div class="field">
          <label>Nueva contraseña</label>
          <input v-model="pwForm.nueva" type="password" class="input" :class="{ 'input-error': pwErrors.nueva }" />
          <span v-if="pwErrors.nueva" class="field-error">{{ pwErrors.nueva }}</span>
        </div>
        <div class="field">
          <label>Confirmar nueva contraseña</label>
          <input v-model="pwForm.confirmar" type="password" class="input" :class="{ 'input-error': pwErrors.confirmar }" />
          <span v-if="pwErrors.confirmar" class="field-error">{{ pwErrors.confirmar }}</span>
        </div>
        <div class="form-actions">
          <button class="btn btn-primary" :disabled="savingPw" @click="cambiarPassword">
            {{ savingPw ? 'Guardando...' : 'Cambiar contraseña' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { Sun, Moon } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { userFromToken } from '@/utils'
import { useAppearance, ACCENT_PRESETS } from '@/composables/useAppearance'

const auth   = useAuthStore()
const toast  = useToast()
const router = useRouter()
const { theme, accent, setAccent } = useAppearance()

const user = computed(() => auth.token ? userFromToken(auth.token) : null)

// Nombre
const editNombre   = ref(false)
const nuevoNombre  = ref('')
const savingNombre = ref(false)

function startEditNombre() {
  nuevoNombre.value = user.value?.nombre ?? ''
  editNombre.value  = true
}

async function guardarNombre() {
  if (!nuevoNombre.value.trim()) return
  savingNombre.value = true
  const error = await auth.updateNombre(nuevoNombre.value.trim())
  savingNombre.value = false
  if (error) {
    toast.error(error)
  } else {
    toast.success('Nombre actualizado')
    editNombre.value = false
  }
}

// Contraseña
const pwForm   = reactive({ actual: '', nueva: '', confirmar: '' })
const pwErrors = ref<Partial<Record<'actual' | 'nueva' | 'confirmar', string>>>({})
const savingPw = ref(false)

async function cambiarPassword() {
  pwErrors.value = {}
  if (!pwForm.actual)           { pwErrors.value.actual    = 'Ingresa tu contraseña actual'; return }
  if (pwForm.nueva.length < 6)  { pwErrors.value.nueva     = 'Mínimo 6 caracteres'; return }
  if (pwForm.nueva !== pwForm.confirmar) { pwErrors.value.confirmar = 'Las contraseñas no coinciden'; return }

  savingPw.value = true
  const error = await auth.changePassword(pwForm.actual, pwForm.nueva)
  savingPw.value = false

  if (error) {
    toast.error(error)
  } else {
    toast.success('Contraseña actualizada')
    pwForm.actual    = ''
    pwForm.nueva     = ''
    pwForm.confirmar = ''
  }
}
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title  { font-size: 1.8rem; font-weight: 800; letter-spacing: -0.02em; margin-top: 0.25rem; }

.back-btn {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  background: none;
  border: none;
  color: var(--text-secondary);
  font-size: 0.75rem;
  cursor: pointer;
  padding: 0;
  margin-bottom: 0.75rem;
  transition: color 0.15s;
}
.back-btn:hover { color: var(--accent); }
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 1rem; }

.field-group { display: flex; flex-direction: column; gap: 1rem; }
.field       { display: flex; flex-direction: column; gap: 0.3rem; }
label        { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.05em; text-transform: uppercase; }

.display-row { display: flex; align-items: center; gap: 0.75rem; }
.edit-row    { display: flex; align-items: center; gap: 0.5rem; }
.display-val { font-size: 0.82rem; color: var(--text-primary); }
.display-val.muted { color: var(--text-muted); }

.theme-options { display: flex; gap: 0.5rem; }
.theme-option {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  font-size: 0.78rem;
  background: var(--surface2);
  border: 1px solid var(--border);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.15s;
}
.theme-option--active { border-color: var(--accent); color: var(--accent); background: color-mix(in srgb, var(--accent) 8%, transparent); }

.accent-swatches { display: flex; align-items: center; gap: 0.5rem; }
.accent-swatch {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  border: 2px solid transparent;
  cursor: pointer;
  padding: 0;
  transition: transform 0.1s, border-color 0.15s;
}
.accent-swatch:hover { transform: scale(1.12); }
.accent-swatch--active { border-color: var(--text-primary); }

.form-actions { display: flex; justify-content: flex-end; margin-top: 0.5rem; }
.btn-sm      { padding: 0.3rem 0.7rem; font-size: 0.72rem; }
.field-error { font-size: 0.65rem; color: var(--red); }
.input-error { border-color: var(--red) !important; }
</style>
