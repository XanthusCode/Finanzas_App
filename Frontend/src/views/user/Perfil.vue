<template>
  <div class="fade-up perfil">
    <div class="page-header">
      <div>
        <button class="back-btn" title="Volver" @click="router.back()">
          <ArrowLeft :size="14" :stroke-width="2" />
          Volver
        </button>
        <div class="section-label">Cuenta</div>
        <h1 class="page-title">Mi perfil</h1>
      </div>
    </div>

    <div class="settings-grid">
      <!-- ── Columna izquierda ── -->
      <div class="settings-col">
        <!-- Información personal -->
        <section class="card pf-card">
          <h2 class="pf-title">Información personal</h2>

          <div class="identity">
            <span class="identity-avatar">{{ iniciales }}</span>
            <div class="identity-main">
              <div v-if="!editNombre" class="name-row">
                <span class="name">{{ user?.nombre ?? '—' }}</span>
                <button class="icon-btn" title="Editar nombre" aria-label="Editar nombre" @click="startEditNombre">
                  <Pencil :size="14" :stroke-width="1.9" />
                </button>
              </div>
              <div v-else class="name-edit">
                <input
                  v-model="nuevoNombre"
                  class="pf-input"
                  aria-label="Nombre"
                  maxlength="80"
                  autofocus
                  @keydown.enter="guardarNombre"
                  @keydown.escape="editNombre = false"
                />
                <button class="icon-btn" title="Cancelar" aria-label="Cancelar" @click="editNombre = false">
                  <X :size="15" :stroke-width="2" />
                </button>
                <button class="icon-btn icon-btn--ok" title="Guardar" aria-label="Guardar nombre" :disabled="savingNombre" @click="guardarNombre">
                  <Check :size="15" :stroke-width="2.4" />
                </button>
              </div>
              <span class="email"><Mail :size="13" :stroke-width="2" /> {{ user?.email ?? '—' }}</span>
            </div>
          </div>

          <!-- WhatsApp -->
          <div class="wa">
            <div class="wa-row">
              <span class="wa-icon"><MessageCircle :size="17" :stroke-width="2" /></span>
              <div class="wa-main">
                <span class="wa-name">WhatsApp</span>
                <span class="wa-sub">Registra gastos escribiéndole al bot, por ejemplo <strong>25000 almuerzo</strong>.</span>
              </div>
              <span v-if="whatsappVinculado !== undefined" class="status" :class="whatsappVinculado ? 'status--on' : 'status--off'">
                <span class="status-dot" />{{ whatsappVinculado ? 'Vinculado' : 'Sin vincular' }}
              </span>
            </div>

            <div class="wa-actions">
              <button v-if="whatsappVinculado" class="btn btn-sm btn-ghost" :disabled="savingWhatsapp" @click="desvincularWhatsapp">
                <Unlink :size="13" :stroke-width="2" /> Desvincular
              </button>
              <button
                v-else-if="whatsappVinculado === false && !codigoWhatsapp"
                class="btn btn-sm btn-primary"
                :disabled="savingWhatsapp"
                @click="generarCodigoWhatsapp"
              >
                <Link2 :size="13" :stroke-width="2" /> {{ savingWhatsapp ? 'Generando…' : 'Vincular WhatsApp' }}
              </button>
            </div>

            <!-- Pasos para vincular -->
            <ol v-if="codigoWhatsapp && !whatsappVinculado" class="wa-steps">
              <li v-if="WHATSAPP_JOIN">
                <span class="step-n">1</span>
                <div class="step-body">
                  <p>Activa el bot enviando este mensaje al <strong>{{ WHATSAPP_BOT_DISPLAY }}</strong> (si dejas de usarlo unos días, hay que enviarlo de nuevo):</p>
                  <code class="wa-code">{{ WHATSAPP_JOIN }}</code>
                  <a class="btn btn-sm btn-ghost" :href="waUrl(WHATSAPP_JOIN)" target="_blank" rel="noopener">
                    <ExternalLink :size="13" :stroke-width="2" /> Activar bot
                  </a>
                </div>
              </li>
              <li>
                <span class="step-n">{{ WHATSAPP_JOIN ? 2 : 1 }}</span>
                <div class="step-body">
                  <p>
                    <template v-if="WHATSAPP_JOIN">Cuando el bot te responda, envía</template>
                    <template v-else>Envía este mensaje al <strong>{{ WHATSAPP_BOT_DISPLAY }}</strong></template>
                    (el código vence en 15 minutos):
                  </p>
                  <code class="wa-code">vincular {{ codigoWhatsapp }}</code>
                  <div class="step-actions">
                    <a class="btn btn-sm btn-primary" :href="waUrl(`vincular ${codigoWhatsapp}`)" target="_blank" rel="noopener">
                      <ExternalLink :size="13" :stroke-width="2" /> Vincular en WhatsApp
                    </a>
                    <button class="btn btn-sm btn-ghost" @click="cargarWhatsapp">Ya lo envié</button>
                  </div>
                </div>
              </li>
            </ol>
          </div>
        </section>

        <!-- Cambiar contraseña -->
        <section class="card pf-card">
          <h2 class="pf-title">Cambiar contraseña</h2>

          <form class="pw-form" novalidate @submit.prevent="cambiarPassword">
            <div class="field">
              <label for="pf-pw-actual">Contraseña actual</label>
              <input id="pf-pw-actual" v-model="pwForm.actual" type="password" class="pf-input" :class="{ 'has-error': pwErrors.actual }" autocomplete="current-password" />
              <span v-if="pwErrors.actual" class="field-error">{{ pwErrors.actual }}</span>
            </div>
            <div class="pw-pair">
              <div class="field">
                <label for="pf-pw-nueva">Nueva contraseña</label>
                <input id="pf-pw-nueva" v-model="pwForm.nueva" type="password" class="pf-input" :class="{ 'has-error': pwErrors.nueva }" autocomplete="new-password" />
                <span v-if="pwErrors.nueva" class="field-error">{{ pwErrors.nueva }}</span>
              </div>
              <div class="field">
                <label for="pf-pw-confirmar">Confirmar nueva</label>
                <input id="pf-pw-confirmar" v-model="pwForm.confirmar" type="password" class="pf-input" :class="{ 'has-error': pwErrors.confirmar }" autocomplete="new-password" />
                <span v-if="pwErrors.confirmar" class="field-error">{{ pwErrors.confirmar }}</span>
              </div>
            </div>
            <div class="form-actions">
              <button type="submit" class="btn btn-primary" :disabled="savingPw">
                <KeyRound :size="15" :stroke-width="2" />
                {{ savingPw ? 'Guardando…' : 'Cambiar contraseña' }}
              </button>
            </div>
          </form>
        </section>
      </div>

      <!-- ── Columna derecha ── -->
      <div class="settings-col">
        <!-- Apariencia -->
        <section class="card pf-card">
          <h2 class="pf-title">Apariencia</h2>

          <div class="setting">
            <div class="setting-text">
              <span class="setting-name">Tema</span>
              <span class="setting-hint">Cómo se ve la app en este dispositivo</span>
            </div>
            <div class="segmented" role="radiogroup" aria-label="Tema">
              <button
                type="button"
                role="radio"
                class="seg-option"
                :class="{ 'is-active': theme === 'dark' }"
                :aria-checked="theme === 'dark'"
                @click="theme = 'dark'"
              >
                <Moon :size="14" :stroke-width="2" /> Oscuro
              </button>
              <button
                type="button"
                role="radio"
                class="seg-option"
                :class="{ 'is-active': theme === 'light' }"
                :aria-checked="theme === 'light'"
                @click="theme = 'light'"
              >
                <Sun :size="14" :stroke-width="2" /> Claro
              </button>
            </div>
          </div>

          <div class="setting setting--stack">
            <div class="setting-text">
              <span class="setting-name">Color de acento</span>
              <span class="setting-hint">Pinta los botones principales y los detalles · <strong>{{ accentNombre }}</strong></span>
            </div>
            <div class="swatches" role="radiogroup" aria-label="Color de acento">
              <button
                v-for="preset in ACCENT_PRESETS"
                :key="preset.name"
                type="button"
                role="radio"
                class="swatch"
                :class="{ 'is-active': accent === preset.value, 'swatch--default': preset.value === null }"
                :style="{ '--sw': preset.value ?? DEFAULT_ACCENT_COLOR }"
                :aria-checked="accent === preset.value"
                :aria-label="preset.name"
                :title="preset.name"
                @click="setAccent(preset.value)"
              >
                <Check v-if="accent === preset.value" :size="14" :stroke-width="3" />
              </button>
            </div>
            <div class="accent-preview">
              <button type="button" class="btn btn-primary btn-sm" tabindex="-1">Botón principal</button>
              <span class="preview-chip">Vista previa</span>
            </div>
          </div>
        </section>

        <!-- Notificaciones -->
        <section class="card pf-card">
          <h2 class="pf-title">Notificaciones</h2>

          <div class="setting">
            <div class="setting-text">
              <span class="setting-name"><BellRing :size="14" :stroke-width="2" /> Recordatorios por email</span>
              <span class="setting-hint">
                Te escribimos si llevas varios días sin registrar gastos, si tienes cobros por confirmar
                o deudas por vencer. Máximo uno cada pocos días.
              </span>
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
        </section>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Sun, Moon, ArrowLeft, Pencil, X, Check, Mail, MessageCircle, Unlink, Link2, ExternalLink, KeyRound, BellRing,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { userFromToken } from '@/utils'
