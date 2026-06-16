import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Gasto, Ingreso, Resumen, GastoCategoriaAnual, Categoria, Presupuesto, Meta } from '@/types'
import { gastosService, ingresosService, resumenService, categoriasService, presupuestosService, metasService } from '@/services/api'

export const useFinanceStore = defineStore('finance', () => {
  const gastos     = ref<Gasto[]>([])
  const ingresos   = ref<Ingreso[]>([])
  const resumen    = ref<Resumen | null>(null)
  const loading    = ref(false)
  const categorias = ref<Categoria[]>([])
  const presupuestos = ref<Presupuesto[]>([])
  const metas      = ref<Meta[]>([])
  const tendencia  = ref<Resumen[]>([])
  const gastosPorCategoria = ref<GastoCategoriaAnual[]>([])

  const mesActual  = ref(new Date().getMonth() + 1)
  const anioActual = ref(new Date().getFullYear())
  const resumenAnual = ref<Resumen | null>(null)
  const promptRecurrentes         = ref(false)
  const promptIngresosRecurrentes = ref(false)

  const gastosFijos     = computed(() => gastos.value.filter(g => g.tipo === 'FIJO'))
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

  async function actualizarCategoria(id: string, cat: Omit<Categoria, 'id'>) {
    const { data } = await categoriasService.update(id, cat)
    const idx = categorias.value.findIndex(c => c.id === id)
    if (idx !== -1) categorias.value[idx] = data
  }

  async function eliminarCategoria(id: string) {
    await categoriasService.delete(id)
    categorias.value = categorias.value.filter(c => c.id !== id)
  }

  async function cargarDatos() {
    loading.value = true
    gastos.value = []
    ingresos.value = []
    resumen.value = null
    try {
      const [rGastos, rIngresos, rResumen] = await Promise.all([
        gastosService.getAll(mesActual.value, anioActual.value),
        ingresosService.getAll(mesActual.value, anioActual.value),
        resumenService.get(mesActual.value, anioActual.value)
      ])
      gastos.value   = rGastos.data
      ingresos.value = rIngresos.data
      resumen.value  = rResumen.data
    } catch (e) {
      console.error('Error cargando datos:', e)
      gastos.value   = []
      ingresos.value = []
      resumen.value  = null
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

  async function editarGasto(id: string, gasto: Omit<Gasto, 'id'>) {
    const { data } = await gastosService.update(id, { ...gasto, id, mes: mesActual.value, anio: anioActual.value })
    const idx = gastos.value.findIndex(g => g.id === id)
    if (idx !== -1) gastos.value[idx] = data
    await cargarResumen()
  }

  async function eliminarGasto(id: string) {
    await gastosService.delete(id)
    gastos.value = gastos.value.filter(g => g.id !== id)
    await cargarResumen()
  }

  async function importarGastos(filas: Omit<Gasto, 'id' | 'mes' | 'anio'>[]) {
    const { data } = await gastosService.importar(mesActual.value, anioActual.value, filas)
    await cargarDatos()
    return data.importados
  }

  async function copiarRecurrentes() {
    const { data } = await gastosService.copiarRecurrentes(mesActual.value, anioActual.value)
    gastos.value.push(...data)
    await cargarResumen()
    promptRecurrentes.value = false
    return data.length
  }

  async function agregarIngreso(ingreso: Pick<Ingreso, 'concepto' | 'monto'>) {
    const { data } = await ingresosService.create({
      ...ingreso,
      mes: mesActual.value,
      anio: anioActual.value
    })
    ingresos.value.push(data)
    await cargarResumen()
  }

  async function editarIngreso(id: string, ingreso: Pick<Ingreso, 'concepto' | 'monto'>) {
    const { data } = await ingresosService.update(id, { ...ingreso, id, mes: mesActual.value, anio: anioActual.value })
    const idx = ingresos.value.findIndex(i => i.id === id)
    if (idx !== -1) ingresos.value[idx] = data
    await cargarResumen()
  }

  async function eliminarIngreso(id: string) {
    await ingresosService.delete(id)
    ingresos.value = ingresos.value.filter(i => i.id !== id)
    await cargarResumen()
  }

  async function cargarResumen() {
    const { data } = await resumenService.get(mesActual.value, anioActual.value)
    resumen.value = data
  }

  async function cargarResumenAnual() {
    const { data } = await resumenService.getAnual(anioActual.value)
    resumenAnual.value = data
  }

  async function cargarTendencia() {
    const { data } = await resumenService.getTendencia(anioActual.value)
    tendencia.value = data
  }

  async function cargarGastosPorCategoria() {
    const { data } = await resumenService.getGastosPorCategoria(anioActual.value)
    gastosPorCategoria.value = data
  }

  function cambiarMes(mes: number, anio: number) {
    const anioAnterior = anioActual.value
    mesActual.value       = mes
    anioActual.value      = anio
    promptRecurrentes.value         = false
    promptIngresosRecurrentes.value = false
    cargarDatos().then(() => {
      if (gastos.value.length > 0 && !gastos.value.some(g => g.esRecurrente))
        promptRecurrentes.value = true
      if (ingresos.value.length > 0 && !ingresos.value.some(i => i.esRecurrente))
        promptIngresosRecurrentes.value = true
    })
    if (anio !== anioAnterior) {
      cargarResumenAnual()
      cargarTendencia()
      cargarGastosPorCategoria()
    }
  }

  function descartarPromptRecurrentes() {
    promptRecurrentes.value = false
  }

  async function copiarIngresosRecurrentes() {
    const { data } = await ingresosService.copiarRecurrentes(mesActual.value, anioActual.value)
    ingresos.value.push(...data)
    await cargarResumen()
    promptIngresosRecurrentes.value = false
    return data.length
  }

  function descartarPromptIngresosRecurrentes() {
    promptIngresosRecurrentes.value = false
  }

  // Presupuestos
  async function cargarPresupuestos() {
    const { data } = await presupuestosService.getAll()
    presupuestos.value = data
  }

  async function guardarPresupuesto(categoria: string, limite: number) {
    const { data } = await presupuestosService.upsert({ categoria, limite })
    const idx = presupuestos.value.findIndex(p => p.categoria === categoria)
    if (idx !== -1) presupuestos.value[idx] = data
    else presupuestos.value.push(data)
  }

  async function eliminarPresupuesto(id: string) {
    await presupuestosService.delete(id)
    presupuestos.value = presupuestos.value.filter(p => p.id !== id)
  }

  // Metas
  async function cargarMetas() {
    const { data } = await metasService.getAll()
    metas.value = data
  }

  async function crearMeta(meta: Pick<Meta, 'nombre' | 'montoObjetivo' | 'fechaLimite'>) {
    const { data } = await metasService.create(meta)
    metas.value.push(data)
  }

  async function editarMeta(id: string, meta: Pick<Meta, 'nombre' | 'montoObjetivo' | 'fechaLimite'>) {
    const { data } = await metasService.update(id, meta)
    const idx = metas.value.findIndex(m => m.id === id)
    if (idx !== -1) metas.value[idx] = data
  }

  async function abonarMeta(id: string, monto: number) {
    const { data } = await metasService.abonar(id, monto)
    const idx = metas.value.findIndex(m => m.id === id)
    if (idx !== -1) metas.value[idx] = data
  }

  async function eliminarMeta(id: string) {
    await metasService.delete(id)
    metas.value = metas.value.filter(m => m.id !== id)
  }

  return {
    gastos, ingresos, resumen, loading,
    mesActual, anioActual,
    promptRecurrentes, descartarPromptRecurrentes,
    promptIngresosRecurrentes, descartarPromptIngresosRecurrentes, copiarIngresosRecurrentes,
    gastosFijos, gastosVariables,
    categorias, categoriasFijas, categoriasVariables,
    cargarCategorias, agregarCategoria, actualizarCategoria, eliminarCategoria,
    resumenAnual, cargarResumenAnual,
    tendencia, cargarTendencia,
    gastosPorCategoria, cargarGastosPorCategoria,
    cargarDatos, agregarGasto, editarGasto, eliminarGasto, copiarRecurrentes, importarGastos,
    agregarIngreso, editarIngreso, eliminarIngreso, cambiarMes,
    presupuestos, cargarPresupuestos, guardarPresupuesto, eliminarPresupuesto,
    metas, cargarMetas, crearMeta, editarMeta, abonarMeta, eliminarMeta,
  }
})
