<template>
  <div class="auth-page">
    <!-- Panel izquierdo -->
    <div class="auth-panel">
      <div class="panel-logo"><AppLogo /></div>
      <p class="panel-tagline">Tus finanzas,<br>bajo control.</p>
      <ul class="panel-features">
        <li>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/></svg>
          Dashboard con resumen mensual y anual
        </li>
        <li>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
          Control de gastos e ingresos por categoría
        </li>
        <li>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
          Metas de ahorro con seguimiento visual
        </li>
        <li>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
          Presupuestos por categoría con alertas
        </li>
      </ul>
    </div>

    <!-- Panel derecho -->
    <div class="auth-right">
      <div class="auth-card">
        <div class="auth-logo-mobile"><AppLogo /></div>
        <h1 class="auth-title">Crear cuenta</h1>
        <p class="auth-subtitle">Empieza a controlar tus finanzas hoy</p>

        <form class="auth-form" @submit.prevent="onSubmit">
          <div class="field">
            <label class="field-label">Nombre</label>
            <input
              v-model="form.nombre"
              type="text"
              class="field-input"
              :class="{ 'input-error': errors.nombre }"
              placeholder="Tu nombre"
              autocomplete="name"
            />
            <span v-if="errors.nombre" class="field-error">{{ errors.nombre }}</span>
          </div>

          <div class="field">
            <label class="field-label">Correo electrónico</label>
            <input
              v-model="form.email"
              type="email"
              class="field-input"
              :class="{ 'input-error': errors.email }"
              placeholder="tu@correo.com"
              autocomplete="email"
            />
            <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
          </div>

          <div class="field">
            <label class="field-label">Contraseña</label>
            <div class="input-wrap">
              <input
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                class="field-input"
                :class="{ 'input-error': errors.password }"
                placeholder="Crea una contraseña segura"
                autocomplete="new-password"
                @focus="passwordFocused = true"
                @blur="passwordFocused = false"
              />
              <button type="button" class="eye-btn" @click="showPassword = !showPassword" tabindex="-1">
                <svg v-if="!showPassword" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
              </button>
            </div>
            <transition name="pw-reqs">
              <ul v-if="passwordFocused" class="pw-requirements">
                <li v-for="req in passwordRequirements" :key="req.id" :class="{ met: req.test(form.password) }">
                  <svg v-if="req.test(form.password)" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                  <svg v-else width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><circle cx="12" cy="12" r="9"/></svg>
                  {{ req.label }}
                </li>
              </ul>
            </transition>
            <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
          </div>

          <p v-if="serverError" class="server-error">{{ serverError }}</p>

          <button type="submit" class="btn-submit" :disabled="loading">
            <span v-if="loading" class="spinner" />
            {{ loading ? 'Creando cuenta...' : 'Crear cuenta gratis' }}
          </button>
        </form>

        <p class="auth-footer">
          ¿Ya tienes cuenta?
          <RouterLink to="/login" class="auth-link">Inicia sesión aquí</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { z } from 'zod'
import { passwordRequirements, passwordSchema } from '@/schemas'
import AppLogo from '@/components/common/AppLogo.vue'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({ nombre: '', email: '', password: '' })
const errors = reactive({ nombre: '', email: '', password: '' })
const serverError = ref('')
const loading = ref(false)
const showPassword = ref(false)
const passwordFocused = ref(false)

const schema = z.object({
  nombre: z.string().min(2, { message: 'Mínimo 2 caracteres' }).max(80, { message: 'Máximo 80 caracteres' }),
  email: z.email({ message: 'Correo inválido' }),
  password: passwordSchema
})

async function onSubmit() {
  errors.nombre = ''
  errors.email = ''
  errors.password = ''
  serverError.value = ''

  const result = schema.safeParse(form)
  if (!result.success) {
    result.error.issues.forEach(i => {
      const f = i.path[0] as keyof typeof errors
      if (f in errors) errors[f] = i.message
    })
    return
  }

  loading.value = true
  const err = await auth.register(form.nombre, form.email, form.password)
  loading.value = false

  if (err) { serverError.value = err; return }
  router.push('/dashboard')
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 1fr 1fr;
}

