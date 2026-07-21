import {
  Wallet, Target, CreditCard, BarChart3, Wand2,
  LayoutDashboard, ListChecks, TrendingUp,
} from '@lucide/vue'

export interface LandingFeature {
  icon: typeof Wallet
  title: string
  desc: string
}

export const features: LandingFeature[] = [
  { icon: Wallet,        title: 'Gastos e ingresos',   desc: 'Registra gastos fijos y ocasionales, e ingresos recurrentes, organizados por categoría.' },
  { icon: BarChart3,     title: 'Presupuestos',        desc: 'Define límites por categoría y recibe alertas cuando estás cerca de superarlos.' },
  { icon: Target,        title: 'Metas de ahorro',     desc: 'Crea metas y sigue visualmente cuánto te falta para alcanzarlas.' },
  { icon: CreditCard,    title: 'Deudas y cuotas',     desc: 'Lleva el control de lo que debes y lo que te deben, con pagos en cuotas.' },
  { icon: Wand2,         title: 'Simulador what-if',   desc: 'Simula escenarios antes de gastar y ve el impacto en tu mes.' },
  { icon: TrendingUp,    title: 'Historial y proyección', desc: 'Compara meses anteriores y proyecta cómo vas a cerrar el mes actual.' },
]

export interface LandingStat {
  value: string
  label: string
}

export const stats: LandingStat[] = [
  { value: '10',    label: 'Módulos financieros en un solo lugar' },
  { value: '100%', label: 'Tus datos, en tu propia cuenta' },
  { value: '0',    label: 'Hojas de cálculo que necesitas' },
]

export interface LandingStep {
  icon: typeof Wallet
  title: string
  desc: string
}

export const steps: LandingStep[] = [
  { icon: ListChecks,      title: 'Regístrate gratis',            desc: 'Crea tu cuenta en menos de un minuto, sin conectar tu banco.' },
  { icon: Wallet,          title: 'Registra tus movimientos',     desc: 'Anota tus gastos, ingresos y deudas a medida que ocurren.' },
  { icon: LayoutDashboard, title: 'Visualiza y decide',            desc: 'Consulta tu dashboard y estadísticas para tomar mejores decisiones con tu dinero.' },
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
  { question: '¿Necesito conectar mi cuenta bancaria?', answer: 'No. Registras tus movimientos manualmente, así que no compartes tus credenciales bancarias con nadie.' },
  { question: '¿Mis datos están seguros?', answer: 'Tu información se guarda asociada únicamente a tu cuenta y se protege con autenticación mediante token. Solo tú puedes acceder a tus datos.' },
  { question: '¿Puedo usarla desde el celular?', answer: 'Sí, la aplicación es responsive y se adapta a celulares y tablets además del navegador de escritorio.' },
  { question: '¿Puedo exportar mi información?', answer: 'Sí, puedes exportar tus gastos a CSV desde la vista de Gastos.' },
]

export function scrollToSection(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}
