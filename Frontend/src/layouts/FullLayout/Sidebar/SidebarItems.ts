import {
  LayoutDashboard,
  Receipt,
  Banknote,
  PieChart,
  Target,
  HandCoins,
  CreditCard,
  Landmark,
  Calculator,
  TrendingUp,
  History,
  Tags
} from 'lucide-vue-next'

export const menu = [
  {
    label: 'Registro',
    items: [
      {
        title: 'Dashboard',
        icon: LayoutDashboard,
        route: '/dashboard'
      },
      {
        title: 'Gastos',
        icon: Receipt,
        route: '/gastos'
      },
      {
        title: 'Ingresos',
        icon: Banknote,
        route: '/ingresos'
      }
    ]
  },

  {
    label: 'Planificación',
    items: [
      {
        title: 'Presupuesto',
        icon: PieChart,
        route: '/presupuesto'
      },
      {
        title: 'Metas',
        icon: Target,
        route: '/metas'
      },
      {
        title: 'Deudas',
        icon: HandCoins,
        route: '/deudas'
      },
      {
        title: 'Cuotas',
        icon: CreditCard,
        route: '/cuotas'
      },
      {
        title: 'Cuentas',
        icon: Landmark,
        route: '/cuentas'
      }
    ]
  },

  {
    label: 'Análisis',
    items: [
      {
        title: 'Simulador',
        icon: Calculator,
        route: '/simulador'
      },
      {
        title: 'Proyección',
        icon: TrendingUp,
        route: '/proyeccion'
      },
      {
        title: 'Historial',
        icon: History,
        route: '/historial'
      }
    ]
  },

  {
    label: 'Configuración',
    items: [
      {
        title: 'Categorías',
        icon: Tags,
        route: '/categorias'
      }
    ]
  }
]