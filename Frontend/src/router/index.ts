import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '@/views/Dashboard.vue'
import Gastos from '@/views/Gastos.vue'
import Ingresos from '@/views/Ingresos.vue'
import Categirias from '@/views/Categorias.vue'

export default createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: Dashboard, meta: { title: 'Dashboard' } },
    { path: '/gastos', component: Gastos, meta: { title: 'Gastos' } },
    { path: '/ingresos', component: Ingresos, meta: { title: 'Ingresos' } },
    { path: '/Categorias', component: Categirias, meta: { title: 'Categorías' } }
  ]
})