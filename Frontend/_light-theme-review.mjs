import { chromium } from 'playwright';

const BASE = 'http://localhost:5173';
const OUT = 'C:/Users/Johan/AppData/Local/Temp/claude/c--Users-Johan-Documents-Programaci-n-SeparateProjects-Finanzas-App/966fc9ec-5c9d-467e-a9b3-44832031d010/scratchpad';

const resumen = (over = {}) => ({
  mes: 7, anio: 2026,
  totalIngresos: 4315000,
  totalFijos: 1850000,
  totalVariables: 980000,
  totalGastos: 2830000,
  ahorro: 1485000,
  pctAhorro: 34,
  ...over
});

const gastos = [
  { id: '1', categoria: 'Arriendo', detalle: 'Apto', monto: 1200000, tipo: 'FIJO', mes: 7, anio: 2026, esRecurrente: true },
  { id: '2', categoria: 'Servicios', detalle: 'Luz y agua', monto: 250000, tipo: 'FIJO', mes: 7, anio: 2026, esRecurrente: true },
  { id: '3', categoria: 'Internet', detalle: 'Plan hogar', monto: 120000, tipo: 'FIJO', mes: 7, anio: 2026, esRecurrente: true },
  { id: '4', categoria: 'Comida', detalle: 'Mercado', monto: 480000, tipo: 'VARIABLE', mes: 7, anio: 2026, esRecurrente: false },
  { id: '5', categoria: 'Transporte', detalle: 'Uber/gasolina', monto: 210000, tipo: 'VARIABLE', mes: 7, anio: 2026, esRecurrente: false },
  { id: '6', categoria: 'Ocio', detalle: 'Cine y salidas', monto: 150000, tipo: 'VARIABLE', mes: 7, anio: 2026, esRecurrente: false },
];

const tendencia = Array.from({ length: 12 }, (_, i) => resumen({
  totalIngresos: 3800000 + i * 45000,
  totalGastos: 2400000 + (i % 4) * 90000,
}));

const gastosPorCategoria = [
  { categoria: 'Arriendo', datos: tendencia.map(() => 1200000) },
  { categoria: 'Comida', datos: tendencia.map((_, i) => 400000 + i * 8000) },
  { categoria: 'Transporte', datos: tendencia.map((_, i) => 180000 + i * 4000) },
];

const deudas = [
  { id: 'd1', persona: 'Banco XY', descripcion: 'Tarjeta de crédito', monto: 500000, tipo: 'LE_DEBO', fechaLimite: '2026-07-28', pagada: false, creadoEn: '2026-01-01', montoPagado: 100000, montoRestante: 400000 },
  { id: 'd2', persona: 'Juan', descripcion: 'Préstamo personal', monto: 300000, tipo: 'ME_DEBEN', fechaLimite: '2026-08-15', pagada: false, creadoEn: '2026-01-01', montoPagado: 0, montoRestante: 300000 },
];

const presupuestos = [
  { id: 'p1', categoria: 'Comida', limite: 400000 },
  { id: 'p2', categoria: 'Transporte', limite: 200000 },
];

const resumenCategoria = [
  { categoria: 'Comida', total: 480000 },
  { categoria: 'Transporte', total: 210000 },
];

const categorias = [
  { id: 'c1', nombre: 'Comida', tipo: 'VARIABLE', activa: true },
  { id: 'c2', nombre: 'Transporte', tipo: 'VARIABLE', activa: true },
  { id: 'c3', nombre: 'Arriendo', tipo: 'FIJO', activa: true },
  { id: 'c4', nombre: 'Servicios', tipo: 'FIJO', activa: true },
];

const heatmap = Array.from({ length: 60 }, (_, i) => {
  const d = new Date(2026, 5, 1 + i);
  return { fecha: d.toISOString(), total: Math.random() > 0.5 ? Math.round(Math.random() * 200000) : 0 };
});

const metas = [
  { id: 'm1', nombre: 'Vacaciones', montoObjetivo: 3000000, montoActual: 1200000, fechaLimite: '2026-12-01', completada: false, creadoEn: '2026-01-01' },
  { id: 'm2', nombre: 'Fondo emergencia', montoObjetivo: 5000000, montoActual: 5000000, completada: true, creadoEn: '2026-01-01' },
];

const OUT_PATH = (name) => `${OUT}/${name}`;

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1440, height: 1000 } });
const page = await context.newPage();
page.on('console', msg => { if (msg.type() === 'error') console.log('CONSOLE ERROR:', msg.text().slice(0, 300)); });
page.on('pageerror', err => console.log('PAGE ERROR:', err.message));

await page.route('**/api/**', async route => {
  const url = new URL(route.request().url());
  const p = url.pathname.replace(/^\/api/, '');
  const json = (body) => route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(body) });

  if (p === '/gastos' && route.request().method() === 'GET') return json(gastos);
  if (p === '/gastos/resumen-categoria') return json(resumenCategoria);
  if (p === '/gastos/heatmap') return json(heatmap);
  if (p === '/gastos/suscripciones') return json([]);
  if (p === '/gastos/cuotas') return json([]);
  if (p === '/ingresos') return json([]);
  if (p === '/categorias') return json(categorias);
  if (p === '/presupuestos') return json(presupuestos);
  if (p === '/deudas') return json(deudas);
  if (p.match(/\/deudas\/.*\/abonos/)) return json([]);
  if (p === '/metas') return json(metas);
  if (p === '/cuentas') return json([]);
  if (p === '/resumen/anual') return json(resumen({ ahorro: 9200000, pctAhorro: 28, totalIngresos: 44000000, totalGastos: 34800000 }));
  if (p === '/resumen/tendencia') return json(tendencia);
  if (p === '/resumen/gastos-por-categoria') return json(gastosPorCategoria);
  if (p === '/resumen/insights') return json({ estado: 'ok', texto: 'Vas bien este mes.' });
  if (p === '/resumen') return json(resumen());
  return json({});
});

await page.addInitScript(() => {
  localStorage.setItem('finanzas_token', 'fake.jwt.token');
  localStorage.setItem('finanzas_theme', 'light');
});

async function shot(path, name, opts = {}) {
  await page.goto(BASE + path, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1200);
  await page.screenshot({ path: OUT_PATH(name), fullPage: true, ...opts });
  console.log('shot:', name);
}

await shot('/dashboard', 'v2-dashboard.png');
await shot('/historial', 'v2-historial.png');
await shot('/categorias', 'v2-categorias.png');

// Open category add form for a live preview of the icon-badge color
await page.click('button:has-text("Nueva categoría")');
await page.fill('input[placeholder="Nombre de la categoría"]', 'Mercado');
await page.waitForTimeout(300);
await page.screenshot({ path: OUT_PATH('v2-categorias-form.png') });

await shot('/metas', 'v2-metas.png');
await shot('/deudas', 'v2-deudas.png');
await shot('/simulador', 'v2-simulador.png');
await shot('/gastos', 'v2-gastos.png');

console.log('DONE');
await browser.close();
