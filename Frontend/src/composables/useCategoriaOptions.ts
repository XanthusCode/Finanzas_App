import {
  Tag, Utensils, Coffee, Car, Motorbike, Fuel,Gem, Home, Shirt, HeartPulse, Dumbbell, GraduationCap,
  Plane, PawPrint, Gift, Zap, Wifi, Phone, Gamepad2, Film, ShoppingBag,Cog,
  Bot, Laptop, Droplets, HandCoins, Music, Pill, Landmark, Baby, Wrench, Beer, Scissors,
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

function hslToHex(h: number, s: number, l: number): string {
  s /= 100
  l /= 100
  const a = s * Math.min(l, 1 - l)
  const f = (n: number) => {
    const k = (n + h / 30) % 12
    const c = l - a * Math.max(-1, Math.min(k - 3, 9 - k, 1))
    return Math.round(c * 255).toString(16).padStart(2, '0')
  }
  return `#${f(0)}${f(8)}${f(4)}`
}

function esTemaClaro() {
  return typeof document !== 'undefined' && document.documentElement.getAttribute('data-theme') === 'light'
}

const KEYWORD_ICONS: [string[], typeof Tag][] = [
  [['comida', 'mercado', 'restaurante', 'almuerzo', 'super', 'domicilio', 'rappi'], Utensils],
  [['cafe'], Coffee],
  [[ 'moto', 'motocicleta' ], Motorbike],
  [[ 'gasolina' ], Fuel],
  [['accesorio', 'repuesto', 'llanta', 'casco', 'aceite'], Cog],
  [['joya', 'bijou', 'pulsera', 'anillo'], Gem],
  [['transporte', 'carro', 'uber', 'taxi', 'bus', 'peaje', 'parqueadero'], Car],
  [['arriendo', 'casa', 'vivienda', 'hogar', 'administracion'], Home],
  [['ropa', 'vestir', 'zapato'], Shirt],
  [['salud', 'medico', 'eps', 'droguer', 'farmacia', 'odonto', 'dentista'], HeartPulse],
  [['suplement', 'proteina', 'vitamina', 'creatina'], Pill],
  [['gym', 'gimnasio', 'deporte', 'ejercicio'], Dumbbell],
  [['estudio', 'universidad', 'colegio', 'curso', 'educacion', 'libro'], GraduationCap],
  [['viaje', 'vuelo', 'vacaciones', 'hotel'], Plane],
  [['mascota', 'perro', 'gato', 'veterinar'], PawPrint],
  [['regalo', 'cumplea'], Gift],
  [['servicio', 'luz', 'agua', 'electric', 'energia'], Zap],
  [['internet', 'wifi'], Wifi],
  [['telefono', 'celular', 'plan'], Phone],
  [['computador', 'laptop', 'pc', 'tecnologia', 'software', 'hosting', 'dominio'], Laptop],
  [['ia', 'agente', 'chatgpt', 'claude', 'openai', 'gemini'], Bot],
  [['spotify', 'musica'], Music],
  [['entretenimiento', 'cine', 'netflix', 'juego', 'streaming', 'steam', 'xbox', 'playstation'], Gamepad2],
  [['pelicula', 'serie'], Film],
  [['prestamo', 'deuda', 'credito', 'cuota'], HandCoins],
  [['banco', 'impuesto', 'ahorro', 'inversion', 'seguro'], Landmark],
  [['higiene', 'aseo', 'limpieza', 'cuidado'], Droplets],
  [['peluqueria', 'barberia', 'corte', 'belleza'], Scissors],
  [['bebe', 'hijo', 'pañal'], Baby],
  [['reparacion', 'mantenimiento', 'taller', 'herramienta', 'accesorio'], Wrench],
  [['fiesta', 'bar', 'trago', 'cerveza', 'salida'], Beer],
  [['compra', 'tienda', 'app'], ShoppingBag],
]

function normalizar(texto: string): string {
  return texto.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '')
}

export function guessIcono(nombre: string) {
  const palabras = normalizar(nombre).split(/\s+/)
  for (const [keywords, icono] of KEYWORD_ICONS) {
    if (keywords.some(k => palabras.some(p => p.startsWith(k)))) return icono
  }
  return Tag
}

export function guessColor(nombre: string) {
  let hash = 0
  for (let i = 0; i < nombre.length; i++) {
    hash = (hash << 5) - hash + nombre.charCodeAt(i)
    hash |= 0
  }
  const hue = (Math.abs(hash) * 137.508) % 360
  return esTemaClaro() ? hslToHex(hue, 60, 42) : hslToHex(hue, 65, 62)
}