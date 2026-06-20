# Presupuestos y Metas Inteligentes — Diseño

**Fecha:** 2026-06-19
**Sub-proyecto:** 1 de 3 (Dashboard mejorado y Reportes vienen después)

---

## Goal

Hacer que los presupuestos sean mensuales reales (filtrados por mes/año en BD) y agregar proyecciones de cumplimiento + alertas de urgencia a las metas de ahorro.

## Architecture

Enfoque híbrido: el backend provee un endpoint de resumen mensual por categoría (query de BD eficiente), el frontend calcula proyecciones y urgencia de metas con aritmética local. No se modifica el esquema de BD.

## Tech Stack

- Backend: ASP.NET Core 10, EF Core, LINQ
- Frontend: Vue 3, TypeScript, Pinia, composables

---

## Backend

### Endpoint nuevo

`GET /gastos/resumen-categoria?mes={mes}&anio={anio}`

- Requiere JWT (`[Authorize]`)
- Filtra `Gastos` por `UserId + Mes + Anio`
- Agrupa por `Categoria` y suma `Monto`
- Devuelve `IEnumerable<ResumenCategoriaDto>`

```csharp
public class ResumenCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
```

- Se agrega en `GastosController` (ya existe)
- Se agrega en `IGastosRepository` + `GastosRepository`
- No requiere cambios en entidades ni en BD

### Sin cambios en entidad Presupuesto

El límite de `Presupuesto` es mensual recurrente (aplica igual todos los meses). No necesita campo `Mes` ni `Anio`.

---

## Frontend

### Composable 1: `usePresupuestoMensual`

**Archivo:** `Frontend/src/composables/usePresupuestoMensual.ts`

**Recibe:** `mes: Ref<number>`, `anio: Ref<number>`

**Hace:**
1. Llama a `GET /gastos/resumen-categoria?mes={mes}&anio={anio}` cuando cambia mes/anio
2. Cruza resultado con `store.presupuestos`
3. Devuelve lista reactiva:

```ts
interface PresupuestoMensualItem {
  categoria: string
  limite: number | null      // null si no tiene presupuesto asignado
  gastado: number
  pct: number                // 0-100+
  estado: 'ok' | 'warn' | 'danger' | 'sin-limite'
}
```

**Reglas de estado:**
- `pct >= 100` → `danger`
- `pct >= 80` → `warn`
- `pct < 80` → `ok`
- Sin límite asignado → `sin-limite`

### Composable 2: `useMetaProjection`

**Archivo:** `Frontend/src/composables/useMetaProjection.ts`

**Recibe:** `meta: Meta`

**Devuelve:**

```ts
interface MetaProjection {
  fechaEstimada: Date | null   // null si no hay tasa de ahorro
  urgencia: 'ok' | 'warn' | 'danger' | null  // null si no tiene FechaLimite
  diasRestantes: number | null
}
```

**Lógica de fecha estimada:**
```
diasDesdeCreacion = hoy - CreadoEn
if (MontoActual === 0 || diasDesdeCreacion < 1) → fechaEstimada = null
tasaDiaria = MontoActual / diasDesdeCreacion
diasNecesarios = (MontoObjetivo - MontoActual) / tasaDiaria
fechaEstimada = hoy + diasNecesarios
```

**Lógica de urgencia** (solo si tiene `FechaLimite`):
```
pct = MontoActual / MontoObjetivo * 100
diasRestantes = FechaLimite - hoy
si diasRestantes < 0               → danger (vencida)
si diasRestantes < 30 y pct < 50  → danger
si diasRestantes < 60 y pct < 75  → warn
caso contrario                     → ok
```

**Casos borde:**
- Meta `Completada` → devuelve `{ fechaEstimada: null, urgencia: null, diasRestantes: null }`
- Sin `FechaLimite` → `urgencia: null`, pero sí calcula `fechaEstimada`
- Meta con <1 día de vida y MontoActual = 0 → `fechaEstimada: null`

### Cambio en `Presupuesto.vue`

- Agrega `MonthSelector` (componente ya existe en `components/common/`)
- Reemplaza el cálculo manual de `gastosPorCategoria` por `usePresupuestoMensual`
- Consume la propiedad `estado` para clases CSS (ya tiene `bar-ok/warn/danger`)
- Añade banner de alerta en la parte superior si alguna categoría está en `danger`

### Cambio en `Metas.vue`

En cada tarjeta de meta, agrega (usando `useMetaProjection`):
- **Badge de urgencia:** punto amarillo/rojo si `urgencia === 'warn'/'danger'`
- **Chip de fecha estimada:** "~Mar 2027" si hay `fechaEstimada` y la meta no está completada
- **Chip de días restantes:** "15 días" en rojo si `urgencia === 'danger'`

### Componente nuevo: `PresupuestosAlerta.vue`

**Archivo:** `Frontend/src/components/common/PresupuestosAlerta.vue`

**Uso:** Widget en el Dashboard, siempre con mes/año actual.

**Comportamiento:**
- Si no hay presupuestos configurados → no renderiza nada
- Si hay categorías en `warn` o `danger` → lista cada una con su porcentaje
- Si todo está `ok` → muestra "✓ Todos los presupuestos en orden este mes"

---

## Casos borde globales

| Caso | Comportamiento |
|---|---|
| Categoría con gastos pero sin límite | Se muestra sin barra, sin estado |
| Cambio de mes en Presupuesto.vue | Re-fetch automático (reactividad del composable) |
| Meta sin FechaLimite | No hay urgencia, sí hay fecha estimada si hay abonos |
| Meta recién creada sin abonos | No hay fecha estimada |
| Meta completada | No se muestra proyección ni urgencia |
| Sin presupuestos configurados | Widget del Dashboard no aparece |
| Todo en orden | Dashboard muestra checkmark verde |
