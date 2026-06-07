<template>
  <div class="auth-page">
    <div class="auth-card">
      <div class="auth-logo">GASTOS<span>.</span></div>
      <h1 class="auth-title">Bienvenido de vuelta</h1>
      <p class="auth-subtitle">Ingresa a tu cuenta para continuar</p>

      <form class="auth-form" @submit.prevent="onSubmit">
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
            placeholder="••••••••"
            autocomplete="current-password"
          />
          <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
        </div>

        <p v-if="serverError" class="server-error">{{ serverError }}</p>

        <button type="submit" class="btn-submit" :disabled="loading">
          <span v-if="loading" class="spinner" />
          {{ loading ? 'Ingresando...' : 'Iniciar sesión' }}
        </button>
      </form>

      <p class="auth-footer">
        ¿No tienes cuenta?
        <RouterLink to="/register" class="auth-link">Regístrate aquí</RouterLink>
      </p>
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

const form = reactive({ email: '', password: '' })
const errors = reactive({ email: '', password: '' })
const serverError = ref('')
const loading = ref(false)

const schema = z.object({
  email: z.email({ message: 'Correo inválido' }),
  password: z.string().min(6, { message: 'Mínimo 6 caracteres' })
})

async function onSubmit() {
  errors.email = ''
  errors.password = ''
  serverError.value = ''

  const result = schema.safeParse(form)
  if (!result.success) {
    result.error.issues.forEach(i => {
      const f = i.path[0] as 'email' | 'password'
      if (f in errors) errors[f] = i.message
    })
    return
  }

  loading.value = true
  const err = await auth.login(form.email, form.password)
  loading.value = false

  if (err) { serverError.value = err; return }
  router.push('/dashboard')
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg);
  padding: 1.5rem;
}

.auth-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 2.5rem 2rem;
  width: 100%;
  max-width: 400px;
}

.auth-logo {
  font-family: var(--ff-display);
  font-weight: 800;
  font-size: 1.1rem;
  letter-spacing: 0.04em;
  color: var(--text-primary);
  margin-bottom: 1.5rem;
}
.auth-logo span { color: var(--accent); }

.auth-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0 0 0.25rem;
}

.auth-subtitle {
  font-size: 0.82rem;
  color: var(--text-muted);
  margin: 0 0 2rem;
}

.auth-form { display: flex; flex-direction: column; gap: 1.1rem; }

.field { display: flex; flex-direction: column; gap: 0.35rem; }

.field-label {
  font-size: 0.75rem;
  color: var(--text-secondary);
  font-weight: 500;
  letter-spacing: 0.03em;
}

.field-input {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 6px;
  padding: 0.6rem 0.75rem;
  font-size: 0.85rem;
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s;
}
.field-input:focus { border-color: var(--accent); }
.field-input.input-error { border-color: #f56565; }

.field-error {
  font-size: 0.72rem;
  color: #f56565;
}

.server-error {
  font-size: 0.8rem;
  color: #f56565;
  background: rgba(245, 101, 101, 0.08);
  border: 1px solid rgba(245, 101, 101, 0.2);
  border-radius: 6px;
  padding: 0.6rem 0.75rem;
  margin: 0;
}

.btn-submit {
  background: var(--accent);
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.7rem;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s;
  margin-top: 0.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}
.btn-submit:hover:not(:disabled) { opacity: 0.88; }
.btn-submit:disabled { opacity: 0.6; cursor: not-allowed; }

.spinner {
  width: 14px;
  height: 14px;
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
  margin: 1.5rem 0 0;
}

.auth-link {
  color: var(--accent);
  text-decoration: none;
  font-weight: 500;
}
.auth-link:hover { text-decoration: underline; }
</style>
