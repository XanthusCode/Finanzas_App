import { jwtDecode } from 'jwt-decode'

export const fmtCOP = (n: number): string => {
  const abs = Math.round(Math.abs(n))
  return (n < 0 ? '-$' : '$') + abs.toLocaleString('es-CO')
}

export function exportCSV(rows: Record<string, unknown>[], filename: string) {
  if (rows.length === 0) return
  const headers = Object.keys(rows[0]!)
  const csv = [
    headers.join(','),
    ...rows.map(r => headers.map(h => {
      const v = String(r[h] ?? '')
      return v.includes(',') ? `"${v}"` : v
    }).join(','))
  ].join('\n')
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const url  = URL.createObjectURL(blob)
  const a    = Object.assign(document.createElement('a'), { href: url, download: filename })
  a.click()
  URL.revokeObjectURL(url)
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
