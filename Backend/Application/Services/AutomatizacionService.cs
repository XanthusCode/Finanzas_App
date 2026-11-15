using Finanzas.Application.Interfaces;

namespace Finanzas.Application.Services
{
    public class AutomatizacionService(IGastosRepository gastosRepo, IUserRepository userRepo, GastosService gastosService)
    {
        public async Task ProcesarAsync()
        {
            var hoy = DateTime.UtcNow.AddHours(-5);   // Colombia
            var periodo = hoy.Year * 100 + hoy.Month;
            var (mesPrev, anioPrev) = hoy.Month == 1 ? (12, hoy.Year - 1) : (hoy.Month - 1, hoy.Year);

            foreach (var user in await userRepo.GetAllAsync())
            {
                var ultima = await gastosRepo.GetUltimaActividadAsync(user.Id);
                if ((hoy - (ultima ?? user.CreadoEn)).TotalDays > 60) continue;   // abandonó la app

                var delMes = (await gastosRepo.GetByMesAsync(hoy.Month, hoy.Year, user.Id)).ToList();

                // a) Copiar los fijos una sola vez al mes
                if (user.RecurrentesCopiadosHasta != periodo)
                {
                    if (!delMes.Any(g => g.EsRecurrente))
                    {
                        await gastosService.CopiarRecurrentesAsync(hoy.Month, hoy.Year, user.Id, generadoAuto: true);
                        delMes = (await gastosRepo.GetByMesAsync(hoy.Month, hoy.Year, user.Id)).ToList();
                    }
                    user.RecurrentesCopiadosHasta = periodo;
                    await userRepo.UpdateAsync(user);
                }

                // b) Confirmar el cobro si ya pasó el día del mes anterior
                var previos = await gastosRepo.GetByMesAsync(mesPrev, anioPrev, user.Id);
                var diasMes = DateTime.DaysInMonth(hoy.Year, hoy.Month);
                foreach (var g in delMes.Where(g => g.Fecha is null && g.EsRecurrente))
                {
                    var antes = previos.FirstOrDefault(p => p.Fecha is not null
                        && p.Categoria == g.Categoria && p.Detalle == g.Detalle);
                    if (antes is null) continue;

                    var dia = Math.Min(antes.Fecha!.Value.Day, diasMes);
                    if (hoy.Day < dia) continue;

                    g.Fecha = new DateTime(hoy.Year, hoy.Month, dia, 0, 0, 0, DateTimeKind.Utc);
                    g.GeneradoAuto = true;
                    await gastosRepo.UpdateAsync(g);
                }
            }
        }

    }
}
