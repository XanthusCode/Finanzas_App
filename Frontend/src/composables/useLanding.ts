import {
  Wallet, Target, CreditCard, BarChart3, Wand2, Landmark, Sparkles, Search,
  LayoutDashboard, ListChecks, TrendingUp, HandCoins,
} from '@lucide/vue'

export interface LandingFeature {
  icon: typeof Wallet
  title: string
  desc: string
  color: string
}

// Tarjetas pequeñas del bento (Dashboard y WhatsApp son las grandes, con visual propio)
export const features: LandingFeature[] = [
  { icon: BarChart3,  title: 'Presupuestos',          desc: 'Límites por categoría con alertas antes de pasarte.',            color: '#d4a85a' },
  { icon: Target,     title: 'Metas de ahorro',       desc: 'Mira cuánto te falta y en cuántos meses llegas a tu ritmo.',    color: '#34d399' },
  { icon: Landmark,   title: 'Cuentas',               desc: 'Banco, efectivo o alcancía: tu saldo real, cuadrado.',           color: '#60a5fa' },
  { icon: HandCoins,  title: 'Deudas y cuotas',       desc: 'Lo que debes, lo que te deben y cada cuota en su mes.',         color: '#f87171' },
  { icon: Sparkles,   title: 'Análisis con IA',       desc: 'Patrones y recomendaciones de tu mes en un clic.',               color: '#a78bfa' },
  { icon: Wand2,      title: 'Simulador what-if',     desc: '¿Y si gasto menos en comida? Mira el impacto antes de decidir.', color: '#60a5fa' },
  { icon: TrendingUp, title: 'Proyección de cierre',  desc: 'Cómo vas a terminar el mes según tu ritmo de gasto.',            color: '#34d399' },
  { icon: Search,     title: 'Búsqueda global',       desc: 'Ctrl + K: encuentra "Netflix" o "$50.000" en cualquier mes.',   color: '#94a3b8' },
]

export interface LandingStat {
  value: string
  label: string
}

export const stats: LandingStat[] = [
  { value: '12',   label: 'módulos financieros en un solo lugar' },
  { value: '0',    label: 'conexiones bancarias necesarias' },
  { value: '1',    label: 'mensaje de WhatsApp para registrar un gasto' },
]

export interface LandingStep {
  icon: typeof Wallet
  title: string
  desc: string
}

export const steps: LandingStep[] = [
  { icon: ListChecks,      title: 'Crea tu cuenta',            desc: 'En menos de un minuto y sin conectar tu banco ni tarjeta.' },
  { icon: Wallet,          title: 'Registra tus movimientos',  desc: 'Desde la app o escribiéndole al bot de WhatsApp, como un chat.' },
  { icon: LayoutDashboard, title: 'Decide con claridad',       desc: 'Tu dashboard te dice cuánto te queda, en qué se va y cómo cerrarás el mes.' },
]

export interface LandingTestimonial {
  quote: string
  author: string
}

export const testimonials: LandingTestimonial[] = [
  { quote: 'La app reemplazó por completo mi libreta de gastos en papel. La experiencia es muy buena.', author: 'Usuario beta' },
  { quote: 'Me gustó mucho en general — se siente útil de verdad para llevar el control de mis finanzas.', author: 'Usuario beta' },
  { quote: 'Solo hacen falta pequeños detalles para que sea aún más intuitiva, pero ya la uso todos los días.', author: 'Usuario beta' },
]

export interface LandingFaqItem {
  question: string
  answer: string
}

export const faqItems: LandingFaqItem[] = [
  { question: '¿Tiene algún costo?', answer: 'La aplicación es gratuita mientras está en fase beta.' },
  { question: '¿Necesito conectar mi cuenta bancaria?', answer: 'No. Registras tus movimientos manualmente (o por WhatsApp), así que no compartes tus credenciales bancarias con nadie.' },
  { question: '¿Cómo funciona el registro por WhatsApp?', answer: 'Vinculas tu WhatsApp desde tu perfil y luego le escribes al bot algo como "25000 almuerzo". El gasto queda registrado con la fecha de hoy y la categoría que coincida.' },
  { question: '¿Mis datos están seguros?', answer: 'Tu información se guarda asociada únicamente a tu cuenta y se protege con autenticación mediante token. Solo tú puedes acceder a tus datos.' },
  { question: '¿Puedo usarla desde el celular?', answer: 'Sí, la aplicación es responsive y se adapta a celulares y tablets además del navegador de escritorio.' },
  { question: '¿Puedo exportar mi información?', answer: 'Sí, puedes exportar tus gastos e ingresos a Excel desde sus respectivas vistas, además de generar un reporte mensual en PDF desde el Dashboard.' },
]

export function scrollToSection(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}
