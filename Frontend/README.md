# Finanzas App — Frontend

SPA de finanzas personales construida con Vue 3, TypeScript y Vite. Consume la API en `Backend/` (ASP.NET Core) para gestionar gastos, ingresos, categorías, presupuestos y metas.

## Stack

- **Vue 3** (Composition API) + **TypeScript**
- **Vite** como bundler/dev server
- **Pinia** para estado global (`auth`, `finance`)
- **Vue Router** con guards de autenticación
- **Axios** con interceptores JWT
- **Zod** para validación de formularios
- **Chart.js / vue-chartjs** para gráficos del dashboard

## Estructura

```
src/
  views/          Páginas: Dashboard, Gastos, Ingresos, Categorias, Presupuesto, Metas, Historial, Simulador, auth
  components/
    common/        KpiCard, ConfirmModal, CurrencyInput, MonthSelector, UserMenu
    charts/        BarChart, DonutChart
    gastos/        GastoForm, GastoRow
  stores/          auth.ts, finance.ts (Pinia)
  services/        api.ts (axios con baseURL /api e interceptores JWT)
  router/          index.ts + routes.ts
  types/           Interfaces TypeScript compartidas
  schemas/         Validaciones Zod
  utils/           Helpers varios
```

## Requisitos

- Node `^20.19.0` o `>=22.12.0`
- Backend corriendo en `https://localhost:44311` (ver `Backend/API`)

## Puesta en marcha

```sh
npm install
npm run dev
```

La app queda disponible en `http://localhost:5173`. El proxy de Vite reescribe las peticiones `/api` hacia el backend (`vite.config.ts`), por lo que el backend debe estar corriendo para que el login y las llamadas a la API funcionen.

## Scripts disponibles

| Script | Descripción |
|---|---|
| `npm run dev` | Servidor de desarrollo con hot-reload |
| `npm run build` | Type-check + build de producción |
| `npm run build-only` | Build sin type-check (más rápido) |
| `npm run preview` | Sirve el build de producción localmente |
| `npm run type-check` | Chequeo de tipos con `vue-tsc` |

## Autenticación

El token JWT se guarda en `localStorage` bajo la clave `finanzas_token`. Si el backend responde `401`, el interceptor de axios limpia el token y redirige a `/login`.

## Configuración recomendada del editor

[VS Code](https://code.visualstudio.com/) + extensión [Vue (Official)](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (deshabilitar Vetur si está instalado). El proyecto usa `vue-tsc` en lugar de `tsc` para que TypeScript entienda los tipos de archivos `.vue`.
