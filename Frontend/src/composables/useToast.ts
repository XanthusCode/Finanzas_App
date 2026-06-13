import { ref } from 'vue'

type ToastType = 'success' | 'error' | 'info'

interface Toast {
  id: number
  message: string
  type: ToastType
}

const toasts = ref<Toast[]>([])
let _id = 0

export function useToast() {
  function push(message: string, type: ToastType, duration = 3200) {
    const id = _id++
    toasts.value.push({ id, message, type })
    setTimeout(() => dismiss(id), duration)
  }

  function dismiss(id: number) {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }

  return {
    toasts,
    success: (m: string) => push(m, 'success'),
    error:   (m: string) => push(m, 'error'),
    info:    (m: string) => push(m, 'info'),
    dismiss,
  }
}
