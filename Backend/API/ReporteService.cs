using Finanzas.Application.DTOs;
using Finanzas.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Finanzas.API;

public class ReporteService
{
    private static readonly string[] NombresMes =
        ["", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
         "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];

    private static string Fmt(decimal n) => "$" + Math.Round(n).ToString("N0");
    private static string MesNombre(int mes) => mes is >= 1 and <= 12 ? NombresMes[mes] : mes.ToString();
    private static string PctLabel(decimal parte, decimal total) =>
        total > 0 ? $"{Math.Round(parte / total * 100)}% del ingreso" : "0%";

    public byte[] Generar(ResumenDto resumen, IEnumerable<Gasto> gastos, IEnumerable<Ingreso> ingresos)
    {
        var gastosList = gastos.ToList();
        var ingresosList = ingresos.ToList();

        var porCategoria = gastosList
            .GroupBy(g => g.Categoria)
            .Select(g => (Nombre: g.Key, Total: g.Sum(x => x.Monto)))
            .OrderByDescending(x => x.Total)
            .ToList();

        decimal maxCat = porCategoria.Count > 0 ? porCategoria[0].Total : 1;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.8f, Unit.Centimetre);
                page.MarginVertical(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(9).FontColor("#111827"));

                // ── Header ──────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Finanzas App")
                            .FontSize(18).Bold().FontColor("#34d399");
                        row.ConstantItem(180).AlignRight().Column(c =>
                        {
                            c.Item().Text("Reporte Mensual").FontSize(11).Bold();
                            c.Item().Text($"{MesNombre(resumen.Mes)} {resumen.Anio}")
                                .FontSize(9).FontColor("#6b7280");
                        });
                    });
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor("#e5e7eb");
                });

                // ── Content ──────────────────────────────────────────────
                page.Content().PaddingTop(14).Column(col =>
                {
                    // KPIs
                    col.Item().Row(row =>
                    {
                        KpiBox(row.RelativeItem(), "Ingresos", Fmt(resumen.TotalIngresos), null, "#34d399");
                        row.ConstantItem(6);
                        KpiBox(row.RelativeItem(), "Gastos fijos", Fmt(resumen.TotalFijos), PctLabel(resumen.TotalFijos, resumen.TotalIngresos), "#f87171");
                        row.ConstantItem(6);
                        KpiBox(row.RelativeItem(), "Gastos variables", Fmt(resumen.TotalVariables), PctLabel(resumen.TotalVariables, resumen.TotalIngresos), "#fbbf24");
                        row.ConstantItem(6);
                        KpiBox(row.RelativeItem(), "Ahorro", Fmt(resumen.Ahorro), $"{resumen.PctAhorro}% del ingreso", resumen.Ahorro >= 0 ? "#34d399" : "#f87171");
                    });

                    // Ingresos
                    col.Item().PaddingTop(16).Text("Ingresos del mes").FontSize(10).Bold();
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(100); });
                        table.Header(h =>
                        {
                            h.Cell().Background("#f3f4f6").Padding(5).Text("Concepto").FontSize(8.5f).Bold();
                            h.Cell().Background("#f3f4f6").Padding(5).AlignRight().Text("Monto").FontSize(8.5f).Bold();
                        });
                        foreach (var ing in ingresosList)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor("#f3f4f6").Padding(5)
                                .Text(ing.Concepto).FontSize(8.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor("#f3f4f6").Padding(5)
                                .AlignRight().Text(Fmt(ing.Monto)).FontSize(8.5f);
                        }
                        if (ingresosList.Count == 0)
                        {
                            table.Cell().ColumnSpan(2).Padding(5)
                                .Text("Sin ingresos registrados").FontSize(8.5f).FontColor("#9ca3af");
                        }
                    });

                    // Gastos por categoría (barra horizontal)
                    col.Item().PaddingTop(16).Text("Gastos por categoría").FontSize(10).Bold();
                    col.Item().PaddingTop(8).Column(barCol =>
                    {
                        foreach (var (nombre, total) in porCategoria)
                        {
                            float filled = maxCat > 0 ? (float)(total / maxCat) : 0f;
                            float empty  = Math.Max(0f, 1f - filled);

                            barCol.Item().PaddingBottom(6).Row(r =>
                            {
                                r.ConstantItem(110).AlignMiddle().Text(nombre).FontSize(8.5f);
                                r.RelativeItem().AlignMiddle().Row(bar =>
                                {
                                    if (filled > 0)
                                        bar.RelativeItem(filled).Height(7).Background("#34d399");
                                    if (empty > 0)
                                        bar.RelativeItem(empty).Height(7).Background("#f3f4f6");
                                });
                                r.ConstantItem(90).AlignMiddle().AlignRight()
                                    .Text(Fmt(total)).FontSize(8.5f);
                            });
                        }
                        if (porCategoria.Count == 0)
                            barCol.Item().Text("Sin gastos registrados").FontSize(8.5f).FontColor("#9ca3af");
                    });

                    // Detalle de gastos
                    col.Item().PaddingTop(16).Text("Detalle de gastos").FontSize(10).Bold();
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.ConstantColumn(100);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Background("#f3f4f6").Padding(5).Text("Categoría").FontSize(8.5f).Bold();
                            h.Cell().Background("#f3f4f6").Padding(5).Text("Detalle").FontSize(8.5f).Bold();
                            h.Cell().Background("#f3f4f6").Padding(5).AlignRight().Text("Monto").FontSize(8.5f).Bold();
                        });
                        foreach (var g in gastosList.OrderBy(g => g.Categoria))
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor("#f3f4f6").Padding(5)
                                .Text(g.Categoria).FontSize(8.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor("#f3f4f6").Padding(5)
                                .Text(g.Detalle).FontSize(8.5f).FontColor("#6b7280");
                            table.Cell().BorderBottom(0.5f).BorderColor("#f3f4f6").Padding(5)
                                .AlignRight().Text(Fmt(g.Monto)).FontSize(8.5f);
                        }
                        if (gastosList.Count == 0)
                        {
                            table.Cell().ColumnSpan(3).Padding(5)
                                .Text("Sin gastos registrados").FontSize(8.5f).FontColor("#9ca3af");
                        }
                    });
                });

                // ── Footer ──────────────────────────────────────────────
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}  •  Página ")
                        .FontSize(7.5f).FontColor("#9ca3af");
                    t.CurrentPageNumber().FontSize(7.5f).FontColor("#9ca3af");
                    t.Span(" de ").FontSize(7.5f).FontColor("#9ca3af");
                    t.TotalPages().FontSize(7.5f).FontColor("#9ca3af");
                });
            });
        }).GeneratePdf();
    }

    private static void KpiBox(IContainer c, string label, string value, string? sub, string color)
    {
        c.Border(1).BorderColor("#e5e7eb").Background("#fafafa").Padding(10).Column(col =>
        {
            col.Item().Text(label).FontSize(7.5f).FontColor("#6b7280").Bold();
            col.Item().PaddingTop(4).Text(value).FontSize(14).Bold().FontColor(color);
            if (sub != null)
                col.Item().PaddingTop(2).Text(sub).FontSize(7.5f).FontColor("#9ca3af");
        });
    }
}