import { useAppearance, ACCENT_PRESETS } from '@/composables/useAppearance'
import { perfilService } from '@/services/api'

const auth   = useAuthStore()
const toast  = useToast()
const router = useRouter()
const { theme, accent, setAccent } = useAppearance()
const DEFAULT_ACCENT_COLOR = '#5b7a95' // Acero: --accent por defecto del tema oscuro

const user = computed(() => auth.token ? userFromToken(auth.token) : null)
const iniciales = computed(() =>
  (user.value?.nombre ?? '').trim().split(/\s+/).map(w => w[0]).slice(0, 2).join('').toUpperCase()
)
const accentNombre = computed(() => ACCENT_PRESETS.find(p => p.value === accent.value)?.name ?? 'Personalizado')

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
.perfil { max-width: 1080px; }

.back-btn {
  display: inline-flex; align-items: center; gap: 0.35rem;
  margin-bottom: 0.75rem; padding: 0.25rem 0.5rem 0.25rem 0.3rem;
  background: none; border: none; border-radius: 8px;
  font: inherit; font-size: 0.75rem; color: var(--text-secondary);
  cursor: pointer; transition: color 0.15s, background 0.15s;
}
.back-btn:hover { color: var(--text-primary); background: var(--surface2); }

/* Dos columnas que se apilan; cada tarjeta mide lo que su contenido */
.settings-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; align-items: start; }
.settings-col  { display: flex; flex-direction: column; gap: 1rem; min-width: 0; }

