<template>
  <div v-if="deudas.length > 0" class="alerta-widget">
    <p class="alerta-titulo">Deudas</p>

    <div v-if="loading" class="alerta-loading">
      <div v-for="i in 2" :key="i" class="skeleton-alerta" />
    </div>

    <template v-else>
      <div v-if="vencidas.length === 0 && proximas.length === 0" class="alerta-ok">
        <span class="alerta-ok-icon"><ShieldCheck :size="26" :stroke-width="1.8" /></span>
        <p class="alerta-ok-title">{{ sinPendientes ? '¡Felicidades! No tienes deudas pendientes' : 'Todo al día' }}</p>
        <p class="alerta-ok-sub">{{ sinPendientes ? 'Sigue así, tus finanzas están limpias.' : 'Ninguna deuda vence en los próximos 7 días.' }}</p>
      </div>

      <div v-else class="alerta-lista">
        <RouterLink
          v-for="d in [...vencidas, ...proximas]"
          :key="d.id"
          to="/deudas"
          class="alerta-row"
          :class="estaVencida(d) ? 'danger' : 'warn'"
        >
          <span class="alerta-cat">{{ d.persona }} · {{ d.descripcion }}</span>
          <span class="alerta-pct">{{ estaVencida(d) ? 'Vencida' : fmtFecha(d.fechaLimite!) }}</span>
        </RouterLink>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import type { Deuda } from '@/types'
import { deudasService } from '@/services/api'
import { ShieldCheck } from 'lucide-vue-next'

const deudas  = ref<Deuda[]>([])
const loading = ref(false)

function estaVencida(d: Deuda) {
  return !d.pagada && !!d.fechaLimite && new Date(d.fechaLimite) < new Date()
}

function diasParaVencer(d: Deuda) {
  if (!d.fechaLimite) return Infinity
  const ms = new Date(d.fechaLimite).getTime() - Date.now()
  return ms / (1000 * 60 * 60 * 24)
}

function fmtFecha(f: string) {
  return new Date(f).toLocaleDateString('es-CO', { day: 'numeric', month: 'short' })
}

const pendientes = computed(() => deudas.value.filter(d => !d.pagada && d.fechaLimite))
const sinPendientes = computed(() => deudas.value.every(d => d.pagada))
const vencidas    = computed(() => pendientes.value.filter(d => estaVencida(d)))
const proximas    = computed(() =>
  pendientes.value.filter(d => !estaVencida(d) && diasParaVencer(d) <= 7)
)

onMounted(async () => {
  loading.value = true
  try {
    const { data } = await deudasService.getAll()
    deudas.value = data
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.alerta-widget {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-card);
  padding: 1.25rem;
  box-shadow: var(--shadow-card);
}

.alerta-titulo {
  font-size: 0.66rem;
  font-weight: 500;
  color: var(--text-secondary);
  letter-spacing: 0.1em;
  text-transform: uppercase;
  margin-bottom: 0.75rem;
}

.alerta-ok {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 0.35rem;
  padding: 1rem 0.5rem 0.75rem;
}
.alerta-ok-icon {
  display: grid;
  place-items: center;
  width: 56px;
  height: 56px;
  margin-bottom: 0.4rem;
  border-radius: 999px;
  color: #10b981;
  background: color-mix(in srgb, #10b981 12%, transparent);
  box-shadow: 0 0 0 6px color-mix(in srgb, #10b981 6%, transparent);
}
.alerta-ok-title { font-size: 0.84rem; font-weight: 500; color: var(--text-primary); }
.alerta-ok-sub { font-size: 0.7rem; color: var(--text-muted); }

.alerta-lista { display: flex; flex-direction: column; gap: 0.35rem; }

.alerta-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.78rem;
  padding: 0.3rem 0.5rem;
  border-radius: 5px;
  text-decoration: none;
  transition: background 0.15s;
}
.alerta-row.warn   { background: color-mix(in srgb, var(--amber) 8%, transparent); color: var(--amber); }
.alerta-row.danger { background: color-mix(in srgb, var(--red) 8%, transparent); color: var(--red); }
.alerta-row:hover  { filter: brightness(1.15); }

.alerta-cat { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; margin-right: 0.5rem; }
.alerta-pct { font-weight: 600; font-size: 0.72rem; flex-shrink: 0; }

.skeleton-alerta {
  height: 28px;
  border-radius: 5px;
  background: linear-gradient(90deg, var(--surface) 0%, var(--surface2) 50%, var(--surface) 100%);
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
  margin-bottom: 0.35rem;
}
@keyframes shimmer { to { background-position: -200% 0; } }
</style>
