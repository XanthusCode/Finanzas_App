import Dashboard from '@/views/Dashboard.vue'
import Gastos from '@/views/Gastos.vue'
import Ingresos from '@/views/Ingresos.vue'
import Categorias from '@/views/Categorias.vue'
import Login from '@/views/user/login.vue'
import Register from '@/views/user/Register.vue'

const routes = [
  { path: '/', redirect: '/dashboard' },

  { path: '/dashboard',  component: Dashboard,  meta: { requiresAuth: true } },
  { path: '/gastos',     component: Gastos,     meta: { requiresAuth: true } },
  { path: '/ingresos',   component: Ingresos,   meta: { requiresAuth: true } },
  { path: '/categorias', component: Categorias, meta: { requiresAuth: true } },

  { path: '/login',    component: Login,    meta: { guest: true } },
  { path: '/register', component: Register, meta: { guest: true } },
]

export default routes
