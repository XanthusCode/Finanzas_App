# Zaldo

**Controla tus finanzas. Cumple tus metas.**

Zaldo es una aplicación web de finanzas personales para registrar gastos, ingresos y deudas en un solo lugar, entender en qué se va el dinero y tomar mejores decisiones cada mes. Está pensada para el contexto colombiano: montos en pesos (COP), fechas en hora de Colombia y cuentas como Bancolombia, Nequi o efectivo.

---

## Funcionalidades

| Módulo | Qué hace |
|---|---|
| **Dashboard** | Resumen del mes: ingresos, gastos fijos/variables, ahorro y % de ahorro, con gráficos por categoría. |
| **Gastos** | Gastos fijos y variables, recurrentes (copia al mes siguiente), compras a cuotas, importación desde Excel y **escaneo de recibos con IA**. |
| **Ingresos** | Ingresos recurrentes con frecuencia (mensual, quincenal…) y día de pago. |
| **Categorías** | Categorías propias con ícono/color, conteo de categorías en uso y el historial de gastos de cada una (en COP, sin cuotas futuras). |
| **Cuentas** | Saldo calculado por cuenta (movimientos + ingresos − gastos) con historial unificado y ajustes de saldo para cuadrar con el banco. |
| **Cuotas** | Planes de pago a cuotas: cuánto va pagado, compromiso mensual y total pendiente. |
| **Presupuestos** | Límite mensual por categoría con alertas al acercarse o pasarse. |
| **Metas** | Metas de ahorro que pueden funcionar como "sobre" dentro de una cuenta. |
| **Deudas** | Lo que te deben y lo que debes, con abonos parciales vinculados a una cuenta. |
| **Historial / Proyección / Simulador** | Vista anual, proyección de saldo y simulador de escenarios. |
| **Insights con IA** | Análisis del mes en lenguaje natural generado con Groq, a partir de un resumen agregado (nunca transacciones crudas). |
| **WhatsApp** | Registrar un gasto escribiendo `25000 almuerzo` o `25k mercado` al bot. |
| **Recordatorios por email** | Deudas por vencer, presupuestos excedidos, cobros por confirmar y días sin registrar gastos. |

---

## Stack

**Frontend** — Vue 3 (Composition API) + TypeScript, Vite, Pinia, Vue Router, Axios, Zod, Chart.js, Lucide.

**Backend** — ASP.NET Core Web API (.NET 10) con Clean Architecture, EF Core + PostgreSQL, AutoMapper, JWT Bearer, QuestPDF.

**Servicios externos** — [Groq](https://groq.com) (IA), [Resend](https://resend.com) (email), [Twilio](https://www.twilio.com) (WhatsApp sandbox).

**Despliegue** — Frontend en Vercel, backend en Render (Docker), GitHub Actions para recordatorios diarios y keep-alive.

---

## Estructura

```
Backend/
  Domain/          Entidades (Gasto, Ingreso, Categoria, Cuenta, Meta, Deuda, User…)
  Application/     DTOs, interfaces de repositorios, servicios, AutoMapper
  Infrastructure/  AppDbContext (EF Core) y repositorios
  API/             Controllers, Program.cs, configuración
Frontend/
  src/views/       Páginas (Dashboard, Gastos, Cuentas, Cuotas, Metas, landing…)
  src/components/  Componentes por dominio + comunes y gráficos
  src/stores/      Pinia (auth, finance)
  src/services/    api.ts (axios con interceptores JWT)
.github/workflows/ recordatorios.yml (diario) y keep-alive.yml
Dockerfile         Imagen del backend para Render
```

---

## Cómo correrlo en local

### Requisitos

- .NET SDK 10
- Node `^20.19.0` o `>=22.12.0`
- PostgreSQL
- Certificado HTTPS de desarrollo: `dotnet dev-certs https --trust`

### 1. Configurar secretos del backend

Los valores de `appsettings.json` son placeholders (`CHANGE_ME`). Configúralos con user-secrets:

```powershell
cd Backend/API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=finanzas_db;Username=postgres;Password=..."
dotnet user-secrets set "Jwt:Key" "<cadena aleatoria de 32+ caracteres>"
dotnet user-secrets set "Groq:ApiKey" "gsk_..."             # opcional: insights y escaneo de recibos
dotnet user-secrets set "Resend:ApiKey" "re_..."            # opcional: emails
dotnet user-secrets set "Recordatorios:CronSecret" "..."    # opcional: endpoint de recordatorios
dotnet user-secrets set "Twilio:AuthToken" "..."            # opcional: bot de WhatsApp
dotnet user-secrets set "Twilio:WebhookUrl" "https://.../whatsapp/webhook"
```

En producción se usan variables de entorno con doble guion bajo (`Groq__ApiKey`, `Jwt__Key`, …).

### 2. Backend

```powershell
cd Backend/API
dotnet run
```

Escucha en `https://localhost:44311` (Swagger en `/swagger`). La base de datos y sus tablas se crean solas en el primer arranque.

### 3. Frontend

```powershell
cd Frontend
npm install
npm run dev
```

Abre `http://localhost:5173`. Vite redirige `/api` al backend.

---

## Notas de desarrollo

- **Sin migraciones EF.** El esquema se crea con `EnsureCreated()`, que no modifica una base existente. Cada tabla o columna nueva necesita su `CREATE TABLE IF NOT EXISTS` / `ALTER TABLE … ADD COLUMN IF NOT EXISTS` en el bloque SQL de `Program.cs`, y reiniciar el backend.
- **Saldo calculado, no almacenado.** Para cuadrar una cuenta con el banco se registra un movimiento de "Ajuste de saldo", no un gasto falso.
- **Meses futuros no cuentan.** Las cuotas de meses que aún no llegan no afectan saldos ni historiales hasta su mes.
- **Modelos de IA.** Se configuran en `Groq:Model` (texto) y `Groq:VisionModel` (recibos). Groq retira modelos con frecuencia: si el análisis falla con `model_not_found`, revisa los disponibles en `GET https://api.groq.com/openai/v1/models`.
- `CLAUDE.md` documenta la arquitectura y el historial de errores conocidos con sus soluciones.

---

## Checklist antes de un PR

- [ ] `dotnet build` sin errores
- [ ] `npm run type-check` sin errores
- [ ] Campo nuevo actualizado en Entidad → DTO → MappingProfile → Controller (y en el SQL de `Program.cs`)
- [ ] Sin `dotnet ef migrations add`