.pf-card { padding: 1.5rem; }
.pf-card:hover { border-color: var(--border); }
.pf-title {
  margin-bottom: 1.25rem;
  font-family: var(--ff-body);
  font-size: 0.74rem; font-weight: 500; letter-spacing: 0.1em; text-transform: uppercase;
  color: var(--text-primary);
}

/* ── Identidad ── */
.identity { display: flex; align-items: center; gap: 1rem; }
.identity-avatar {
  display: grid; place-items: center; flex-shrink: 0;
  width: 56px; height: 56px; border-radius: 16px;
  background: var(--avatar-gradient);
  box-shadow: 0 0 0 3px var(--surface), 0 0 0 4px var(--border-hover);
  font-size: 1.05rem; font-weight: 700; letter-spacing: 0.04em; color: #fff;
}
.identity-main { display: flex; flex-direction: column; gap: 0.35rem; min-width: 0; flex: 1; }
.name-row { display: flex; align-items: center; gap: 0.35rem; min-width: 0; }
.name {
  font-size: 1.15rem; font-weight: 700; color: var(--text-primary);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.name-row .icon-btn { opacity: 0.55; transition: opacity 0.15s, color 0.15s, background 0.15s; }
.identity:hover .name-row .icon-btn,
.name-row .icon-btn:focus-visible { opacity: 1; }
.name-edit { display: flex; align-items: center; gap: 0.3rem; }
.name-edit .pf-input { height: 38px; font-weight: 600; }
.email { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.8rem; color: var(--text-subtle); min-width: 0; overflow-wrap: anywhere; }

.icon-btn {
  display: grid; place-items: center; flex-shrink: 0;
  width: 32px; height: 32px; padding: 0;
  background: none; border: none; border-radius: 8px;
  color: var(--text-secondary); cursor: pointer;
  transition: color 0.15s, background 0.15s;
}
.icon-btn:hover:not(:disabled) { color: var(--text-primary); background: var(--surface2); }
.icon-btn--ok { color: var(--c-pos); }
.icon-btn--ok:hover:not(:disabled) { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 12%, transparent); }
.icon-btn:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }

