<template>
  <div v-if="deudas.length > 0" class="alerta-widget">
    <p class="alerta-titulo">Deudas</p>

    <div v-if="loading" class="alerta-loading">
      <div v-for="i in 2" :key="i" class="skeleton-alerta" />
    </div>

    <template v-else>
      <div v-if="vencidas.length === 0 && proximas.length === 0" class="alerta-ok">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
          <polyline points="20 6 9 17 4 12"/>
        </svg>
        Sin deudas por vencer
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
  border-radius: 10px;
  padding: 1rem;
}

.alerta-titulo {
  font-size: 0.65rem;
  color: var(--text-secondary);
  letter-spacing: 0.1em;
  text-transform: uppercase;
  margin-bottom: 0.75rem;
}

.alerta-ok {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.78rem;
  color: var(--accent);
}

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
