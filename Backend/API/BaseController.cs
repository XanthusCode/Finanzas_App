using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Authorize]
[ApiController]
public class BaseController : ControllerBase
{
    protected Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? Guid.Empty.ToString());

    protected static bool PeriodoValido(int mes, int anio) =>
    mes is >= 1 and <= 12 && anio is >= 2000 and <= 2100;

}
