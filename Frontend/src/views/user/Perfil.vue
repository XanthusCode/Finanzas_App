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

    <div class="settings-grid">
       <!-- Datos del perfil -->
        <div class="card">
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

            <div class="field">
              <label>WhatsApp</label>
              <div class="display-row">
                <span class="display-val" :class="{ muted: !whatsappVinculado }">
                  {{ whatsappVinculado === undefined ? '—' : whatsappVinculado ? 'Vinculado' : 'Sin vincular' }}
                </span>
                <button
                  v-if="whatsappVinculado" class="btn btn-sm" :disabled="savingWhatsapp"
                  @click="desvincularWhatsapp"
                >Desvincular</button>
                <button
                  v-else-if="whatsappVinculado === false && !codigoWhatsapp" class="btn btn-sm" :disabled="savingWhatsapp"
                  @click="generarCodigoWhatsapp"
                >{{ savingWhatsapp ? '...' : 'Vincular' }}</button>
              </div>
              <div v-if="codigoWhatsapp && !whatsappVinculado" class="wa-codigo">
                <template v-if="WHATSAPP_JOIN">
                  <p class="toggle-hint">
                    <strong>1.</strong> Activa el bot enviando este mensaje al <strong>{{ WHATSAPP_BOT_DISPLAY }}</strong>
                    (si dejas de usarlo por unos días, hay que enviarlo de nuevo):
                  </p>
                  <code class="wa-codigo-msg">{{ WHATSAPP_JOIN }}</code>
                  <div class="edit-row">
                    <a class="btn btn-sm" :href="waUrl(WHATSAPP_JOIN)" target="_blank" rel="noopener">Activar bot</a>
                  </div>
                </template>
                <p class="toggle-hint">
                  <template v-if="WHATSAPP_JOIN"><strong>2.</strong> Cuando el bot te responda, envía</template>
                  <template v-else>Envía este mensaje al <strong>{{ WHATSAPP_BOT_DISPLAY }}</strong></template>
                  (el código vence en 15 minutos):
                </p>
                <code class="wa-codigo-msg">vincular {{ codigoWhatsapp }}</code>
                <div class="edit-row">
                  <a class="btn btn-primary btn-sm" :href="waUrl(`vincular ${codigoWhatsapp}`)" target="_blank" rel="noopener">Vincular en WhatsApp</a>
                  <button class="btn btn-sm" @click="cargarWhatsapp">Ya lo envié</button>
                </div>
              </div>
              <p v-else class="toggle-hint">
                Registra gastos escribiéndole al bot, por ejemplo <strong>25000 almuerzo</strong>.
              </p>
            </div>
          </div>
        </div>

        <!-- Apariencia -->
        <div class="card">
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
                  :style="{ background: preset.value ?? DEFAULT_ACCENT_COLOR }"
                  :title="preset.name"
                  @click="setAccent(preset.value)"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Recordatorios por email -->
        <div class="card" >
          <p class="card-title">Notificaciones</p>

          <div class="toggle-row">
            <div>
              <div class="display-val">Recordatorios por email</div>
              <p class="toggle-hint">
                Te escribimos si llevas varios días sin registrar gastos, si tienes cobros por confirmar
                o deudas por vencer. Máximo uno cada pocos días.
              </p>
            </div>
            <button
              type="button"
              role="switch"
              class="switch"
              :class="{ 'switch--on': recordatorios }"
              :aria-checked="recordatorios ?? false"
              :disabled="recordatorios === null || savingRecordatorios"
              aria-label="Recordatorios por email"
              @click="toggleRecordatorios"
            >
              <span class="switch-thumb" />
            </button>
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Sun, Moon } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { userFromToken } from '@/utils'
import { useAppearance, ACCENT_PRESETS } from '@/composables/useAppearance'
import { perfilService } from '@/services/api'

const auth   = useAuthStore()
const toast  = useToast()
const router = useRouter()
const { theme, accent, setAccent } = useAppearance()
const DEFAULT_ACCENT_COLOR = '#2d5e80' // pon aquí el color original de Acero

const user = computed(() => auth.token ? userFromToken(auth.token) : null)

// Recordatorios (null mientras carga: el switch queda deshabilitado)
const recordatorios       = ref<boolean | null>(null)
const savingRecordatorios = ref(false)

