import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import axios from 'axios'

interface AuthUser {
  email: string
  nombre: string
}

interface AuthResponse {
  token: string
  email: string
  nombre: string
  expiresIn: number
}

const TOKEN_KEY = 'finanzas_token'
const USER_KEY  = 'finanzas_user'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const user  = ref<AuthUser | null>(
    JSON.parse(localStorage.getItem(USER_KEY) ?? 'null')
  )

  const isAuthenticated = computed(() => !!token.value)

  function setSession(data: AuthResponse) {
    token.value = data.token
    user.value  = { email: data.email, nombre: data.nombre }
    localStorage.setItem(TOKEN_KEY, data.token)
    localStorage.setItem(USER_KEY, JSON.stringify(user.value))
  }

  function clearSession() {
    token.value = null
    user.value  = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  async function login(email: string, password: string): Promise<string | null> {
    try {
      const { data } = await axios.post<AuthResponse>('/api/auth/login', { email, password })
      setSession(data)
      return null
    } catch (err: any) {
      return err?.response?.data?.message ?? 'Error al iniciar sesión'
    }
  }

  async function register(nombre: string, email: string, password: string): Promise<string | null> {
    try {
      const { data } = await axios.post<AuthResponse>('/api/auth/register', { nombre, email, password })
      setSession(data)
      return null
    } catch (err: any) {
      return err?.response?.data?.message ?? 'Error al registrarse'
    }
  }

  async function changePassword(currentPassword: string, newPassword: string): Promise<string | null> {
    try {
      await axios.put('/api/auth/password', { currentPassword, newPassword })
      return null
    } catch (err: any) {
      return err?.response?.data?.message ?? 'Error al cambiar la contraseña'
    }
  }

  function logout() {
    clearSession()
  }

  return { token, user, isAuthenticated, login, register, changePassword, logout }
})
