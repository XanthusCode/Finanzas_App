<template>
  <!-- Usuario nuevo: bienvenida con un camino claro -->
  <section v-if="variante === 'nuevo'" class="empty-start">
    <div class="hero">
      <span class="hero-icon"><Sparkles :size="28" :stroke-width="1.8" /></span>
      <h2 class="hero-title">¡Empecemos! Tu dashboard se arma solo</h2>
      <p class="hero-sub">Registra tus movimientos y aquí verás cuánto ganas, en qué gastas y cuánto te queda cada mes.</p>
      <RouterLink to="/gastos?nuevo=1" class="btn btn-primary btn-hero">
        <Plus :size="17" :stroke-width="2.4" />
        Registra tu primer gasto
      </RouterLink>
    </div>

    <ol class="steps">
      <li v-for="(paso, i) in PASOS" :key="paso.to">
        <RouterLink :to="paso.to" class="step">
          <span class="step-num">{{ i + 1 }}</span>
          <span class="step-icon" :style="{ '--sc': paso.color }"><component :is="paso.icon" :size="18" :stroke-width="1.9" /></span>
          <span class="step-text">
            <strong>{{ paso.titulo }}</strong>
            <span>{{ paso.desc }}</span>
          </span>
          <ArrowRight :size="16" :stroke-width="2" class="step-arrow" />
        </RouterLink>
      </li>
    </ol>

    <p class="tip">
      <MessageCircle :size="14" :stroke-width="2" />
      ¿Prefieres el celular? Vincula WhatsApp en tu <RouterLink to="/perfil">perfil</RouterLink> y envía <code>25000 almuerzo</code>.
    </p>
  </section>

  <!-- Mes sin movimientos -->
  <section v-else class="empty-start empty-start--mes">
    <div class="hero hero--compact">
      <span class="hero-icon hero-icon--muted"><CalendarPlus :size="26" :stroke-width="1.8" /></span>
      <h2 class="hero-title">Aún no hay movimientos en {{ mesLabel }}</h2>
      <p class="hero-sub">Registra lo de este mes o trae tus gastos e ingresos fijos del mes anterior en un clic.</p>
      <div class="hero-actions">
        <RouterLink to="/gastos?nuevo=1" class="btn btn-primary btn-hero">
          <Plus :size="17" :stroke-width="2.4" />
          Registrar un gasto
        </RouterLink>
        <RouterLink to="/ingresos?nuevo=1" class="btn btn-hero">
          <Banknote :size="16" :stroke-width="2" />
          Agregar ingreso
        </RouterLink>
        <button class="btn btn-hero btn-ghost" :disabled="copiando" @click="copiarRecurrentes">
          <Repeat :size="16" :stroke-width="2" :class="{ 'spin': copiando }" />
          {{ copiando ? 'Copiando…' : 'Copiar fijos del mes anterior' }}
        </button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { RouterLink } from 'vue-router'
import {
  Sparkles, Plus, ArrowRight, Banknote, Receipt, Landmark, MessageCircle, CalendarPlus, Repeat,
} from 'lucide-vue-next'
import { useFinanceStore } from '@/stores/finance'
import { useToast } from '@/composables/useToast'

defineProps<{ variante: 'nuevo' | 'mes' }>()

const store = useFinanceStore()
const toast = useToast()

const MESES = ['enero','febrero','marzo','abril','mayo','junio','julio','agosto','septiembre','octubre','noviembre','diciembre']
const mesLabel = computed(() => `${MESES[store.mesActual - 1]} ${store.anioActual}`)

const PASOS = [
  { titulo: 'Agrega tu ingreso',  desc: 'Salario, freelance o lo que recibas al mes', to: '/ingresos?nuevo=1', icon: Banknote, color: 'var(--c-pos)' },
  { titulo: 'Registra un gasto',  desc: 'Fijos como arriendo u ocasionales como comida', to: '/gastos?nuevo=1', icon: Receipt,  color: 'var(--c-want)' },
  { titulo: 'Crea tus cuentas',   desc: 'Banco, efectivo o alcancía para ver tu saldo real', to: '/cuentas',     icon: Landmark, color: 'var(--c-need)' },
]