/* ── WhatsApp ── */
.wa { margin-top: 1.4rem; padding-top: 1.3rem; border-top: 1px solid var(--border); display: flex; flex-direction: column; gap: 0.85rem; }
.wa-row  { display: flex; align-items: flex-start; gap: 0.85rem; }
.wa-icon {
  display: grid; place-items: center; flex-shrink: 0;
  width: 38px; height: 38px; border-radius: 11px;
  color: #25d366; background: color-mix(in srgb, #25d366 13%, transparent);
}
.wa-main { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 0.25rem; }
.wa-name { font-size: 0.88rem; font-weight: 600; color: var(--text-primary); }
.wa-sub  { font-size: 0.78rem; line-height: 1.5; color: var(--text-subtle); }
.wa-sub strong { color: var(--text-primary); font-family: var(--ff-mono); font-weight: 500; }
.wa-actions { display: flex; gap: 0.5rem; padding-left: calc(38px + 0.85rem); }
.wa-actions:empty { display: none; }

.status {
  display: inline-flex; align-items: center; gap: 0.4rem; flex-shrink: 0;
  padding: 0.22rem 0.65rem; border-radius: 999px;
  font-size: 0.68rem; font-weight: 500;
}
.status-dot { width: 6px; height: 6px; border-radius: 99px; background: currentColor; }
.status--on  { color: var(--c-pos); background: color-mix(in srgb, var(--c-pos) 13%, transparent); }
.status--off { color: var(--text-secondary); background: var(--surface2); }

.wa-steps { list-style: none; display: flex; flex-direction: column; gap: 0.75rem; padding-left: calc(38px + 0.85rem); }
.wa-steps li { display: flex; gap: 0.7rem; }
.step-n {
  display: grid; place-items: center; flex-shrink: 0;
  width: 22px; height: 22px; border-radius: 999px;
  font-family: var(--ff-mono); font-size: 0.68rem; font-weight: 700;
  color: var(--accent-strong); background: color-mix(in srgb, var(--accent) 16%, transparent);
}
.step-body { display: flex; flex-direction: column; align-items: flex-start; gap: 0.5rem; min-width: 0; }
.step-body p { font-size: 0.76rem; line-height: 1.5; color: var(--text-subtle); }
.step-body p strong { color: var(--text-primary); }
.step-actions { display: flex; flex-wrap: wrap; gap: 0.4rem; }
.wa-code {
  padding: 0.45rem 0.75rem; border-radius: 8px;
  font-family: var(--ff-mono); font-size: 0.9rem; letter-spacing: 0.03em; color: var(--text-primary);
  background: var(--bg); border: 1px solid var(--border-hover);
  user-select: all;
}
.btn-ghost { background: transparent; border-color: var(--border-hover); color: var(--text-secondary); text-decoration: none; }
.btn-ghost:hover:not(:disabled) { background: var(--surface2); color: var(--text-primary); }

/* ── Ajustes (fila: texto a la izquierda, control a la derecha) ── */
.setting { display: flex; align-items: center; justify-content: space-between; gap: 1.25rem; }
.setting + .setting { margin-top: 1.25rem; padding-top: 1.25rem; border-top: 1px solid var(--border); }
.setting--stack { flex-direction: column; align-items: stretch; gap: 0.9rem; }
.setting-text { display: flex; flex-direction: column; gap: 0.25rem; min-width: 0; }
.setting-name { display: inline-flex; align-items: center; gap: 0.4rem; font-size: 0.86rem; font-weight: 600; color: var(--text-primary); }
.setting-name svg { color: var(--text-secondary); }
.setting-hint { font-size: 0.74rem; line-height: 1.5; color: var(--text-subtle); }
.setting-hint strong { color: var(--text-primary); font-weight: 500; }

