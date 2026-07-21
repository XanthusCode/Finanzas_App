import {
  Tag, Utensils, Car, Home, ShoppingBag, HeartPulse, Gamepad2, GraduationCap,
  Plane, Dumbbell, PawPrint, Gift, Zap, Phone, Shirt, Coffee, Wifi, Film,
} from 'lucide-vue-next'

export const COLOR_PALETTE = [
  '#63b3ff', // accent
  '#a78bfa', // violeta
  '#34d399', // verde
  '#f87171', // rojo
  '#fbbf24', // ámbar
  '#fb923c', // naranja
  '#f472b6', // rosa
  '#22d3ee', // cian
  '#a3e635', // lima
  '#94a3b8', // gris
]

// Palabras clave (sin tildes) → ícono. Se evalúan en orden, la primera que aparezca en el
// nombre de la categoría gana. Así el ícono se infiere solo, sin pedirle al usuario que elija uno.
const KEYWORD_ICONS: [string[], typeof Tag][] = [
  [['comida', 'mercado', 'restaurante', 'almuerzo', 'super', 'supermercado'], Utensils],
  [['cafe'], Coffee],
  [['transporte', 'carro', 'gasolina', 'uber', 'taxi', 'bus', 'moto'], Car],
  [['arriendo', 'casa', 'vivienda', 'hogar'], Home],
  [['ropa', 'vestir'], Shirt],
  [['salud', 'medico', 'eps', 'droguer', 'farmacia'], HeartPulse],
  [['gym', 'gimnasio', 'deporte', 'ejercicio'], Dumbbell],
  [['estudio', 'universidad', 'colegio', 'curso', 'educacion'], GraduationCap],
  [['viaje', 'vuelo', 'vacaciones', 'hotel'], Plane],
  [['mascota', 'perro', 'gato', 'veterinar'], PawPrint],
  [['regalo', 'cumplea'], Gift],
  [['servicio', 'luz', 'agua', 'gas', 'electric'], Zap],
  [['internet', 'wifi'], Wifi],
  [['telefono', 'celular', 'plan'], Phone],
  [['entretenimiento', 'cine', 'netflix', 'juego', 'streaming'], Gamepad2],
  [['pelicula', 'serie'], Film],
  [['compra', 'ropa', 'tienda'], ShoppingBag],
]

function sinTildes(s: string) {
  return s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase()
}

export function guessIcono(nombre: string) {
  const n = sinTildes(nombre)
  for (const [keywords, icon] of KEYWORD_ICONS) {
    if (keywords.some(k => n.includes(k))) return icon
  }
  return Tag
}

// Mismo nombre → mismo color siempre, distinto entre categorías. Sin pedirle nada al usuario ni guardar nada.
export function guessColor(nombre: string) {
  let hash = 0
  for (let i = 0; i < nombre.length; i++) {
    hash = (hash << 5) - hash + nombre.charCodeAt(i)
    hash |= 0
  }
  return COLOR_PALETTE[Math.abs(hash) % COLOR_PALETTE.length]!
}