.auth-panel {
  background: #0d0f12;
  background-image:
    radial-gradient(ellipse 80% 60% at 20% 50%, color-mix(in srgb, var(--accent) 8%, transparent) 0%, transparent 70%),
    linear-gradient(180deg, #0d0f12 0%, #14181c 100%);
  padding: 3rem 3.5rem;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 2rem;
  position: relative;
  overflow: hidden;
}
.auth-panel::before {
  content: '';
  position: absolute;
  inset: 0;
  background-image: radial-gradient(color-mix(in srgb, var(--accent) 4%, transparent) 1px, transparent 1px);
  background-size: 28px 28px;
}

.panel-logo {
  font-family: var(--ff-display);
  font-weight: 800;
  font-size: 1.3rem;
  letter-spacing: 0.08em;
  color: #fff;
  position: relative;
}
.panel-logo span { color: var(--accent); }

.panel-tagline {
  font-size: 2rem;
  font-weight: 800;
  line-height: 1.2;
  color: #fff;
  margin: 0;
  position: relative;
}

.panel-features {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  position: relative;
}
.panel-features li {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  font-size: 0.8rem;
  color: rgba(255,255,255,0.55);
}
.panel-features li svg { color: var(--accent); flex-shrink: 0; }

.auth-right {
  background: var(--bg);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 1.5rem;
}

.auth-card {
  width: 100%;
  max-width: 380px;
}

.auth-logo-mobile {
  display: none;
  font-family: var(--ff-display);
  font-weight: 800;
  font-size: 1.1rem;
  letter-spacing: 0.06em;
  color: var(--text-primary);
  margin-bottom: 1.5rem;
}
.auth-logo-mobile span { color: var(--accent); }

.auth-title {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--text-primary);
  margin: 0 0 0.3rem;
  letter-spacing: -0.02em;
}

.auth-subtitle {
  font-size: 0.82rem;
  color: var(--text-muted);
  margin: 0 0 2rem;
}

.auth-form { display: flex; flex-direction: column; gap: 1.1rem; }
.field { display: flex; flex-direction: column; gap: 0.35rem; }

.field-label {
  font-size: 0.72rem;
  color: var(--text-secondary);
  font-weight: 500;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.field-input {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0.65rem 0.85rem;
  font-size: 0.85rem;
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}
.field-input:focus {
  border-color: var(--accent);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--accent) 10%, transparent);
}
.field-input.input-error { border-color: var(--red); }
.field-error { font-size: 0.72rem; color: var(--red); }

.input-wrap { position: relative; }
.input-wrap .field-input { width: 100%; padding-right: 2.5rem; box-sizing: border-box; }
.eye-btn {
  position: absolute; right: 0.65rem; top: 50%; transform: translateY(-50%);
  background: none; border: none; cursor: pointer; color: var(--text-muted);
  display: flex; align-items: center; padding: 2px;
  transition: color 0.15s;
}
.eye-btn:hover { color: var(--text-primary); }

.pw-requirements {
  list-style: none;
  margin: 0.5rem 0 0;
  padding: 0.6rem 0.75rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}
.pw-requirements li {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  font-size: 0.74rem;
  color: var(--text-muted);
  transition: color 0.15s;
}
.pw-requirements li svg { flex-shrink: 0; color: var(--text-muted); transition: color 0.15s; }
.pw-requirements li.met { color: var(--text-secondary); }
.pw-requirements li.met svg { color: var(--green); }

.pw-reqs-enter-active, .pw-reqs-leave-active { transition: opacity 0.15s, transform 0.15s; }
.pw-reqs-enter-from, .pw-reqs-leave-to { opacity: 0; transform: translateY(-4px); }

.server-error {
  font-size: 0.8rem;
  color: var(--red);
  background: color-mix(in srgb, var(--red) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--red) 20%, transparent);
  border-radius: 8px;
  padding: 0.6rem 0.85rem;
  margin: 0;
}

.btn-submit {
  background: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.75rem;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s, transform 0.15s;
  margin-top: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  letter-spacing: 0.01em;
}
.btn-submit:hover:not(:disabled) { opacity: 0.88; transform: translateY(-1px); }
.btn-submit:active:not(:disabled) { transform: translateY(0); }
.btn-submit:disabled { opacity: 0.5; cursor: not-allowed; }

.spinner {
  width: 14px; height: 14px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.auth-footer {
  text-align: center;
  font-size: 0.78rem;
  color: var(--text-muted);
  margin: 1.75rem 0 0;
}
.auth-link { color: var(--accent); text-decoration: none; font-weight: 500; }
.auth-link:hover { text-decoration: underline; }

@media (max-width: 700px) {
  .auth-page { grid-template-columns: 1fr; }
  .auth-panel { display: none; }
  .auth-right { padding: 2.5rem 1.25rem; align-items: flex-start; padding-top: 3rem; }
  .auth-logo-mobile { display: block; }
}
</style>
