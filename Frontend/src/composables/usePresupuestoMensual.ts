import { ref, computed, watch, type Ref } from 'vue'
import { gastosService } from '@/services/api'
import { useFinanceStore } from '@/stores/finance'

export interface PresupuestoMensualItem {
  categoria: string
  tipo: 'FIJO' | 'VARIABLE'
  limite: number | null
  gastado: number
  pct: number
  estado: 'ok' | 'warn' | 'danger' | 'sin-limite'
}

export function usePresupuestoMensual(mes: Ref<number>, anio: Ref<number>) {
  const store = useFinanceStore()
  const gastadoPorCategoria = ref<Record<string, number>>({})
  const loading = ref(false)

  const items = computed<PresupuestoMensualItem[]>(() =>
    store.categorias
      .filter(c => c.activa)
      .map(c => {
        const presupuesto = store.presupuestos.find(p => p.categoria === c.nombre)
        const gastado = gastadoPorCategoria.value[c.nombre] ?? 0
        const limite = presupuesto?.limite ?? null
        const pct = limite && limite > 0 ? Math.round(gastado / limite * 100) : 0
        const estado: PresupuestoMensualItem['estado'] = !limite
          ? 'sin-limite'
          : pct > 100 ? 'danger'
          : pct >= 80 ? 'warn'
          : 'ok'
        return { categoria: c.nombre, tipo: c.tipo, limite, gastado, pct, estado }
      })
  )

  async function cargar() {
    loading.value = true
    try {
      const { data } = await gastosService.getResumenCategoria(mes.value, anio.value)
      const map: Record<string, number> = {}
      for (const r of data) map[r.categoria] = r.total
      gastadoPorCategoria.value = map
    } finally {
      loading.value = false
    }
  }

  watch([mes, anio], cargar)

  return { items, loading, cargar }
}
