import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import axios from 'axios'
import { api, perfilService } from '@/services/api'

interface AuthResponse {
  token: string
  email: string
  nombre: string
  expiresIn: number
}

const TOKEN_KEY = 'finanzas_token'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))

  const isAuthenticated = computed(() => !!token.value)

  function setSession(data: AuthResponse) {
    token.value = data.token
    localStorage.setItem(TOKEN_KEY, data.token)
  }

  function clearSession() {
    token.value = null
    localStorage.removeItem(TOKEN_KEY)
  }

  async function login(email: string, password: string): Promise<string | null> {
    try {
      const { data } = await api.post<AuthResponse>('/auth/login', { email, password })
      setSession(data)
      return null
    } catch (err) {
      if (axios.isAxiosError(err)) return err.response?.data?.message ?? 'Error al iniciar sesión'
      return 'Error al iniciar sesión'
    }
  }

  async function register(nombre: string, email: string, password: string): Promise<string | null> {
    try {
      const { data } = await api.post<AuthResponse>('/auth/register', { nombre, email, password })
      setSession(data)
      return null
    } catch (err) {
      if (axios.isAxiosError(err)) return err.response?.data?.message ?? 'Error al registrarse'
      return 'Error al registrarse'
    }
  }

  async function changePassword(currentPassword: string, newPassword: string): Promise<string | null> {
    try {
      await api.put('/auth/password', { currentPassword, newPassword })
      return null
    } catch (err) {
      if (axios.isAxiosError(err)) return err.response?.data?.message ?? 'Error al cambiar la contraseña'
      return 'Error al cambiar la contraseña'
    }
  }

  async function updateNombre(nombre: string): Promise<string | null> {
    try {
      const { data } = await perfilService.updateNombre(nombre)
      setSession(data)
      return null
    } catch (err) {
      if (axios.isAxiosError(err)) return err.response?.data?.message ?? 'Error al actualizar el perfil'
      return 'Error al actualizar el perfil'
    }
  }

  function logout() {
    clearSession()
  }

  return { token, isAuthenticated, login, register, changePassword, updateNombre, logout }
})
