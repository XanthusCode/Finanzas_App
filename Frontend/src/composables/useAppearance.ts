import { ref, watch } from 'vue'

const THEME_KEY = 'finanzas_theme'
const ACCENT_KEY = 'finanzas_accent'

export const ACCENT_PRESETS: { name: string; value: string | null }[] = [
  { name: 'Azul (predeterminado)', value: null },
  { name: 'Violeta', value: '#a78bfa' },
  { name: 'Verde', value: '#34d399' },
  { name: 'Rosa', value: '#f472b6' },
  { name: 'Naranja', value: '#fb923c' },
  { name: 'Cian', value: '#22d3ee' },
]

const theme = ref<'dark' | 'light'>(localStorage.getItem(THEME_KEY) === 'light' ? 'light' : 'dark')
const accent = ref<string | null>(localStorage.getItem(ACCENT_KEY))

function apply() {
  document.documentElement.setAttribute('data-theme', theme.value)
  if (accent.value) document.documentElement.style.setProperty('--accent', accent.value)
  else document.documentElement.style.removeProperty('--accent')
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
