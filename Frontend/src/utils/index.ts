import { jwtDecode } from 'jwt-decode'

export const fmtCOP = (n: number): string => {
  const abs = Math.round(Math.abs(n))
  return (n < 0 ? '-$' : '$') + abs.toLocaleString('es-CO')
}

// Fecha de hoy (YYYY-MM-DD) en la zona horaria local. No usar toISOString(): devuelve
// la fecha UTC, que en Colombia ya es "mañana" después de las 7 p.m.
export const hoyLocal = (): string => {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

export async function exportExcel(rows: Record<string, unknown>[], filename: string, sheetName = 'Datos') {
  if (rows.length === 0) return
  const XLSX = await import('xlsx')
  const hoja = XLSX.utils.json_to_sheet(rows)
  const libro = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(libro, hoja, sheetName)
  XLSX.writeFile(libro, filename)
}

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
