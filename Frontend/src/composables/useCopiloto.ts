import { ref, watch } from 'vue'
import { chatIAService } from '@/services/api'
import type { ChatMensaje } from '@/types'

const STORAGE_KEY = 'finanzas_copiloto'
const abierto  = ref(false)
const cargando = ref(false)
const mensajes = ref<ChatMensaje[]>(cargarHistorial())


function cargarHistorial(): ChatMensaje[] {
  try {
    return JSON.parse(sessionStorage.getItem(STORAGE_KEY) ?? '[]')
  } catch {
    return []
  }
}

watch(mensajes, (val) => {
  try { sessionStorage.setItem(STORAGE_KEY, JSON.stringify(val)) } catch { /* sin storage */ }
}, { deep: true })


export function useCopiloto() {
  async function enviar(texto: string) {
    const limpio = texto.trim()
    if (!limpio || cargando.value) return // evita vacíos y doble envío

    mensajes.value.push({ rol: 'user', contenido: limpio })
    cargando.value = true

    try {
      const { data } = await chatIAService.chat(mensajes.value)
      mensajes.value.push({
        rol: 'assistant',
        contenido: data.estado === 'ok' && data.respuesta
          ? data.respuesta
          : 'No pude responder ahora, intenta de nuevo en un momento.',
      })
    } catch {
      mensajes.value.push({ rol: 'assistant', contenido: 'Hubo un error de conexión. Intenta de nuevo.' })
    } finally {
      cargando.value = false
    }
  }

  function limpiar() {
    mensajes.value = []
  }

  return {
    abierto,
    cargando,
    mensajes,
    enviar,
    limpiar,
    abrir:  () => (abierto.value = true),
    cerrar: () => (abierto.value = false),
    toggle: () => (abierto.value = !abierto.value),
  }
}
