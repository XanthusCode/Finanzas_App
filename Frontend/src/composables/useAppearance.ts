import { ref, watch } from 'vue'

const THEME_KEY = 'finanzas_theme'
const ACCENT_KEY = 'finanzas_accent'

// Paleta curada: tonos profundos que llevan bien texto blanco encima (no flúor).
// El color elegido pinta --accent (detalles) y --primary (botones principales).
export const ACCENT_PRESETS: { name: string; value: string | null }[] = [
  { name: 'Predeterminado', value: null },
  { name: 'Esmeralda', value: '#059669' },
  { name: 'Zafiro',    value: '#2563eb' },
  { name: 'Amatista',  value: '#7c3aed' },
  { name: 'Rubí',      value: '#be123c' },
  { name: 'Ámbar',     value: '#b45309' },
  { name: 'Pizarra',   value: '#475569' },
]

const theme = ref<'dark' | 'light'>(localStorage.getItem(THEME_KEY) === 'light' ? 'light' : 'dark')
const accent = ref<string | null>(localStorage.getItem(ACCENT_KEY))

function apply() {
  document.documentElement.setAttribute('data-theme', theme.value)
  const root = document.documentElement.style
  if (accent.value) {
    root.setProperty('--accent', accent.value)
    root.setProperty('--primary', accent.value)
    root.setProperty('--primary-hover', `color-mix(in srgb, ${accent.value} 84%, #fff)`)
  } else {
    root.removeProperty('--accent')
    root.removeProperty('--primary')
    root.removeProperty('--primary-hover')
  }
}
apply()

watch(theme, t => {
  localStorage.setItem(THEME_KEY, t)
  apply()
})

watch(accent, a => {
  if (a) localStorage.setItem(ACCENT_KEY, a)
  else localStorage.removeItem(ACCENT_KEY)
  apply()
})

export function useAppearance() {
  function toggleTheme() {
    theme.value = theme.value === 'dark' ? 'light' : 'dark'
  }
  function setAccent(value: string | null) {
    accent.value = value
  }
  return { theme, accent, toggleTheme, setAccent }
}
