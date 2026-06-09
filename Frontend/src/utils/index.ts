import { jwtDecode } from 'jwt-decode'

export const decodeToken = (token: string): { email: string; nombre: string; exp: number } | null => {
  try {
    return jwtDecode(token)
  } catch (err) {
    console.error('Error decoding token:', err)
    return null
  }
}

export const userFromToken = (token: string) => {
  const decoded = decodeToken(token)
  if (!decoded) return null
  return { email: decoded.email, nombre: decoded.nombre }
}
