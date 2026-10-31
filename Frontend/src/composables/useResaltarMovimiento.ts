import { ref, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'

/**
 * Destino de la búsqueda global: cuando la URL trae `?resaltar=<id>` y ese movimiento
 * ya está cargado, hace scroll hasta la fila (`[data-mov-id="<id>"]`) y la resalta unos segundos.
 *
 * @param disponibles ids de los movimientos cargados en la vista
 * @param cargando    true mientras la vista aún está trayendo datos
 * @param antes       se ejecuta justo antes de resaltar (ej. limpiar filtros que ocultarían la fila)
 */
export function useResaltarMovimiento(
  disponibles: () => (string | undefined)[],
  cargando: () => boolean,
  antes?: () => void,
) {
  const route  = useRoute()
  const router = useRouter()
  const resaltadoId = ref<string | null>(null)

  watch(
    [() => route.query.resaltar, cargando, () => disponibles().length],
    async () => {
      const id = route.query.resaltar
      if (typeof id !== 'string' || cargando() || !disponibles().includes(id)) return

      antes?.()
      resaltadoId.value = id
      const { resaltar: _quitar, ...resto } = route.query
      router.replace({ query: resto })

      await nextTick()
      document.querySelector(`[data-mov-id="${CSS.escape(id)}"]`)
        ?.scrollIntoView({ behavior: 'smooth', block: 'center' })
      setTimeout(() => { if (resaltadoId.value === id) resaltadoId.value = null }, 2600)
    },
    { immediate: true },
  )

  return { resaltadoId }
}
