<template>
  <div class="auth-page">
    <!-- Panel izquierdo -->
    <div class="auth-panel">
      <div class="panel-logo">FINANZAS<span>.</span></div>
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
        <div class="auth-logo-mobile">FINANZAS<span>.</span></div>
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
            <input
              v-model="form.password"
              type="password"
              class="field-input"
              :class="{ 'input-error': errors.password }"
              placeholder="Mínimo 6 caracteres"
              autocomplete="new-password"
            />
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

const router = useRouter()
const auth = useAuthStore()

const form = reactive({ nombre: '', email: '', password: '' })
const errors = reactive({ nombre: '', email: '', password: '' })
const serverError = ref('')
const loading = ref(false)

const schema = z.object({
  nombre: z.string().min(2, { message: 'Mínimo 2 caracteres' }).max(80, { message: 'Máximo 80 caracteres' }),
  email: z.email({ message: 'Correo inválido' }),
  password: z.string().min(6, { message: 'Mínimo 6 caracteres' })
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
  background: #080d12;
  background-image:
    radial-gradient(ellipse 80% 60% at 20% 50%, rgba(99,179,255,0.08) 0%, transparent 70%),
    linear-gradient(180deg, #080d12 0%, #0d1620 100%);
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
  background-image: radial-gradient(rgba(99,179,255,0.04) 1px, transparent 1px);
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
  box-shadow: 0 0 0 3px rgba(99,179,255,0.1);
}
.field-input.input-error { border-color: #f56565; }
.field-error { font-size: 0.72rem; color: #f56565; }

.server-error {
  font-size: 0.8rem;
  color: #f56565;
  background: rgba(245, 101, 101, 0.08);
  border: 1px solid rgba(245, 101, 101, 0.2);
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
