import {
  Tag, Utensils, Car, Home, ShoppingBag, HeartPulse, Gamepad2, GraduationCap,
  Plane, Dumbbell, PawPrint, Gift, Zap, Phone, Shirt, Coffee, Wifi, Film,
} from 'lucide-vue-next'

export const COLOR_PALETTE = [
  '#6a8caf', // azul polvo
  '#7fa3a3', // verde azulado
  '#8f8fbd', // lavanda
  '#b08baf', // malva
  '#7fa88f', // salvia
  '#c2a06b', // dorado
  '#a3798f', // rosa apagado
  '#6bb0a8', // aguamarina
  '#9b8fc2', // violeta
  '#8a97a3', // gris neutro
]

// Mismos tonos pero más oscuros/saturados — los de arriba pierden casi todo el
// contraste sobre un fondo claro (blanco de tarjeta + su propio tinte al 13%).
const COLOR_PALETTE_LIGHT = [
  '#306088', // azul polvo
  '#227775', // verde azulado
  '#4744a7', // lavanda
  '#983e7a', // malva
  '#297a4b', // salvia
  '#956723', // dorado
  '#9b3b5e', // rosa apagado
  '#227769', // aguamarina
  '#6146a4', // violeta
  '#57616b', // gris neutro
]

function esTemaClaro() {
  return typeof document !== 'undefined' && document.documentElement.getAttribute('data-theme') === 'light'
}

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
  const idx = Math.abs(hash) % COLOR_PALETTE.length
  return (esTemaClaro() ? COLOR_PALETTE_LIGHT : COLOR_PALETTE)[idx]!
}