const copiando = ref(false)
async function copiarRecurrentes() {
  copiando.value = true
  try {
    const [g, i] = await Promise.all([store.copiarRecurrentes(), store.copiarIngresosRecurrentes()])
    if (g + i > 0) toast.success(`Copiados ${g} gasto${g !== 1 ? 's' : ''} y ${i} ingreso${i !== 1 ? 's' : ''} recurrentes`)
    else toast.info('El mes anterior no tiene movimientos recurrentes para copiar')
  } catch {
    toast.error('No se pudieron copiar los recurrentes')
  } finally {
    copiando.value = false
  }
}
</script>

<style scoped>
.empty-start {
  display: flex; flex-direction: column; gap: 1.25rem;
  max-width: 760px; margin: 1rem auto 0;
}

.hero {
  display: flex; flex-direction: column; align-items: center; text-align: center; gap: 0.6rem;
  padding: 2.5rem 1.5rem 2.25rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  background:
    radial-gradient(90% 120% at 50% 0%, color-mix(in srgb, var(--primary) 14%, transparent), transparent 65%),
    var(--surface);
  box-shadow: var(--shadow-card);
}
.hero--compact { padding: 2rem 1.5rem; }
.hero-icon {
  display: grid; place-items: center;
  width: 64px; height: 64px; margin-bottom: 0.5rem; border-radius: 20px;
  color: #fff;
  background: linear-gradient(135deg, var(--primary), #8b5cf6);
  box-shadow: 0 12px 30px -10px color-mix(in srgb, var(--primary) 70%, transparent);
}
.hero-icon--muted {
  color: var(--accent-strong);
  background: color-mix(in srgb, var(--accent) 16%, var(--surface2));
  box-shadow: none;
}
.hero-title { font-size: 1.35rem; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); }
.hero-sub   { font-size: 0.82rem; line-height: 1.55; color: var(--text-subtle); max-width: 460px; }

.btn-hero { min-height: 46px; padding: 0.7rem 1.4rem; font-size: 0.86rem; border-radius: 12px; text-decoration: none; margin-top: 0.75rem; }
.hero-actions { display: flex; flex-wrap: wrap; justify-content: center; gap: 0.6rem; }
.hero-actions .btn-hero { margin-top: 0.6rem; }
.btn-ghost { background: transparent; border-color: transparent; }
.btn-ghost:hover:not(:disabled) { background: var(--surface2); border-color: var(--border); }
.spin { animation: spin 0.9s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Pasos */
.steps { list-style: none; display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem; }
.step {
  position: relative;
  display: flex; flex-direction: column; gap: 0.7rem; height: 100%;
  padding: 1.1rem 1.15rem;
  background: var(--surface); border: 1px solid var(--border);
  border-radius: var(--radius-card);
  text-decoration: none;
  transition: border-color 0.15s ease, transform 0.15s ease, background 0.15s ease;
}
.step:hover { border-color: color-mix(in srgb, var(--primary) 45%, var(--border)); background: var(--surface2); transform: translateY(-2px); }
.step-num {
  position: absolute; top: 0.9rem; right: 1rem;
  font-family: var(--ff-mono); font-size: 0.7rem; font-weight: 700; color: var(--text-muted);
}
.step-icon {
  display: grid; place-items: center;
  width: 38px; height: 38px; border-radius: 11px;
  color: var(--sc); background: color-mix(in srgb, var(--sc) 14%, transparent);
}
.step-text { display: flex; flex-direction: column; gap: 0.25rem; }
.step-text strong { font-size: 0.86rem; font-weight: 700; color: var(--text-primary); }
.step-text span   { font-size: 0.72rem; line-height: 1.45; color: var(--text-subtle); }
.step-arrow { color: var(--text-muted); margin-top: auto; transition: transform 0.15s ease, color 0.15s ease; }
.step:hover .step-arrow { color: var(--primary); transform: translateX(3px); }

.tip {
  display: flex; align-items: center; justify-content: center; flex-wrap: wrap; gap: 0.4rem;
  font-size: 0.74rem; color: var(--text-muted); text-align: center;
}
.tip a { color: var(--accent-strong); }
.tip code {
  font-family: var(--ff-mono); font-size: 0.7rem; color: var(--text-primary);
  padding: 0.1rem 0.4rem; border-radius: 5px; background: var(--surface2);
}

@media (max-width: 700px) {
  .steps { grid-template-columns: 1fr; }
  .step { flex-direction: row; align-items: center; }
  .step-num { position: static; order: -1; }
  .step-arrow { margin-top: 0; margin-left: auto; }
  .hero { padding: 2rem 1.1rem; }
  .hero-actions { flex-direction: column; align-items: stretch; }
}
</style>
