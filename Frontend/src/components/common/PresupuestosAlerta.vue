<template>
  <div v-if="store.presupuestos.length > 0" class="alerta-widget">
    <p class="alerta-titulo">Presupuesto del mes</p>

    <div v-if="loading" class="alerta-loading">
      <div v-for="i in 2" :key="i" class="skeleton-alerta" />
    </div>

    <template v-else>
      <div v-if="enRiesgo.length === 0" class="alerta-ok">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
          <polyline points="20 6 9 17 4 12"/>
        </svg>
        Todos los presupuestos en orden
      </div>

      <div v-else class="alerta-lista">
        <div
          v-for="item in enRiesgo"
          :key="item.categoria"
          class="alerta-row"
          :class="item.estado"
        >
          <span class="alerta-cat">{{ item.categoria }}</span>
          <span class="alerta-pct">{{ item.pct }}%</span>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useFinanceStore } from '@/stores/finance'
import { usePresupuestoMensual } from '@/composables/usePresupuestoMensual'

const store = useFinanceStore()

const mes  = ref(new Date().getMonth() + 1)
const anio = ref(new Date().getFullYear())

const { items, loading, cargar } = usePresupuestoMensual(mes, anio)

const enRiesgo = computed(() =>
  items.value.filter(i => i.estado === 'warn' || i.estado === 'danger')
)

onMounted(async () => {
  if (store.categorias.length === 0) await store.cargarCategorias()
  if (store.presupuestos.length === 0) await store.cargarPresupuestos()
  await cargar()
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
}
.alerta-row.warn   { background: rgba(245,158,11,0.08); color: #f59e0b; }
.alerta-row.danger { background: rgba(248,113,113,0.08); color: var(--red); }

.alerta-cat { flex: 1; }
.alerta-pct { font-weight: 600; font-size: 0.72rem; }

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