onMounted(async () => {
  cargarWhatsapp()
  try {
    const { data } = await perfilService.getRecordatorios()
    recordatorios.value = data.activos
  } catch {
    toast.error('No se pudo cargar la preferencia de recordatorios')
  }
})
async function toggleRecordatorios() {
  if (recordatorios.value === null) return
  const nuevo = !recordatorios.value
  recordatorios.value = nuevo
  savingRecordatorios.value = true
  try {
    await perfilService.setRecordatorios(nuevo)
    toast.success(nuevo ? 'Recordatorios activados' : 'Recordatorios desactivados')
  } catch {
    recordatorios.value = !nuevo
    toast.error('No se pudo guardar la preferencia')
  } finally {
    savingRecordatorios.value = false
  }
}

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

// WhatsApp (undefined mientras carga). Se vincula con un código que el usuario envía al bot:
// Twilio no siempre entrega el número (usuarios con nombre de usuario de WhatsApp).
const WHATSAPP_BOT = '14155238886'
const WHATSAPP_BOT_DISPLAY = '+1 415 523 8886'
const whatsappVinculado = ref<boolean | undefined>(undefined)
const codigoWhatsapp    = ref<string | null>(null)
const savingWhatsapp    = ref(false)
// Frase "join xxx-yyy" del sandbox de Twilio (cada número debe enviarla antes de usar el bot).
const WHATSAPP_JOIN = (import.meta.env.VITE_WHATSAPP_JOIN as string | undefined)?.trim() || 'join stems-hunter'
const waUrl = (texto: string) => `https://wa.me/${WHATSAPP_BOT}?text=${encodeURIComponent(texto)}`

async function cargarWhatsapp() {
  try {
    const { data } = await perfilService.getWhatsapp()
    whatsappVinculado.value = data.vinculado
    if (data.vinculado) codigoWhatsapp.value = null
  } catch {
    toast.error('No se pudo cargar el estado de WhatsApp')
  }
}

async function generarCodigoWhatsapp() {
  savingWhatsapp.value = true
  try {
    const { data } = await perfilService.generarCodigoWhatsapp()
    codigoWhatsapp.value = data.codigo
  } catch {
    toast.error('No se pudo generar el código')
  } finally {
    savingWhatsapp.value = false
  }
}

async function desvincularWhatsapp() {
  savingWhatsapp.value = true
  try {
    await perfilService.desvincularWhatsapp()
    whatsappVinculado.value = false
    toast.success('WhatsApp desvinculado')
  } catch {
    toast.error('No se pudo desvincular')
  } finally {
    savingWhatsapp.value = false
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

.settings-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

.card {
  padding: 1.25rem;
  border-radius: 8px;
  background: var(--surface2);
  border: 1px solid var(--border);
}
.card-title  { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 1rem; }

.field-group { display: flex; flex-direction: column; gap: 1rem; }
.field       { display: flex; flex-direction: column; gap: 0.3rem; }
label        { font-size: 0.65rem; color: var(--text-secondary); letter-spacing: 0.05em; text-transform: uppercase; }

.display-row { display: flex; align-items: center; gap: 0.75rem; }
.edit-row    { display: flex; align-items: center; gap: 0.5rem; }
.display-val { font-size: 0.82rem; color: var(--text-primary); }
.display-val.muted { color: var(--text-muted); }

.toggle-row  { display: flex; align-items: center; justify-content: space-between; gap: 1rem; }
.wa-codigo     { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.4rem; }
.wa-codigo-msg { align-self: flex-start; padding: 0.4rem 0.7rem; border-radius: 6px; background: var(--surface); border: 1px solid var(--border); font-size: 0.95rem; letter-spacing: 0.04em; user-select: all; }
.toggle-hint { margin-top: 0.3rem; font-size: 0.72rem; line-height: 1.45; color: var(--text-muted); max-width: 110ch; }

.switch {
  position: relative;
  flex-shrink: 0;
  width: 38px;
  height: 22px;
  padding: 0;
  border-radius: 999px;
  background: var(--surface2);
  border: 1px solid var(--border);
  cursor: pointer;
  transition: background 0.2s, border-color 0.2s;
}
.switch:disabled { opacity: 0.5; cursor: default; }
.switch-thumb {
  position: absolute;
  top: 2px;
  left: 2px;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: var(--text-secondary);
  transition: transform 0.2s cubic-bezier(0.2, 0, 0, 1), background 0.2s;
}
.switch--on { background: var(--accent); border-color: var(--accent); }
.switch--on .switch-thumb { transform: translateX(16px); background: #fff; }

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