/* Segmented control */
.segmented {
  display: inline-flex; flex-shrink: 0; gap: 2px;
  padding: 3px; border-radius: 11px;
  background: var(--bg); border: 1px solid var(--border);
}
.seg-option {
  display: inline-flex; align-items: center; gap: 0.4rem;
  height: 32px; padding: 0 0.85rem;
  background: none; border: none; border-radius: 8px;
  font: inherit; font-size: 0.76rem; font-weight: 500; color: var(--text-secondary);
  cursor: pointer; transition: color 0.15s ease, background 0.15s ease, box-shadow 0.15s ease;
}
.seg-option:hover:not(.is-active) { color: var(--text-primary); }
.seg-option.is-active {
  color: var(--text-primary);
  background: var(--surface2);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.25), inset 0 0 0 1px var(--border-hover);
}
.seg-option:focus-visible { outline: 2px solid var(--accent); outline-offset: 1px; }

/* Swatches */
.swatches { display: flex; flex-wrap: wrap; gap: 0.7rem; }
.swatch {
  position: relative;
  display: grid; place-items: center;
  width: 32px; height: 32px; padding: 0;
  border: none; border-radius: 999px;
  background: var(--sw);
  color: #fff;
  cursor: pointer;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.12);
  transition: transform 0.15s var(--ease-out), box-shadow 0.15s ease;
}
.swatch--default { background: conic-gradient(from 210deg, var(--sw) 0 50%, #2563eb 50% 100%); }
.swatch:hover { transform: scale(1.08); }
.swatch.is-active { box-shadow: 0 0 0 2px var(--surface), 0 0 0 4px var(--text-primary); }
.swatch:focus-visible { outline: none; box-shadow: 0 0 0 2px var(--surface), 0 0 0 4px var(--accent-strong); }

.accent-preview { display: flex; align-items: center; gap: 0.6rem; }
.accent-preview .btn { pointer-events: none; }
.preview-chip { font-size: 0.66rem; color: var(--text-muted); }

/* Switch */
.switch {
  position: relative; flex-shrink: 0;
  width: 36px; height: 20px; padding: 0;
  border: none; border-radius: 999px;
  background: color-mix(in srgb, var(--text-muted) 40%, var(--surface2));
  cursor: pointer;
  transition: background 0.2s ease;
}
.switch:disabled { opacity: 0.5; cursor: default; }
.switch:focus-visible { outline: none; box-shadow: var(--focus-ring); }
.switch-thumb {
  position: absolute; top: 2px; left: 2px;
  width: 16px; height: 16px; border-radius: 50%;
  background: #fff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.35);
  transition: transform 0.22s var(--ease-out);
}
.switch--on { background: var(--c-pos); }
.switch--on .switch-thumb { transform: translateX(16px); }

/* ── Contraseña: inputs como en Login/Registro ── */
.pw-form { display: flex; flex-direction: column; gap: 1rem; }
.pw-pair { display: grid; grid-template-columns: 1fr 1fr; gap: 0.85rem; }
.field { display: flex; flex-direction: column; gap: 0.4rem; min-width: 0; }
.field label { margin: 0; font-size: 0.68rem; font-weight: 500; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text-subtle); }
.pf-input {
  width: 100%; height: 44px; padding: 0 0.9rem;
  font: inherit; font-size: 0.86rem; color: var(--text-primary);
  background: var(--bg);
  border: 1px solid var(--border-hover);
  border-radius: 10px;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}
.pf-input:hover { border-color: color-mix(in srgb, var(--accent) 40%, var(--border-hover)); }
.pf-input:focus { border-color: var(--accent-strong); box-shadow: var(--focus-ring), 0 0 18px -6px color-mix(in srgb, var(--accent) 60%, transparent); }
.pf-input.has-error { border-color: var(--c-neg); }
.form-actions { display: flex; justify-content: flex-end; margin-top: 0.25rem; }
.field-error { font-size: 0.7rem; color: var(--c-neg); }

@media (max-width: 900px) {
  .settings-grid { grid-template-columns: 1fr; }
}
@media (max-width: 520px) {
  .pf-card { padding: 1.2rem; }
  .pw-pair { grid-template-columns: 1fr; }
  .setting { flex-direction: column; align-items: stretch; }
  .segmented { align-self: flex-start; }
  .wa-actions, .wa-steps { padding-left: 0; }
  .form-actions .btn { width: 100%; }
}
</style>
