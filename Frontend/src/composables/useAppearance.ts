import { ref, watch } from 'vue'

const THEME_KEY = 'finanzas_theme'
const ACCENT_KEY = 'finanzas_accent'

export const ACCENT_PRESETS: { name: string; value: string | null }[] = [
  { name: 'Acero (predeterminado)', value: null },

  { name: 'Bosque', value: '#4a8a6e' },
  { name: 'Océano', value: '#4f83a8' },
  { name: 'Índigo', value: '#6875b5' },
  { name: 'Violeta', value: '#8667a8' },

  { name: 'Vino', value: '#a85a6e' },
  { name: 'Coral', value: '#b86f68' },
  { name: 'Terracota', value: '#b8795f' },

  { name: 'Dorado', value: '#c2a06b' },
  { name: 'Oliva', value: '#8d9160' },
  { name: 'Turquesa', value: '#57999a' },

  { name: 'Pizarra', value: '#6b6f78' },
  { name: 'Carbón', value: '#555b66' },
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
