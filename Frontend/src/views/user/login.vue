<template>
  <div class="auth-page">
    <!-- Luces de fondo (se filtran a través del cristal) -->
    <div class="glow glow--blue" aria-hidden="true" />
    <div class="glow glow--green" aria-hidden="true" />
    <div class="glow glow--violet" aria-hidden="true" />
    <div class="grain" aria-hidden="true" />

    <div class="auth-shell">
      <!-- ── Showcase: widgets de la app flotando (bento) ── -->
      <section class="showcase" aria-label="Vista previa de Zaldo">
        <div class="brand"><AppLogo /></div>
        <h2 class="showcase-title">Tu dinero, <span>claro</span> por fin.</h2>
        <p class="showcase-sub">Gastos, metas, cuentas y presupuestos en un solo lugar. Así se ve tu mes en Zaldo:</p>

        <div class="bento">
          <!-- Saldo total + sparkline -->
          <article class="widget w-saldo">
            <div class="w-head">
              <span class="w-label">Total en cuentas</span>
              <span class="w-chip w-chip--pos"><TrendingUp :size="11" :stroke-width="2.4" /> 12%</span>
            </div>
            <p class="w-big">$4.820.000</p>
            <svg class="spark" viewBox="0 0 200 48" preserveAspectRatio="none" aria-hidden="true">
              <defs>
                <linearGradient id="sparkFill" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stop-color="#3b82f6" stop-opacity="0.45" />
                  <stop offset="100%" stop-color="#3b82f6" stop-opacity="0" />
                </linearGradient>
              </defs>
              <path d="M0 38 L22 34 L44 36 L66 26 L88 29 L110 20 L132 22 L154 12 L176 15 L200 6 L200 48 L0 48 Z" fill="url(#sparkFill)" />
              <path d="M0 38 L22 34 L44 36 L66 26 L88 29 L110 20 L132 22 L154 12 L176 15 L200 6" fill="none" stroke="#60a5fa" stroke-width="2" stroke-linejoin="round" stroke-linecap="round" />
            </svg>
          </article>

          <!-- Anillo de ahorro -->
          <article class="widget w-ahorro">
            <span class="w-label">Ahorro del mes</span>
            <div class="ring">
              <svg viewBox="0 0 64 64" aria-hidden="true">
                <circle cx="32" cy="32" r="26" class="ring-track" />
                <circle cx="32" cy="32" r="26" class="ring-fill" />
              </svg>
              <span class="ring-val">29%</span>
            </div>
            <span class="w-foot">+$924.067</span>
          </article>

          <!-- Presupuestos -->
          <article class="widget w-presu">
            <span class="w-label">Presupuestos</span>
            <ul class="presu-list">
              <li v-for="p in PRESUPUESTOS" :key="p.nombre">
                <div class="presu-top"><span>{{ p.nombre }}</span><span class="presu-pct">{{ p.pct }}%</span></div>
                <div class="presu-track"><span :style="{ width: p.pct + '%', background: p.color }" /></div>
              </li>
            </ul>
          </article>

          <!-- WhatsApp -->
          <article class="widget w-chat">
            <span class="w-label"><MessageCircle :size="12" :stroke-width="2.2" /> Gasto por WhatsApp</span>
            <div class="bubble bubble--out">25000 almuerzo</div>
            <div class="bubble bubble--in"><Check :size="12" :stroke-width="3" /> Registrado en Comida</div>
          </article>
        </div>
      </section>

      <!-- ── Formulario: cristal esmerilado ── -->
      <section class="glass">
        <div class="glass-logo"><AppLogo /></div>
        <h1 class="auth-title">Bienvenido de vuelta</h1>
        <p class="auth-subtitle">Ingresa a tu cuenta para continuar</p>

        <form class="auth-form" novalidate @submit.prevent="onSubmit">
          <div class="field">
            <label class="field-label" for="login-email">Correo electrónico</label>
            <div class="input-wrap" :class="{ 'has-error': errors.email }">
              <Mail :size="16" :stroke-width="1.9" class="input-icon" />
              <input
                id="login-email"
                v-model="form.email"
                type="email"
                class="field-input"
                placeholder="tu@correo.com"
                autocomplete="email"
                :aria-invalid="!!errors.email"
              />
            </div>
            <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
          </div>

          <div class="field">
            <label class="field-label" for="login-password">Contraseña</label>
            <div class="input-wrap" :class="{ 'has-error': errors.password }">
              <LockKeyhole :size="16" :stroke-width="1.9" class="input-icon" />
              <input
                id="login-password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                class="field-input"
                placeholder="••••••••"
                autocomplete="current-password"
                :aria-invalid="!!errors.password"
              />
              <button
                type="button"
                class="eye-btn"
                :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                @click="showPassword = !showPassword"
              >
                <EyeOff v-if="showPassword" :size="16" :stroke-width="1.9" />
                <Eye v-else :size="16" :stroke-width="1.9" />
              </button>
            </div>
            <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
          </div>

          <p v-if="serverError" class="server-error" role="alert">
            <CircleAlert :size="15" :stroke-width="2" /> {{ serverError }}
          </p>

          <button type="submit" class="btn-submit" :disabled="loading">
            <span v-if="loading" class="spinner" />
            {{ loading ? 'Ingresando…' : 'Iniciar sesión' }}
            <ArrowRight v-if="!loading" :size="16" :stroke-width="2.2" class="btn-arrow" />
          </button>
        </form>

        <p class="auth-footer">
          ¿No tienes cuenta?
          <RouterLink to="/register" class="auth-link">Créala gratis</RouterLink>
        </p>

        <p class="trust">
          <ShieldCheck :size="13" :stroke-width="2" />
          Tus datos viajan cifrados y nunca los compartimos.
        </p>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import {
  Mail, LockKeyhole, Eye, EyeOff, ArrowRight, CircleAlert, ShieldCheck, TrendingUp, MessageCircle, Check,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { z } from 'zod'
import AppLogo from '@/components/common/AppLogo.vue'
import '@/assets/auth.css'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({ email: '', password: '' })
const errors = reactive({ email: '', password: '' })
const serverError = ref('')
const loading = ref(false)
const showPassword = ref(false)

// Datos ilustrativos del showcase (no son del usuario)
const PRESUPUESTOS = [
  { nombre: 'Comida',     pct: 72, color: '#10b981' },
  { nombre: 'Transporte', pct: 48, color: '#60a5fa' },
  { nombre: 'Ocio',       pct: 91, color: '#d4a85a' },
]

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
