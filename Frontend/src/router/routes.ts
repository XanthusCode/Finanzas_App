import Dashboard   from '@/views/Dashboard.vue'
import Gastos      from '@/views/Gastos.vue'
import Ingresos    from '@/views/Ingresos.vue'
import Categorias  from '@/views/Categorias.vue'
import Presupuesto from '@/views/Presupuesto.vue'
import Metas       from '@/views/Metas.vue'
import Perfil      from '@/views/user/Perfil.vue'
import Login       from '@/views/user/login.vue'
import Register    from '@/views/user/Register.vue'

const auth = { requiresAuth: true }

const routes = [
  { path: '/', redirect: '/dashboard' },

  { path: '/dashboard',   component: Dashboard,   meta: auth },
  { path: '/gastos',      component: Gastos,      meta: auth },
  { path: '/ingresos',    component: Ingresos,    meta: auth },
  { path: '/categorias',  component: Categorias,  meta: auth },
  { path: '/presupuesto', component: Presupuesto, meta: auth },
  { path: '/metas',       component: Metas,       meta: auth },
  { path: '/perfil',      component: Perfil,      meta: auth },

  { path: '/login',    component: Login,    meta: { guest: true } },
  { path: '/register', component: Register, meta: { guest: true } },
]

export default routes
