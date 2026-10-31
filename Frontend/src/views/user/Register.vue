<template>
  <div class="auth-page">
    <!-- Luces de fondo (se filtran a través del cristal) -->
    <div class="glow glow--blue" aria-hidden="true" />
    <div class="glow glow--green" aria-hidden="true" />
    <div class="glow glow--violet" aria-hidden="true" />
    <div class="grain" aria-hidden="true" />

    <div class="auth-shell">
      <!-- ── Showcase: ventajas en bento ── -->
      <section class="showcase" aria-label="Qué incluye Zaldo">
        <div class="brand"><AppLogo /></div>
        <h2 class="showcase-title">Tus finanzas, <span>bajo control</span>.</h2>
        <p class="showcase-sub">Crea tu cuenta en menos de un minuto. Gratis, sin tarjeta de crédito.</p>

        <ul class="bento bento--features">
          <li v-for="(f, i) in VENTAJAS" :key="f.titulo" class="widget feature" :style="{ '--fc': f.color, animationDelay: `${-i * 1.6}s` }">
            <span class="feature-icon"><component :is="f.icon" :size="18" :stroke-width="1.9" /></span>
            <strong class="feature-title">{{ f.titulo }}</strong>
            <span class="feature-desc">{{ f.desc }}</span>
          </li>
        </ul>
      </section>

      <!-- ── Formulario: cristal esmerilado ── -->
      <section class="glass">
        <div class="glass-logo"><AppLogo /></div>
        <h1 class="auth-title">Crea tu cuenta</h1>
        <p class="auth-subtitle">Empieza a controlar tus finanzas hoy</p>

        <form class="auth-form" novalidate @submit.prevent="onSubmit">
          <div class="field">
            <label class="field-label" for="reg-nombre">Nombre</label>
            <div class="input-wrap" :class="{ 'has-error': errors.nombre }">
              <UserRound :size="16" :stroke-width="1.9" class="input-icon" />
              <input
                id="reg-nombre"
                v-model="form.nombre"
                type="text"
                class="field-input"
                placeholder="¿Cómo te llamas?"
                autocomplete="name"
                :aria-invalid="!!errors.nombre"
              />
            </div>
            <span v-if="errors.nombre" class="field-error">{{ errors.nombre }}</span>
          </div>

          <div class="field">
            <label class="field-label" for="reg-email">Correo electrónico</label>
            <div class="input-wrap" :class="{ 'has-error': errors.email }">
              <Mail :size="16" :stroke-width="1.9" class="input-icon" />
              <input
                id="reg-email"
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
            <label class="field-label" for="reg-password">Contraseña</label>
            <div class="input-wrap" :class="{ 'has-error': errors.password }">
              <LockKeyhole :size="16" :stroke-width="1.9" class="input-icon" />
              <input
                id="reg-password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                class="field-input"
                placeholder="Crea una contraseña segura"
                autocomplete="new-password"
                aria-describedby="reg-pw-strength"
                :aria-invalid="!!errors.password"
                @focus="passwordFocused = true"
                @blur="passwordFocused = false"
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

            <!-- Fuerza: una barra por requisito cumplido -->
            <div id="reg-pw-strength" class="pw-strength" :class="`pw-strength--${fuerza.nivel}`" aria-live="polite">
              <div class="pw-bars">
                <span v-for="n in passwordRequirements.length" :key="n" :class="{ 'is-on': n <= cumplidos }" />
              </div>
              <span class="pw-label">{{ form.password ? fuerza.texto : 'Mín. 8 caracteres, mayúscula, minúscula y número' }}</span>
            </div>

            <Transition name="pw-reqs">
              <ul v-if="passwordFocused && cumplidos < passwordRequirements.length" class="pw-requirements">
                <li v-for="req in passwordRequirements" :key="req.id" :class="{ met: req.test(form.password) }">
                  <Check v-if="req.test(form.password)" :size="12" :stroke-width="3" />
                  <Circle v-else :size="12" :stroke-width="2.4" />
                  {{ req.label }}
                </li>
              </ul>
            </Transition>
            <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
          </div>

          <p v-if="serverError" class="server-error" role="alert">
            <CircleAlert :size="15" :stroke-width="2" /> {{ serverError }}
          </p>

          <button type="submit" class="btn-submit" :disabled="loading">
            <span v-if="loading" class="spinner" />
            {{ loading ? 'Creando cuenta…' : 'Crear cuenta gratis' }}
            <ArrowRight v-if="!loading" :size="16" :stroke-width="2.2" class="btn-arrow" />
          </button>
        </form>

        <p class="auth-footer">
          ¿Ya tienes cuenta?
          <RouterLink to="/login" class="auth-link">Inicia sesión</RouterLink>
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
import { ref, reactive, computed } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import {
  UserRound, Mail, LockKeyhole, Eye, EyeOff, ArrowRight, CircleAlert, ShieldCheck, Check, Circle,
  LayoutDashboard, Receipt, PiggyBank, MessageCircle,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { z } from 'zod'
import { passwordRequirements, passwordSchema } from '@/schemas'
import AppLogo from '@/components/common/AppLogo.vue'
import '@/assets/auth.css'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({ nombre: '', email: '', password: '' })
const errors = reactive({ nombre: '', email: '', password: '' })
const serverError = ref('')
const loading = ref(false)
const showPassword = ref(false)
const passwordFocused = ref(false)

const VENTAJAS = [
  { titulo: 'Dashboard claro',       desc: 'Tu mes y tu año en un vistazo',         icon: LayoutDashboard, color: '#60a5fa' },
  { titulo: 'Gastos por categoría',  desc: 'Fijos, ocasionales y cuotas',            icon: Receipt,         color: '#d4a85a' },
  { titulo: 'Metas de ahorro',       desc: 'Mira cuánto te falta y a qué ritmo',     icon: PiggyBank,       color: '#34d399' },
  { titulo: 'Registro por WhatsApp', desc: 'Escribe "25000 almuerzo" y listo',       icon: MessageCircle,   color: '#a78bfa' },
]

// Fuerza de la contraseña = requisitos reales cumplidos (los mismos que valida passwordSchema)
const cumplidos = computed(() => passwordRequirements.filter(r => r.test(form.password)).length)
const fuerza = computed(() => {
  const n = cumplidos.value
  const total = passwordRequirements.length
  if (!form.password) return { nivel: 'vacia', texto: '' }
  if (n === total)    return { nivel: 'fuerte', texto: 'Contraseña segura' }
  if (n >= total - 1) return { nivel: 'media', texto: 'Casi lista' }
  return { nivel: 'debil', texto: 'Contraseña débil' }
})

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
