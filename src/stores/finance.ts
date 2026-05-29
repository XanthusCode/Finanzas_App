import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Gasto, Ingreso, Resumen, Categoria } from '@/types'
import { gastosService, ingresosService, resumenService, categoriasService } from '@/services/api'

export const useFinanceStore = defineStore('finance', () => {
  const gastos = ref<Gasto[]>([])
  const ingresos = ref<Ingreso[]>([])
  const resumen = ref<Resumen | null>(null)
  const loading = ref(false)
  const categorias = ref<Categoria[]>([])

  const mesActual = ref(new Date().getMonth() + 1)
  const anioActual = ref(new Date().getFullYear())

  const gastosFijos = computed(() => gastos.value.filter(g => g.tipo === 'FIJO'))
  const gastosVariables = computed(() => gastos.value.filter(g => g.tipo === 'VARIABLE'))
  const categoriasFijas     = computed(() => categorias.value.filter(c => c.tipo === 'FIJO'    && c.activa))
  const categoriasVariables = computed(() => categorias.value.filter(c => c.tipo === 'VARIABLE' && c.activa))
 

  async function cargarCategorias() {
    const { data } = await categoriasService.getAll()
    categorias.value = data
  }
 
  async function agregarCategoria(cat: Omit<Categoria, 'id'>) {
    const { data } = await categoriasService.create(cat)
    categorias.value.push(data)
  }
 
  async function actualizarCategoria(id: number, cat: Omit<Categoria, 'id'>) {
    const { data } = await categoriasService.update(id, cat)
    const idx = categorias.value.findIndex(c => c.id === id)
    if (idx !== -1) categorias.value[idx] = data
  }
 
  async function eliminarCategoria(id: number) {
    await categoriasService.delete(id)
    categorias.value = categorias.value.filter(c => c.id !== id)
  }

  // async function cargarDatos() {
  //   loading.value = true
  //   try {
  //     const [rGastos, rIngresos, rResumen] = await Promise.all([
  //       gastosService.getAll(mesActual.value, anioActual.value),
  //       ingresosService.getAll(mesActual.value, anioActual.value),
  //       resumenService.get(mesActual.value, anioActual.value)
  //     ])
  //     gastos.value = rGastos.data
  //     ingresos.value = rIngresos.data
  //     resumen.value = rResumen.data
  //   } catch (e) {
  //     console.error('Error cargando datos:', e)
  //   } finally {
  //     loading.value = false
  //   }
  // }

  // finance.ts - reemplaza cargarDatos temporalmente
    async function cargarDatos() {
      loading.value = true
      gastos.value = []
      ingresos.value = []
      resumen.value = null
      loading.value = false
      try {
        const [rGastos, rIngresos, rResumen] = await Promise.all([
          gastosService.getAll(mesActual.value, anioActual.value),
          ingresosService.getAll(mesActual.value, anioActual.value),
          resumenService.get(mesActual.value, anioActual.value)
        ])
        gastos.value = rGastos.data
        ingresos.value = rIngresos.data
        resumen.value = rResumen.data
      } catch (e) {
        console.error('Error cargando datos:', e)
        gastos.value = []      // <-- agrega esto
        ingresos.value = []    // <-- agrega esto
        resumen.value = null   // <-- agrega esto
      } finally {
        loading.value = false
      }
    }

  async function agregarGasto(gasto: Omit<Gasto, 'id'>) {
    const { data } = await gastosService.create({
      ...gasto,
      mes: mesActual.value,
      anio: anioActual.value
    })
    gastos.value.push(data)
    await cargarResumen()
  }

  async function eliminarGasto(id: number) {
    await gastosService.delete(id)
    gastos.value = gastos.value.filter(g => g.id !== id)
    await cargarResumen()
  }

  async function agregarIngreso(ingreso: Omit<Ingreso, 'id'>) {
    const { data } = await ingresosService.create({
      ...ingreso,
      mes: mesActual.value,
      anio: anioActual.value
    })
    ingresos.value.push(data)
    await cargarResumen()
  }

  async function eliminarIngreso(id: number) {
    await ingresosService.delete(id)
    ingresos.value = ingresos.value.filter(i => i.id !== id)
    await cargarResumen()
  }

  async function cargarResumen() {
    const { data } = await resumenService.get(mesActual.value, anioActual.value)
    resumen.value = data
  }

  function cambiarMes(mes: number, anio: number) {
    mesActual.value = mes
    anioActual.value = anio
    cargarDatos()
  }

  return {
    gastos, ingresos, resumen, loading,
    mesActual, anioActual,
    gastosFijos, gastosVariables,
    categoriasFijas, categoriasVariables, 
    cargarCategorias, agregarCategoria, 
    actualizarCategoria, eliminarCategoria,
    cargarDatos, agregarGasto, eliminarGasto,
    agregarIngreso, eliminarIngreso, cambiarMes
  }
})