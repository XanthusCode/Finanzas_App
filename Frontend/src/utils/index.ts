import { jwtDecode } from 'jwt-decode'

export const fmtCOP = (n: number): string => {
  const abs = Math.round(Math.abs(n))
  return (n < 0 ? '-$' : '$') + abs.toLocaleString('es-CO')
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
